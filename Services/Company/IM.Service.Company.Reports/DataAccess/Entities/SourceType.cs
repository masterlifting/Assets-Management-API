using CommonServices.Models.Entity;

using System.Collections.Generic;

namespace IM.Service.Company.Reports.DataAccess.Entities
{
    public class SourceType : CommonEntityType
    {
        public virtual IEnumerable<Ticker> Tickers { get; set; }
    }
}