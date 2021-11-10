using System;
using IM.Service.Recommendations.DataAccess.Entities;

namespace IM.Service.Recommendations.Models.Dto
{
    public class PurchaseDto
    {
        public PurchaseDto() { }
        public PurchaseDto(Purchase purchase)
        {
            Ticker = purchase.CompanyId;
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