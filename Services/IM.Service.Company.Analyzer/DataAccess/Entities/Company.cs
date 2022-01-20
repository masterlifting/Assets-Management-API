using System.Collections.Generic;

namespace IM.Service.Company.Analyzer.DataAccess.Entities;

public class Company : Common.Net.Models.Entity.Company
{
    public Rating Rating { get; init; } = null!;
    public IEnumerable<AnalyzedEntity>? AnalyzedEntities { get; init; }
}