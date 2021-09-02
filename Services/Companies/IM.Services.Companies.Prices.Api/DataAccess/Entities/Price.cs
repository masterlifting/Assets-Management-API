using CommonServices.Models.Entity;

namespace IM.Services.Companies.Prices.Api.DataAccess.Entities
{
    public class Price : PriceIdentity
    {
        public virtual Ticker Ticker { get; set; }
        public decimal Value { get; set; }
    }
}