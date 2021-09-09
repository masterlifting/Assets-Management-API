namespace CommonServices.Models.Dto.Http
{
    public class PaginationRequestModel
    {
        public PaginationRequestModel(int page, int limit)
        {
            Page = page <= 0 ? 1 : page >= int.MaxValue ? int.MaxValue : page;
            Limit = limit <= 0 ? 10 : limit > 100 ? limit == int.MaxValue ? int.MaxValue : 100 : limit;
        }

        public int Page { get; }
        public int Limit { get; }
        public string QueryParams => $"page={Page}&limit={Limit}";
    }
}