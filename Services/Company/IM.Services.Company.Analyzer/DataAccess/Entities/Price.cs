using CommonServices.Models.Entity;

using System.ComponentModel.DataAnnotations.Schema;

namespace IM.Services.Company.Analyzer.DataAccess.Entities
{
    public class Price : PriceIdentity
    {
        public virtual Ticker Ticker { get; set; } = null!;
        public string SourceType { get; set; } = null!;


        [Column(TypeName = "Decimal(18,4)")]
        public decimal Result { get; set; }

        public virtual Status Status { get; set; } = null!;
        public byte StatusId { get; set; }
    }
}
