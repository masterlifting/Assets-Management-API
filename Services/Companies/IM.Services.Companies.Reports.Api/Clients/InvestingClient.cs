using HtmlAgilityPack;
using IM.Services.Companies.Reports.Api.Settings;
using Microsoft.Extensions.Options;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace IM.Services.Companies.Reports.Api.Clients
{
    public class InvestingClient
    {
        private readonly HttpClient httpClient;
        private readonly InvestingSettings settings;

        public InvestingClient(HttpClient httpClient, IOptions<ServiceSettings> options)
        {
            httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:80.0) Gecko/20100101 Firefox/80.0");
            httpClient.DefaultRequestHeaders.Add("Accept", "application/json, text/html, */*; q=0.01");
            httpClient.DefaultRequestHeaders.Add("Connection", "keep-alive");
            this.httpClient = httpClient;
            settings = options.Value.InvestingSettings;
        }

        public async Task<HtmlDocument> GetMainPageAsync(string value) => await GetHtmlDocumentAsync($"https://{settings.Host}/{settings.Path}/{value}");
        public async Task<HtmlDocument> GetFinancialPageAsync(string value) => await GetHtmlDocumentAsync($"https://{settings.Host}/{settings.Path}/{value}-{settings.Finantial}");
        public async Task<HtmlDocument> GetBalancePageAsync(string value) => await GetHtmlDocumentAsync($"https://{settings.Host}/{settings.Path}/{value}-{settings.Balance}");
        public async Task<HtmlDocument> GetDividendPageAsync(string value) => await GetHtmlDocumentAsync($"https://{settings.Host}/{settings.Path}/{value}-{settings.Dividends}");

        private async Task<HtmlDocument> GetHtmlDocumentAsync(string uri)
        {
            var pageAsString = await httpClient.GetStringAsync(uri);

            var htmlDocument = new HtmlDocument();

            htmlDocument.LoadHtml(pageAsString);

            if (htmlDocument is null)
                throw new NullReferenceException($"Страница по адресу {uri} не была загружена");

            return htmlDocument;
        }
    }
}
