using CommonServices.Models.Entity;

using System.Collections.Generic;

namespace IM.Service.Company.Reports.DataAccess.Entities
{
    // ReSharper disable once ClassWithVirtualMembersNeverInherited.Global
    public class SourceType : CommonEntityType
    {
        // ReSharper disable once UnusedMember.Global
        public virtual IEnumerable<Ticker>? Tickers { get; set; }
    }
}