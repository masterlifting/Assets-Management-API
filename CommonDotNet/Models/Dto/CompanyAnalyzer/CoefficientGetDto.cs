namespace CommonServices.Models.Dto.CompanyAnalyzer
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class CoefficientGetDto
    {
        public string Ticker { get; set; } = null!;
        public int Year { get; set; }
        public byte Quarter { get; set; }

        public string ReportSource { get; set; } = null!;

        public decimal Pe { get; set; }
        public decimal Pb { get; set; }
        public decimal DebtLoad { get; set; }
        public decimal Profitability { get; set; }
        public decimal Roa { get; set; }
        public decimal Roe { get; set; }
        public decimal Eps { get; set; }
    }
}
