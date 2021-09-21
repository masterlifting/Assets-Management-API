namespace CommonServices.Models.Entity
{
    public class ReportIdentity
    {
        public string TickerName { get; init; } = null!;
        public int Year { get; init; }
        public byte Quarter { get; init; }
    }
}
