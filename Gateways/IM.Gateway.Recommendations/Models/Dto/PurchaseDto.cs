using System;
using IM.Gateway.Recommendations.DataAccess.Entities;

namespace IM.Gateway.Recommendations.Models.Dto
{
    public class PurchaseDto
    {
        public PurchaseDto() { }
        public PurchaseDto(Purchase purchase)
        {
            Ticker = purchase.TickerName;
            Price = purchase.Price;
            Percent = purchase.Percent;
            UpdateTime = DateTime.UtcNow;
        }
        public string Ticker { get;} = null!;
        public DateTime UpdateTime { get;}
        public decimal Price { get; }
        public int Percent { get; }
    }
}