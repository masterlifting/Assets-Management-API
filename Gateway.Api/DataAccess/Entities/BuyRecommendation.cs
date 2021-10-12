using System;
using System.Collections.Generic;

#nullable disable

namespace Gateway.Api.DataAccess.Entities
{
    public partial class BuyRecommendation
    {
        public long Id { get; set; }
        public DateTime DateUpdate { get; set; }
        public decimal Price { get; set; }
        public long CompanyId { get; set; }

        public virtual Company Company { get; set; }
    }
}
