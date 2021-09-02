using System;

namespace CommonServices.Models.Dto.Http
{
    public class ResponseModel<T> where T : class
    {
        public T? Data { get; set; }
        public string[] Errors { get; set; } = Array.Empty<string>();
    }
}