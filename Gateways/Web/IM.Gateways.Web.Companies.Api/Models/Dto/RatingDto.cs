using System;

namespace IM.Gateways.Web.Companies.Api.Models.Dto
{
    public class RatingDto
    {
        public string Ticker { get; } = null!;
        public DateTime UpdateTime { get; }

        public int Place { get; }
        public decimal Result { get; }

        public decimal? PriceComparison { get; }
        public decimal? ReportComparison { get; }
        public decimal? CashFlowPositiveBalance { get; }
        public decimal? CoefficientComparison { get; }
        public decimal? CoefficientAverage { get; }
    }
}
