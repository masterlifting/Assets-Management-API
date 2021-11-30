using System;
using IM.Service.Common.Net.Models.Entity.CompanyServices.Interfaces;

namespace IM.Service.Common.Net.Models.Dto.Mq.CompanyServices;

public record CompanyDateIdentityDto : ICompanyDateIdentity
{
    public string CompanyId { get; init; } = null!;
    public DateTime Date { get; init; }
}