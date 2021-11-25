using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using IM.Service.Common.Net.Models.Entity.Companies.Interfaces;

namespace IM.Service.Company.Analyzer.DataAccess.Entities;

// ReSharper disable once ClassWithVirtualMembersNeverInherited.Global
public class Rating : ICompanyId
{
    [Key]
    public int Place { get; set; }

    public Company Company { get; init; } = null!;
    public string CompanyId { get; init; } = null!;

    public DateTime UpdateTime { get; set; } = DateTime.UtcNow;

    [Column(TypeName = "Decimal(18,2)")]
    public decimal Result { get; set; }

    [Column(TypeName = "Decimal(18,4)")]
    public decimal ResultPrice { get; set; }
    [Column(TypeName = "Decimal(18,4)")]
    public decimal ResultReport { get; set; }

}