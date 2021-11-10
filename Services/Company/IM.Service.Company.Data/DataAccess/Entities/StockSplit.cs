using IM.Service.Common.Net.Models.Entity.Companies;
using IM.Service.Common.Net.Models.Entity.Companies.Interfaces;

using System;

namespace IM.Service.Company.Data.DataAccess.Entities
{
    public class StockSplit : StockSplitBody, ICompanyDateIdentity
    {
        public virtual Company Company { get; init; } = null!;
        public string CompanyId { get; init; } = null!;

        public DateTime Date { get; init; }
    }
}
