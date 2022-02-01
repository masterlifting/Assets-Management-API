namespace IM.Service.Broker.Data;

public static class Enums
{
    public enum Brokers : byte
    {
        Default = 0,
        Bcs = 1,
        Tinkoff = 2
    }
    public enum Exchanges : byte
    {
        Default = 0,
        Spb = 1,
        Moex = 2
    }
    public enum TransactionActionTypes : byte
    {
        Default = 0,
        Приход = 1,
        Расход = 2
    }
    public enum TransactionActions : byte
    {
        Default = 0,
        Пополнение_счета = 1,
        Продажа_валюты = 2,
        Продажа_акции = 3,
        Выделение_акции = 4,
        Дивиденд = 5,

        Вывод_с_счета = 6,
        Покупка_валюты = 7,
        Покупка_акции = 8,
        Делистинг_акции = 9,
        Налог_с_дивиденда = 10,
        НДФЛ = 11,
        Комиссия_брокера = 12,
        Комиссия_депозитария = 13
    }

    public enum Currencies : byte
    {
        Default = 0,
        Rub = 1,
        Usd = 2,
        Eur = 3
    }
}