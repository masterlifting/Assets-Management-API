﻿using CommonServices.Models.Dto;
using CommonServices.Models.Dto.Http;

using IM.Services.Analyzer.Api.Settings;
using IM.Services.Analyzer.Api.Settings.Client;

using Microsoft.Extensions.Options;

using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace IM.Services.Analyzer.Api.Clients
{
    public class PricesClient : IDisposable
    {
        private readonly HttpClient httpClient;
        private readonly HostModel settings;

        public PricesClient(HttpClient httpClient, IOptions<ServiceSettings> options)
        {
            this.httpClient = httpClient;
            settings = options.Value.ClientSettings.ClientCompaniesPrices;
        }

        public async Task<ResponseModel<PaginationResponseModel<PriceDto>>> GetPricesAsync(string ticker, FilterRequestModel filter, PaginationRequestModel pagination) =>
            await httpClient.GetFromJsonAsync<ResponseModel<PaginationResponseModel<PriceDto>>>
                ($"{settings.Schema}://{settings.Host}:{settings.Port}/{settings.Controller}/{ticker}/filter?{filter.QueryParams}&{pagination.QueryParams}") ?? new();

        public void Dispose()
        {
            httpClient.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
