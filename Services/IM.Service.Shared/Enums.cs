namespace IM.Service.Shared;

public static class Enums
{
    public enum AssetTypes : byte
    {
        Default = 255,
        Valuable = 1,
        Currency = 2,
        Stock = 3,
        Bond = 4,
        ETF = 5,
        CryptoCurrency = 6,
        NFT = 7,
        RealEstate = 8,
        PersonalEstate = 9,
        Crowdlending = 10,
        Crowdfunding = 11,
        Venture = 12
    }
    public enum Exchanges : byte
    {
        Default = 255,
        Spbex = 1,
        Moex = 2,
        Nyse = 3,
        Nasdaq = 4,
        Fwb = 5,
        Hkse = 6,
        Lse = 7,
        Sse = 8,
        Binance = 9,
        FTX2 = 10,
        Coinbase = 11
    }
    public enum Currencies : byte
    {
        Default = 255,
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
        Default = 255,
        Rus = 1,
        Usa = 2,
        Chn = 3,
        Gbr = 4,
        Deu = 5,
        Che = 6,
        Jpn = 7
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
