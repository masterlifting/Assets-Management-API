using System.Collections.Generic;

namespace IM.Service.Common.Net.Models.Dto.Mq.CompanyServices;

public class CompanyDto : Entity.Company
{
    public IEnumerable<EntityTypeDto>? Sources { get; init; }
}