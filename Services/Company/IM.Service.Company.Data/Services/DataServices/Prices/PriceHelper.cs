using System;
using System.Collections.Generic;
using System.Linq;

namespace IM.Service.Company.Data.Services.DataServices.Prices;

public static class PriceHelper
{
    private static readonly Dictionary<string, DateOnly[]> ExchangeWeekend = new(StringComparer.InvariantCultureIgnoreCase)
    {
        {
            "moex",
            new DateOnly[]
            {
                new(2021, 06, 14),
                new(2021, 11, 04),
                new(2021, 11, 05),
                new(2021, 12, 31)
            }
        },
        {
            "tdameritrade",
            new DateOnly[]
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

    public static DateOnly GetExchangeWorkDate(string sourceType, DateOnly? date = null)
    {
        return CheckWorkday(sourceType, date ?? DateOnly.FromDateTime(DateTime.UtcNow));

        static DateOnly CheckWorkday(string sourceType, DateOnly checkDate) =>
            IsExchangeWeekend(sourceType, checkDate)
                ? CheckWorkday(sourceType, checkDate.AddDays(-1))
                : checkDate;
    }

    private static bool IsExchangeWeekend(string sourceType, DateOnly date) =>
        date.DayOfWeek is DayOfWeek.Sunday or DayOfWeek.Saturday
        && ExchangeWeekend.ContainsKey(sourceType)
        && ExchangeWeekend[sourceType].Contains(date);
}