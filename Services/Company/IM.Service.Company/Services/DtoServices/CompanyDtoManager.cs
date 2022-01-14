using System;
using IM.Service.Common.Net.HttpServices;
using IM.Service.Common.Net.Models.Dto.Http;
using IM.Service.Company.DataAccess.Entities;
using IM.Service.Company.DataAccess.Repository;
using IM.Service.Company.Models.Dto;
using IM.Service.Company.Services.MqServices;

using Microsoft.EntityFrameworkCore;

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IM.Service.Common.Net.RepositoryService.Comparators;

namespace IM.Service.Company.Services.DtoServices;

public class CompanyDtoManager
{
    private readonly Repository<DataAccess.Entities.Company> companyRepository;
    private readonly Repository<Industry> industryRepository;
    private readonly Repository<Sector> sectorRepository;
    private readonly RabbitSyncService rabbitService;

    public CompanyDtoManager(
        Repository<DataAccess.Entities.Company> companyRepository,
        Repository<Industry> industryRepository,
        Repository<Sector> sectorRepository,
        RabbitSyncService rabbitService)
    {
        this.companyRepository = companyRepository;
        this.industryRepository = industryRepository;
        this.sectorRepository = sectorRepository;
        this.rabbitService = rabbitService;
    }

    public async Task<ResponseModel<PaginatedModel<CompanyGetDto>>> GetAsync(HttpPagination pagination)
    {
        var count = await companyRepository.GetCountAsync();
        var paginatedResult = companyRepository.GetPaginationQuery(pagination, x => x.Name);

        var companies = await paginatedResult.Select(x => new CompanyGetDto
        {
            Ticker = x.Id,
            Name = x.Name,
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
    public async Task<ResponseModel<CompanyGetDto>> GetAsync(string companyId)
    {
        var company = await companyRepository.FindAsync(companyId.Trim());

        if (company is null)
            return new() { Errors = new[] { "company not found" } };

        var industry = await industryRepository.FindAsync(company.IndustryId);

        if (industry is null)
            return new() { Errors = new[] { "industry not found" } };

        var sector = await sectorRepository.FindAsync(industry.SectorId);

        return new()
        {
            Data = new()
            {
                Ticker = company.Id,
                Name = company.Name,
                Description = company.Description,
                Industry = industry.Name,
                Sector = sector!.Name
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

        var (error, createdEntity) = await companyRepository.CreateAsync(entity, entity.Name);

        if (error is not null)
            return new ResponseModel<string> { Errors = new[] { error } };

        rabbitService.CreateCompany(new CompanyPostDto
        {
            Id = createdEntity!.Id,
            Name = createdEntity.Name,
            DataSources = model.DataSources
        });

        return new() { Data = $"'{createdEntity.Name}' was created" };
    }
    public async Task<ResponseModel<string>> CreateAsync(IEnumerable<CompanyPostDto> models)
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
        }).ToArray();

        var (error, createdEntities) = await companyRepository.CreateAsync(entities, new CompanyComparer<DataAccess.Entities.Company>(), nameof(CreateAsync));

        if (error is not null)
            return new() { Errors = new[] { error } };

        rabbitService.CreateCompany(createdEntities.Join(dtos, x => x.Id, y => string.Intern(y.Id.Trim().ToUpperInvariant()), (x, y) => new CompanyPostDto
        {
            Id = x.Id,
            Name = x.Name,
            DataSources = y.DataSources,
        }));

        return new() { Data = $"Companies count: {createdEntities.Length} was successed" };
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

        var (error, updatedEntity) = await companyRepository.UpdateAsync(new[] { entity.Id }, entity, entity.Name);

        if (error is not null)
            return new ResponseModel<string> { Errors = new[] { error } };

        rabbitService.UpdateCompany(new CompanyPostDto
        {
            Id = updatedEntity!.Id,
            Name = updatedEntity.Name,
            DataSources = model.DataSources,
        });

        return new ResponseModel<string> { Data = $"'{updatedEntity.Name}' was updated" };
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
        }).ToArray();

        var (error, updatedEntities) = await companyRepository.UpdateAsync(entities, nameof(UpdateAsync));

        if (error is not null)
            return new() { Errors = new[] { error } };

        rabbitService.UpdateCompany(updatedEntities.Join(dtos, x => x.Id, y => string.Intern(y.Id.Trim().ToUpperInvariant()), (x, y) => new CompanyPostDto
        {
            Id = x.Id,
            Name = x.Name,
            DataSources = y.DataSources,
        }));

        return new() { Data = $"Companies count: {updatedEntities.Length} was updated" };
    }
    public async Task<ResponseModel<string>> DeleteAsync(string companyId)
    {
        companyId = string.Intern(companyId.ToUpperInvariant().Trim());

        var (error, company) = await companyRepository.DeleteByIdAsync(new[] { companyId }, companyId);

        if (error is not null)
            return new ResponseModel<string> { Errors = new[] { error } };

        rabbitService.DeleteCompany(companyId);

        return new ResponseModel<string> { Data = $"'{company!.Name}' was deleted" };
    }

    public async Task<string> SyncAsync()
    {
        var data = await companyRepository.GetSampleAsync(x => ValueTuple.Create(x.Id, x.Name));
        return rabbitService.Sync(data);
    }
}