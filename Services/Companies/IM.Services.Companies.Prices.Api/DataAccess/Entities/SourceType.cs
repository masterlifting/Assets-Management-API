using CommonServices.Models.Entity;

using System.Collections.Generic;

namespace IM.Services.Companies.Prices.Api.DataAccess.Entities
{
    public class SourceType : CommonEntityType
    {
        public virtual IEnumerable<Ticker> Tickers { get; set; }
    }
}