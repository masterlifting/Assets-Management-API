using System.Collections.Generic;
using IM.Service.Common.Net.Models.Entity.Companies;

namespace IM.Service.Common.Net.Models.Dto.Mq.Companies
{
    public class CompanyDto : Company
    {
        public IEnumerable<EntityTypeDto>? Sources { get; init; }
    }
}
