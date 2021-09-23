using System.ComponentModel.DataAnnotations;
using CommonServices.Models.Entity;

namespace IM.Service.Company.Prices.DataAccess.Entities
{
    // ReSharper disable once ClassWithVirtualMembersNeverInherited.Global
    public class Price : PriceIdentity
    {
        public virtual Ticker Ticker { get; set; } = null!;

        [Required, StringLength(50, MinimumLength = 3)]
        public string SourceType { get; set; } = null!;
        
        public decimal Value { get; set; }
    }
}