using System.ComponentModel.DataAnnotations;

namespace IM.Gateways.Web.Company.DataAccess.Entities
{
    public class Company
    {
        [Key, StringLength(10, MinimumLength = 1)]
        public string Ticker { get; set; } = null!;

        [Required, StringLength(300)]
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
    }
}
