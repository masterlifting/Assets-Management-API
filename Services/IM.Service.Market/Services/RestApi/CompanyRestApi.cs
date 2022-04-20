using IM.Service.Common.Net.HttpServices;
using IM.Service.Common.Net.Models.Dto.Http;
using IM.Service.Common.Net.RabbitServices;
using IM.Service.Common.Net.RabbitServices.Configuration;
using IM.Service.Market.Domain.DataAccess;
using IM.Service.Market.Domain.DataAccess.Comparators;
using IM.Service.Market.Domain.Entities;
using IM.Service.Market.Models.Api.Amqp;
using IM.Service.Market.Models.Api.Http;
using IM.Service.Market.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace IM.Service.Market.Services.RestApi;

public class CompanyRestApi
{
    private readonly Repository<Company> companyRepo;
    private readonly string rabbitConnectionString;
    public CompanyRestApi(
        IOptions<ServiceSettings> options,
        Repository<Company> companyRepo)
    {
        this.companyRepo = companyRepo;
        rabbitConnectionString = options.Value.ConnectionStrings.Mq;
    }

    public async Task<ResponseModel<CompanyGetDto>> GetAsync(string companyId)
    {
        var company = await companyRepo.FindAsync(companyId.Trim().ToUpperInvariant())
                      ?? await companyRepo.FindAsync(x => x.Name.Contains(companyId)); //TODO: TO DELETE

        return company is null
            ? new() { Errors = new[] { $"'{companyId}' not found" } }
            : new()
            {
                Data = new()
                {
                    Id = company.Id,
                    Name = company.Name,
                    Country = company.Country.Name,
                    Industry = company.Industry.Name,
                    Sector = company.Industry.Sector.Name,
                    Description = company.Description,
                }
            };
    }
    public async Task<ResponseModel<PaginatedModel<CompanyGetDto>>> GetAsync(HttpPagination pagination)
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

        return new()
        {
            Data = new()
            {
                Items = companies,
                Count = count
            }
        };
    }

    public async Task<(string? error, Company? result)> CreateAsync(CompanyPostDto model)
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
    public async Task<(string? message, Company[] results)> CreateAsync(IEnumerable<CompanyPostDto> models)
    {
        var dtos = models.ToArray();

        if (!dtos.Any())
            return ("Data for creating not found", Array.Empty<Company>());

        var entities = dtos.Select(x => new Company
        {
            Id = string.Intern(x.Id.ToUpperInvariant().Trim()),
            Name = x.Name,
            IndustryId = x.IndustryId,
            CountryId = x.CountryId,
            Description = x.Description
        }).ToArray();

        return await companyRepo.CreateAsync(entities, new CompanyComparer(), string.Join("; ", entities.Select(x => x.Id)));
    }
    public async Task<(string? error, Company? result)> UpdateAsync(string companyId, CompanyPutDto model)
    {
        var entity = new Company
        {
            Id = string.Intern(companyId.ToUpperInvariant().Trim()),
            Name = model.Name,
            IndustryId = model.IndustryId,
            Description = model.Description
        };

        return await companyRepo.UpdateAsync(new[] { entity.Id }, entity, entity.Name);
    }
    public async Task<(string? message, Company[] results)> UpdateAsync(IEnumerable<CompanyPostDto> models)
    {
        var dtos = models.ToArray();

        if (!dtos.Any())
            return ("Data for updating not found", Array.Empty<Company>());

        var entities = dtos.Select(x => new Company
        {
            Id = string.Intern(x.Id.ToUpperInvariant().Trim()),
            Name = x.Name,
            IndustryId = x.IndustryId,
            Description = x.Description
        })
        .ToArray();

        return await companyRepo.UpdateAsync(entities, string.Join("; ", entities.Select(x => x.Id)));
    }
    public Task<(string? error, Company? result)> DeleteAsync(string companyId)
    {
        companyId = string.Intern(companyId.ToUpperInvariant().Trim());

        return companyRepo.DeleteByIdAsync(new[] { companyId }, companyId);
    }

    public async Task<string> SyncAsync()
    {
        var companies = await companyRepo.GetSampleAsync(x => ValueTuple.Create(x.Id, x.CountryId, x.Name));

        if (!companies.Any())
            return "Data for sync not found";

        var data = companies
            .Select(x => new CompanyDto(x.Item1, x.Item2, x.Item3))
            .ToArray();

        var publisher = new RabbitPublisher(rabbitConnectionString, QueueExchanges.Sync);

        foreach (var queue in new[] { QueueNames.Recommendation, QueueNames.Portfolio })
            publisher.PublishTask(queue, QueueEntities.Companies, QueueActions.CreateUpdateDelete, data);

        return "Task of sync is running...";
    }
}