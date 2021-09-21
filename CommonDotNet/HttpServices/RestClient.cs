using CommonServices.Models.Http;

using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace CommonServices.HttpServices
{
    public abstract class RestClient : IDisposable
    {
        private readonly HttpClient httpClient;
        private readonly StringBuilder uriBuilder;

        protected RestClient(HttpClient httpClient, HostModel settings)
        {
            this.httpClient = httpClient;
            uriBuilder = new StringBuilder(300);
            uriBuilder.Append(settings.Schema);
            uriBuilder.Append("://");
            uriBuilder.Append(settings.Host);
            uriBuilder.Append(':');
            uriBuilder.Append(settings.Port);
        }

        public async Task<ResponseModel<PaginatedModel<TGet>>> Get<TGet>(string controller, string? queryString, HttpPagination pagination) where TGet : class
        {
            uriBuilder.Append('/');
            uriBuilder.Append(controller);

            if (queryString is not null)
            {
                uriBuilder.Append(queryString);
                uriBuilder.Append(queryString.Contains('?') ? '&' : '?');
            }
            else
                uriBuilder.Append('?');

            uriBuilder.Append(pagination.QueryParams);

            var uri = uriBuilder.ToString();

            var get = await httpClient.GetFromJsonAsync<ResponseModel<PaginatedModel<TGet>>?>(uri);

            uriBuilder.Clear();

            return get ?? new()
            {
                Data = new PaginatedModel<TGet>(),
                Errors = new[]
                {
                    "get response failed"
                }
            };
        }
        public async Task<ResponseModel<TGet>> Get<TGet>(string controller, params object[] parameters) where TGet : class
        {
            uriBuilder.Append('/');
            uriBuilder.Append(controller);

            var uri = GetUriByQueryParams(parameters);

            var get = await httpClient.GetFromJsonAsync<ResponseModel<TGet>?>(uri);

            return get ?? new()
            {
                Errors = new[]
                {
                    "get response failed"
                }
            };
        }
        public async Task<ResponseModel<string>> Post<TPost>(string controller, TPost model) where TPost : class
        {
            uriBuilder.Append('/');
            uriBuilder.Append(controller);

            var uri = uriBuilder.ToString();

            var post = await httpClient.PostAsJsonAsync(uri, model);

            uriBuilder.Clear();

            var errors = !post.IsSuccessStatusCode ? new[] { "post response error", post.ReasonPhrase } : Array.Empty<string>();

            return new()
            {
                Data = errors.Any() ? null : "post response is success",
                Errors = errors!
            };
        }
        public async Task<ResponseModel<string>> Put<TPost>(string controller, TPost model, params object[] parameters) where TPost : class
        {
            uriBuilder.Append('/');
            uriBuilder.Append(controller);

            var uri = GetUriByQueryParams(parameters);

            var post = await httpClient.PutAsJsonAsync(uri, model);

            var errors = !post.IsSuccessStatusCode ? new[] { "put response error", post.ReasonPhrase } : Array.Empty<string>();

            return new()
            {
                Data = errors.Any() ? null : "put response is success",
                Errors = errors!
            };
        }
        public async Task<ResponseModel<string>> Delete(string controller, params object[] parameters)
        {
            uriBuilder.Append('/');
            uriBuilder.Append(controller);

            var uri = GetUriByQueryParams(parameters);

            var delete = await httpClient.DeleteAsync(uri);
            var errors = !delete.IsSuccessStatusCode ? new[] { "delete response error", delete.ReasonPhrase } : Array.Empty<string>();

            return new()
            {
                Data = errors.Any() ? null : "delete response is success",
                Errors = errors!
            };
        }

        private string GetUriByQueryParams(params object[] parameters)
        {
            foreach (var param in parameters)
            {
                uriBuilder.Append('/');
                uriBuilder.Append(param);
            }

            var result = uriBuilder.ToString();
            uriBuilder.Clear();
            return result;
        }

        public void Dispose()
        {
            httpClient.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
