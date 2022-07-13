using Castle.Core.Internal;

using IM.Service.Market.Domain.DataAccess;
using IM.Service.Market.Domain.DataAccess.Comparators;
using IM.Service.Market.Domain.Entities;
using IM.Service.Market.Domain.Entities.ManyToMany;
using IM.Service.Market.Models.Api.Http;
using IM.Service.Market.Services.RabbitMq;
using IM.Service.Shared.Models.RabbitMq.Api;
using IM.Service.Shared.Models.Service;
using IM.Service.Shared.RabbitMq;

using Microsoft.EntityFrameworkCore;

using System.Collections;

using static IM.Service.Shared.Helpers.ServiceHelper;

namespace IM.Service.Market.Services.Http;

public class CompanyApi
{
    private readonly Repository<Company> companyRepo;
    private readonly Repository<CompanySource> companySourceRepo;

    private readonly RabbitAction rabbitAction;
    public CompanyApi(
        RabbitAction rabbitAction,
        Repository<Company> companyRepo,
        Repository<CompanySource> companySourceRepo)
    {
        this.companyRepo = companyRepo;
        this.companySourceRepo = companySourceRepo;
        this.rabbitAction = rabbitAction;
    }

    public async Task<CompanyGetDto> GetAsync(string companyId)
    {
        var company = await companyRepo.FindAsync(companyId.Trim().ToUpperInvariant())
                      ?? await companyRepo.FindAsync(x => x.Name.Contains(companyId)); //TODO: TO DELETE

        return company is not null
            ? new CompanyGetDto
            {
                Id = company.Id,
                Name = company.Name,
                Country = company.Country.Name,
                Industry = company.Industry.Name,
                Sector = company.Industry.Sector.Name,
                Description = company.Description,
                Data = company.GetType().GetProperties()
                    .Where(x => x.PropertyType != typeof(string))
                    .Where(x => x.PropertyType.GetInterface(nameof(IEnumerable)) != null)
                    .Where(x => !((IEnumerable?)x.GetValue(company)).IsNullOrEmpty())
                    .Select(x => x.Name.ToLowerInvariant())
                    .ToArray()
            }
            : throw new NullReferenceException($"'{companyId}' not found");
    }
    public async Task<PaginationModel<CompanyGetDto>> GetAsync(Paginatior pagination)
    {
        var count = await companyRepo.GetCountAsync();
        var paginatedResult = companyRepo.GetPaginationQuery(pagination, x => x.Name);

        var companies = await paginatedResult.Select(x => new CompanyGetDto
        {
            Id = x.Id,
            Name = x.Name,
            Country = x.Country.Name,
            Sector = x.Industry.Sector.Name,
            Industry = x.Industry.Name,
            Description = x.Description
        })
        .ToArrayAsync();

        return new PaginationModel<CompanyGetDto>
        {
            Items = companies,
            Count = count
        };
    }

