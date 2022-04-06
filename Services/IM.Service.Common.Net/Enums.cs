using Microsoft.Extensions.Logging;

namespace IM.Service.Common.Net;

public static class Enums
{
    public enum Exchanges : byte
    {
        Default = 0,
        Spbex = 1,
        Moex = 2,
        Nyse = 3,
        Nasdaq = 4,
        Fwb = 5,
        Hkse = 6,
        Lse = 7,
        Sse = 8
    }
    public enum Currencies : byte
    {
        Default = 0,
        Rub = 1,
        Usd = 2,
        Eur = 3,
        Gbp = 4,
        Chy = 5
    }
    public enum Countries : byte
    {
        Rus = 1,
        Usa = 2,
        Chn = 3,
        Gbr = 4,
        Deu = 5
    }
    public enum RepositoryActions 
    {
        Create,
        Update,
        Delete
    }
    public enum HttpRequestFilterType : byte
    {
        Equal = 1,
        More = 2
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

    public static EventId Function = new(3001, "call function");
    public static EventId Sync = new(3002, "data synchronization");
    public static EventId Transfer = new(3003, "data transfer");

    public static EventId Processing = new(4000, "logic pipeline");

    public static EventId Configuration = new(5000, "configuration");
}