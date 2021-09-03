using CommonServices.Models.Dto.CompaniesPricesService;
using CommonServices.Models.Entity;

using System.Collections.Generic;

namespace IM.Services.Companies.Prices.Api.DataAccess.Entities
{
    public class Ticker : TickerIdentity
    {
        public Ticker() { }
        public Ticker(CompaniesPricesTickerDto ticker)
        {
            Name = ticker.Name;
            SourceTypeId = ticker.SourceTypeId;
        }
        public virtual SourceType SourceType { get; set; }
        public byte SourceTypeId { get; set; }

        public virtual IEnumerable<Price> Prices { get; set; }
    }
}