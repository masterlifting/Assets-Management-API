using System;

namespace IM.Gateways.Web.Companies.Api.Models.Dto
{
    public class PriceDto
    {
        public string Ticker { get; set; } = null!;
        public DateTime Date { get; set; }
        public decimal Value { get; set; }
    }
}
