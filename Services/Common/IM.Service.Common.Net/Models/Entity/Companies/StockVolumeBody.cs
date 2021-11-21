using IM.Service.Common.Net.Attributes;

namespace IM.Service.Common.Net.Models.Entity.Companies
{
    public abstract class StockVolumeBody : SourceTypeBody
    {
        [NotZero(nameof(Value))]
        public long Value { get; set; }
    }
}
