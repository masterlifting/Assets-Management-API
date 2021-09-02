using static IM.Services.Analyzer.Api.Enums;

namespace IM.Services.Analyzer.Api.Models.Calculator
{
    public class Sample
    {
        public uint Index { get; set; }
        public decimal Value { get; set; }
        public CompareType CompareType { get; set; }
    }
}
