using IM.Service.Common.Net.RabbitServices;
using IM.Service.Common.Net.RabbitServices.Configuration;
using IM.Service.Common.Net.RepositoryService;
using IM.Service.Recommendations.DataAccess;
using IM.Service.Recommendations.DataAccess.Entities;
using IM.Service.Recommendations.DataAccess.Repository;

using System.Threading.Tasks;
using IM.Service.Recommendations.Models.Dto;

namespace IM.Service.Recommendations.Services.MqServices.Implementations;

public class RabbitSyncService : IRabbitActionService
{
    private readonly Repository<Company> companyRepository;
    public RabbitSyncService(Repository<Company> companyRepository) => this.companyRepository = companyRepository;

    public async Task<bool> GetActionResultAsync(QueueEntities entity, QueueActions action, string data) => entity switch
    {
        QueueEntities.Company => await GetCompanyResultAsync(action, data),
        _ => true
    };
    private async Task<bool> GetCompanyResultAsync(QueueActions action, string data)
    {
        if (action == QueueActions.Delete)
            return (await companyRepository.DeleteByIdAsync(new[] { data }, $"{nameof(GetCompanyResultAsync)}.{data}")).error is not null;

        if (!RabbitHelper.TrySerialize(data, out CompanyDto? dto))
            return false;

        var company = new Company
        {
            Id = dto!.Id,
            Name = dto.Name
        };

        return await GetActionAsync(companyRepository, action, new[] { company.Id }, company, $"{nameof(GetCompanyResultAsync)}.{company.Name}");
    }
    private static async Task<bool> GetActionAsync<T>(Repository<T, DatabaseContext> repository, QueueActions action, object[] id, T entity, string info) where T : class => action switch
    {
        QueueActions.Create => (await repository.CreateAsync(entity, info)).error is null,
        QueueActions.Update => (await repository.CreateUpdateAsync(id, entity, info)).error is null,
        _ => true
    };
}