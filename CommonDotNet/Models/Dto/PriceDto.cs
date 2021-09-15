using CommonServices.Models.Entity;

namespace CommonServices.Models.Dto
{
    public class PriceDto : PriceIdentity
    {
        // ReSharper disable once MemberCanBeProtected.Global
        // ReSharper disable once PropertyCanBeMadeInitOnly.Global
        public string SourceType { get; set; } = null!;
        // ReSharper disable once MemberCanBeProtected.Global
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public byte SourceTypeId { get; set; }

        // ReSharper disable once MemberCanBeProtected.Global
        // ReSharper disable once PropertyCanBeMadeInitOnly.Global
        public decimal Value { get; set; }
    }
}
