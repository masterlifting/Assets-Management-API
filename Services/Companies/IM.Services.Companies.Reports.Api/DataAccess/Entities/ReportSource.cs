using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace IM.Services.Companies.Reports.Api.DataAccess.Entities
{
    public class ReportSource
    {
        [Key]
        public int Id { get; set; }

        [Required, StringLength(300)]
        public string Value { get; set; }

        public bool IsActive { get; set; }

        public virtual ReportSourceType ReportSourceType { get; set; }
        public byte ReportSourceTypeId { get; set; }

        public virtual Ticker Ticker { get; set; }
        public string TickerName { get; set; }

        public virtual IEnumerable<Report> Reports { get; set; }
    }
}