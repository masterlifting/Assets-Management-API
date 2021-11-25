using IM.Service.Common.Net.Models.Entity.Companies.Interfaces;

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IM.Service.Company.Analyzer.DataAccess.Entities;

// ReSharper disable once ClassWithVirtualMembersNeverInherited.Global
public class Price : ICompanyDateIdentity
{
    public Company Company { get; init; } = null!;
    public string CompanyId { get; init; } = null!;
    [Column(TypeName = "Date")]
    public DateTime Date { get; init; }

    [Column(TypeName = "Decimal(18,4)")]
    public decimal Result { get; set; }

    public Status Status { get; set; } = null!;
    [Range(1, byte.MaxValue)]
    public byte StatusId { get; set; }

}