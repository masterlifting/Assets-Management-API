using IM.Service.Common.Net.HttpServices;
using IM.Service.Common.Net.Models.Dto.Http;
using IM.Service.Common.Net.Models.Dto.Http.Companies;
using IM.Service.Common.Net.Models.Dto.Mq.Companies;
using IM.Service.Common.Net.RabbitServices;

using IM.Service.Company.Data.DataAccess.Entities;
using IM.Service.Company.Data.DataAccess.Repository;
using IM.Service.Company.Data.Settings;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using IM.Service.Common.Net.RepositoryService.Comparators;
using IM.Service.Common.Net.RepositoryService.Filters;

namespace IM.Service.Company.Data.Services.DtoServices
{
    public class StockVolumesDtoManager
    {
        private readonly RepositorySet<DataAccess.Entities.Company> companyRepository;
        private readonly RepositorySet<StockVolume> stockVolumeRepository;
        private readonly string rabbitConnectionString;
        public StockVolumesDtoManager(
            RepositorySet<DataAccess.Entities.Company> companyRepository,
            RepositorySet<StockVolume> stockVolumeRepository,
            IOptions<ServiceSettings> options)
        {
            this.companyRepository = companyRepository;
            this.stockVolumeRepository = stockVolumeRepository;
            rabbitConnectionString = options.Value.ConnectionStrings.Mq;
        }

        public async Task<ResponseModel<StockVolumeGetDto>> GetAsync(string companyId, DateTime date)
        {
            var company = await companyRepository.FindAsync(companyId.ToUpperInvariant().Trim());

            if (company is null)
                return new() { Errors = new[] { "company not found" } };

            var stockVolume = await stockVolumeRepository.FindAsync(company.Id, date);

            if (stockVolume is null)
                return new() { Errors = new[] { "stock volume not found" } };

            return new()
            {
                Errors = Array.Empty<string>(),
                Data = new()
                {
                    Company = company.Name,
                    Date = stockVolume.Date,
                    SourceType = stockVolume.SourceType,
                    Value = stockVolume.Value,
                }
            };
        }
        public async Task<ResponseModel<PaginatedModel<StockVolumeGetDto>>> GetAsync(CompanyDataFilterByDate<StockVolume> filter, HttpPagination pagination)
        {
            var filteredQuery = stockVolumeRepository.GetQuery(filter.FilterExpression);
            var count = await stockVolumeRepository.GetCountAsync(filteredQuery);
            var paginatedQuery = stockVolumeRepository.GetPaginationQuery(filteredQuery, pagination, x => x.Date);

            var result = await paginatedQuery
                .Join(companyRepository.GetDbSet(), x => x.CompanyId, y => y.Id, (x, y) => new StockVolumeGetDto
                {
                    Company = y.Name,
                    Date = x.Date,
                    Value = x.Value,
                    SourceType = x.SourceType
                })
            .ToArrayAsync();

            return new()
            {
                Errors = Array.Empty<string>(),
                Data = new()
                {
                    Items = result,
                    Count = count
                }
            };
        }
        public async Task<ResponseModel<PaginatedModel<StockVolumeGetDto>>> GetLastAsync(CompanyDataFilterByDate<StockVolume> filter, HttpPagination pagination)
        {
            var filteredQuery = stockVolumeRepository.GetQuery(filter.FilterExpression);

            var queryResult = await filteredQuery
                .Join(companyRepository.GetDbSet(), x => x.CompanyId, y => y.Id, (x, y) => new StockVolumeGetDto
                {
                    Company = y.Name,
                    Date = x.Date,
                    Value = x.Value,
                    SourceType = x.SourceType
                })
                .ToArrayAsync();

            var groupedResult = queryResult
                .GroupBy(x => x.Company)
                .Select(x => x
                    .OrderBy(y => y.Date)
                    .Last())
                .ToArray();

            return new()
            {
                Errors = Array.Empty<string>(),
                Data = new()
                {
                    Items = pagination.GetPaginatedResult(groupedResult),
                    Count = groupedResult.Length
                }
            };
        }

