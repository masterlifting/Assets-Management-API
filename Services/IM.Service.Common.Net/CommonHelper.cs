using System;

namespace IM.Service.Common.Net;

public static class CommonHelper
{
    public static class QuarterHelper
    {
        public static DateTime ToDateTime(int year, byte quarter) => new(year, GetLastMonth(quarter), 28);
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

        public static (int year, byte quarter) SubtractQuarter(DateTime date)
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
}