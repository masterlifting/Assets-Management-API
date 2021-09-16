using System;
using System.ComponentModel.DataAnnotations;

using CommonServices.Models.Dto.Http;

namespace IM.Gateway.Companies.DataAccess.Entities
{
    public class StockSplit : IFilterDate
    {
        [Key]
        public int Id { get; init; }

        public Company Company { get; set; } = null!;
        public string CompanyTicker { get; set; } = null!;

        public DateTime Date { get; set; }
        public int Divider { get; set; }
    }
}
