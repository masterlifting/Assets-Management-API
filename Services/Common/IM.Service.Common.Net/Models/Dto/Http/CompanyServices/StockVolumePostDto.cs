﻿using System;
using IM.Service.Common.Net.Models.Entity.CompanyServices.Interfaces;

namespace IM.Service.Common.Net.Models.Dto.Http.CompanyServices;

public class StockVolumePostDto : StockVolumePutDto, ICompanyDateIdentity
{
    public string CompanyId { get; init; } = null!;
    public DateTime Date { get; init; }
}