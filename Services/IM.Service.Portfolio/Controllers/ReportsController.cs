using System;

using IM.Service.Portfolio.Services.HttpRestApi;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace IM.Service.Portfolio.Controllers;

[ApiController, Route("[controller]")]
public class ReportsController : ControllerBase
{
    private readonly ReportRestApi api;
    public ReportsController(ReportRestApi api) => this.api = api;

    [HttpPost]
    public IActionResult Post(IFormFileCollection files)
    {
        try
        {
            return Ok(api.Load(files, "0f9075e9-bbcf-4eef-a52d-d9dcad816f5e"));
        }
        catch (Exception exception)
        {
            return BadRequest(exception.Message);
        }
    }
}