using IM.Service.Common.Net.Models.Dto.Mq.Companies;
using IM.Service.Common.Net.RabbitServices;

using IM.Service.Company.Data.DataAccess.Entities;
using IM.Service.Company.Data.DataAccess.Repository;
using IM.Service.Company.Data.Models.Data;
using IM.Service.Company.Data.Settings;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using IM.Service.Common.Net.RepositoryService.Comparators;
using static IM.Service.Company.Data.Services.DataServices.Prices.PriceHelper;

namespace IM.Service.Company.Data.Services.DataServices.Prices
{
    public class PriceLoader : IDataLoad<Price,PriceDataConfigModel>
    {
        private readonly RepositorySet<Price> priceRepository;
        private readonly RepositorySet<DataAccess.Entities.Company> companyRepository;
        private readonly RepositorySet<CompanySourceType> companySourceTypeRepository;
        private readonly PriceParser parser;
        private readonly string rabbitConnectionString;
        public PriceLoader(
            IOptions<ServiceSettings> options,
            RepositorySet<Price> priceRepository,
            PriceParser parser,
            RepositorySet<DataAccess.Entities.Company> companyRepository,
            RepositorySet<CompanySourceType> companySourceTypeRepository)
        {
            this.priceRepository = priceRepository;
            this.companyRepository = companyRepository;
            this.companySourceTypeRepository = companySourceTypeRepository;
            this.parser = parser;
            rabbitConnectionString = options.Value.ConnectionStrings.Mq;
        }

        public async Task<Price[]> DataSetAsync(string companyId)
        {
            var result = Array.Empty<Price>();
            var company = await companyRepository.FindAsync(companyId);

            if (company is null)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"'{companyId}' was not found");
                Console.ForegroundColor = ConsoleColor.Gray;
                return result;
            }

            var sources = await companySourceTypeRepository
                .GetSampleAsync(x => x.CompanyId == company.Id, x => new
                {
                    x.SourceType.Name,
                    x.Value
                });

