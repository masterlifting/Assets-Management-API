using System;

namespace IM.Service.Common.Net.Models.Dto.Http.Companies
{
    public record StockSplitGetDto
    {
        public string Company { get; init; } = null!;
        public DateTime Date { get; init; }
        public string SourceType { get; init; } = null!;
        public int Value { get; init; }
    }
}
