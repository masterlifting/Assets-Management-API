using IM.Service.Common.Net.Models.Entity.CompanyServices;
using IM.Service.Common.Net.Models.Entity.CompanyServices.Interfaces;

namespace IM.Service.Market.DataAccess.Entities;

public class Report : ReportBody, ICompanyQuarterIdentity
{
    public virtual Company Company { get; init; } = null!;
    public string CompanyId { get; init; } = null!;

    public int Year { get; set; }
    public byte Quarter { get; set; }
}