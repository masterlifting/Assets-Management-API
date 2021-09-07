using CommonServices.Models.Dto;
using CommonServices.Models.Dto.AnalyzerService;
using CommonServices.Models.Dto.Http;

using IM.Gateways.Web.Companies.Api.Models.Dto;
using IM.Gateways.Web.Companies.Api.Services.CompanyServices;
using IM.Gateways.Web.Companies.Api.Services.DtoServices;

using Microsoft.AspNetCore.Mvc;

using System.Threading.Tasks;

namespace IM.Gateways.Web.Companies.Api.Controllers
{
    [ApiController, Route("api/[controller]")]
    public class CompaniesController : Controller
    {
        private readonly CompanyDtoAgregator agregator;
        private readonly CompanyManager manager;

        public CompaniesController(CompanyDtoAgregator agregator, CompanyManager manager)
        {
            this.agregator = agregator;
            this.manager = manager;
        }

        public async Task<ResponseModel<PaginationResponseModel<CompanyGetDto>>> Get(int page = 1, int limit = 10) =>
            await agregator.GetCompaniesAsync(new(page, limit));

        [HttpGet("{ticker}")]
        public async Task<ResponseModel<CompanyGetDto>> Get(string ticker) => await agregator.GetCompanyAsync(ticker);
        [HttpPost]
        public async Task<ResponseModel<string>> Post(CompanyPostDto company) => await manager.CreateCompanyAsync(company);

        [HttpPut("{ticker}")]
        public async Task<ResponseModel<string>> Put(string ticker, CompanyPostDto company) => await manager.UpdateCompanyAsync(ticker, company);
        [HttpDelete("{ticker}")]
        public async Task<ResponseModel<string>> Delete(string ticker) => await manager.DeleteCompanyAsync(ticker);


        [HttpGet("prices/")]
        public async Task<ResponseModel<PaginationResponseModel<PriceDto>>> GetLastPrices(int page = 1, int limit = 10) =>
           await agregator.PricesDtoAgregator.GetPricesAsync(new(page, limit));
        [HttpGet("{ticker}/prices/")]
        public async Task<ResponseModel<PaginationResponseModel<PriceDto>>> GetHistoryPrices(string ticker, int page = 1, int limit = 10) =>
            await agregator.PricesDtoAgregator.GetPricesAsync(ticker, new(page, limit));


        [HttpGet("reports/")]
        public async Task<ResponseModel<PaginationResponseModel<ReportDto>>> GetLastReports(int page = 1, int limit = 10) =>
            await agregator.ReportsDtoAgregator.GetReportsAsync(new(page, limit));
        [HttpGet("{ticker}/reports/")]
        public async Task<ResponseModel<PaginationResponseModel<ReportDto>>> GetHistoryReports(string ticker, int page = 1, int limit = 10) =>
            await agregator.ReportsDtoAgregator.GetReportsAsync(ticker, new(page, limit));


        [HttpGet("recommendations/")]
        public async Task<ResponseModel<PaginationResponseModel<AnalyzerRecommendationDto>>> GetRecommendations(int page = 1, int limit = 10) =>
           await agregator.AnalyzerDtoAgregator.GetRecommendationsAsync(new(page, limit));
        [HttpGet("{ticker}/recommendation/")]
        public async Task<ResponseModel<AnalyzerRecommendationDto>> GetRecommendation(string ticker) => await agregator.AnalyzerDtoAgregator.GetRecommendationAsync(ticker);


        [HttpGet("ratings/")]
        public async Task<ResponseModel<PaginationResponseModel<AnalyzerRatingDto>>> GetRatings(int page = 1, int limit = 10) => await agregator.AnalyzerDtoAgregator.GetRatingsAsync(new(page, limit));
        [HttpGet("{ticker}/rating/")]
        public async Task<ResponseModel<AnalyzerRatingDto>> GetRating(string ticker) => await agregator.AnalyzerDtoAgregator.GetRatingAsync(ticker);


        [HttpGet("{ticker}/coefficients/")]
        public async Task<ResponseModel<PaginationResponseModel<AnalyzerCoefficientDto>>> GetCoefficients(string ticker, int page = 1, int limit = 10) =>
            await agregator.AnalyzerDtoAgregator.GetCoefficientsAsync(ticker, new(page, limit));
    }
}
