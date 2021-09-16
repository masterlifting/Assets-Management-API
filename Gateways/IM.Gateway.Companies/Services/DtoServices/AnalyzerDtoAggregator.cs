using CommonServices.Models.Dto.CompanyAnalyzer;
using CommonServices.Models.Dto.Http;
using System.Threading.Tasks;
using IM.Gateway.Companies.Clients;

namespace IM.Gateway.Companies.Services.DtoServices
{
    public class AnalyzerDtoAggregator
    {
        private readonly AnalyzerClient httpClient;
        public AnalyzerDtoAggregator(AnalyzerClient httpClient) => this.httpClient = httpClient;

        public Task<ResponseModel<PaginationResponseModel<CompanyAnalyzerRatingDto>>> GetRatingsAsync(PaginationRequestModel pagination) =>
            httpClient.GetRatingsAsync(pagination);
        public Task<ResponseModel<CompanyAnalyzerRatingDto>> GetRatingAsync(string ticker) => httpClient.GetRatingAsync(ticker);

        public Task<ResponseModel<PaginationResponseModel<CompanyAnalyzerCoefficientDto>>> GetCoefficientsAsync(string ticker, PaginationRequestModel pagination) =>
            httpClient.GetCoefficientsAsync(ticker, pagination);
    }
}
