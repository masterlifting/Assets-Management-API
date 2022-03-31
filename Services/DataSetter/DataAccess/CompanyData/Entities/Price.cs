using System;

namespace DataSetter.DataAccess.CompanyData.Entities;

// ReSharper disable once ClassWithVirtualMembersNeverInherited.Global
public class Price
{
    public virtual Company Company { get; init; } = null!;
    public string CompanyId { get; init; } = null!;

    public DateOnly Date { get; set; }
}