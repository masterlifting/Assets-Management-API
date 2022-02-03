using IM.Service.Common.Net.Models.Entity.CompanyServices;
using IM.Service.Common.Net.Models.Entity.CompanyServices.Interfaces;

using System;

namespace DataSetter.DataAccess.CompanyData.Entities;

public class StockVolume : StockVolumeBody, ICompanyDateIdentity
{
    public virtual Company Company { get; init; } = null!;
    public string CompanyId { get; init; } = null!;
    public DateOnly Date { get; set; }
}