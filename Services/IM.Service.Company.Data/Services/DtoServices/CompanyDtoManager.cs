using IM.Service.Common.Net.HttpServices;
using IM.Service.Common.Net.Models.Dto;
using IM.Service.Common.Net.Models.Dto.Http;
using IM.Service.Common.Net.Models.Dto.Mq.CompanyServices;
using IM.Service.Common.Net.RabbitServices;
using IM.Service.Common.Net.RabbitServices.Configuration;
using IM.Service.Common.Net.RepositoryService.Comparators;
using IM.Service.Company.Data.DataAccess.Comparators;
using IM.Service.Company.Data.DataAccess.Entities.ManyToMany;
using IM.Service.Company.Data.DataAccess.Repository;
using IM.Service.Company.Data.Models.Dto;
using IM.Service.Company.Data.Settings;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace IM.Service.Company.Data.Services.DtoServices;

public class CompanyDtoManager
{
    private readonly Repository<DataAccess.Entities.Company> repository;
    private readonly Repository<CompanySource> sourceRepository;
    private readonly string rabbitConnectionString;
    public CompanyDtoManager(
        IOptions<ServiceSettings> options,
        Repository<DataAccess.Entities.Company> repository,
        Repository<CompanySource> sourceRepository)
    {
        this.repository = repository;
        this.sourceRepository = sourceRepository;
        rabbitConnectionString = options.Value.ConnectionStrings.Mq;
    }

    public async Task<ResponseModel<CompanyGetDto>> GetAsync(string companyId)
    {
        var company = await repository.FindAsync(companyId.Trim().ToUpperInvariant()) 
                      ?? await repository.FindAsync(x => x.Name.Contains(companyId)); //TO DELETE

        return company is null
            ? new() { Errors = new[] { $"'{companyId}' not found" } }
            : new()
            {
                Data = new()
                {
                    Ticker = company.Id,
                    Name = company.Name,
                    Description = company.Description,
                    Industry = company.Industry.Name,
                    Sector = company.Industry.Sector.Name,
                    Sources = company.CompanySources?
                            .Select(x => new EntityTypeGetDto(x.Source.Name, x.Value))
                            .ToArray()
                }
            };
    }
    public async Task<ResponseModel<PaginatedModel<CompanyGetDto>>> GetAsync(HttpPagination pagination)
    {
        var count = await repository.GetCountAsync();
        var paginatedResult = repository.GetPaginationQuery(pagination, x => x.Name);

        var companies = await paginatedResult.Select(x => new CompanyGetDto
        {
            Ticker = x.Id,
            Name = x.Name,
            Sector = x.Industry.Sector.Name,
            Industry = x.Industry.Name,
            Description = x.Description
        })
        .ToArrayAsync();

        if (companies.Any())
        {

            var sourcesQuery = sourceRepository.GetQuery().Join(paginatedResult, x => x.CompanyId, y => y.Id, (x, y) => x);
            var sources = await sourceRepository.GetSampleAsync(sourcesQuery, x => new { x.CompanyId, x.Source.Name, x.Value });
            var sourceDictionary = sources.GroupBy(x => x.CompanyId).ToDictionary(x => x.Key);

            foreach (var company in companies)
                company.Sources = sourceDictionary.ContainsKey(company.Ticker)
                    ? sourceDictionary[company.Ticker].Select(x => new EntityTypeGetDto(x.Name, x.Value)).ToArray()
                    : Array.Empty<EntityTypeGetDto>();
        }


        return new()
        {
            Data = new()
            {
                Items = companies,
                Count = count
            }
        };
    }

