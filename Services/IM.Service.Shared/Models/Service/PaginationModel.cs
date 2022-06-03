using System;

namespace IM.Service.Shared.Models.Service;

public class PaginationModel<T> where T : class
{
    public T[] Items { get; init; } = Array.Empty<T>();
    public int Count { get; init; }
}