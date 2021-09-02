namespace CommonServices.Models.Dto.Http
{
    public class PaginationRequestModel
    {
        public PaginationRequestModel(int page = 1, int limit = 10)
        {
            Page = page;
            Limit = limit > 100 ? limit == int.MaxValue ? int.MaxValue : 100 : limit;
        }

        public int Page { get; }
        public int Limit { get; }
        public string QueryParams { get => string.Intern($"page={Page}&limit={Limit}"); }
    }
}