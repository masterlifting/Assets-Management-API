using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace IM.Services.Analyzer.Api.DataAccess.Entities
{
    public class Ticker
    {
        [Key, StringLength(10, MinimumLength = 1)]
        public string Name { get; set; } = null!;

        public virtual List<Coefficient> Coefficients { get; set; } = null!;
        
        public virtual Rating Rating { get; set; } = null!;
        public virtual Recommendation Recommendation { get; set; } = null!;
    }
}
