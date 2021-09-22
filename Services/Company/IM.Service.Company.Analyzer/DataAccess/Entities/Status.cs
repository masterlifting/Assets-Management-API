using CommonServices.Models.Entity;

using System.Collections.Generic;

namespace IM.Service.Company.Analyzer.DataAccess.Entities
{
    // ReSharper disable once ClassWithVirtualMembersNeverInherited.Global
    public class Status : CommonEntityType
    {
        public virtual IEnumerable<Report>? Reports { get; set; }
        public virtual IEnumerable<Price>? Prices { get; set; }
    }
}
