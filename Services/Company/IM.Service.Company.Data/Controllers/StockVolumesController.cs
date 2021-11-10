using IM.Service.Common.Net.Models.Dto.Http;
using IM.Service.Common.Net.Models.Dto.Http.Companies;

using IM.Service.Company.Data.DataAccess.Entities;
using IM.Service.Company.Data.Services.DtoServices;

using Microsoft.AspNetCore.Mvc;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using IM.Service.Common.Net.RepositoryService.Filters;
using static IM.Service.Common.Net.CommonEnums;

namespace IM.Service.Company.Data.Controllers
{
    [ApiController, Route("[controller]")]
    public class StockVolumesController : ControllerBase
    {
        private readonly StockVolumesDtoManager manager;
        //private readonly StockVolumeLoader loader;
        public StockVolumesController(StockVolumesDtoManager manager)
        {
            this.manager = manager;
            //this.loader = loader;
        }

        public async Task<ResponseModel<PaginatedModel<StockVolumeGetDto>>> Get(int year = 0, int month = 0, int day = 0, int page = 0, int limit = 0) =>
            await manager.GetAsync(new CompanyDataFilterByDate<StockVolume>(HttpRequestFilterType.More, year, month, day), new(page, limit));

        [HttpGet("last/")]
        public async Task<ResponseModel<PaginatedModel<StockVolumeGetDto>>> GetLast(int year = 0, int month = 0, int day = 0, int page = 0, int limit = 0) =>
            await manager.GetLastAsync(new CompanyDataFilterByDate<StockVolume>(HttpRequestFilterType.More, year, month, day), new(page, limit));

        [HttpGet("{companyId}")]
        public async Task<ResponseModel<PaginatedModel<StockVolumeGetDto>>> Get(string companyId, int year = 0, int month = 0, int day = 0, int page = 0, int limit = 0) =>
            await manager.GetAsync(new CompanyDataFilterByDate<StockVolume>(HttpRequestFilterType.More, companyId, year, month, day), new(page, limit));

        [HttpGet("{companyId}/{Year:int}")]
        public async Task<ResponseModel<PaginatedModel<StockVolumeGetDto>>> GetEqual(string companyId, int year, int page = 0, int limit = 0) =>
            await manager.GetAsync(new CompanyDataFilterByDate<StockVolume>(companyId, year), new(page, limit));

        [HttpGet("{companyId}/{Year:int}/{Month:int}")]
        public async Task<ResponseModel<PaginatedModel<StockVolumeGetDto>>> GetEqual(string companyId, int year, int month, int page = 0, int limit = 0) =>
            await manager.GetAsync(new CompanyDataFilterByDate<StockVolume>(companyId, year, month), new(page, limit));

        [HttpGet("{companyId}/{Year:int}/{Month:int}/{Day:int}")]
        public async Task<ResponseModel<StockVolumeGetDto>> Get(string companyId, int year, int month, int day) =>
            await manager.GetAsync(companyId, new DateTime(year, month, day));


        [HttpGet("{Year:int}")]
        public async Task<ResponseModel<PaginatedModel<StockVolumeGetDto>>> GetEqual(int year, int page = 0, int limit = 0) =>
            await manager.GetAsync(new CompanyDataFilterByDate<StockVolume>(year), new(page, limit));

        [HttpGet("{Year:int}/{Month:int}")]
        public async Task<ResponseModel<PaginatedModel<StockVolumeGetDto>>> GetEqual(int year, int month, int page = 0, int limit = 0) =>
            await manager.GetAsync(new CompanyDataFilterByDate<StockVolume>(year, month), new(page, limit));

        [HttpGet("{Year:int}/{Month:int}/{Day:int}")]
        public async Task<ResponseModel<PaginatedModel<StockVolumeGetDto>>> GetEqual(int year, int month, int day, int page = 0, int limit = 0) =>
            await manager.GetAsync(new CompanyDataFilterByDate<StockVolume>(HttpRequestFilterType.Equal, year, month, day), new(page, limit));


        [HttpPost]
        public async Task<ResponseModel<string>> Post(StockVolumePostDto model) => await manager.CreateAsync(model);
        [HttpPost("collection/")]
        public async Task<ResponseModel<string>> Post(IEnumerable<StockVolumePostDto> models) => await manager.CreateAsync(models);

        [HttpPut("{companyId}/{Year:int}/{Month:int}/{Day:int}")]
        public async Task<ResponseModel<string>> Put(string companyId, int year, int month, int day, StockVolumePutDto model) =>
            await manager.UpdateAsync(new StockVolumePostDto
            {
                CompanyId = companyId,
                Date = new DateTime(year, month, day),
                SourceType = model.SourceType,
                Value = model.Value
            });
        [HttpDelete("{companyId}/{Year:int}/{Month:int}/{Day:int}")]
        public async Task<ResponseModel<string>> Delete(string companyId, int year, int month, int day) =>
            await manager.DeleteAsync(companyId, new DateTime(year, month, day));

        //[HttpPost("load/")]
        //public async Task<string> Load()
        //{
        //    var prices = await loader.LoadAsync();
        //    return $"loaded stock volumes count: {prices.Length}";
        //}
    }
}
