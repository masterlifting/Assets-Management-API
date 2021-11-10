using System;

namespace IM.Service.Company.Data.Models.Data
{
    public record PriceDataConfigModel
    {
        public string CompanyId { get; init; } = null!;
        public DateTime Date { get; init; }
        public string? SourceValue { get; init; }
    }
}