    public async Task<ResponseModel<string>> CreateAsync(CompanyPostDto model)
    {
        var entity = new DataAccess.Entities.Company
        {
            Id = string.Intern(model.Id.Trim().ToUpperInvariant()),
            Name = model.Name,
            IndustryId = model.IndustryId,
            Description = model.Description
        };

        var (error, _) = await repository.CreateAsync(entity, entity.Name);

        if (error is not null)
            return new() { Errors = new[] { error } };

        string? sourceError = null;

        if (model.Sources != null)
        {
            var sources = model.Sources
            .Select(x => new CompanySource
            {
                CompanyId = entity.Id,
                SourceId = x.Id,
                Value = x.Value
            })
            .ToArray();

            (sourceError, _) = await sourceRepository.CreateAsync(sources, new CompanySourceComparer(), entity.Name);
        }

        return new()
        {
            Errors = sourceError is not null ? new[] { sourceError } : Array.Empty<string>(),
            Data = $"'{entity.Name}' was created"
        };
    }
    public async Task<ResponseModel<string>> CreateAsync(IEnumerable<CompanyPostDto> models)
    {
        var dtos = models.ToArray();

        if (!dtos.Any())
            return new() { Errors = new[] { "Company data for creating not found" } };

        var entities = dtos.Select(x => new DataAccess.Entities.Company
        {
            Id = string.Intern(x.Id.ToUpperInvariant().Trim()),
            Name = x.Name,
            IndustryId = x.IndustryId,
            Description = x.Description
        }).ToArray();

        var (error, createdEntities) = await repository.CreateAsync(entities, new CompanyComparer<DataAccess.Entities.Company>(), $"Source count: {entities.Length}");

        if (error is not null)
            return new() { Errors = new[] { error } };

        var sources = dtos
            .Where(x => x.Sources != null)
            .Join(createdEntities, x => x.Id, y => y.Id, (x, y) => new { x.Sources, CompanyId = y.Id })
            .SelectMany(x => x.Sources!
                .Where(y => !string.IsNullOrWhiteSpace(y.Value))
                .Select(y => new CompanySource
                {
                    CompanyId = x.CompanyId,
                    SourceId = y.Id,
                    Value = y.Value
                }))
            .ToArray();

        var (sourceError, _) = await sourceRepository.CreateAsync(sources, new CompanySourceComparer(), $"Source count: {sources.Length}");

        return new()
        {
            Errors = sourceError is not null ? new[] { sourceError } : Array.Empty<string>(),
            Data = $"Companies count: {createdEntities.Length} was successed"
        };
    }
    public async Task<ResponseModel<string>> UpdateAsync(string companyId, CompanyPutDto model)
    {
        var entity = new DataAccess.Entities.Company
        {
            Id = string.Intern(companyId.ToUpperInvariant().Trim()),
            Name = model.Name,
            IndustryId = model.IndustryId,
            Description = model.Description
        };

        var (error, _) = await repository.UpdateAsync(new[] { entity.Id }, entity, entity.Name);

        if (error is not null)
            return new() { Errors = new[] { error } };

        string? sourceError = null;

        if (model.Sources != null)
        {
            var sources = model.Sources
                .Select(x => new CompanySource
                {
                    CompanyId = entity.Id,
                    SourceId = x.Id,
                    Value = x.Value
                })
                .ToArray();

            (sourceError, _) = await sourceRepository.CreateUpdateDeleteAsync(sources, new CompanySourceComparer(), entity.Name);
        }

        return new()
        {
            Errors = sourceError is not null ? new[] { sourceError } : Array.Empty<string>(),
            Data = $"'{entity.Name}' was updated"
        };
    }
    public async Task<ResponseModel<string>> UpdateAsync(IEnumerable<CompanyPostDto> models)
    {
        var dtos = models.ToArray();

        if (!dtos.Any())
            return new() { Errors = new[] { "company data for creating not found" } };

        var entities = dtos.Select(x => new DataAccess.Entities.Company
        {
            Id = string.Intern(x.Id.ToUpperInvariant().Trim()),
            Name = x.Name,
            IndustryId = x.IndustryId,
            Description = x.Description
        })
        .ToArray();

        var (error, updatedEntities) = await repository.UpdateAsync(entities, $"Source count: {entities.Length}");

        if (error is not null)
            return new() { Errors = new[] { error } };

        var sources = dtos
            .Where(x => x.Sources != null)
            .Join(updatedEntities, x => x.Id, y => y.Id, (x, y) => new { x.Sources, CompanyId = y.Id })
            .SelectMany(x => x.Sources!
                .Where(y => !string.IsNullOrWhiteSpace(y.Value))
                .Select(y => new CompanySource
                {
                    CompanyId = x.CompanyId,
                    SourceId = y.Id,
                    Value = y.Value
                }))
            .ToArray();

        var (sourceError, _) = await sourceRepository.CreateUpdateDeleteAsync(sources, new CompanySourceComparer(), $"Source count: {sources.Length}");

        return new()
        {
            Errors = sourceError is not null ? new[] { sourceError } : Array.Empty<string>(),
            Data = $"Companies count: {updatedEntities.Length} was updated"
        };
    }
    public async Task<ResponseModel<string>> DeleteAsync(string companyId)
    {
        companyId = string.Intern(companyId.ToUpperInvariant().Trim());

        var (error, deletedEntity) = await repository.DeleteByIdAsync(new[] { companyId }, companyId);

        return error is not null
            ? new() { Errors = new[] { error } }
            : new() { Data = $"'{deletedEntity!.Name}' was deleted" };
    }

    public async Task<string> SyncAsync()
    {
        var companies = await repository.GetSampleAsync(x => ValueTuple.Create(x.Id, x.Name));

        if (!companies.Any())
            return "Data for sync not found";

        var prepareData = companies.Select(x => new CompanyDto
        {
            Id = x.Item1,
            Name = x.Item2
        });

        var data = JsonSerializer.Serialize(prepareData);

        var publisher = new RabbitPublisher(rabbitConnectionString, QueueExchanges.Sync);

        foreach (var queue in new[] { QueueNames.CompanyAnalyzer, QueueNames.Recommendation, QueueNames.BrokerData, QueueNames.BrokerSummary })
            publisher.PublishTask(queue, QueueEntities.Companies, QueueActions.CreateUpdateDelete, data);

        return "Task of sync is running...";
    }
}