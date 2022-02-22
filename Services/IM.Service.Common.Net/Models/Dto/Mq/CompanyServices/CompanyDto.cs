namespace IM.Service.Common.Net.Models.Dto.Mq.CompanyServices;

public class CompanyDto : Entity.Company
{
    public string Country { get; init; } = null!;
}