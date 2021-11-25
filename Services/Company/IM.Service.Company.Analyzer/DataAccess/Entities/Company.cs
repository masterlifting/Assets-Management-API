using System.Collections.Generic;

namespace IM.Service.Company.Analyzer.DataAccess.Entities;

// ReSharper disable once ClassWithVirtualMembersNeverInherited.Global
public class Company : Common.Net.Models.Entity.Companies.Company
{
    public Rating Rating { get; init; } = null!;
    public IEnumerable<Report>? Reports { get; init; }
    public IEnumerable<Price>? Prices { get; init; }
}