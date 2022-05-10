using Microsoft.AspNetCore.Mvc;

namespace IM.Service.Recommendations.Controllers;

[ApiController, Route("[controller]")]
public class SalesController : Controller
{
    public IActionResult Index()
    {
        return Ok();
    }
}