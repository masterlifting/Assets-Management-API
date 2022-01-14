using IM.Service.Common.Net.Models.Dto.Mq.CompanyServices;
using IM.Service.Common.Net.RabbitServices;
using IM.Service.Company.Models.Dto;
using IM.Service.Company.Settings;

using Microsoft.Extensions.Options;

using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using IM.Service.Common.Net.RabbitServices.Configuration;

namespace IM.Service.Company.Services.MqServices;

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

        foreach (var (queue, data) in companyQueues)
            publisher.PublishTask(queue, QueueEntities.Company, QueueActions.Create, data);
    }
    public void CreateCompany(IEnumerable<CompanyPostDto> companies)
    {
        companies = companies.ToArray();

        if (!companies.Any())
            return;

        var companyAnalyzerData = JsonSerializer.Serialize(companies
            .Select(x => new CompanyDto
            {
                Id = x.Id,
                Name = x.Name
            })
            .ToArray());

        var companyDataData = JsonSerializer.Serialize(companies
            .Select(x => new CompanyDto
            {
                Id = x.Id,
                Name = x.Name,
                Sources = x.DataSources
            })
            .ToArray());

        var companyQueues = new Dictionary<QueueNames, string>
        {
            {QueueNames.CompanyData,companyDataData },
            {QueueNames.CompanyAnalyzer,companyAnalyzerData }
        };

        foreach (var (queue, data) in companyQueues)
            publisher.PublishTask(queue, QueueEntities.Companies, QueueActions.Create, data);
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
    public void UpdateCompany(IEnumerable<CompanyPostDto> companies)
    {
        companies = companies.ToArray();

        if (!companies.Any())
            return;

        var companyAnalyzerData = JsonSerializer.Serialize(companies
            .Select(x => new CompanyDto
            {
                Id = x.Id,
                Name = x.Name
            })
            .ToArray());

        var companyDataData = JsonSerializer.Serialize(companies
            .Select(x => new CompanyDto
            {
                Id = x.Id,
                Name = x.Name,
                Sources = x.DataSources
            })
            .ToArray());

        var companyQueues = new Dictionary<QueueNames, string>
        {
            {QueueNames.CompanyData,companyDataData },
            {QueueNames.CompanyAnalyzer,companyAnalyzerData }
        };

        foreach (var (queue, data) in companyQueues)
            publisher.PublishTask(queue, QueueEntities.Companies, QueueActions.Update, data);
    }
    public void DeleteCompany(string companyId) => publisher.PublishTask(new[]
        {
            QueueNames.CompanyData,
            QueueNames.CompanyAnalyzer
        }
        , QueueEntities.Company
        , QueueActions.Delete
        , companyId);

    public string Sync(IEnumerable<(string Id, string Name)> companies)
    {
        companies = companies.ToArray();
        if (!companies.Any())
            return "Data for sync not found";

        var prepareData = companies.Select(x => new CompanyDto
        {
            Id = x.Id,
            Name = x.Name
        });

        var data = JsonSerializer.Serialize(prepareData);

        foreach (var queue in new[] { QueueNames.CompanyData, QueueNames.CompanyAnalyzer })
            publisher.PublishTask(queue, QueueEntities.Companies, QueueActions.CreateUpdateDelete, data);

        return "Task of sync is running.";
    }
}