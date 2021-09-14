using System;
using System.Linq;

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

        public T[] GetPaginatedResult<T>(T[]? data) where T : class => data is not null ? data.Skip((Page - 1) * Limit).Take(Limit).ToArray() : Array.Empty<T>();
    }
}