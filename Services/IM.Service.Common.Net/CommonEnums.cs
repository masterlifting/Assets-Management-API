using Microsoft.Extensions.Logging;

namespace IM.Service.Common.Net;

public static class CommonEnums
{
    public enum RepositoryActions 
    {
        Create,
        CreateUpdate,
        CreateUpdateDelete,
        Update,
        Delete
    }
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
    public static EventId Read = new(2000, "read");
    public static EventId Create = new(2001, "create");
    public static EventId Update = new(2002, "update");
    public static EventId Delete = new(2003, "delete");
    public static EventId CreateUpdate = new(2012, "create or update");
    public static EventId CreateUpdateDelete = new(2234, "create or update and delete");

    public static EventId Call = new(3001, "call function");
    public static EventId Sync = new(3002, "data synchronization");
    public static EventId Transfer = new(3003, "data transfer");

    public static EventId Processing = new(4000, "logic pipeline");

    public static EventId Configuration = new(5000, "configuration");
}