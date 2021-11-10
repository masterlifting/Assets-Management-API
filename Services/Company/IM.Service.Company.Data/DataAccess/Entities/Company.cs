using System.Collections.Generic;

namespace IM.Service.Company.Data.DataAccess.Entities
{
    // ReSharper disable once ClassWithVirtualMembersNeverInherited.Global
    public class Company : Common.Net.Models.Entity.Companies.Company
    {
        public virtual IEnumerable<CompanySourceType>? CompanySourceTypes { get; set; }

        public virtual IEnumerable<Price>? Prices { get; init; }
        public virtual IEnumerable<Report>? Reports { get; init; }
        public virtual IEnumerable<StockSplit>? StockSplits { get; init; }
        public virtual IEnumerable<StockVolume>? StockVolumes { get; init; }
    }
}