        public async Task<ResponseModel<string>> CreateAsync(StockVolumePostDto model)
        {
            var ctxEntity = new StockVolume
            {
                CompanyId = model.CompanyId,
                SourceType = model.SourceType,
                Date = model.Date.Date,
                Value = model.Value
            };
            var message = $"stock volume of '{model.CompanyId}' create at {model.Date:yyyy MMMM dd}";
            var (error, stockVolume) = await stockVolumeRepository.CreateAsync(ctxEntity, message);

            if (error is not null)
                return new() { Errors = new[] { error } };

            //set to queue
            var publisher = new RabbitPublisher(rabbitConnectionString, QueueExchanges.Function);
            publisher.PublishTask(
                QueueNames.CompanyAnalyzer
                , QueueEntities.Price
                , QueueActions.Call
                , JsonSerializer.Serialize(new PriceIdentityDto
                {
                    CompanyId = stockVolume!.CompanyId,
                    Date = stockVolume.Date,
                }));

            return new() { Data = message + " success" };
        }
        public async Task<ResponseModel<string>> CreateAsync(IEnumerable<StockVolumePostDto> models)
        {
            var stockVolumes = models.ToArray();

            if (!stockVolumes.Any())
                return new() { Errors = new[] { "stock volume data for creating not found" } };

            var ctxEntities = stockVolumes.GroupBy(x => x.Date.Date).Select(x => new StockVolume
            {
                CompanyId = x.Last().CompanyId,
                SourceType = x.Last().SourceType,
                Date = x.Last().Date.Date,
                Value = x.Last().Value
            });

            var message = $"stock volumes of '{stockVolumes[0].CompanyId}' create";
            var (error, _) = await stockVolumeRepository.CreateAsync(ctxEntities, new CompanyDateComparer<StockVolume>(), message);

            return error is not null
                ? new() { Errors = new[] { error } }
                : new() { Data = message + " success" };
        }
        public async Task<ResponseModel<string>> UpdateAsync(StockVolumePostDto model)
        {
            var ctxEntity = new StockVolume
            {
                CompanyId = model.CompanyId,
                SourceType = model.SourceType,
                Date = model.Date,
                Value = model.Value
            };

            var message = $"stock volume of '{model.CompanyId}' update at {model.Date:yyyy MMMM dd}";
            var (error, stockVolume) = await stockVolumeRepository.UpdateAsync(ctxEntity, message);

            if (error is not null)
                return new() { Errors = new[] { error } };

            //set to queue
            var publisher = new RabbitPublisher(rabbitConnectionString, QueueExchanges.Function);
            publisher.PublishTask(
                QueueNames.CompanyAnalyzer
                , QueueEntities.Price
                , QueueActions.Call
                , JsonSerializer.Serialize(new PriceIdentityDto
                {
                    CompanyId = stockVolume!.CompanyId,
                    Date = stockVolume.Date,
                }));

            return new() { Data = message + " success" };
        }
        public async Task<ResponseModel<string>> DeleteAsync(string companyId, DateTime date)
        {
            companyId = companyId.ToUpperInvariant().Trim();

            var message = $"stock volume of '{companyId}' delete at {date:yyyy MMMM dd}";
            var (error, deletedEntity) = await stockVolumeRepository.DeleteAsync(message, companyId, date);

            if (error is not null)
                return new() { Errors = new[] { error } };

            var publisher = new RabbitPublisher(rabbitConnectionString, QueueExchanges.Function);
            publisher.PublishTask(
                QueueNames.CompanyAnalyzer
                , QueueEntities.Price
                , QueueActions.Call
                , JsonSerializer.Serialize(new PriceIdentityDto
                {
                    CompanyId = deletedEntity!.CompanyId,
                    Date = deletedEntity.Date,
                }));

            return new() { Data = message + " success" };
        }
    }
}
