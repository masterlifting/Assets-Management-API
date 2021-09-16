using CommonServices.Models.Dto.CompanyAnalyzer;
using CommonServices.Models.Entity;

using System.Collections.Generic;

namespace IM.Gateway.Recommendations.DataAccess.Entities
{
    public class Ticker : TickerIdentity
    {
        public Ticker() { }
        public Ticker(CompanyAnalyzerTickerDto ticker)
        {
            Name = ticker.Name;
        }

        public virtual Purchase? Purchase { get; set; }
        public virtual IEnumerable<Sale>? Sales { get; set; }
    }
}
