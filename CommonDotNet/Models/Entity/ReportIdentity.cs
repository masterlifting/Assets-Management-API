namespace CommonServices.Models.Entity
{
    public class ReportIdentity
    {
        public string TickerName { get; set; } = null!;
        public int Year { get; set; }
        public byte Quarter { get; set; }
    }
}
