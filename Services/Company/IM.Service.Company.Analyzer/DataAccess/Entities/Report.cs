using IM.Service.Common.Net.Models.Entity.Companies.Interfaces;

using System.ComponentModel.DataAnnotations.Schema;

namespace IM.Service.Company.Analyzer.DataAccess.Entities
{
    // ReSharper disable once ClassWithVirtualMembersNeverInherited.Global
    public class Report : ICompanyQuarterIdentity
    {
        public Company Company { get; init; } = null!;
        public string CompanyId { get; init; } = null!;
        public int Year { get; init; }
        public byte Quarter { get; init; }

        [Column(TypeName = "Decimal(18,4)")]
        public decimal Result { get; set; }

        public Status Status { get; set; } = null!;
        public byte StatusId { get; set; }
    }
}
