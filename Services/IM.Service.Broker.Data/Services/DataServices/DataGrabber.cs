using IM.Service.Broker.Data.DataAccess.Entities;
using IM.Service.Broker.Data.Models.Dto.Mq;

using System.Collections.Generic;
using System.Threading.Tasks;

namespace IM.Service.Broker.Data.Services.DataServices;

public abstract class DataGrabber
{
    private readonly Dictionary<byte, IDataGrabber> grabber;
    protected DataGrabber(Dictionary<byte, IDataGrabber> grabber) => this.grabber = grabber;

    public bool IsSource(byte brokerId) => grabber.ContainsKey(brokerId);

    public Task GrabDataAsync(byte brokerId, ReportFileDto file, IEnumerable<Account> accounts) => grabber.ContainsKey(brokerId) 
        ? grabber[brokerId].GrabDataAsync(file, accounts) 
        : Task.CompletedTask;
}