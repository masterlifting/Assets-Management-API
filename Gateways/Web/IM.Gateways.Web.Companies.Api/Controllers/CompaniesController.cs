using IM.Gateways.Web.Companies.Api.Models.Dto;
using IM.Gateways.Web.Companies.Api.Models.Http;
using IM.Gateways.Web.Companies.Api.Services.Agregators.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace IM.Gateways.Web.Companies.Api.Controllers
{
    [ApiController, Route("api/[controller]")]
    public class CompaniesController : Controller
    {
        private readonly ICompaniesDtoAgregator agregator;
        public CompaniesController(ICompaniesDtoAgregator agregator) => this.agregator = agregator;

        public async Task<ResponseModel<PaginationResponseModel<CompanyDto>>> Get(int page = 1, int limit = 10) =>
            await agregator.GetCompaniesAsync(new(page, limit));

        [HttpGet("{ticker}")]
        public async Task<ResponseModel<CompanyDto>> Get(string ticker) => await agregator.GetCompanyAsync(ticker);
        [HttpDelete("{ticker}")]
        public async Task<ResponseModel<string>> Delete(string ticker) => await agregator.DeleteCompanyAsync(ticker);

        
        [HttpGet("prices/last/")]
        public async Task<ResponseModel<PaginationResponseModel<PriceDto>>> GetLastPrices(int page = 1, int limit = 10) =>
           await agregator.PricesDtoAgregators.GetLastPricesAsync(new(page, limit));
        [HttpGet("{ticker}/prices/last/")]
        public async Task<ResponseModel<PriceDto>> GetLastPrice(string ticker) => await agregator.PricesDtoAgregators.GetLastPriceAsync(ticker);
        [HttpGet("{ticker}/prices/history/")]
        public async Task<ResponseModel<PaginationResponseModel<PriceDto>>> GetHistoryPrices(string ticker, int page = 1, int limit = 10) =>
            await agregator.PricesDtoAgregators.GetHistoryPricesAsync(ticker, new(page, limit));
        
        
        [HttpGet("reports/last/")]
        public async Task<ResponseModel<PaginationResponseModel<ReportDto>>> GetLastReports(int page = 1, int limit = 10) =>
            await agregator.ReportsDtoAgregators.GetLastReportsAsync(new(page, limit));
        [HttpGet("{ticker}/reports/last/")]
        public async Task<ResponseModel<ReportDto>> GetLastReport(string ticker) => await agregator.ReportsDtoAgregators.GetLastReportAsync(ticker);
        [HttpGet("{ticker}/reports/history/")]
        public async Task<ResponseModel<PaginationResponseModel<ReportDto>>> GetHistoryReports(string ticker, int page = 1, int limit = 10) =>
            await agregator.ReportsDtoAgregators.GetHistoryReportsAsync(ticker, new(page, limit));
        
        
        [HttpGet("{ticker}/coefficients/")]
        public async Task<ResponseModel<PaginationResponseModel<CoefficientDto>>> GetCoefficients(string ticker, int page = 1, int limit = 10) =>
            await agregator.AnalyzerDtoAgregators.GetCoefficientsAsync(ticker, new(page, limit));
        [HttpGet("ratings/")]
        public async Task<ResponseModel<PaginationResponseModel<RatingDto>>> GetRatings(int page = 1, int limit = 10) => await agregator.AnalyzerDtoAgregators.GetRatingsAsync(new(page, limit));
        [HttpGet("{ticker}/rating/")]
        public async Task<ResponseModel<RatingDto>> GetRating(string ticker) => await agregator.AnalyzerDtoAgregators.GetRatingAsync(ticker);
        [HttpGet("recommendations/")]
        public async Task<ResponseModel<PaginationResponseModel<RecommendationDto>>> GetRecommendations(int page = 1, int limit = 10) =>
           await agregator.AnalyzerDtoAgregators.GetRecommendationsAsync(new(page, limit));
        [HttpGet("{ticker}/recommendation/")]
        public async Task<ResponseModel<RecommendationDto>> GetRecommendation(string ticker) => await agregator.AnalyzerDtoAgregators.GetRecommendationAsync(ticker);
    }
}
