using IM.Service.Common.Net.Attributes;

namespace IM.Service.Common.Net.Models.Entity.Companies
{
    public abstract class StockSplitBody : SourceTypeBody
    {
        [NotZero(nameof(Value))]
        public int Value { get; set; }
    }
}
