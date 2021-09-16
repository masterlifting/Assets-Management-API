using System;
using System.Linq.Expressions;

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
            var yearNow = DateTime.UtcNow.Year;

            Year = year > yearNow ? yearNow : year <= 0 ? yearNow : year;
            Quarter = quarter > 4 ? (byte)4 : quarter <= 0 ? (byte)1 : quarter;
            QueryParams = $"year={Year}&quarter={Quarter}";
        }
        public FilterRequestModel(int year, int month, int day)
        {
            var yearNow = DateTime.UtcNow.Year;
            var monthNow = DateTime.UtcNow.Month;
            var dayNow = DateTime.UtcNow.Month;

            Year = year > yearNow ? yearNow : year <= 0 ? yearNow : year;
            Month = month > 12 ? 12 : month <= 0 ? dayNow != 1 ? monthNow : DateTime.UtcNow.AddMonths(-1).Month : month;
            Day = day > 31 ? 31 : day <= 0 ? 1 : day;
            QueryParams = $"year={Year}&month={Month}&day={Day}";
        }

        public string QueryParams { get; }

        /// <summary>
        /// Year, Quarter
        /// </summary>
        public Expression<Func<T, bool>> FilterQuarterExpression<T>() where T : class, IFilterQuarter => x =>
            x.Year > Year || x.Year == Year && x.Quarter >= Quarter;
        /// <summary>
        /// Year, Month, Day
        /// </summary>
        public Expression<Func<T, bool>> FilterDateExpression<T>() where T : class, IFilterDate => x =>
             x.Date.Year > Year || x.Date.Year == Year && (x.Date.Month == Month && x.Date.Day >= Day || x.Date.Month > Month);
    }

    public interface IFilterDate
    {
        DateTime Date { get; set; }
    }

    public interface IFilterQuarter
    {
        int Year { get; set; }
        byte Quarter { get; set; }
    }
}
