using IM.Gateway.Companies.Services.Attributes;

using System;
using System.ComponentModel.DataAnnotations;

namespace IM.Gateway.Companies.Models.Dto
{
    public class StockSplitPostDto
    {
        [Required, StringLength(10)]
        public string Ticker { get; set; } = null!;

        public DateTime Date { get; set; }
        [Zero, Range(1, 100)]
        public int Divider { get; set; }
    }
}
