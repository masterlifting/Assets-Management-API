using System.Linq;
using IM.Service.Broker.Data.DataAccess.Entities;
using IM.Service.Broker.Data.DataAccess.Entities.ManyToMany;
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
    private readonly Repository<Account> accountRepository;
    private readonly Repository<User> userRepository;
    private readonly Repository<BrokerUser> brokerUserRepository;

    public ReportLoader(
        ILogger<ReportLoader> logger,
        ReportGrabber grabber,
        Repository<Account> accountRepository,
        Repository<User> userRepository,
        Repository<BrokerUser> brokerUserRepository)
    {
        this.logger = logger;
        this.grabber = grabber;
        this.accountRepository = accountRepository;
        this.userRepository = userRepository;
        this.brokerUserRepository = brokerUserRepository;
    }

    public async Task DataSetAsync(ReportFileDto file)
    {
        var user = await userRepository.FindAsync(file.UserId);

        if (user is null)
        {
            logger.LogWarning(LogEvents.Processing, "'{userId}' was not found", file.UserId);
            await userRepository.CreateAsync(new User { Id = file.UserId, Name = "Andrey Pestunov" }, "Andrey Pestunov");
            return;
        }

        var broker = ReportHelper.GetBroker(file.Name);

        if (broker is null)
        {
            logger.LogWarning(LogEvents.Processing, "Broker from '{fileName}' was not recognized", file.Name);
            return;
        }

        var brokerUser = await brokerUserRepository.FindAsync(x => x.BrokerId == (byte)broker && x.UserId == user.Id);
        
        if (brokerUser is null)
        {
            logger.LogWarning(LogEvents.Processing, "Broker '{broker}' for '{user}' was not found", broker, user.Name);
            brokerUser = new BrokerUser {UserId = user.Id, BrokerId = (byte) broker};
            await brokerUserRepository.CreateAsync(brokerUser , $"UserId - '{user.Id}'. BrokerId - '{(byte)broker}'");
            return;
        }

        var accounts = await accountRepository.GetSampleAsync(x => x.BrokerUserId == brokerUser.Id);

        if (!accounts.Any())
        {
            logger.LogWarning(LogEvents.Processing, "Account for '{user}' with '{broker}' was not found", user.Name, broker);
            await accountRepository.CreateAsync(new Account { Name = "Test account", BrokerUserId = brokerUser.Id }, "Test account");
            await accountRepository.CreateAsync(new Account { Name = "472746/18-м-иис от 17.07.2018", BrokerUserId = brokerUser.Id }, "T472746/18-м-иис от 17.07.2018");
            return;
        }

        await grabber.GrabDataAsync(brokerUser.BrokerId, file, accounts);
    }
}