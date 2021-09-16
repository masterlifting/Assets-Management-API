using CommonServices.Models.Dto;
using CommonServices.Models.Dto.CompanyAnalyzer;
using CommonServices.Models.Dto.Http;

using IM.Gateway.Companies.Models.Dto;
using IM.Gateway.Companies.Services.DtoServices;

using Microsoft.AspNetCore.Mvc;

using System.Threading.Tasks;

namespace IM.Gateway.Companies.Controllers
{
    [ApiController, Route("api/[controller]")]
    public class CompaniesController : Controller
    {
        private readonly CompanyDtoAggregator aggregator;
        public CompaniesController(CompanyDtoAggregator aggregator) => this.aggregator = aggregator;

        public async Task<ResponseModel<PaginationResponseModel<CompanyGetDto>>> Get(int page = 0, int limit = 0) =>
            await aggregator.GetAsync((PaginationRequestModel) new(page, limit));

        [HttpGet("{ticker}")]
        public async Task<ResponseModel<CompanyGetDto>> Get(string ticker) => await aggregator.GetAsync(ticker);
        [HttpPost]
        public async Task<ResponseModel<string>> Post(CompanyPostDto company) => await aggregator.CreateAsync(company);
        [HttpPut("{ticker}")]
        public async Task<ResponseModel<string>> Put(string ticker, CompanyPostDto company) => await aggregator.UpdateAsync(ticker, company);
        [HttpDelete("{ticker}")]
        public async Task<ResponseModel<string>> Delete(string ticker) => await aggregator.DeleteAsync(ticker);


        [HttpGet("prices/")]
        public async Task<ResponseModel<PaginationResponseModel<PriceDto>>> GetLastPrices(int year = 0, int month = 0, int day = 0, int page = 0, int limit = 0) =>
           await aggregator.PricesDtoAggregator.GetPricesAsync(new(year, month, day), new(page, limit));
        [HttpGet("{ticker}/prices/")]
        public async Task<ResponseModel<PaginationResponseModel<PriceDto>>> GetHistoryPrices(string ticker, int year = 0, int month = 0, int day = 0, int page = 0, int limit = 0) =>
            await aggregator.PricesDtoAggregator.GetPricesAsync(ticker, new(year, month, day), new(page, limit));


        [HttpGet("reports/")]
        public async Task<ResponseModel<PaginationResponseModel<ReportDto>>> GetLastReports(int year = 0, byte quarter = 0, int page = 0, int limit = 0) =>
            await aggregator.ReportsDtoAggregator.GetReportsAsync(new(year, quarter), new(page, limit));
        [HttpGet("{ticker}/reports/")]
        public async Task<ResponseModel<PaginationResponseModel<ReportDto>>> GetHistoryReports(string ticker, int year = 0, byte quarter = 0, int page = 0, int limit = 0) =>
            await aggregator.ReportsDtoAggregator.GetReportsAsync(ticker, new(year, quarter), new(page, limit));


        [HttpGet("ratings/")]
        public async Task<ResponseModel<PaginationResponseModel<CompanyAnalyzerRatingDto>>> GetRatings(int page = 1, int limit = 10) => await aggregator.AnalyzerDtoAggregator.GetRatingsAsync(new(page, limit));
        [HttpGet("{ticker}/ratings/")]
        public async Task<ResponseModel<CompanyAnalyzerRatingDto>> GetRating(string ticker) => await aggregator.AnalyzerDtoAggregator.GetRatingAsync(ticker);


        [HttpGet("{ticker}/coefficients/")]
        public async Task<ResponseModel<PaginationResponseModel<CompanyAnalyzerCoefficientDto>>> GetCoefficients(string ticker, int page = 1, int limit = 10) =>
            await aggregator.AnalyzerDtoAggregator.GetCoefficientsAsync(ticker, new(page, limit));
    }
}
