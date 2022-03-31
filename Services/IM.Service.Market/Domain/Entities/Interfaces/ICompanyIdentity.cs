namespace IM.Service.Market.Domain.Entities.Interfaces;

public interface ICompanyIdentity
{
    Company Company { get; init; }
    string CompanyId { get; set; }
}