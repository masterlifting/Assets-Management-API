using System;
using System.Collections.Generic;
using System.Linq;

using static CommonServices.CommonEnums;

namespace CommonServices
{
    public static class CommonHelper
    {
        public static readonly Dictionary<PriceSourceTypes, DateTime[]> ExchangeWeekend = new()
        {
            {
                PriceSourceTypes.MOEX,
                new DateTime[]
                {
                            new(2021, 06, 14),
                            new(2021, 11, 04),
                            new(2021, 11, 05),
                            new(2021, 12, 31)
                }
            },
            {
                PriceSourceTypes.Tdameritrade,
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
        public static byte GetQuarter(int month) => month switch
        {
            int x when x >= 1 && x < 4 => 1,
            int x when x >= 4 && x < 7 => 2,
            int x when x >= 7 && x < 10 => 3,
            int x when x >= 10 && x <= 12 => 4,
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
        public static (int year, byte quarter) GetYearAndQuarter(DateTime date) => (date.Year, GetQuarter(date.Month));
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
        public static bool IsExchangeWeekend(DateTime date) => date.DayOfWeek == DayOfWeek.Sunday || date.DayOfWeek == DayOfWeek.Saturday;
        public static bool IsExchangeWeekend(PriceSourceTypes sourceType, DateTime date) => ExchangeWeekend[sourceType].Contains(date.Date);
        public static DateTime GetExchangeLastWorkday(PriceSourceTypes sourceType)
        {
            return CheckWorkday(sourceType, DateTime.UtcNow.AddDays(-1));

            static DateTime CheckWorkday(PriceSourceTypes sourceType, DateTime checkingDate) =>
                IsExchangeWeekend(checkingDate)
                ? CheckWorkday(sourceType, checkingDate.AddDays(-1))
                : IsExchangeWeekend(sourceType, checkingDate)
                    ? CheckWorkday(sourceType, checkingDate.AddDays(-1))
                    : checkingDate.Date;
        }
        public static DateTime GetExchangeLastWorkday(PriceSourceTypes sourceType, DateTime date)
        {
            return CheckWorkday(sourceType, date.AddDays(-1));

            static DateTime CheckWorkday(PriceSourceTypes sourceType, DateTime checkingDate) =>
                IsExchangeWeekend(checkingDate)
                ? CheckWorkday(sourceType, checkingDate.AddDays(-1))
                : IsExchangeWeekend(sourceType, checkingDate)
                    ? CheckWorkday(sourceType, checkingDate.AddDays(-1))
                    : checkingDate.Date;
        }
    }
}
