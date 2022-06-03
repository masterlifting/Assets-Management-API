using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using IM.Service.Shared.Helpers;
using IM.Service.Shared.RabbitMq;
using IM.Service.Portfolio.Domain.DataAccess;
using IM.Service.Portfolio.Domain.Entities;
using IM.Service.Portfolio.Models.Api.Mq;
using IM.Service.Portfolio.Services.Data.Reports;

using Microsoft.Extensions.Logging;

using static IM.Service.Portfolio.Enums;

namespace IM.Service.Portfolio.Services.RabbitMq.Function.Processes;

public sealed class ReportProcess : IRabbitProcess
{
    private readonly ILogger<ReportProcess> logger;
    private readonly ReportGrabber grabber;
    private readonly Repository<User> userRepo;

    public ReportProcess(
        ILogger<ReportProcess> logger,
        ReportGrabber grabber,
        Repository<User> userRepo)
    {
        this.logger = logger;
        this.grabber = grabber;
        this.userRepo = userRepo;
    }

    public Task ProcessAsync<T>(QueueActions action, T model) where T : class => action switch
    {
        QueueActions.Get => model switch
        {
            ReportFileDto file => SetAsync(file),
            _ => Task.CompletedTask
        },
        _ => Task.CompletedTask
    };
    public Task ProcessRangeAsync<T>(QueueActions action, IEnumerable<T> models) where T : class
    {
        throw new NotImplementedException($"{nameof(ReportProcess)}.ProcessAsync for objects '{typeof(T).Name}' is not implemented.");
    }

    public async Task SetAsync(ReportFileDto file)
    {
        var user = await userRepo.FindAsync(file.UserId); // delete!

        if (user is null)
        {
            logger.LogWarning(nameof(SetAsync), file.UserId, "User not found");
            await userRepo.CreateAsync(new User
            {
                Id = file.UserId,
                Name = "Пестунов Андрей Викторович"
            }, "Пестунов Андрей Викторович");
            return;
        }

        if (!TryGetBroker(file.Name, out var broker))
        {
            logger.LogWarning(nameof(SetAsync), file.Name, "Broker not recognized");
            return;
        }

        await grabber.GetDataAsync(file, broker);
    }
    private static bool TryGetBroker(string fileName, out Brokers result)
    {
        result = Brokers.Default;

        foreach (var (pattern, broker) in brokerMatcher)
        {
            var match = Regex.Match(fileName, pattern, RegexOptions.IgnoreCase);

            if (!match.Success)
                continue;

            result = broker;
            break;
        }

        return result != Brokers.Default;
    }
    private static readonly Dictionary<string, Brokers> brokerMatcher = new()
    {
        { "^B_k-(.+)_ALL(.+).xls$", Brokers.Bcs }
    };
}