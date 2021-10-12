using CommonServices.Models.Dto.CompanyPrices;
using CommonServices.Models.Entity;

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace IM.Service.Company.Prices.DataAccess.Entities
{
    // ReSharper disable once ClassWithVirtualMembersNeverInherited.Global
    public class Ticker : TickerIdentity
    {
        public Ticker() { }
        public Ticker(TickerPostDto ticker)
        {
            Name = ticker.Name;
            SourceTypeId = ticker.SourceTypeId;
            SourceValue = ticker.SourceValue;
        }
        public virtual SourceType SourceType { get; set; } = null!;
        [Range(1, byte.MaxValue)]
        public byte SourceTypeId { get; set; }

        [StringLength(10)]
        public string? SourceValue { get; set; }

        public virtual IEnumerable<Price>? Prices { get; set; }
    }
}