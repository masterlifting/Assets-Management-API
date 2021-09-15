using System;

namespace IM.Gateway.Companies.Models.Dto
{
    public class StockSplitGetDto
    {
        public string Company { get; set; } = null!;
        public string Ticker { get; set; } = null!;
        public DateTime Date { get; set; }
        public int Divider { get; set; }
    }
}
