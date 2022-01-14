using System;
using System.ComponentModel.DataAnnotations.Schema;
using IM.Service.Common.Net.Models.Entity.CompanyServices;
using IM.Service.Common.Net.Models.Entity.CompanyServices.Interfaces;

namespace DataSetter.DataAccess.CompanyData.Entities;

// ReSharper disable once ClassWithVirtualMembersNeverInherited.Global
public class Price : PriceBody, ICompanyDateIdentity
{
    public virtual Company Company { get; init; } = null!;
    public string CompanyId { get; init; } = null!;

    [Column(TypeName = "Date")]
    public DateTime Date { get; set; }
}