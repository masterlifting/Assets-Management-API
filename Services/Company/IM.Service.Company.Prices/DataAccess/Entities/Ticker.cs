using CommonServices.Models.Dto.CompanyPrices;
using CommonServices.Models.Entity;

using System.Collections.Generic;

namespace IM.Service.Company.Prices.DataAccess.Entities
{
    // ReSharper disable once ClassWithVirtualMembersNeverInherited.Global
    public class Ticker : TickerIdentity
    {
        public Ticker() { }
        public Ticker(CompaniesPricesTickerDto ticker)
        {
            Name = ticker.Name;
            SourceTypeId = ticker.SourceTypeId;
        }
        public virtual SourceType SourceType { get; set; } = null!;
        public byte SourceTypeId { get; set; }

        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public virtual IEnumerable<Price>? Prices { get; set; }
    }
}