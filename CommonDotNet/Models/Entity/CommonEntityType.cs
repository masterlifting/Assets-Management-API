
using System.ComponentModel.DataAnnotations;

namespace CommonServices.Models.Entity
{
    public class CommonEntityType
    {
        [Key]
        public byte Id { get; init; }
        
        [Required, StringLength(50, MinimumLength = 3)]
        public string Name { get; set; } = null!;
        
        [StringLength(200)]
        public string? Description { get; set; }
    }
}
