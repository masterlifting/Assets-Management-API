using System;
using System.Collections.Generic;
using System.Linq;

namespace CommonServices
{
    public static class CommonHelper
    {
        private static readonly Dictionary<string, DateTime[]> ExchangeWeekend = new(StringComparer.InvariantCultureIgnoreCase)
        {
            {
                "MOEX",
                new DateTime[]
                {
                            new(2021, 06, 14),
                            new(2021, 11, 04),
                            new(2021, 11, 05),
                            new(2021, 12, 31)
                }
            },
            {
                "Tdameritrade",
                new DateTime[]
                {
                            new(2021, 05, 31),
                            new(2021, 06, 05),
                            new(2021, 09, 06),
                            new(2021, 10, 11),
                            new(2021, 11, 11),
                            new(2021, 11, 25),
                            new(2021, 12, 24),
                            new(2021, 12, 31)
                }
            }
        };
        
        public static DateTime GetExchangeLastWorkday(string sourceType, DateTime? date = null)
        {
            return  CheckWorkday(sourceType, date ?? DateTime.UtcNow.AddDays(-1));

            static DateTime CheckWorkday(string sourceType, DateTime checkingDate) =>
                IsExchangeWeekend(checkingDate)
                ? CheckWorkday(sourceType, checkingDate.AddDays(-1))
                : IsExchangeWeekend(sourceType, checkingDate)
                    ? CheckWorkday(sourceType, checkingDate.AddDays(-1))
                    : checkingDate.Date;
        }
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
        public static (int year, int month, int day) GetQuarterFirstDate(int year, byte quarter) => (year, GetFirstMonth(quarter), 1);
        public static (int year, byte quarter) SubstractQuarter(DateTime date)
        {
            var (year, quarter) = GetYearAndQuarter(date);

            if (quarter == 1)
            {
                year--;
                quarter = 4;
            }
            else
                quarter--;

            return (year, quarter);
        }
        public static (int year, byte quarter) SubstractQuarter(int year, byte quarter)
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
        public static bool IsExchangeWeekend(DateTime date) => date.DayOfWeek is DayOfWeek.Sunday or DayOfWeek.Saturday;

        private static byte GetFirstMonth(byte quarter) => quarter switch
        {
            1 => 1,
            2 => 4,
            3 => 7,
            4 => 10,
            _ => throw new NotSupportedException()
        };
        private static bool IsExchangeWeekend(string sourceType, DateTime date) => 
            ExchangeWeekend.ContainsKey(sourceType) && ExchangeWeekend[sourceType].Contains(date.Date);
        private static (int year, byte quarter) GetYearAndQuarter(DateTime date) => (date.Year, GetQuarter(date.Month));
    }
}
