using System;
namespace IM.Service.Common.Net.Models.Dto.Http
{
    public class PaginatedModel<T> where T : class
    {
        public T[] Items { get; init; } = Array.Empty<T>();
        public int Count { get; init; }
    }
}