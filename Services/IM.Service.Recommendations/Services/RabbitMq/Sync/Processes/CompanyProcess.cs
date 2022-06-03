using IM.Service.Shared.RabbitMq;
using IM.Service.Recommendations.Domain.DataAccess;
using IM.Service.Recommendations.Domain.DataAccess.Comparators;
using IM.Service.Recommendations.Domain.Entities;

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IM.Service.Shared.Models.RabbitMq.Api;

namespace IM.Service.Recommendations.Services.RabbitMq.Sync.Processes;

public class CompanyProcess : IRabbitProcess
{
    private const string serviceName = "Company sinchronization";
    private readonly Repository<Company> companyRepo;
    public CompanyProcess(Repository<Company> companyRepo) => this.companyRepo = companyRepo;

    public Task ProcessAsync<T>(QueueActions action, T model) where T : class => model switch
    {
        CompanyMqDto company => action switch
        {
            QueueActions.Create => companyRepo.CreateAsync(GetCompany(company), serviceName),
            QueueActions.Update => companyRepo.UpdateAsync(new[] { company.Id }, GetCompany(company), serviceName),
            QueueActions.Delete => companyRepo.DeleteAsync(new[] { company.Id }, serviceName),
            _ => Task.CompletedTask
        },
        _ => Task.CompletedTask
    };
    public Task ProcessRangeAsync<T>(QueueActions action, IEnumerable<T> models) where T : class => models switch
    {
        CompanyMqDto[] companies => action switch
        {
            QueueActions.Set => companyRepo.CreateUpdateDeleteAsync(GetCompanies(companies), new CompanyComparer(), serviceName),
            QueueActions.Create => companyRepo.CreateRangeAsync(GetCompanies(companies), new CompanyComparer(), serviceName),
            QueueActions.Update => companyRepo.UpdateRangeAsync(GetCompanies(companies), serviceName),
            QueueActions.Delete => companyRepo.DeleteRangeAsync(GetCompanies(companies), serviceName),
            _ => Task.CompletedTask
        },
        _ => Task.CompletedTask
    };

    private static Company GetCompany(CompanyMqDto model) =>
        new()
        {
            Id = model.Id,
            Name = model.Name
        };
    private static Company[] GetCompanies(IEnumerable<CompanyMqDto> models) =>
        models
            .Select(x => new Company
            {
                Id = x.Id,
                Name = x.Name
            })
            .ToArray();
}