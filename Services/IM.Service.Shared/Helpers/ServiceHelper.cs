using Microsoft.Extensions.Logging;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace IM.Service.Shared.Helpers;

public static class ServiceHelper
{
    public static class ExpressionHelper
    {
        public static Expression<Func<T, bool>> Combine<T>(Expression<Func<T, bool>> firstExpression, Expression<Func<T, bool>> secondExpression)
        {
            var invokedExpression = Expression.Invoke(secondExpression, firstExpression.Parameters);
            var combinedExpression = Expression.AndAlso(firstExpression.Body, invokedExpression);
            return Expression.Lambda<Func<T, bool>>(combinedExpression, firstExpression.Parameters);
        }
    }
    public class SettingsConverter<T> where T : class
    {
        private readonly Dictionary<string, string> environments;
        public SettingsConverter(string environmentValue)
        {
            try
            {
                environments = environmentValue
                    .Split(';')
                    .Select(x => x.Split('='))
                    .ToDictionary(x => x[0], y => y[1]);

                Model = Activator.CreateInstance<T>();

                Convert(Model);
            }
            catch (Exception exception)
            {
                throw new KeyNotFoundException("SettingConverter error: " + exception.Message);
            }
        }
        private void Convert(T model)
        {
            var modelProperties = typeof(T).GetProperties();

            foreach (var propertyInfo in modelProperties)
            {
                var propName = string.Intern(propertyInfo.Name);

                if (!environments.TryGetValue(propName, out var value))
                    continue;

                var type = model.GetType();
                var property = type.GetProperty(propName);

                property?.SetValue(model, value);
            }
        }
        public T Model { get; }
    }
    public class Paginatior
    {
        public Paginatior(int page, int limit)
        {
            Page = page <= 0 ? 1 : page >= int.MaxValue ? int.MaxValue : page;
            Limit = limit <= 0 ? 10 : limit > 500 ? limit == int.MaxValue ? int.MaxValue : 500 : limit;
        }

        public int Page { get; }
        public int Limit { get; }
        public string QueryParams => $"page={Page}&limit={Limit}";

        public T[] GetPaginatedResult<T>(IEnumerable<T>? collection) where T : class => collection is not null ? collection.Skip((Page - 1) * Limit).Take(Limit).ToArray() : Array.Empty<T>();
    }
    public static class TaskHelper
    {
        public static async Task WhenAny(string methodName, ILogger logger, List<Task> tasks)
        {
            while (tasks.Any())
            {
                try
                {
                    var finishedTask = await Task.WhenAny(tasks);
                    tasks.Remove(finishedTask);

                    if (finishedTask.Exception?.InnerException is not null)
                        throw new ApplicationException(finishedTask.Exception.InnerException.Message);
                }
                catch (Exception exception)
                {
                    logger.LogError(methodName, exception);
                }
            }
        }
    }
}