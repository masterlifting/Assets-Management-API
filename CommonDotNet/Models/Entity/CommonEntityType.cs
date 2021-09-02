using System.ComponentModel.DataAnnotations;

namespace CommonServices.Models.Entity
{
    public abstract class CommonEntityType
    {
        [Key]
        public byte Id { get; set; }
        [Required, StringLength(50, MinimumLength = 3)]
        public string Name { get; set; } = null!;
        [StringLength(200)]
        public string? Description { get; set; }
    }
}
