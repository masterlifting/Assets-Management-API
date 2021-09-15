using System.ComponentModel.DataAnnotations;

namespace CommonServices.Models.Entity
{
    public class TickerIdentity
    {
        [Key, StringLength(10, MinimumLength = 1)]
        public string Name { get; init; } = null!;
    }
}
