using CommonServices.Models.Entity;

using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace IM.Services.Analyzer.Api.DataAccess.Entities
{
    public class Report : ReportIdentity
    {
        [Column(TypeName = "Decimal(18,4)")]
        public decimal? Result { get; set; }
        public DateTime Date { get; set; } = DateTime.UtcNow;

        public virtual Status Status { get; set; } = null!;
        public int StatusId { get; set; }

        public virtual Ticker Ticker { get; set; } = null!;
        public string TickerName { get; set; } = null!;
    }
}
