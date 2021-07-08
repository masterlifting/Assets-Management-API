using IM.Services.Analyzer.Api.Models.Http;
using IM.Services.Analyzer.Api.Models.Dto;
using IM.Services.Analyzer.Api.Services.Agregators.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace IM.Services.Analyzer.Api.Controllers
{
    [ApiController, Route("[controller]")]
    public class CoefficientsController : Controller
    {
        private readonly ICoefficientDtoAgregator agregator;
        public CoefficientsController(ICoefficientDtoAgregator agregator) => this.agregator = agregator;

        [HttpGet("{ticker}")]
        public async Task<ResponseModel<PaginationResponseModel<CoefficientDto>>> Get(string ticker, int page = 1, int limit = 10) =>
            await agregator.GetCoefficientsAsync(ticker, new(page, limit));
    }
}
