﻿using IM.Service.Common.Net.Models.Entity.CompanyServices;
using IM.Service.Common.Net.Models.Entity.CompanyServices.Interfaces;

namespace IM.Service.Company.Data.DataAccess.Entities;

public class Report : ReportBody, ICompanyQuarterIdentity
{
    public virtual Company Company { get; init; } = null!;
    public string CompanyId { get; init; } = null!;

    public int Year { get; init; }
    public byte Quarter { get; init; }
}