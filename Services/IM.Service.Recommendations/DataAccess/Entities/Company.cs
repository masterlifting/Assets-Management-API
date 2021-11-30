using System.Collections.Generic;

namespace IM.Service.Recommendations.DataAccess.Entities;

public class Company : Common.Net.Models.Entity.Company
{
    public virtual Purchase? Purchase { get; set; }
    public virtual IEnumerable<Sale>? Sales { get; set; }
}