using IM.Service.Common.Net.Models.Entity.CompanyServices.Interfaces;

namespace IM.Service.Common.Net.Models.Dto.Http.CompanyServices;

public class ReportPostDto : ReportPutDto, ICompanyQuarterIdentity
{
    public string CompanyId { get; init; } = null!;
    public int Year { get; init; }
    public byte Quarter { get; init; }
}