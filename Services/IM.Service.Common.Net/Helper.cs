using System;
using System.Text.Json;
using IM.Service.Common.Net.HttpServices.JsonConvertors;

namespace IM.Service.Common.Net;

public static class Helper
{
    public static class QuarterHelper
    {
        public static DateOnly ToDate(int year, byte quarter) => new(year, GetLastMonth(quarter), 28);
        public static byte GetQuarter(int month) => month switch
        {
            >= 1 and < 4 => 1,
            < 7 and >= 4 => 2,
            >= 7 and < 10 => 3,
            <= 12 and >= 10 => 4,
            _ => throw new NotSupportedException()
        };

        public static byte GetLastMonth(byte quarter) => quarter switch
        {
            1 => 3,
            2 => 6,
            3 => 9,
            4 => 12,
            _ => throw new NotSupportedException()
        };
        public static byte GetFirstMonth(byte quarter) => quarter switch
        {
            1 => 1,
            2 => 4,
            3 => 7,
            4 => 10,
            _ => throw new NotSupportedException()
        };

        public static (int year, byte quarter) SubtractQuarter(DateOnly date)
        {
            var (year, quarter) = (date.Year, GetQuarter(date.Month));

            if (quarter == 1)
            {
                year--;
                quarter = 4;
            }
            else
                quarter--;

            return (year, quarter);
        }
        public static (int year, byte quarter) SubtractQuarter(int year, byte quarter)
        {
            if (quarter == 1)
            {
                year--;
                quarter = 4;
            }
            else
                quarter--;

            return (year, quarter);
        }
    }
    public static class JsonHelper
    {
        public static JsonSerializerOptions Options { get; }

        static JsonHelper()
        {
            Options = new(JsonSerializerDefaults.Web);
            Options.Converters.Add(new DateOnlyConverter());
            Options.Converters.Add(new TimeOnlyConverter());
        }
    }
}