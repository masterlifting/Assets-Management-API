using System;
using IM.Services.Companies.Prices.Api.DataAccess.Entities;

namespace IM.Services.Companies.Prices.Api.Models.Dto
{
    public class PriceDto
    {
        public PriceDto() { }
        public PriceDto(Price price, string ticker)
        {
            Ticker = ticker.ToUpperInvariant();
            if (price is not null)
            {
                Date = price.Date;
                Value = price.Value;
            }
        }
        public string Ticker { get; set; }
        public DateTime Date { get; set; }
        public decimal Value { get; set; }
    }
}