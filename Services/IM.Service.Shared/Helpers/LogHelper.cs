using Microsoft.Extensions.Logging;

using System;
using System.Collections.Generic;

namespace IM.Service.Shared.Helpers;

public static class LogHelper
{
    private static readonly Action<ILogger, DateTime, string, string, Exception?> logError =
         LoggerMessage.Define<DateTime, string, string>(LogLevel.Error, LogEvents.Defined, "[{time:yyyy-MM-dd HH:mm:ss}] {method}: {errors}");
    private static readonly Action<ILogger, DateTime, string, string, object, Exception?> logWarning =
        LoggerMessage.Define<DateTime, string, string, object>(LogLevel.Warning, LogEvents.Defined, "[{time:yyyy-MM-dd HH:mm:ss}] {method}: {info} - {result}");
    private static readonly Action<ILogger, DateTime, string, string, object, Exception?> logInfo =
        LoggerMessage.Define<DateTime, string, string, object>(LogLevel.Information, LogEvents.Defined, "[{time:yyyy-MM-dd HH:mm:ss}] {method}: {info} - {result}");
    private static readonly Action<ILogger, DateTime, string, string, object, Exception?> logDebug =
        LoggerMessage.Define<DateTime, string, string, object>(LogLevel.Debug, LogEvents.Defined, "[{time:yyyy-MM-dd HH:mm:ss}] {method}: {info} - {result}");
    private static readonly Action<ILogger, DateTime, string, string, object, Exception?> logTrace =
        LoggerMessage.Define<DateTime, string, string, object>(LogLevel.Trace, LogEvents.Defined, "[{time:yyyy-MM-dd HH:mm:ss}] {method}: {info} - {result}");


    public static void LogError(this ILogger logger, string method, IEnumerable<string> errors) => logError(logger, DateTime.Now, method, string.Join("\n\t- ", errors), null);
    public static void LogError(this ILogger logger, string method, Exception exception) => logError(logger, DateTime.Now, method, exception.InnerException?.Message ?? exception.Message, null);
    public static void LogError<T>(this ILogger logger, string method, Exception exception) => logError(logger, DateTime.Now, $"{typeof(T).Name}.{method}", exception.InnerException?.Message ?? exception.Message, null);
    public static void LogError(this ILogger logger, string method, string error) => logError(logger, DateTime.Now, method, error, null);
    public static void LogWarning(this ILogger logger, string method, string info, object result) => logWarning(logger, DateTime.Now, method, info, result, null);
    public static void LogInfo(this ILogger logger, string method, string info, object result) => logInfo(logger, DateTime.Now, method, info, result, null);
    public static void LogInfo<T>(this ILogger logger, string method, string info, object result) => logInfo(logger, DateTime.Now, $"{typeof(T).Name}.{method}", info, result, null);
    public static void LogDebug(this ILogger logger, string method, string info, object result) => logDebug(logger, DateTime.Now, method, info, result, null);
    public static void LogTrace(this ILogger logger, string method, string info, object result) => logTrace(logger, DateTime.Now, method, info, result, null);

    private static class LogEvents
    {
        public static readonly EventId Defined = new(1000, "defined log");
    }
}