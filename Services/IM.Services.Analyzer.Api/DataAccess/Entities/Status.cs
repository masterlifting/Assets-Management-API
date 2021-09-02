using CommonServices.Models.Entity;

using System.Collections.Generic;

namespace IM.Services.Analyzer.Api.DataAccess.Entities
{
    public class Status : CommonEntityType
    {
        public virtual IEnumerable<Report>? Reports { get; set; }
        public virtual IEnumerable<Price>? Prices { get; set; }
    }
}
