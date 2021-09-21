
using IM.Service.Company.Reports.DataAccess.Entities;

using System;
using System.Linq.Expressions;

using static CommonServices.CommonEnums;
using static CommonServices.HttpServices.QueryDateSetter;

namespace IM.Service.Company.Reports.Services.DtoServices
{
    public class HttpFilter
    {
        private readonly HttpRequestFilterType filterType;
        private readonly FilterReportEqualType equalType;
        private readonly string? ticker;
        private readonly int year;
        private readonly byte quarter;

        public HttpFilter(int year)
        {
            equalType = FilterReportEqualType.Year;
            this.year = SetYear(year);
        }
        public HttpFilter(string ticker, int year)
        {
            equalType = FilterReportEqualType.Year;
            this.ticker = ticker.ToUpperInvariant().Trim();
            this.year = SetYear(year);
        }
        public HttpFilter(int year, int quarter)
        {
            equalType = FilterReportEqualType.YearQuarter;
            this.year = SetYear(year);
            this.quarter = SetQuarter(quarter);
        }
        public HttpFilter(string ticker, int year, int quarter)
        {
            equalType = FilterReportEqualType.YearQuarter;
            this.ticker = ticker.ToUpperInvariant().Trim();
            this.year = SetYear(year);
            this.quarter = SetQuarter(quarter);
        }
        public HttpFilter(HttpRequestFilterType filterType, int year, int quarter)
        {
            equalType = FilterReportEqualType.YearQuarter;
            this.filterType = filterType;
            this.year = SetYear(year);
            this.quarter = SetQuarter(quarter);
        }
        public HttpFilter(HttpRequestFilterType filterType, string ticker, int year, int quarter)
        {
            equalType = FilterReportEqualType.YearQuarter;
            this.filterType = filterType;
            this.ticker = ticker.ToUpperInvariant().Trim();

            this.year = SetYear(year);
            this.quarter = SetQuarter(quarter);
        }

        public Expression<Func<Report, bool>> FilterExpression => x => ticker == null | ticker == x.TickerName
            && (filterType == HttpRequestFilterType.More
                ? x.Year > year || x.Year == year && x.Quarter >= quarter
                : equalType == FilterReportEqualType.Year
                    ? x.Year == year
                    : x.Year == year && x.Quarter == quarter);
    };

    internal enum FilterReportEqualType
    {
        Year,
        YearQuarter
    }
}
