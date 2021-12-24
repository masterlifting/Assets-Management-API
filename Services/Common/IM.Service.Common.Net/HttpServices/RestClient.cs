using IM.Service.Common.Net.Models.Dto.Http;

using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;

namespace IM.Service.Common.Net.HttpServices;

public abstract class RestClient
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

    public async Task<ResponseModel<PaginatedModel<TGet>>> Get<TGet>(string controller, string? queryString, HttpPagination pagination, bool toCache = false) where TGet : class
    {
        if (toCache)
            await semaphore.WaitAsync();

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

        ResponseModel<PaginatedModel<TGet>>? response;

        try
        {
            if (toCache)
            {
                response = await cache.GetOrCreateAsync(uri, async entry =>
                {
                    entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(10);
                    return await httpClient.GetFromJsonAsync<ResponseModel<PaginatedModel<TGet>>?>(uri);
                });

                semaphore.Release();
            }
            else
                response = await httpClient.GetFromJsonAsync<ResponseModel<PaginatedModel<TGet>>?>(uri);
        }
        catch (Exception ex)
        {
            response = new()
            {
                Data = new PaginatedModel<TGet>(),
                Errors = new[] { ex.Message }
            };
        }

        uriBuilder.Clear();

        return response ?? new()
        {
            Data = new PaginatedModel<TGet>(),
            Errors = new[] { "get response is null" }
        };
    }
    public async Task<ResponseModel<string>> Post<TPost>(string controller, TPost model) where TPost : class
    {
        uriBuilder.Clear();
        uriBuilder.Append(baseUri);

        uriBuilder.Append('/');
        uriBuilder.Append(controller);

        var uri = uriBuilder.ToString();

        HttpResponseMessage? response = null;
        ResponseModel<string>? result = null;

        try
        {
            response = await httpClient.PostAsJsonAsync(uri, model);
        }
        catch (Exception ex)
        {
            result = new()
            {
                Errors = new[] { ex.Message }
            };
        }

        uriBuilder.Clear();

        result = result is null
            ? response is not null
                ? response.IsSuccessStatusCode
                    ? new() { Data = "post response is success" }
                    : new()
                    {
                        Errors = new[]
                        {
                            response.ToString()
                        }
                    }
                : new() { Errors = new[] { "post response is null" } }
            : new() { Errors = new[] { "post response failed" } };

        return result;
    }
    public async Task<ResponseModel<string>> Put<TPost>(string controller, TPost model, params object[] parameters) where TPost : class
    {
        uriBuilder.Clear();
        uriBuilder.Append(baseUri);

        uriBuilder.Append('/');
        uriBuilder.Append(controller);

        var uri = GetUriByQueryParams(parameters);

        HttpResponseMessage? response = null;
        ResponseModel<string>? result = null;

        try
        {
            response = await httpClient.PutAsJsonAsync(uri, model);
        }
        catch (Exception ex)
        {
            result = new()
            {
                Errors = new[] { ex.Message }
            };
        }

        uriBuilder.Clear();

        result = result is null
            ? response is not null
                ? response.IsSuccessStatusCode
                    ? new() { Data = "put response is success" }
                    : new() { Errors = new[] { response.ToString() } }
                : new() { Errors = new[] { "put response is null" } }
            : new() { Errors = new[] { "put response failed" } };

        return result;
    }
    public async Task<ResponseModel<string>> Delete(string controller, params object[] parameters)
    {
        uriBuilder.Clear();
        uriBuilder.Append(baseUri);

        uriBuilder.Append('/');
        uriBuilder.Append(controller);

        var uri = GetUriByQueryParams(parameters);

        HttpResponseMessage? response = null;
        ResponseModel<string>? result = null;

        try
        {
            response = await httpClient.DeleteAsync(uri);
        }
        catch (Exception ex)
        {
            result = new()
            {
                Errors = new[] { ex.Message }
            };
        }

        uriBuilder.Clear();

        result = result is null
            ? response is not null
                ? response.IsSuccessStatusCode
                    ? new() { Data = "delete response is success" }
                    : new() { Errors = new[] { response.ToString() } }
                : new() { Errors = new[] { "delete response is null" } }
            : new() { Errors = new[] { "delete response failed" } };

        return result;
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