namespace IM.Services.Companies.Prices.Api.Models
{
    public class PaginationRequestModel
    {
        public PaginationRequestModel(int page = 1, int limit = 10)
        {
            Page = page;
            Limit = limit > 100 ? 100 : limit;
        }

        public int Page { get; }
        public int Limit { get; }
    }
}