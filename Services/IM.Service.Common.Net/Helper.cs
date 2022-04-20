using System;
using System.Linq.Expressions;

namespace IM.Service.Common.Net;

public static class Helper
{
    public static class QuarterHelper
    {
        public static DateOnly ToDate(int year, byte quarter, int day = 28) => new(year, GetLastMonth(quarter), day);
        public static byte GetQuarter(int month) => month switch
        {
            >= 1 and < 4 => 1,
            < 7 and >= 4 => 2,
            >= 7 and < 10 => 3,
            <= 12 and >= 10 => 4,
            _ => throw new NotSupportedException()
        };
        public static byte[] GetMonths(byte quarter) => quarter switch
        {
            1 => new byte[] { 1, 2, 3 },
            2 => new byte[] { 4, 5, 6 },
            3 => new byte[] { 7, 8, 9 },
            4 => new byte[] { 10, 11, 12 },
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
    public static class ExpressionHelper
    {
        public static Expression<Func<T, bool>> Combine<T>(Expression<Func<T, bool>> firstExpression, Expression<Func<T, bool>> secondExpression)
        {
            var invokedExpression = Expression.Invoke(secondExpression, firstExpression.Parameters);
            var combinedExpression = Expression.AndAlso(firstExpression.Body, invokedExpression);
            return Expression.Lambda<Func<T, bool>>(combinedExpression, firstExpression.Parameters);
        }
    }
}