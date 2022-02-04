﻿using System;
using IM.Service.Common.Net.Models.Entity.CompanyServices;
using IM.Service.Common.Net.Models.Entity.CompanyServices.Interfaces;

namespace IM.Service.Common.Net.Models.Dto.Http.CompanyServices;

public class PricePutDto : PriceBody, ICompanyDateIdentity
{
    public string CompanyId { get; init; } = null!;
    public DateOnly Date { get; set; }
}