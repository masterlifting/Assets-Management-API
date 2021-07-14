﻿using IM.Services.Analyzer.Api.Models.Http;
using IM.Services.Analyzer.Api.Models.Dto;
using IM.Services.Analyzer.Api.Services.Agregators.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace IM.Services.Analyzer.Api.Controllers
{
    [ApiController, Route("[controller]")]
    public class RecommendationsController : Controller
    {
        private readonly IRecommendationDtoAgregator agregator;
        public RecommendationsController(IRecommendationDtoAgregator agregator) => this.agregator = agregator;

        public async Task<ResponseModel<PaginationResponseModel<RecommendationDto>>> Get(int page = 1, int limit = 10) => 
            await agregator.GetRecommendationsAsync(new(page, limit));

        [HttpGet("{ticker}")]
        public async Task<ResponseModel<RecommendationDto>> Get(string ticker) => await agregator.GetRecommendationAsync(ticker);
    }
}