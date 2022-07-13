using IM.Service.Portfolio.Domain.DataAccess;
using IM.Service.Portfolio.Domain.Entities;
using IM.Service.Portfolio.Models.Api.Mq;
using IM.Service.Portfolio.Services.Data.Reports;
using IM.Service.Shared.Helpers;

using Microsoft.Extensions.Logging;

using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;


namespace IM.Service.Portfolio.Services.Entity;

public class ReportService
{
    public ILogger<ReportService> Logger { get; }
    private readonly ReportGrabber grabber;
    private readonly Repository<User> userRepo;

    public ReportService(
        ILogger<ReportService> logger,
        ReportGrabber grabber,
        Repository<User> userRepo)
    {
        this.Logger = logger;
        this.grabber = grabber;
        this.userRepo = userRepo;
    }

    public async Task SetAsync(ProviderReportDto report)
    {
        var user = await userRepo.FindAsync(report.UserId);

        if (user is null)
        {
            Logger.LogWarning(nameof(SetAsync), report.UserId, "User not found");
                
            //todo delete
            await userRepo.CreateAsync(new User
            {
                Id = report.UserId,
                Name = "Пестунов Андрей"
            }, report.UserId);
            //todo set return
        }

        if (!TryGetProvider(report.Name, out var provider))
        {
            Logger.LogWarning(nameof(SetAsync), report.Name, "Provider not recognized");
            return;
        }

        await grabber.ProcessAsync(report, provider);
    }
    private static bool TryGetProvider(string fileName, out Enums.Providers result)
    {
        result = Enums.Providers.Default;

        foreach (var (pattern, provider) in providerMatcher)
        {
            var match = Regex.Match(fileName, pattern, RegexOptions.IgnoreCase);

            if (!match.Success)
                continue;

            result = provider;
            break;
        }

        return result != Enums.Providers.Default;
    }
    private static readonly Dictionary<string, Enums.Providers> providerMatcher = new()
    {
        { "^B_k-(.+)_ALL(.+).xls$", Enums.Providers.Bcs }
    };
}