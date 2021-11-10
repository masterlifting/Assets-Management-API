using System;
using System.Collections.Generic;
using System.Linq;

namespace IM.Service.Company.Data.Services.DataServices.Prices
{
    public static class PriceHelper
    {
        private static readonly Dictionary<string, DateTime[]> ExchangeWeekend = new(StringComparer.InvariantCultureIgnoreCase)
        {
            {
                "moex",
                new DateTime[]
                {
                    new(2021, 06, 14),
                    new(2021, 11, 04),
                    new(2021, 11, 05),
                    new(2021, 12, 31)
                }
            },
            {
                "tdameritrade",
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

        public static DateTime GetExchangeWorkDate(string sourceType, DateTime? date = null)
        {
            return CheckWorkday(sourceType, date ?? DateTime.UtcNow);

            static DateTime CheckWorkday(string sourceType, DateTime checkDate) =>
                IsExchangeWeekend(sourceType, checkDate)
                    ? CheckWorkday(sourceType, checkDate.AddDays(-1))
                    : checkDate;
        }
        public static bool IsExchangeWeekend(string sourceType, DateTime date) =>
            date.DayOfWeek is DayOfWeek.Sunday or DayOfWeek.Saturday
            && ExchangeWeekend.ContainsKey(sourceType)
            && ExchangeWeekend[sourceType].Contains(date.Date);
    }
}
