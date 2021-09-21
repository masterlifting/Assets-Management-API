
using IM.Service.Company.Prices.DataAccess.Entities;

using System;
using System.Linq.Expressions;

using static CommonServices.CommonEnums;
using static CommonServices.HttpServices.QueryDateSetter;

namespace IM.Service.Company.Prices.Services.DtoServices
{
    public class HttpFilter
    {
        private readonly HttpRequestFilterType filterType;
        private readonly FilterPriceEqualType equalType;
        private readonly string? ticker;
        private readonly int year;
        private readonly int month;
        private readonly int day;

        public HttpFilter(int year)
        {
            equalType = FilterPriceEqualType.Year;
            this.year = SetYear(year);
        }
        public HttpFilter(string ticker, int year)
        {
            equalType = FilterPriceEqualType.Year;
            this.ticker = ticker.ToUpperInvariant().Trim();
            this.year = SetYear(year);
        }

        public HttpFilter(int year, int month)
        {
            equalType = FilterPriceEqualType.YearMonth;
            this.year = SetYear(year);
            this.month = SetMonth(month, filterType);
        }
        public HttpFilter(string ticker, int year, int month)
        {
            equalType = FilterPriceEqualType.YearMonth;
            this.ticker = ticker.ToUpperInvariant().Trim();
            this.year = SetYear(year);
            this.month = SetMonth(month, filterType);
        }

        public HttpFilter(HttpRequestFilterType filterType, int year, int month, int day)
        {
            equalType = FilterPriceEqualType.YearMonthDay;
            this.filterType = filterType;
            this.year = SetYear(year);
            this.month = SetMonth(month, filterType);
            this.day = SetDay(day);
        }
        public HttpFilter(HttpRequestFilterType filterType, string ticker, int year, int month, int day)
        {
            equalType = FilterPriceEqualType.YearMonthDay;
            this.filterType = filterType;
            this.ticker = ticker.ToUpperInvariant().Trim();
            this.year = SetYear(year);
            this.month = SetMonth(month, filterType);
            this.day = SetDay(day);
        }


        public Expression<Func<Price, bool>> FilterExpression => x => ticker == null | ticker == x.TickerName 
            && (filterType == HttpRequestFilterType.More
                ? x.Date.Year > year || x.Date.Year == year && (x.Date.Month == month && x.Date.Day >= day || x.Date.Month > month)
                : equalType == FilterPriceEqualType.Year
                    ? x.Date.Year == year
                    : equalType == FilterPriceEqualType.YearMonth
                        ? x.Date.Year == year && x.Date.Month == month
                        : x.Date.Year == year && x.Date.Month == month && x.Date.Day == day);
    }

    internal enum FilterPriceEqualType
    {
        Year,
        YearMonth,
        YearMonthDay
    }
}
