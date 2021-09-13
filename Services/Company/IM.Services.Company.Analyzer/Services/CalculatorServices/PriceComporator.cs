﻿using System.Collections.Generic;
using CommonServices.Models.Dto;
using System.Linq;
using IM.Services.Company.Analyzer.DataAccess.Entities;
using IM.Services.Company.Analyzer.Models.Calculator;
using IM.Services.Company.Analyzer.Services.CalculatorServices.Interfaces;
using static IM.Services.Company.Analyzer.Enums;

namespace IM.Services.Company.Analyzer.Services.CalculatorServices
{
    public class PriceComporator : IAnalyzerComparator<Price>
    {
        private readonly PriceDto[] prices;
        private readonly Sample[] valueCollection;

        public PriceComporator(IReadOnlyCollection<PriceDto> prices)
        {
            this.prices = prices.OrderBy(x => x.Date).ToArray();
            valueCollection = new Sample[prices.Count];
            SetData();
        }
        public Price[] GetComparedSample()
        {
            var comparedSample = RatingComparator.CompareSample(valueCollection);
            var result = new Price[comparedSample.Length];

            for (uint i = 0; i < prices.Length; i++)
                for (uint j = 0; j < comparedSample.Length; j++)
                    if (comparedSample[j].Index == i)
                    {
                        result[j] = new Price
                        {
                            TickerName = prices[i].TickerName,
                            Date = prices[i].Date,
                            SourceType = prices[i].SourceType,
                            Result = comparedSample[j].Value,
                            StatusId = (byte)Enums.StatusType.Calculated
                        };

                        break;
                    }
            return result;
        }

        private void SetData()
        {
            for (uint i = 0; i < prices.Length; i++)
                valueCollection[i] = new Sample { Index = i, CompareType = Enums.CompareType.Asc, Value = prices[i].Value };
        }
    }
}