using Microsoft.Extensions.Logging;

namespace IM.Service.Common.Net;

public static class CommonEnums
{
    public enum HttpRequestFilterType : byte
    {
        Equal = 1,
        More = 2
    }
    public enum FilterDateEqualType
    {
        Year,
        YearMonth,
        YearMonthDay
    }
    public enum FilterQuarterEqualType
    {
        Year,
        YearQuarter
    }
}
public static class LogEvents
{
    public static EventId Get = new(2000, "data get");
    public static EventId Create = new(2002, "data create");
    public static EventId Update = new(2003, "data update");
    public static EventId Remove = new(2004, "data delete");
    public static EventId CreateUpdate = new(2023, "data create and update");
    public static EventId CreateUpdateDelete = new(2234, "data create and update and delete");
    public static EventId ReCreate = new(2042, "data recreate");

    public static EventId QueueConfig = new(3000, "queue configuration");
    public static EventId Call = new(3001, "queue call function");
    public static EventId Sync = new(3002, "queue data synchronization");
    public static EventId Transfer = new(3003, "queue data transfer");

    public static EventId Processing = new(4000, "logic pipeline");

    public static EventId Test = new(5000, "test");
}