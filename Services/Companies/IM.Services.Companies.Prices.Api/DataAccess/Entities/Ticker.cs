using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace IM.Services.Companies.Prices.Api.DataAccess.Entities
{
    public class Ticker
    {
        [Key, StringLength(10, MinimumLength = 1)]
        public string Name { get; set; }

        public virtual PriceSourceType SourceType { get; set; }
        public int PriceSourceTypeId { get; set; }

        public virtual IEnumerable<Price> Prices { get; set; }
    }
}