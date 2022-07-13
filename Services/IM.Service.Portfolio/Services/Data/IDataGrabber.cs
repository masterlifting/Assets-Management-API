using System.Threading.Tasks;
using IM.Service.Portfolio.Models.Api.Mq;

namespace IM.Service.Portfolio.Services.Data;

public interface IDataGrabber
{
    Task ProcessAsync(ProviderReportDto report);
}