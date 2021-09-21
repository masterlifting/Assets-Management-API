using System;
namespace CommonServices.Models.Http
{
    public class PaginatedModel<T> where T : class
    {
        public T[] Items { get; init; } = Array.Empty<T>();
        public int Count { get; init; }
    }
}