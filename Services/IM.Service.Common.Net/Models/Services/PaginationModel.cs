using System;

namespace IM.Service.Common.Net.Models.Services;

public class PaginationModel<T> where T : class
{
    public T[] Items { get; init; } = Array.Empty<T>();
    public int Count { get; init; }
}