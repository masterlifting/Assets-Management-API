
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace IM.Gateway.Companies.DataAccess.Entities
{
    // ReSharper disable once ClassWithVirtualMembersNeverInherited.Global
    public class Company
    {
        [Key, StringLength(10, MinimumLength = 1)]
        public string Ticker { get; set; } = null!;

        [Required, StringLength(300)]
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        
        public IEnumerable<StockSplit>? StockSplits { get; set; }
    }
}
