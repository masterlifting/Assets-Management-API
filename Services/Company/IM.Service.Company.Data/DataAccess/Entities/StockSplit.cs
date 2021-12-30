using IM.Service.Common.Net.Models.Entity.CompanyServices;
using IM.Service.Common.Net.Models.Entity.CompanyServices.Interfaces;

using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace IM.Service.Company.Data.DataAccess.Entities;

public class StockSplit : StockSplitBody, ICompanyDateIdentity
{
    public virtual Company Company { get; init; } = null!;
    public string CompanyId { get; init; } = null!;
    [Column(TypeName = "Date")]
    public DateTime Date { get; set; }
}