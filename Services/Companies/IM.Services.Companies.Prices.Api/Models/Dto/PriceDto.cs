using System;

using IM.Services.Companies.Prices.Api.DataAccess.Entities;

using static IM.Services.Companies.Prices.Api.DataAccess.DataEnums;

namespace IM.Services.Companies.Prices.Api.Models.Dto
{
    public class PriceDto
    {
        public PriceDto() { }
        public PriceDto(Price price, byte sourceTypeId, string ticker)
        {
            Ticker = ticker.ToUpperInvariant();
            if (price is not null)
            {
                SourceType = Enum.GetName(Enum.Parse<PriceSourceTypes>(sourceTypeId.ToString()));
                Date = price.Date;
                Value = price.Value;
            }
        }
        public string Ticker { get; set; }
        public string SourceType { get; }
        public DateTime Date { get; set; }
        public decimal Value { get; set; }
    }
}