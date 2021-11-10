using System;
using IM.Service.Common.Net.Models.Entity.Interfaces;
using static IM.Service.Common.Net.CommonEnums;
using static IM.Service.Common.Net.RepositoryService.Filters.DataFilterSetter;

namespace IM.Service.Common.Net.RepositoryService.Filters
{
    public class DataFilterByDate
    {
        private readonly HttpRequestFilterType filterType;
        private readonly FilterDateEqualType equalType;
        private int Year { get; }
        private int Month { get; }
        private int Day { get; }

        protected DataFilterByDate(int year)
        {
            equalType = FilterDateEqualType.Year;
            Year = SetYear(year);
        }
        protected DataFilterByDate(int year, int month)
        {
            equalType = FilterDateEqualType.YearMonth;
            Year = SetYear(year);
            Month = SetMonth(month, filterType);
        }
        protected DataFilterByDate(HttpRequestFilterType filterType, int year, int month, int day)
        {
            equalType = FilterDateEqualType.YearMonthDay;
            this.filterType = filterType;
            Year = SetYear(year);
            Month = SetMonth(month, filterType);
            Day = SetDay(day);
        }

        protected Func<IDateIdentity, bool> Filter => x => filterType == HttpRequestFilterType.More
                ? x.Date.Year > Year || x.Date.Year == Year && (x.Date.Month == Month && x.Date.Day >= Day || x.Date.Month > Month)
                : equalType == FilterDateEqualType.Year
                    ? x.Date.Year == Year
                    : equalType == FilterDateEqualType.YearMonth
                        ? x.Date.Year == Year && x.Date.Month == Month
                        : x.Date.Year == Year && x.Date.Month == Month && x.Date.Day == Day;
    }
}
