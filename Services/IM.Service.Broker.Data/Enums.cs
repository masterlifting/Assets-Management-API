namespace IM.Service.Broker.Data;

public static class Enums
{
    public enum Brokers : byte
    {
        Bcs = 1,
        Tinkoff = 2
    }
    public enum Exchanges : byte
    {
        Spb = 1,
        Moex = 2
    }

    public enum TransactionActionTypes : byte
    {
        Приход = 1,
        Расход = 2,
        Перемещение = 3
    }

    public enum TransactionActions : byte
    {
        Ввод_средств = 1,
        Вывод_средств = 2,
        Покупка_акции = 3,
        Продажа_акции = 4,
        Выделение_акции = 5,
        Делистинг_акции = 6,
        Сплит_акции = 7,
        Покупка_валюты = 8,
        Продажа_валюты = 9,
        Поступление_дивиденда = 10,
        Удержание_налога_с_дивиденда = 11,
        Удержание_налога_НДФЛ = 12,
        Комиссия_брокера = 13,
        Комиссия_депозитария = 14
    }
    public enum Currencies : byte
    {
        Rub = 1,
        Usd = 2,
        Eur = 3
    }
}