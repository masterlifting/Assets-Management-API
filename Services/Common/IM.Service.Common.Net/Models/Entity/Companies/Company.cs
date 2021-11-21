using System.ComponentModel.DataAnnotations;
using IM.Service.Common.Net.Attributes;

namespace IM.Service.Common.Net.Models.Entity.Companies
{
    public abstract class Company
    {
        [Key, StringLength(10, MinimumLength = 1), Upper]
        public string Id { get; init; } = null!;

        [StringLength(100, MinimumLength = 2)]
        public string Name { get; set; } = null!;
    }
}
