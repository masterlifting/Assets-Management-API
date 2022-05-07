using IM.Service.Common.Net.Helpers;
using IM.Service.Portfolio.Domain.DataAccess;
using IM.Service.Portfolio.Domain.Entities;
using IM.Service.Portfolio.Models.Api.Mq;

using Microsoft.Extensions.Logging;

using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static IM.Service.Portfolio.Enums;

namespace IM.Service.Portfolio.Services.Data.Reports;

public sealed class ReportLoader
{
    private readonly ILogger<ReportLoader> logger;
    private readonly ReportGrabber grabber;
    private readonly Repository<User> userRepo;

    public ReportLoader(
        ILogger<ReportLoader> logger,
        ReportGrabber grabber,
        Repository<User> userRepo)
    {
        this.logger = logger;
        this.grabber = grabber;
        this.userRepo = userRepo;
    }

    public async Task LoadAsync(string data)
    {
        if (!JsonHelper.TryDeserialize(data, out ReportFileDto? file))
            throw new SerializationException(nameof(ReportFileDto));

        var user = await userRepo.FindAsync(file!.UserId); // delete!

        if (user is null)
        {
            logger.LogWarning(nameof(LoadAsync), file.UserId, "User not found");
            await userRepo.CreateAsync(new User
            {
                Id = file.UserId,
                Name = "Пестунов Андрей Викторович"
            }, "Пестунов Андрей Викторович");
            return;
        }

        if (!TryGetBroker(file.Name, out var broker))
        {
            logger.LogWarning(nameof(LoadAsync), file.Name, "Broker not recognized");
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