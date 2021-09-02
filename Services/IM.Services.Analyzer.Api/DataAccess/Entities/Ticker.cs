using CommonServices.Models.Entity;

using System.Collections.Generic;

namespace IM.Services.Analyzer.Api.DataAccess.Entities
{
    public class Ticker : TickerIdentity
    {
        public virtual Rating Rating { get; set; } = null!;
        public virtual Recommendation Recommendation { get; set; } = null!;

        public virtual IEnumerable<Report>? Reports { get; set; }
        public virtual IEnumerable<Price>? Prices { get; set; }
    }
}
