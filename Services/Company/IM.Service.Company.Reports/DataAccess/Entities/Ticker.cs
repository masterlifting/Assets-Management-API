using CommonServices.Models.Dto.CompanyReports;
using CommonServices.Models.Entity;

using System.Collections.Generic;

namespace IM.Service.Company.Reports.DataAccess.Entities
{
    // ReSharper disable once ClassWithVirtualMembersNeverInherited.Global
    public class Ticker : TickerIdentity
    {
        public Ticker() { }
        public Ticker(CompaniesReportsTickerDto ticker)
        {
            Name = ticker.Name;
            SourceTypeId = ticker.SourceTypeId;
            SourceValue = ticker.SourceValue;
        }

        public virtual SourceType SourceType { get; set; } = null!;
        public byte SourceTypeId { get; set; }

        public string? SourceValue { get; set; }

        public virtual IEnumerable<Report>? Reports { get; set; }
    }
}