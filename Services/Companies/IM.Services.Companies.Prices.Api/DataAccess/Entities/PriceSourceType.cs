using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IM.Services.Companies.Prices.Api.DataAccess.Entities
{
    public class PriceSourceType
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }
        [Required, StringLength(50)]
        public string Name { get; set; }

        public virtual IEnumerable<Ticker> Tickers { get; set; }
    }
}