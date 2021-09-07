using CommonServices.Models.Dto.CompaniesReportsService;
using CommonServices.Models.Entity;

using System.Collections.Generic;

namespace IM.Services.Companies.Reports.Api.DataAccess.Entities
{
    public class Ticker : TickerIdentity
    {
        public Ticker() { }
        public Ticker(CompaniesReportsTickerDto ticker)
        {
            Name = ticker.Name;
            SourceTypeId = ticker.SourceTypeId;
            SourceValue = ticker.SourceValue;
        }
        public virtual SourceType SourceType { get; set; }
        public byte SourceTypeId { get; set; }

        public string SourceValue { get; set; }

        public virtual IEnumerable<Report> Reports { get; set; }
    }
}