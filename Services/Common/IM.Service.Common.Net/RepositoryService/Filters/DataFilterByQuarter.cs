using System;
using IM.Service.Common.Net.Models.Entity.Interfaces;
using static IM.Service.Common.Net.CommonEnums;
using static IM.Service.Common.Net.RepositoryService.Filters.DataFilterSetter;

namespace IM.Service.Common.Net.RepositoryService.Filters
{
    public class DataFilterByQuarter
    {
        private readonly HttpRequestFilterType filterType;
        private readonly FilterQuarterEqualType equalType;
        private int Year { get; }
        private byte Quarter { get; }

        protected DataFilterByQuarter(int year)
        {
            equalType = FilterQuarterEqualType.Year;
            Year = SetYear(year);
        }
        protected DataFilterByQuarter(int year, int quarter)
        {
            equalType = FilterQuarterEqualType.YearQuarter;
            Year = SetYear(year);
            Quarter = SetQuarter(quarter);
        }
        protected DataFilterByQuarter(HttpRequestFilterType filterType, int year, int quarter)
        {
            equalType = FilterQuarterEqualType.YearQuarter;
            this.filterType = filterType;
            Year = SetYear(year);
            Quarter = SetQuarter(quarter);
        }

        protected Func<IQuarterIdentity, bool> Filter => x => filterType == HttpRequestFilterType.More
                ? x.Year > Year || x.Year == Year && x.Quarter >= Quarter
                : equalType == FilterQuarterEqualType.Year
                    ? x.Year == Year
                    : x.Year == Year && x.Quarter == Quarter;
    }
}
