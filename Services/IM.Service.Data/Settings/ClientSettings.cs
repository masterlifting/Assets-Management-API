﻿using IM.Service.Common.Net.Models.Dto.Http;
using IM.Service.Data.Models.Settings;

namespace IM.Service.Data.Settings;

public class ClientSettings
{
    public HostModel Moex { get; set; } = null!;
    public HostModel TdAmeritrade { get; set; } = null!;
    public InvestingModel Investing { get; set; } = null!;
}