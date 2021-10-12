
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace IM.Service.Companies.DataAccess.Entities
{
    // ReSharper disable once ClassWithVirtualMembersNeverInherited.Global
    public class Company
    {
        [Key, StringLength(10, MinimumLength = 1)]
        public string Ticker { get; set; } = null!;

        [Required, StringLength(300)]
        public string Name { get; set; } = null!;

        [Range(1, byte.MaxValue)]
        public byte IndustryId { get; set; }
        public virtual Industry Industry { get; set; } = null!;

        public string? Description { get; set; }

        public virtual IEnumerable<StockSplit>? StockSplits { get; set; }
    }
}
