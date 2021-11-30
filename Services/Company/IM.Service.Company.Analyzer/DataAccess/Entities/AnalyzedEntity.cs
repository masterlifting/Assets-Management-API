using IM.Service.Common.Net.Models.Entity.CompanyServices.Interfaces;

using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace IM.Service.Company.Analyzer.DataAccess.Entities;

public class AnalyzedEntity : ICompanyDateIdentity
{
    public Company Company { get; init; } = null!;
    public string CompanyId { get; init; } = null!;
    
    [Column(TypeName = "Date")]
    public DateTime Date { get; init; }
    
    public AnalyzedEntityType EntityType { get; set; } = null!;
    public byte AnalyzedEntityTypeId { get; init; }

    
    public Status Status { get; set; } = null!;
    public byte StatusId { get; set; }

  
    [Column(TypeName = "Decimal(18,4)")]
    public decimal Result { get; set; }
}