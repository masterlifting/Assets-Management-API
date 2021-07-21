using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace IM.Services.Companies.Prices.Api.DataAccess.Entities
{
    public class PriceSourceType
    {
        [Key]
        public byte Id { get; set; }
        [Required, StringLength(50)]
        public string Name { get; set; }

        public virtual IEnumerable<Ticker> Tickers { get; set; }
    }
}