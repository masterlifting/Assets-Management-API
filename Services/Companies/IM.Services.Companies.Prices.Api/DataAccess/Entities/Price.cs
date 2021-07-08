using System;

namespace IM.Services.Companies.Prices.Api.DataAccess.Entities
{
    public class Price
    {
        public decimal Value { get; set; }
        public DateTime Date { get; set; }

        public virtual Ticker Ticker { get; set; }
        public string TickerName { get; set; }
    }
}