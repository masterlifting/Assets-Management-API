namespace IM.Services.Analyzer.Api.Settings
{
    public class CalculatorSettings
    {
        public RatingWeights RatingWeights { get; set; } = null!;
    }
    public class RatingWeights
    {
        public PriceWeights PriceWeights { get; set; } = null!;
        public CoefficientWeights CoefficientWeights { get; set; } = null!;
        public ReportWeights ReportWeights { get; set; } = null!;
    }
    public class PriceWeights
    {
        public decimal ComparisionWeght { get; set; }
    }
    public class CoefficientWeights
    {
        public decimal ComparisionWeght { get; set; }
        public decimal AverageWeght { get; set; }
    }
    public class ReportWeights
    {
        public decimal ComparisionWeght { get; set; }
        public decimal CashFlowWeght { get; set; }
        public int CashFlowMaxPercentResult { get; set; }
    }
}
