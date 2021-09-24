using System;
using System.Collections.Generic;
using System.Linq;
using IM.Gateway.Recommendations.DataAccess.Entities;

namespace IM.Gateway.Recommendations.Models.Dto
{
    public class SaleDto
    {
        public SaleDto() { }
        public SaleDto(Sale sale)
        {
            Ticker = sale.TickerName;
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
