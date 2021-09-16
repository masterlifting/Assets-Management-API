using System;

namespace CommonServices.Models.Dto
{
    public class StockSplitDto
    {
        public string Ticker { get; set; } = null!;
        public DateTime Date { get; set; }
        public int Divider { get; set; }
    }
}
