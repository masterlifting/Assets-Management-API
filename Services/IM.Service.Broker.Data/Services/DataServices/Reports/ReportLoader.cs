using IM.Service.Broker.Data.DataAccess.Entities;
using IM.Service.Broker.Data.DataAccess.Repository;
using IM.Service.Broker.Data.Models.Dto.Mq;
using IM.Service.Common.Net;

using Microsoft.Extensions.Logging;

using System.Threading.Tasks;

namespace IM.Service.Broker.Data.Services.DataServices.Reports;

public class ReportLoader
{
    private readonly ILogger<ReportLoader> logger;
    private readonly ReportGrabber grabber;
    private readonly Repository<User> userRepository;

    public ReportLoader(
        ILogger<ReportLoader> logger,
        ReportGrabber grabber,
        Repository<User> userRepository)
    {
        this.logger = logger;
        this.grabber = grabber;
        this.userRepository = userRepository;
    }

    public async Task DataSetAsync(ReportFileDto file)
    {
        var user = await userRepository.FindAsync(file.UserId); // delete!

        if (user is null)
        {
            logger.LogWarning(LogEvents.Processing, "'{userId}' was not found", file.UserId);
            await userRepository.CreateAsync(new User { Id = file.UserId, Name = "Пестунов Андрей Викторович" }, "Пестунов Андрей Викторович");
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