using IM.Service.Common.Net.Models.Entity.Companies.Interfaces;

namespace IM.Service.Common.Net.Models.Dto.Http.Companies
{
    public class ReportPostDto : ReportPutDto, ICompanyQuarterIdentity
    {
        public string CompanyId { get; init; } = null!;
        public int Year { get; init; }
        public byte Quarter { get; init; }
    }
}
