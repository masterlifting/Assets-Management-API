﻿using IM.Services.Companies.Prices.Api.Models;
using IM.Services.Companies.Prices.Api.Models.Dto;
using IM.Services.Companies.Prices.Api.Services.Agregators.Interfaces;

using Microsoft.AspNetCore.Mvc;

using System.Threading.Tasks;

namespace IM.Services.Companies.Prices.Api.Controllers
{
    [ApiController, Route("[controller]")]
    public class PricesController : Controller
    {
        private readonly IPricesDtoAgregator agregator;
        public PricesController(IPricesDtoAgregator agregator) => this.agregator = agregator;

        public async Task<ResponseModel<PaginationResponseModel<PriceDto>>> Get(int page = 1, int limit = 10) =>
            await agregator.GetPricesAsync(new(page, limit));
        
        [HttpGet("{ticker}")]
        public async Task<ResponseModel<PaginationResponseModel<PriceDto>>> Get(string ticker, int page = 1, int limit = 10) =>
            await agregator.GetPricesAsync(ticker, new(page, limit));
    }
}