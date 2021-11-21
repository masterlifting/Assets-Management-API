using IM.Service.Common.Net.Models.Entity.Companies;

using System;
using System.Collections.Generic;

namespace IM.Service.Common.Net.RepositoryService.Comparators
{
    public class CompanyComparer : IEqualityComparer<Company>
    {
        public bool Equals(Company? x, Company? y) => string.Equals(x!.Name, y!.Name, StringComparison.InvariantCultureIgnoreCase);
        public int GetHashCode(Company obj) => obj.Name.GetHashCode();
    }
}