    public async Task<Company> CreateAsync(CompanyPostDto model)
    {
        var entity = new Company
        {
            Id = string.Intern(model.Id.Trim().ToUpperInvariant()),
            Name = model.Name,
            CountryId = model.CountryId,
            IndustryId = model.IndustryId,
            Description = model.Description
        };

        return await companyRepo.CreateAsync(entity, entity.Name);
    }
    public async Task<Company[]> CreateAsync(IEnumerable<CompanyPostDto> models)
    {
        var dtos = models.ToArray();

        if (!dtos.Any())
            return Array.Empty<Company>();

        var entities = dtos.Select(x => new Company
        {
            Id = string.Intern(x.Id.ToUpperInvariant().Trim()),
            Name = x.Name,
            IndustryId = x.IndustryId,
            CountryId = x.CountryId,
            Description = x.Description
        }).ToArray();

        return await companyRepo.CreateRangeAsync(entities, new CompanyComparer(), string.Join("; ", entities.Select(x => x.Id)));
    }
    public async Task<Company> UpdateAsync(string companyId, CompanyPutDto model)
    {
        var entity = new Company
        {
            Id = string.Intern(companyId.ToUpperInvariant().Trim()),
            Name = model.Name,
            IndustryId = model.IndustryId,
            CountryId = model.CountryId,
            Description = model.Description
        };

        return await companyRepo.UpdateAsync(new[] { entity.Id }, entity, entity.Name);
    }
    public async Task<Company[]> UpdateAsync(IEnumerable<CompanyPostDto> models)
    {
        var dtos = models.ToArray();

        if (!dtos.Any())
            return Array.Empty<Company>();

        var entities = dtos.Select(x => new Company
        {
            Id = string.Intern(x.Id.ToUpperInvariant().Trim()),
            Name = x.Name,
            IndustryId = x.IndustryId,
            CountryId = x.CountryId,
            Description = x.Description
        })
        .ToArray();

        return await companyRepo.UpdateRangeAsync(entities, string.Join("; ", entities.Select(x => x.Id)));
    }
    public async Task<string> DeleteAsync(string companyId)
    {
        companyId = string.Intern(companyId.ToUpperInvariant().Trim());
        await companyRepo.DeleteAsync(new[] { companyId }, companyId);
        return companyId;
    }

    public async Task<string> LoadAsync()
    {
        var companySources = await companySourceRepo.GetSampleAsync(x => x);

        rabbitAction.Publish(QueueExchanges.Function, QueueNames.Market, QueueEntities.AssetSources, QueueActions.Get, companySources);

        return $"Load data by {string.Join(";", companySources.Select(x => Enum.Parse<Enums.Statuses>(x.SourceId.ToString())))} is starting...";
    }
    public async Task<string> LoadAsync(string companyId)
    {
        companyId = companyId.Trim().ToUpperInvariant();
        var companySources = await companySourceRepo.GetSampleAsync(x => x.CompanyId == companyId);

        rabbitAction.Publish(QueueExchanges.Function, QueueNames.Market, QueueEntities.AssetSources, QueueActions.Get, companySources);

        return $"Load data by {string.Join(";", companySources.Select(x => Enum.Parse<Enums.Statuses>(x.SourceId.ToString())))} is starting...";
    }
    public async Task<string> LoadAsync(byte sourceId)
    {
        var companySources = await companySourceRepo.GetSampleAsync(x => x.SourceId == sourceId);

        rabbitAction.Publish(QueueExchanges.Function, QueueNames.Market, QueueEntities.AssetSources, QueueActions.Get, companySources);

        return $"Load data by {string.Join(";", companySources.Select(x => Enum.Parse<Enums.Statuses>(x.SourceId.ToString())))} is starting...";
    }
    public async Task<string> LoadAsync(string companyId, byte sourceId)
    {
        companyId = companyId.Trim().ToUpperInvariant();
        var companySource = await companySourceRepo
            .FindAsync(x =>
                x.CompanyId == companyId
                && x.SourceId == sourceId);

        if (companySource?.Value is null)
            return $"source for '{companyId}' not found";

        rabbitAction.Publish(QueueExchanges.Function, QueueNames.Market, QueueEntities.AssetSource, QueueActions.Get, companySource);

        return $"Load data by {Enum.Parse<Enums.Statuses>(companySource.SourceId.ToString())} is starting...";
    }

    public async Task<string> SyncAsync()
    {
        var companies = await companyRepo.GetSampleAsync(x => ValueTuple.Create(x.Id, x.CountryId, x.Name));

        if (!companies.Any())
            return "Data for sync not found";

        var data = companies
            .Select(x => new AssetMqDto(x.Item1, (byte)Shared.Enums.AssetTypes.Stock, x.Item2, x.Item3))
            .ToArray();

        rabbitAction.Publish(QueueExchanges.Sync, new[] { QueueNames.Recommendations, QueueNames.Portfolio }, QueueEntities.Assets, QueueActions.Set, data);

        return "Task of sync is running...";
    }
}