namespace IM.Service.Portfolio;

public static class Enums
{
    public enum Providers
    {
        Default = int.MaxValue,
        Safe = 1,
        Bcs = 2,
        Tinkoff = 3,
        Vtb = 4,
        LedgerNanoSPlus= 5,
        JetLend = 6
    }
    public enum OperationTypes : byte
    {
        Default = 255,
        Income = 1,
        Expense = 2
    }
    public enum EventTypes : byte
    {
        Default = 255,
        Refill = 1,
        Withdraw = 2,
        AddIncome = 3,
        TaxIncome = 4,
        TaxPersonal = 5,
        TaxProvider = 6,
        TaxThirdParty = 7
    }
}