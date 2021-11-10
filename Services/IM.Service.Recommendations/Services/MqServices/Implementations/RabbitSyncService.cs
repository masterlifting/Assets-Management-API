using IM.Service.Common.Net.Models.Dto.Mq.Companies;
using IM.Service.Common.Net.RabbitServices;

using IM.Service.Recommendations.DataAccess.Entities;
using IM.Service.Recommendations.DataAccess.Repository;

using System.Threading.Tasks;

namespace IM.Service.Recommendations.Services.MqServices.Implementations
{
    public class RabbitSyncService : IRabbitActionService
    {
        private readonly RepositorySet<Company> companyRepository;
        public RabbitSyncService(RepositorySet<Company> companyRepository) => this.companyRepository = companyRepository;

        public async Task<bool> GetActionResultAsync(QueueEntities entity, QueueActions action, string data) => entity switch
        {
            QueueEntities.Company => await GetCompanyResultAsync(action, data),
            _ => true
        };
        private async Task<bool> GetCompanyResultAsync(QueueActions action, string data)
        {
            if (action == QueueActions.Delete)
                return (await companyRepository.DeleteAsync(data, data)).error is not null;

            if (!RabbitHelper.TrySerialize(data, out CompanyDto? dto))
                return false;

            var entity = new Company
            {
                Id = dto!.Id,
                Name = dto.Name
            };

            return await GetActionAsync(companyRepository, action, entity, entity.Name);
        }
        private static async Task<bool> GetActionAsync<T>(RepositorySet<T> repository, QueueActions action, T data, string value) where T : class => action switch
        {
            QueueActions.Create => (await repository.CreateAsync(data, value)).error is not null,
            QueueActions.Update => (await repository.CreateUpdateAsync(data, value)).error is not null,
            _ => true
        };
    }
}
