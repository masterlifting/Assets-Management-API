using System;
using System.Collections.Generic;

namespace DataSetter.DataAccess.Entities
{
    public partial class SellRecommendation
    {
        public long Id { get; set; }
        public DateTime DateUpdate { get; set; }
        public string UserId { get; set; } = null!;
        public int LotMin { get; set; }
        public decimal PriceMin { get; set; }
        public int LotMid { get; set; }
        public decimal PriceMid { get; set; }
        public int LotMax { get; set; }
        public decimal PriceMax { get; set; }
        public long CompanyId { get; set; }

        public virtual Company Company { get; set; } = null!;
    }
}
