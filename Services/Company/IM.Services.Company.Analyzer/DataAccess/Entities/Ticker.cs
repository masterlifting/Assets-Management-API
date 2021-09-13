using CommonServices.Models.Dto.AnalyzerService;
using CommonServices.Models.Entity;

using System.Collections.Generic;

namespace IM.Services.Company.Analyzer.DataAccess.Entities
{
    public class Ticker : TickerIdentity
    {
        public Ticker() { }
        public Ticker(AnalyzerTickerDto ticker)
        {
            Name = ticker.Name;
        }
        public virtual Rating Rating { get; set; } = null!;
        public virtual Recommendation Recommendation { get; set; } = null!;

        public virtual IEnumerable<Report>? Reports { get; set; }
        public virtual IEnumerable<Price>? Prices { get; set; }
    }
}
