namespace IM.Service.Portfolio;

public static class Enums
{
    public enum Brokers : byte
    {
        Default = 0,
        Bcs = 1,
        Tinkoff = 2
    }
    public enum UnderlyingAssetTypes
    {
        Default = 0,
        Stock = 1,
        Bond = 2,
        ETF = 3,
        Currency = 4
    }
    public enum OperationTypes : byte
    {
        Default = 0,
        Приход = 1,
        Расход = 2
    }
    public enum EventTypes : byte
    {
        Default = 0,
        Пополнение_счета = 1,
        Дополнительный_выпуск_акции = 2,
        Дивиденд = 3,
        Вывод_с_счета = 4,
        Делистинг_акции = 5,
        Налог_с_дивиденда = 6,
        НДФЛ = 7,
        Комиссия_брокера = 8,
        Комиссия_депозитария = 9
    }
}