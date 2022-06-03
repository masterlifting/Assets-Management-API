namespace IM.Service.Shared;

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
        Chy = 5,
        Btc = 6,
        Eth = 7
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
    public enum CompareType : byte
    {
        Equal = 1,
        More = 2
    }
  }
