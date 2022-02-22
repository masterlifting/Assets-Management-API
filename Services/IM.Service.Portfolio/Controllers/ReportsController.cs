using IM.Service.Portfolio.Services.DtoServices;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace IM.Service.Portfolio.Controllers;

[ApiController, Route("[controller]")]

public class ReportsController
{
    private readonly ReportDtoManager manager;
    public ReportsController(ReportDtoManager manager) => this.manager = manager;

    [HttpPost]
    public string Post(IFormFileCollection files) => manager.Load(files, "0f9075e9-bbcf-4eef-a52d-d9dcad816f5e");
}