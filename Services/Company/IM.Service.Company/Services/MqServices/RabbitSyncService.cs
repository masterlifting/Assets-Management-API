using IM.Service.Common.Net.RabbitServices;

using IM.Service.Company.Settings;

using Microsoft.Extensions.Options;

using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using IM.Service.Common.Net.Models.Dto.Mq.CompanyServices;
using IM.Service.Company.Models.Dto;

namespace IM.Service.Company.Services.MqServices
{
    public class RabbitSyncService
    {
        private readonly RabbitPublisher publisher;
        public RabbitSyncService(IOptions<ServiceSettings> options) =>
            publisher = new RabbitPublisher(options.Value.ConnectionStrings.Mq, QueueExchanges.Sync);

        public void CreateCompany(CompanyPostDto company)
        {
            var companyAnalyzer = JsonSerializer.Serialize(new CompanyDto
            {
                Id = company.Id,
                Name = company.Name
            });
            var companyData = JsonSerializer.Serialize(new CompanyDto
            {
                Id = company.Id,
                Name = company.Name,
                Sources = company.DataSources
            });

            var companyQueues = new Dictionary<QueueNames, string>
            {
                {QueueNames.CompanyData,companyData },
                {QueueNames.CompanyAnalyzer,companyAnalyzer }
            };

            foreach (var (key, value) in companyQueues)
                publisher.PublishTask(key, QueueEntities.Company, QueueActions.Create, value);
        }
        public void CreateCompany(IEnumerable<CompanyPostDto> companies)
        {
            var array = companies.ToArray();

            var companyAnalyzer = array.Select(x => JsonSerializer.Serialize(new CompanyDto
            {
                Id = x.Id,
                Name = x.Name
            }));

            var companyData = array.Select(x => JsonSerializer.Serialize(new CompanyDto
            {
                Id = x.Id,
                Name = x.Name,
                Sources = x.DataSources
            }));

            var companyQueues = new Dictionary<QueueNames, IEnumerable<string>>
            {
                {QueueNames.CompanyData,companyData },
                {QueueNames.CompanyAnalyzer,companyAnalyzer }
            };

            foreach (var (queue, models) in companyQueues)
                foreach (var model in models)
                    publisher.PublishTask(queue, QueueEntities.Company, QueueActions.Create, model);
        }
        public void UpdateCompany(CompanyPostDto company)
        {
            var companyData = JsonSerializer.Serialize(new CompanyDto
            {
                Id = company.Id,
                Name = company.Name,
                Sources = company.DataSources
            });
            var companyAnalyzer = JsonSerializer.Serialize(new CompanyDto
            {
                Id = company.Id,
                Name = company.Name
            });

            publisher.PublishTask(QueueNames.CompanyData, QueueEntities.Company, QueueActions.Update, companyData);
            publisher.PublishTask(QueueNames.CompanyAnalyzer, QueueEntities.Company, QueueActions.Update, companyAnalyzer);
        }
        public void DeleteCompany(string companyId) => publisher.PublishTask(new[]
        {
            QueueNames.CompanyData,
            QueueNames.CompanyAnalyzer
        }
        , QueueEntities.Company
        , QueueActions.Delete
        , companyId);
    }
}