            if (!sources.Any())
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"'{company.Name}' sources was not found");
                Console.ForegroundColor = ConsoleColor.Gray;
                return result;
            }

            foreach (var source in sources)
            {
                var isWeekend = IsExchangeWeekend(source.Name, DateTime.UtcNow);

                if (isWeekend)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"'{company.Name}' source has a day off");
                    Console.ForegroundColor = ConsoleColor.Gray;
                    continue;
                }

                var lastPrice = await GetLastDatabaseDataAsync(company.Id);

                PriceDataConfigModel config = lastPrice is not null
                    ? new()
                    {
                        CompanyId = company.Id,
                        SourceValue = source.Value,
                        Date = lastPrice.Date
                    }
                    : new()
                    {
                        CompanyId = company.Id,
                        SourceValue = source.Value,
                        Date = DateTime.UtcNow.AddYears(-1)
                    };

                var prices = await DataGetAsync(source.Name, config);

                result = !result.Any()
                    ? await SaveAsync(prices)
                    : result.Concat(await SaveAsync(prices)).ToArray();
            }

            if (result.Length <= 0)
                return result;

            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine($"New price config for '{company.Name}'");
            Console.ForegroundColor = ConsoleColor.Gray;
            
            return result;
        }
        public async Task<Price[]> DataSetAsync()
        {
            var lastPrices = await GetLastDatabaseDataAsync();
            var lastPricesDictionary = lastPrices.ToDictionary(x => x.CompanyId, y => y.Date);
            var companySourceTypes = await companyRepository.GetDbSet()
                .Join(companySourceTypeRepository.GetDbSet(), x => x.Id, y => y.CompanyId, (x, y) => new
                {
                    CompanyId = x.Id,
                    SourceName = y.SourceType.Name,
                    SourceValue = y.Value
                })
                .ToArrayAsync();

            var result = Array.Empty<Price>();

            foreach (var source in companySourceTypes.GroupBy(x => x.SourceName))
            {
                var isWeekend = IsExchangeWeekend(source.Key, DateTime.UtcNow);

                if (isWeekend)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"'{source.Key}' source has a day off");
                    Console.ForegroundColor = ConsoleColor.Gray;
                    continue;
                }

                var config = source
                    .Select(x => lastPricesDictionary.ContainsKey(x.CompanyId)
                        ? new PriceDataConfigModel
                        {
                            CompanyId = x.CompanyId,
                            SourceValue = x.SourceValue,
                            Date = lastPricesDictionary[x.CompanyId].Date
                        }
                        : new PriceDataConfigModel
                        {
                            CompanyId = x.CompanyId,
                            SourceValue = x.SourceValue,
                            Date = DateTime.UtcNow.AddYears(-1)
                        })
                    .ToArray();

                var prices = await DataGetAsync(source.Key, config);

                result = !result.Any()
                    ? await SaveAsync(prices)
                    : result.Concat(await SaveAsync(prices)).ToArray();
            }

            if (result.Length <= 0)
                return result;

            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine($"Prices for '{result.GroupBy(x => x.CompanyId).Count()}' companies was processed.");
            Console.ForegroundColor = ConsoleColor.Gray;

            return result;
        }

        public async Task<Price?> GetLastDatabaseDataAsync(string companyId)
        {
            var prices = await priceRepository.GetSampleAsync(x => x.CompanyId == companyId && x.Date >= DateTime.UtcNow.AddMonths(-1));
            return prices
                .OrderBy(x => x.Date)
                .LastOrDefault();
        }
        public async Task<Price[]> GetLastDatabaseDataAsync()
        {
            var prices = await priceRepository.GetSampleAsync(x => x.Date >= DateTime.UtcNow.AddMonths(-1));
            return prices
                .GroupBy(x => x.CompanyId)
                .Select(x =>
                    x.OrderBy(y => y.Date)
                    .Last())
                .ToArray();
        }

        public async Task<Price[]> DataGetAsync(string source, PriceDataConfigModel config)
        {
            var result = await parser.LoadLastPricesAsync(source, config);

            if (config.Date < GetExchangeWorkDate(source))
                result = result.Concat(await parser.LoadHistoryPricesAsync(source, config)).ToArray();

            return result;
        }
        public async Task<Price[]> DataGetAsync(string source, IEnumerable<PriceDataConfigModel> config)
        {
            var _data = config.ToArray();

            if (!_data.Any())
                return Array.Empty<Price>();

            var exchangeDate = GetExchangeWorkDate(source);

            var dataToHistory = _data.Where(x => x.Date < exchangeDate).ToArray();

            var result = await parser.LoadLastPricesAsync(source, _data);

            if (dataToHistory.Any())
                result = result.Concat(await parser.LoadHistoryPricesAsync(source, dataToHistory)).ToArray();

            return result;
        }

        public async Task<Price[]> SaveAsync(IEnumerable<Price> entities)
        {
            var data = entities.ToArray();
            var result = new List<Price>(data.Length);

            var currentPrices = data.Where(x => x.Date == DateTime.UtcNow.Date);
            var restPrices = data.Where(x => x.Date != DateTime.UtcNow.Date);

            var (createError, createdResult) = await priceRepository.CreateAsync(restPrices, new CompanyDateComparer<Price>(), "entities");
            var (updateError, updatedResult) = await priceRepository.CreateUpdateAsync(currentPrices, new CompanyDateComparer<Price>(), "entities");

            if (createError is not null)
                result.AddRange(createdResult!);

            if (updateError is not null)
                result.AddRange(updatedResult!);

            var publisher = new RabbitPublisher(rabbitConnectionString, QueueExchanges.Transfer);
            foreach (var price in result)
                publisher.PublishTask(
                    QueueNames.CompanyAnalyzer
                    , QueueEntities.Price
                    , QueueActions.Create
                    , JsonSerializer.Serialize(new PriceIdentityDto
                    {
                        CompanyId = price.CompanyId,
                        Date = price.Date
                    }));

            return result.ToArray();
        }
    }
}