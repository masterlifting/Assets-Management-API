using IM.Service.Broker.Data.Models.Dto.Mq;

using System.Collections.Generic;
using System.Threading.Tasks;

namespace IM.Service.Broker.Data.Services.DataServices;

public abstract class DataGrabber
{
    private readonly Dictionary<Enums.Brokers, IDataGrabber> grabber;
    protected DataGrabber(Dictionary<Enums.Brokers, IDataGrabber> grabber) => this.grabber = grabber;

    public Task GrabDataAsync(ReportFileDto file, Enums.Brokers broker) => grabber.ContainsKey(broker) 
        ? grabber[broker].GrabDataAsync(file, broker) 
        : Task.CompletedTask;
}