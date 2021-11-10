using System;
using IM.Service.Common.Net.Models.Entity.Companies;
using IM.Service.Common.Net.Models.Entity.Companies.Interfaces;

namespace IM.Service.Company.Data.DataAccess.Entities
{
    // ReSharper disable once ClassWithVirtualMembersNeverInherited.Global
    public class Price : PriceBody, ICompanyDateIdentity
    {
        public virtual Company Company { get; init; } = null!;
        public string CompanyId { get; init; } = null!;

        public DateTime Date { get; init; }
    }
}
