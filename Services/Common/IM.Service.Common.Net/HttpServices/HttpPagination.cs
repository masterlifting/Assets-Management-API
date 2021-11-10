using System;
using System.Collections.Generic;
using System.Linq;

namespace IM.Service.Common.Net.HttpServices
{
    public class HttpPagination
    {
        public HttpPagination(int page, int limit)
        {
            Page = page <= 0 ? 1 : page >= int.MaxValue ? int.MaxValue : page;
            Limit = limit <= 0 ? 10 : limit > 100 ? limit == int.MaxValue ? int.MaxValue : 100 : limit;
        }

        public int Page { get; }
        public int Limit { get; }
        public string QueryParams => $"page={Page}&limit={Limit}";

        public T[] GetPaginatedResult<T>(IEnumerable<T>? collection) where T : class => collection is not null ? collection.Skip((Page - 1) * Limit).Take(Limit).ToArray() : Array.Empty<T>();
    }
}