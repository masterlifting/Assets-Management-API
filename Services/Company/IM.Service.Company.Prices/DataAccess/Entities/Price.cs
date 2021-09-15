using CommonServices.Models.Entity;

namespace IM.Service.Company.Prices.DataAccess.Entities
{
    // ReSharper disable once ClassWithVirtualMembersNeverInherited.Global
    public class Price : PriceIdentity
    {
        public virtual Ticker Ticker { get; set; } = null!;
        public decimal Value { get; set; }
    }
}