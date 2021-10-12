
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

// ReSharper disable PropertyCanBeMadeInitOnly.Global

namespace CommonServices.Models.Entity
{
    public class CommonEntityType
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public byte Id { get; set; }
        
        [Required, StringLength(100, MinimumLength = 3)]
        public string Name { get; set; } = null!;
        
        [StringLength(200)]
        public string? Description { get; set; }
    }
}
