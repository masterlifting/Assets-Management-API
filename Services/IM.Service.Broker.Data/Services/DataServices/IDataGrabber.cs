﻿using IM.Service.Broker.Data.DataAccess.Entities;
using IM.Service.Broker.Data.Models.Dto.Mq;

using System.Collections.Generic;
using System.Threading.Tasks;

namespace IM.Service.Broker.Data.Services.DataServices;

public interface IDataGrabber
{
    Task GrabDataAsync(ReportFileDto file, IEnumerable<Account> accounts);
}