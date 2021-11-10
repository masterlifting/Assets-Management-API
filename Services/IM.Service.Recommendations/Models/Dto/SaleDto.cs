using IM.Service.Recommendations.DataAccess.Entities;

using System;

namespace IM.Service.Recommendations.Models.Dto
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
