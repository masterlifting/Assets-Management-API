using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace IM.Services.Companies.Reports.Api.DataAccess.Entities
{
    public class Ticker
    {
        [Key, StringLength(10, MinimumLength = 1)]
        public string Name { get; set; }

        public virtual IEnumerable<ReportSource> ReportSources { get; set; }
    }
}