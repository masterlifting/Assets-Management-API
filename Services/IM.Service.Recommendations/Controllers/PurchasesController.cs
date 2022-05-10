using Microsoft.AspNetCore.Mvc;

namespace IM.Service.Recommendations.Controllers;

[ApiController, Route("[controller]")]
public class PurchasesController : Controller
{
    public IActionResult Index()
    {
        return Ok();
    }
}