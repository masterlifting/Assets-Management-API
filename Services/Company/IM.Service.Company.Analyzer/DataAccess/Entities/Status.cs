using IM.Service.Common.Net.Models.Entity;

using System.Collections.Generic;

namespace IM.Service.Company.Analyzer.DataAccess.Entities
{
    // ReSharper disable once ClassWithVirtualMembersNeverInherited.Global
    public class Status : CommonEntityType
    {
        public IEnumerable<Report>? Reports { get; set; }
        public IEnumerable<Price>? Prices { get; set; }
    }
}
