using CommonServices.Models.Entity;

using System.Collections.Generic;

namespace IM.Services.Companies.Prices.Api.DataAccess.Entities
{
    public class Ticker : TickerIdentity
    {
        public virtual PriceSourceType SourceType { get; set; }
        public byte PriceSourceTypeId { get; set; }

        public virtual IEnumerable<Price> Prices { get; set; }
    }
}