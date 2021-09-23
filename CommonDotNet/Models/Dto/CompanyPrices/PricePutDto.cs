using System.ComponentModel.DataAnnotations;
using CommonServices.Attributes;

namespace CommonServices.Models.Dto.CompanyPrices
{
    public class PricePutDto
    {
        [NotZero(nameof(Value))]
        public decimal Value { get; init; }
        [Required, StringLength(50, MinimumLength = 3)]
        public string SourceType { get; init; } = null!;
    }
}
