using System;
using IM.Service.Recommendations.Domain.Entities;

namespace IM.Service.Recommendations.Models.Api.Http
{
    public class SaleDto
    {
        public SaleDto() { }
        public SaleDto(Sale sale)
        {
            Ticker = sale.CompanyId;
            Lot = sale.Lot;
            Price = sale.Price;
            Percent = sale.Percent;
            UpdateTime = DateTime.UtcNow;
        }
        public string Ticker { get;} = null!;
        public DateTime UpdateTime { get;}
        public int Lot { get; }
        public decimal Price { get; }
        public int Percent { get; }
    }
}
