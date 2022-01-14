using System.Collections.Generic;

namespace DataSetter.DataAccess.CompanyData.Entities;

public class Company : IM.Service.Common.Net.Models.Entity.Company
{
    public virtual IEnumerable<CompanySourceType>? CompanySourceTypes { get; set; }

    public virtual IEnumerable<Price>? Prices { get; init; }
    public virtual IEnumerable<Report>? Reports { get; init; }
    public virtual IEnumerable<StockSplit>? StockSplits { get; init; }
    public virtual IEnumerable<StockVolume>? StockVolumes { get; init; }
}