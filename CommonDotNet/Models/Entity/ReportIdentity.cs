using CommonServices.Models.Dto.Http;

namespace CommonServices.Models.Entity
{
    public class ReportIdentity : IFilterQuarter
    {
        public string TickerName { get; init; } = null!;
        public int Year { get; set; }
        public byte Quarter { get; set; }
    }
}
