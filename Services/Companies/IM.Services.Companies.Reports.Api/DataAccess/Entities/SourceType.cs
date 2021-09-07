using CommonServices.Models.Entity;

using System.Collections.Generic;

namespace IM.Services.Companies.Reports.Api.DataAccess.Entities
{
    public class SourceType : CommonEntityType
    {
        public virtual IEnumerable<Ticker> Tickers { get; set; }
    }
}