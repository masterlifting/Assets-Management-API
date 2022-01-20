using IM.Service.Common.Net.Models.Entity;

using System.Collections.Generic;

namespace IM.Service.Company.Analyzer.DataAccess.Entities;

public class Status : CommonEntityType
{
    public IEnumerable<AnalyzedEntity>? AnalyzedEntities { get; set; }
}