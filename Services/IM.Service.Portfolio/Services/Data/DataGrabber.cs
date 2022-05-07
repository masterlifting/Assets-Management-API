using System.Collections.Generic;
using System.Threading.Tasks;
using IM.Service.Portfolio.Models.Api.Mq;
using static IM.Service.Portfolio.Enums;

namespace IM.Service.Portfolio.Services.Data;

public abstract class DataGrabber
{
    private readonly Dictionary<Brokers, IDataGrabber> grabber;
    protected DataGrabber(Dictionary<Brokers, IDataGrabber> grabber) => this.grabber = grabber;

    public Task GetDataAsync(ReportFileDto file, Brokers broker) => grabber.ContainsKey(broker) 
        ? grabber[broker].GetDataAsync(file) 
        : Task.CompletedTask;
}