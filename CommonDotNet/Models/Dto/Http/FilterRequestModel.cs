using System;

namespace CommonServices.Models.Dto.Http
{
    public class FilterRequestModel
    {
        public FilterRequestModel(int year, byte quarter)
        {
            Year = year > DateTime.UtcNow.Year ? DateTime.UtcNow.Year : year <= 2000 ? 2001 : year;
            Quarter = quarter > 4 ? (byte)4 : quarter <= 0 ? (byte)1 : quarter;
            QueryParams = $"year={Year}&quarter={Quarter}";
        }
        public FilterRequestModel(int year, int month, int? day = null)
        {
            Year = year > DateTime.UtcNow.Year ? DateTime.UtcNow.Year : year <= 2000 ? 2001 : year;
            Month = month > 12 ? 12 : month <= 0 ? 1 : month;

            string queryParams = $"year={Year}&month={Month}";

            if (day is not null)
            {
                Day = day.Value > 31 ? 31 : day.Value <= 0 ? 1 : day.Value;
                queryParams += $"&day={Day}";
            }
            else
                Day = 1;

            QueryParams = queryParams;
        }
        public int Year { get; }
        public int Month { get; }
        public int Day { get; }
        public byte Quarter { get; }
        public string QueryParams { get; }
        /// <summary>
        /// Year, Quarter
        /// </summary>
        public Func<int, byte, bool> FilterQuarter
        {
            get => (int year, byte quarter) => year > Year || year == Year && quarter >= Quarter;
        }
        /// <summary>
        /// Year, Month, Day
        /// </summary>
        public Func<int, int, int, bool> FilterDate
        {
            get => (int year, int month, int day) => year > Year || year == Year && ((month == Month && day >= Day) || (month > Month));
        }
    }
}
