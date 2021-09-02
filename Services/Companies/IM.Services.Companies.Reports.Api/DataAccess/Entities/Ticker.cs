using CommonServices.Models.Entity;

using System.Collections.Generic;

namespace IM.Services.Companies.Reports.Api.DataAccess.Entities
{
    public class Ticker : TickerIdentity
    {
        public virtual IEnumerable<ReportSource> ReportSources { get; set; }
    }
}