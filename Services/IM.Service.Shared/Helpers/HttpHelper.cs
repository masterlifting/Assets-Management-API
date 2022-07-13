using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Diagnostics.HealthChecks;

using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using IM.Service.Shared.Models.Service;
using static IM.Service.Shared.Helpers.ServiceHelper;
using IM.Service.Shared.Models.Configuration;

namespace IM.Service.Shared.Helpers;

public static class HttpHelper
{
    public class HealthCheck : IHealthCheck
    {
        private readonly string host;
        protected HealthCheck(string host) => this.host = host;

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            Ping ping = new();
            var reply = await ping.SendPingAsync(host).ConfigureAwait(false);

            return reply.Status != IPStatus.Success ? HealthCheckResult.Unhealthy() : HealthCheckResult.Healthy();
        }
    }
    public abstract class RestClient : ControllerBase
    {
        private readonly IMemoryCache cache;
        private readonly HttpClient httpClient;
        private readonly string baseUri;
        private readonly StringBuilder uriBuilder;
        private readonly SemaphoreSlim semaphore = new(1, 1);

        protected RestClient(IMemoryCache cache, HttpClient httpClient, HostModel settings)
        {
            this.cache = cache;
            this.httpClient = httpClient;
            uriBuilder = new StringBuilder();
            uriBuilder.Append(settings.Schema);
            uriBuilder.Append("://");
            uriBuilder.Append(settings.Host);
            uriBuilder.Append(':');
            uriBuilder.Append(settings.Port);
            baseUri = uriBuilder.ToString();
        }

        public async Task<PaginationModel<TGet>> Get<TGet>(string controller, string? queryString, Paginatior pagination, bool toCache = false) where TGet : class
        {
            if (toCache)
                await semaphore.WaitAsync().ConfigureAwait(false);

            uriBuilder.Clear();
            uriBuilder.Append(baseUri);

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

            PaginationModel<TGet>? result;

            if (toCache)
            {
                result = await cache
                .GetOrCreateAsync(uri, async entry =>
                {
                    entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(10);
                    return await httpClient.GetFromJsonAsync<PaginationModel<TGet>?>(uri, JsonHelper.Options).ConfigureAwait(false);
                })
                .ConfigureAwait(false);

                semaphore.Release();
            }
            else
                result = await httpClient.GetFromJsonAsync<PaginationModel<TGet>?>(uri, JsonHelper.Options).ConfigureAwait(false);

            uriBuilder.Clear();

            return result ?? throw new NullReferenceException(nameof(result));
        }
        public async Task<PaginationModel<TGet>> Get<TGet>(string queryString, Paginatior pagination, bool toCache = false) where TGet : class
        {
            if (toCache)
                await semaphore.WaitAsync().ConfigureAwait(false);

            uriBuilder.Clear();
            uriBuilder.Append(baseUri);

            uriBuilder.Append('/');
            uriBuilder.Append(queryString);
            uriBuilder.Append('&');
            uriBuilder.Append(pagination.QueryParams);


            var uri = uriBuilder.ToString();

            PaginationModel<TGet>? result;

            if (toCache)
            {
                result = await cache
                    .GetOrCreateAsync(uri, async entry =>
                    {
                        entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(10);
                        return await httpClient.GetFromJsonAsync<PaginationModel<TGet>?>(uri, JsonHelper.Options).ConfigureAwait(false);
                    })
                    .ConfigureAwait(false);

                semaphore.Release();
            }
            else
                result = await httpClient.GetFromJsonAsync<PaginationModel<TGet>?>(uri, JsonHelper.Options).ConfigureAwait(false);

            uriBuilder.Clear();

            return result ?? throw new NullReferenceException(nameof(result));
        }

        public async Task<IActionResult> Post<TPost>(string controller, TPost model) where TPost : class
        {
            uriBuilder.Clear();
            uriBuilder.Append(baseUri);

            uriBuilder.Append('/');
            uriBuilder.Append(controller);

            var uri = uriBuilder.ToString();

            HttpResponseMessage? response = null;
            string? error = null;

            try
            {
                response = await httpClient.PostAsJsonAsync(uri, model, JsonHelper.Options).ConfigureAwait(false);
            }
            catch (Exception exception)
            {
                error = exception.Message;
            }

            uriBuilder.Clear();

            return error is null
                ? response is not null
                    ? response.IsSuccessStatusCode
                        ? Ok(model)
                        : BadRequest(response.ReasonPhrase)
                    : BadRequest("response is null")
                : BadRequest(error);
        }
        public async Task<IActionResult> Put<TPost>(string controller, TPost model, params object[] parameters) where TPost : class
        {
            uriBuilder.Clear();
            uriBuilder.Append(baseUri);

            uriBuilder.Append('/');
            uriBuilder.Append(controller);

            var uri = GetUriByQueryParams(parameters);

            HttpResponseMessage? response = null;
            string? error = null;

            try
            {
                response = await httpClient.PutAsJsonAsync(uri, model, JsonHelper.Options).ConfigureAwait(false);
            }
            catch (Exception exception)
            {
                error = exception.Message;
            }

            uriBuilder.Clear();

            return error is null
                ? response is not null
                    ? response.IsSuccessStatusCode
                        ? Ok(model)
                        : BadRequest(response.ReasonPhrase)
                    : BadRequest("response is null")
                : BadRequest(error);
        }
        public async Task<IActionResult> Delete(string controller, params object[] parameters)
        {
            uriBuilder.Clear();
            uriBuilder.Append(baseUri);

            uriBuilder.Append('/');
            uriBuilder.Append(controller);

            var uri = GetUriByQueryParams(parameters);

            HttpResponseMessage? response = null;
            string? error = null;

            try
            {
                response = await httpClient.DeleteAsync(uri).ConfigureAwait(false);
            }
            catch (Exception exception)
            {
                error = exception.Message;
            }

            uriBuilder.Clear();

            return error is null
                ? response is not null
                    ? response.IsSuccessStatusCode
                        ? Ok(parameters)
                        : BadRequest(response.ReasonPhrase)
                    : BadRequest("response is null")
                : BadRequest(error);
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
    }
}