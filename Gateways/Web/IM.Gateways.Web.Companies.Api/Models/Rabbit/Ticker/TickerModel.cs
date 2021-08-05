using System.ComponentModel.DataAnnotations;

namespace IM.Gateways.Web.Companies.Api.Models.Rabbit.Ticker
{
    public abstract class TickerModel
    {
        [StringLength(10, MinimumLength = 1)]
        public string Name { get; set; } = null!;
    }
}
