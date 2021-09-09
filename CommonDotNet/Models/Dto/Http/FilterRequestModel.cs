using System;

namespace CommonServices.Models.Dto.Http
{
    public class FilterRequestModel
    {
        public int Year { get; }
        public int Month { get; }
        public int Day { get; }
        public byte Quarter { get; }

        public FilterRequestModel(int year, byte quarter)
        {
            Year = year > DateTime.UtcNow.Year ? DateTime.UtcNow.Year : year <= 0 ? DateTime.UtcNow.Year : year;
            Quarter = quarter > 4 ? (byte)4 : quarter <= 0 ? (byte)1 : quarter;
            QueryParams = $"year={Year}&quarter={Quarter}";
        }
        public FilterRequestModel(int year, int month, int day)
        {
            Year = year > DateTime.UtcNow.Year ? DateTime.UtcNow.Year : year <= 0 ? DateTime.UtcNow.Year : year;
            Month = month > 12 ? 12 : month <= 0 ? 1 : month;
            Day = day > 31 ? 31 : day <= 0 ? 1 : day;
            QueryParams = $"year={Year}&month={Month}&day={Day}";
        }

        public string QueryParams { get; }

        /// <summary>
        /// Year, Quarter
        /// </summary>
        public Func<int, byte, bool> FilterQuarter => (year, quarter) => year > Year || year == Year && quarter >= Quarter;

        /// <summary>
        /// Year, Month, Day
        /// </summary>
        public Func<DateTime, bool> FilterDate => x => x.Year > Year || x.Year == Year && (x.Month == Month && x.Day >= Day || x.Month > Month);
    }
}
