namespace CommonServices
{
    public static class CommonEnums
    {
        public enum RepositoryActionType
        {
            create,
            update,
            delete
        }
        public enum PriceSourceTypes : byte
        {
            MOEX = 1,
            Tdameritrade = 2
        }
        public enum ReportSourceTypes : byte
        {
            Official = 1,
            Investing = 2
        }
    }
}
