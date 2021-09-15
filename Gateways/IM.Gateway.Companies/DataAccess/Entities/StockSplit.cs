using System;
using System.ComponentModel.DataAnnotations;

namespace IM.Gateway.Companies.DataAccess.Entities
{
    public class StockSplit
    {
        [Key]
        public int Id { get; set; }

        public Company Company { get; set; } = null!;
        public string CompanyTicker { get; set; } = null!;

        public DateTime Date { get; set; }
        public int Divider { get; set; }
    }
}
