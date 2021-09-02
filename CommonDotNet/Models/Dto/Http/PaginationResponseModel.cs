using System;
namespace CommonServices.Models.Dto.Http
{
    public class PaginationResponseModel<T> where T : class
    {
        public T[] Items { get; set; } = Array.Empty<T>();
        public int Count { get; set; }
    }
}