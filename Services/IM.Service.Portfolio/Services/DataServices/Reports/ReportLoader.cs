using IM.Service.Common.Net;
using IM.Service.Portfolio.DataAccess.Entities;
using IM.Service.Portfolio.DataAccess.Repositories;
using IM.Service.Portfolio.Models.Dto.Mq;

using Microsoft.Extensions.Logging;

using System.Threading.Tasks;

namespace IM.Service.Portfolio.Services.DataServices.Reports;

public class ReportLoader
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

    public async Task DataSetAsync(ReportFileDto file)
    {
        var user = await userRepo.FindAsync(file.UserId); // delete!

        if (user is null)
        {
            logger.LogWarning(LogEvents.Processing, "'{userId}' was not found", file.UserId);
            await userRepo.CreateAsync(new User { Id = file.UserId, Name = "Пестунов Андрей Викторович" }, "Пестунов Андрей Викторович");
            return;
        }

        if (!ReportHelper.TryGetBroker(file.Name, out var broker))
        {
            logger.LogWarning(LogEvents.Processing, "Broker from '{fileName}' was not recognized", file.Name);
            return;
        }

        await grabber.GrabDataAsync(file, broker);
    }
}