namespace IM.Services.Analyzer.Api.Models.Http
{
    public class ResponseModel<T> where T : class
    {
        public T? Data { get; set; }
        public string[]? Errors { get; set; }
    }
}
