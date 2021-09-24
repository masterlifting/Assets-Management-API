using System;
using System.Linq.Expressions;
using IM.Service.Companies.DataAccess.Entities;
using static CommonServices.CommonEnums;
using static CommonServices.HttpServices.QueryDateSetter;

namespace IM.Service.Companies.Services.DtoServices
{
    public class HttpStockSplitFilter
    {
        private readonly HttpRequestFilterType filterType;
        private readonly FilterPriceEqualType equalType;
        private readonly string? ticker;
        private readonly int year;
        private readonly int month;
        private readonly int day;

        public HttpStockSplitFilter(int year)
        {
            equalType = FilterPriceEqualType.Year;
            this.year = SetYear(year);
        }
        public HttpStockSplitFilter(string ticker, int year)
        {
            equalType = FilterPriceEqualType.Year;
            this.ticker = ticker.ToUpperInvariant().Trim();
            this.year = SetYear(year);
        }

        public HttpStockSplitFilter(int year, int month)
        {
            equalType = FilterPriceEqualType.YearMonth;
            this.year = SetYear(year);
            this.month = SetMonth(month, filterType);
        }
        public HttpStockSplitFilter(string ticker, int year, int month)
        {
            equalType = FilterPriceEqualType.YearMonth;
            this.ticker = ticker.ToUpperInvariant().Trim();
            this.year = SetYear(year);
            this.month = SetMonth(month, filterType);
        }

        public HttpStockSplitFilter(HttpRequestFilterType filterType, int year, int month, int day)
        {
            equalType = FilterPriceEqualType.YearMonthDay;
            this.filterType = filterType;
            this.year = SetYear(year);
            this.month = SetMonth(month, filterType);
            this.day = SetDay(day);
        }
        public HttpStockSplitFilter(HttpRequestFilterType filterType, string ticker, int year, int month, int day)
        {
            equalType = FilterPriceEqualType.YearMonthDay;
            this.filterType = filterType;
            this.ticker = ticker.ToUpperInvariant().Trim();
            this.year = SetYear(year);
            this.month = SetMonth(month, filterType);
            this.day = SetDay(day);
        }

        public Expression<Func<StockSplit, bool>> FilterExpression => x => ticker == null | ticker == x.CompanyTicker
            && (filterType == HttpRequestFilterType.More
            ? x.Date.Year > year || x.Date.Year == year && (x.Date.Month == month && x.Date.Day >= day || x.Date.Month > month)
            : equalType == FilterPriceEqualType.Year
                ? x.Date.Year == year
                : equalType == FilterPriceEqualType.YearMonth
                    ? x.Date.Year == year && x.Date.Month == month
                    : x.Date.Year == year && x.Date.Month == month && x.Date.Day == day);
    };

    internal enum FilterPriceEqualType
    {
        Year,
        YearMonth,
        YearMonthDay
    }
}
