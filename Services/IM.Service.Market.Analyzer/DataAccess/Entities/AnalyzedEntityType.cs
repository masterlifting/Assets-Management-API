using IM.Service.Common.Net.Models.Entity;

using System.Collections.Generic;

namespace IM.Service.Market.Analyzer.DataAccess.Entities;

public class AnalyzedEntityType : Catalog
{
    public IEnumerable<AnalyzedEntity>? AnalyzedEntities { get; set; }
}