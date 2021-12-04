using System;
using IM.Service.Common.Net.Models.Entity.CompanyServices.Interfaces;
using static IM.Service.Company.Analyzer.Enums;

namespace IM.Service.Company.Analyzer.Models.Calculator;

public record struct Sample : ICompanyDateIdentity
{
    public string CompanyId { get; init; } = null!;
    public DateTime Date { get; init; }

    public decimal Value { get; init; }
    public CompareTypes CompareTypes { get; init; }
}