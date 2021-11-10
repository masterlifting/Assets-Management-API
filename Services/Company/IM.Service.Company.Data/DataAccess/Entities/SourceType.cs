using IM.Service.Common.Net.Models.Entity;

using System.Collections.Generic;

namespace IM.Service.Company.Data.DataAccess.Entities
{
    // ReSharper disable once ClassWithVirtualMembersNeverInherited.Global
    public class SourceType : CommonEntityType
    {
        public virtual IEnumerable<CompanySourceType>? CompanySourceTypes { get; init; }
    }
}
