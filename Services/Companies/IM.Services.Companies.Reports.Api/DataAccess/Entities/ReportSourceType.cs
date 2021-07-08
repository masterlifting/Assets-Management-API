using System.ComponentModel.DataAnnotations;

namespace IM.Services.Companies.Reports.Api.DataAccess.Entities
{
    public class ReportSourceType
    {
        [Key]
        public byte Id { get; set; }
        [StringLength(50)]
        public string Name { get; set; }
    }
}