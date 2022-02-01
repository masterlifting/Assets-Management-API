using IM.Service.Broker.Data.Models.Dto.Mq;

using System.Threading.Tasks;

namespace IM.Service.Broker.Data.Services.DataServices;

public interface IDataGrabber
{
    Task GrabDataAsync(ReportFileDto file, Enums.Brokers broker);
}