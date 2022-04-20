using IM.Service.Common.Net.Helpers;
using IM.Service.Common.Net.Models.Dto.Http;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace IM.Service.Common.Net.HttpServices;

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

    public async Task<ResponseModel<PaginatedModel<TGet>>> Get<TGet>(string controller, string? queryString, HttpPagination pagination, bool toCache = false) where TGet : class
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

        ResponseModel<PaginatedModel<TGet>>? response;

        try
        {
            if (toCache)
            {
                response = await cache
                .GetOrCreateAsync(uri, async entry =>
                    {
                        entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(10);
                        return await httpClient.GetFromJsonAsync<ResponseModel<PaginatedModel<TGet>>?>(uri, JsonHelper.Options).ConfigureAwait(false);
                    })
                .ConfigureAwait(false);

                semaphore.Release();
            }
            else
                response = await httpClient.GetFromJsonAsync<ResponseModel<PaginatedModel<TGet>>?>(uri, JsonHelper.Options).ConfigureAwait(false);
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
                    : BadRequest(response.ToString())
                : NotFound("response is null" )
            : BadRequest( error );
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
                    : BadRequest(response.ToString())
                : NotFound("response is null")
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
                    : BadRequest(response.ToString())
                : NotFound("response is null")
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