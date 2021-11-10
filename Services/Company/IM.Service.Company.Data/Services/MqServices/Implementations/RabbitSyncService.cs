using System.Linq;
using IM.Service.Common.Net.Models.Dto.Mq.Companies;
using IM.Service.Common.Net.RabbitServices;
using System.Threading.Tasks;
using IM.Service.Company.Data.DataAccess.Entities;
using IM.Service.Company.Data.DataAccess.Repository;

namespace IM.Service.Company.Data.Services.MqServices.Implementations
{
    public class RabbitSyncService : IRabbitActionService
    {
        private readonly string rabbitConnectionString;
        private readonly RepositorySet<DataAccess.Entities.Company> companyRepository;

        public RabbitSyncService(string rabbitConnectionString, RepositorySet<DataAccess.Entities.Company> companyRepository)
        {
            this.rabbitConnectionString = rabbitConnectionString;
            this.companyRepository = companyRepository;
        }

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

            var entity = new DataAccess.Entities.Company
            {
                Id = dto!.Id,
                Name = dto.Name,
                CompanySourceTypes = dto.Sources?.Select(x => new CompanySourceType
                {
                    CompanyId = dto.Id,
                    SourceTypeId = x.Id,
                    Value = x.Value
                })
            };

            if (!await GetActionAsync(companyRepository, action, entity, entity.Name)) 
                return false;
            
            var publisher = new RabbitPublisher(rabbitConnectionString, QueueExchanges.Function);
                
            publisher.PublishTask(QueueNames.CompanyData, QueueEntities.Report, QueueActions.Call, entity.Id);
            publisher.PublishTask(QueueNames.CompanyData, QueueEntities.Price, QueueActions.Call, entity.Id);
                
            return true;

        }
        private static async Task<bool> GetActionAsync<T>(RepositorySet<T> repository, QueueActions action, T data, string value) where T : class => action switch
        {
            QueueActions.Create => (await repository.CreateAsync(data, value)).error is not null,
            QueueActions.Update => (await repository.CreateUpdateAsync(data, value)).error is not null,
            _ => true
        };
    }
}
