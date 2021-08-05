namespace IM.Services.Companies.Reports.Api.DataAccess
{
    public static class DataEnums
    {
        public enum ReportSourceTypes : byte
        {
            official = 1,
            investing = 2
        }
        public enum RepositoryActionType
        {
            create,
            update,
            delete
        }
    }
}