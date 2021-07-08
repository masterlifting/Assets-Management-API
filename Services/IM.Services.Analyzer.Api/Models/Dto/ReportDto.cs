using IM.Services.Analyzer.Api.Models.Calculator.Rating;

namespace IM.Services.Analyzer.Api.Models.Dto
{
    public class ReportDto : ReportCalculatorModel
    {
        public string Ticker { get; set; } = null!;

        public string ReportSourceType { get; set; } = null!;

        public string ReportSource { get; set; } = null!;
        public int Year { get; set; }
        public byte Quarter { get; set; }

        public long StockVolume { get; set; }
    }
}
