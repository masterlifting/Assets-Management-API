using System;
using System.Collections.Generic;
using System.Linq;

using static IM.Services.Companies.Prices.Api.DataAccess.DataEnums;

namespace IM.Services.Companies.Prices.Api.Services.PriceServices
{
    public static class PriceLoaderHelper
    {
        public static readonly Dictionary<PriceSourceTypes, DateTime[]> SourceTypeWeekend = new()
        {
            {
                PriceSourceTypes.moex,
                new DateTime[]
                {
                    new(2021, 06, 14),
                    new(2021, 11, 04),
                    new(2021, 11, 05),
                    new(2021, 12, 31)
                }
            },
            {
                PriceSourceTypes.tdameritrade,
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
        public static bool IsWeekend(DateTime date) => date.DayOfWeek == DayOfWeek.Sunday || date.DayOfWeek == DayOfWeek.Saturday;
        public static bool IsWeekend(PriceSourceTypes sourceType, DateTime date) => SourceTypeWeekend[sourceType].Contains(date.Date);
        public static DateTime GetLastWorkday(PriceSourceTypes sourceType)
        {
            return CheckWorkday(sourceType, DateTime.UtcNow.AddDays(-1));

            static DateTime CheckWorkday(PriceSourceTypes sourceType, DateTime checkingDate) =>
                IsWeekend(checkingDate)
                ? CheckWorkday(sourceType, checkingDate.AddDays(-1))
                : IsWeekend(sourceType, checkingDate)
                    ? CheckWorkday(sourceType, checkingDate.AddDays(-1))
                    : checkingDate.Date;
        }
    }
}
