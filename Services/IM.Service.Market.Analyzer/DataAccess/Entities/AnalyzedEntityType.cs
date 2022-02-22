using IM.Service.Common.Net.Models.Entity;

using System.Collections.Generic;

namespace IM.Service.Market.Analyzer.DataAccess.Entities;

public class AnalyzedEntityType : CommonEntityType
{
    public IEnumerable<AnalyzedEntity>? AnalyzedEntities { get; set; }
}