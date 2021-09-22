using CommonServices.Models.Dto.CompanyAnalyzer;
using CommonServices.Models.Entity;

using System.Collections.Generic;

namespace IM.Service.Company.Analyzer.DataAccess.Entities
{
    // ReSharper disable once ClassWithVirtualMembersNeverInherited.Global
    public class Ticker : TickerIdentity
    {
        public Ticker() { }
        public Ticker(TickerPostDto ticker)
        {
            Name = ticker.Name;
        }
        public virtual Rating Rating { get; set; } = null!;

        public virtual IEnumerable<Report>? Reports { get; set; }
        public virtual IEnumerable<Price>? Prices { get; set; }
    }
}
