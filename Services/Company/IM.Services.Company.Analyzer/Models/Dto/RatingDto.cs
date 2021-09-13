using System;
using IM.Services.Company.Analyzer.DataAccess.Entities;

namespace IM.Services.Company.Analyzer.Models.Dto
{
    public class RatingDto
    {
        public RatingDto() { }
        public RatingDto(Rating rating)
        {
            if (rating is null)
                throw new NullReferenceException($"{nameof(rating)} is null");

            Ticker = rating.TickerName;
            UpdateTime = rating.UpdateTime;

            Place = rating.Place;
            Result = rating.Result;

            PriceComparison = rating.PriceComparison;
            ReportComparison = rating.ReportComparison;
        }
        public string Ticker { get; } = null!;
        public DateTime UpdateTime { get; }

        public int Place { get; }
        public decimal Result { get; }

        public decimal PriceComparison { get; }
        public decimal ReportComparison { get; }
    }
}
