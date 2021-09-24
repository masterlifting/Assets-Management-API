
using System;
using System.ComponentModel.DataAnnotations;

namespace IM.Service.Companies.DataAccess.Entities
{
    // ReSharper disable once ClassWithVirtualMembersNeverInherited.Global
    public class StockSplit
    {
        public virtual Company Company { get; set; } = null!;
        public string CompanyTicker { get; set; } = null!;

        public DateTime Date { get; init; }
        
        [Range(1, int.MaxValue)]
        public int Value { get; set; }
        
        [Range(1, int.MaxValue)]
        public int Divider { get; set; }
    }
}
