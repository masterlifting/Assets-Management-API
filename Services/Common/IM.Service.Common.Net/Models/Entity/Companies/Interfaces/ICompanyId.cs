using System.ComponentModel.DataAnnotations;
using IM.Service.Common.Net.Attributes;

namespace IM.Service.Common.Net.Models.Entity.Companies.Interfaces
{
    public interface ICompanyId
    {
        [Key, StringLength(10, MinimumLength = 1), Upper]
        string CompanyId { get; init; }
    }
}
