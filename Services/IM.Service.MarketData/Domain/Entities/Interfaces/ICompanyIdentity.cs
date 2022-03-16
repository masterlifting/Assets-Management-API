namespace IM.Service.MarketData.Domain.Entities.Interfaces;

public interface ICompanyIdentity
{
    Company Company { get; init; }
    string CompanyId { get; set; }
}