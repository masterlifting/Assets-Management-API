using System;
namespace IM.Services.Companies.Prices.Api.Models
{
    public class PaginationResponseModel<T> where T : class
    {
        public T[] Items{get;set;} = Array.Empty<T>();
        public int Count{get;set;}
    }
}