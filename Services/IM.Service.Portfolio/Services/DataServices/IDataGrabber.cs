using System.Threading.Tasks;
using IM.Service.Portfolio.Models.Dto.Mq;

namespace IM.Service.Portfolio.Services.DataServices;

public interface IDataGrabber
{
    Task GrabDataAsync(ReportFileDto file);
}