using System.ComponentModel.DataAnnotations;

namespace IM.Service.Common.Net.Models.Entity.Companies
{
    public abstract class SourceTypeBody
    {
        [Required, StringLength(50, MinimumLength = 3)]
        public string SourceType { get; set; } = null!;
    }
}
