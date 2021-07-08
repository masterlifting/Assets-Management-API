namespace IM.Services.Companies.Reports.Api.Models
{
    public class ResponseModel<T> where T : class
    {
        public T Data{get;set;}
        public string[] Errors{get;set;}
    }
}