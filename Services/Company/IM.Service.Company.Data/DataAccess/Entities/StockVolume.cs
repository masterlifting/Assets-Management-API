using System;
using IM.Service.Common.Net.Models.Entity.Companies;
using IM.Service.Common.Net.Models.Entity.Companies.Interfaces;

namespace IM.Service.Company.Data.DataAccess.Entities
{
    public class StockVolume : StockVolumeBody, ICompanyDateIdentity
    {
        public virtual Company Company { get; init; } = null!;
        public string CompanyId { get; init; } = null!;

        public DateTime Date { get; init; }
    }
}
