namespace IM.Service.Data.Domain.Entities.Interfaces;

public interface ICompanyIdentity
{
    Company Company { get; init; }
    string CompanyId { get; set; }
}