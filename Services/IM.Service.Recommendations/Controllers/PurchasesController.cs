using Microsoft.AspNetCore.Mvc;

namespace IM.Service.Recommendations.Controllers;

[ApiController, Route("api/[controller]")]
public class PurchasesController : Controller
{
    public IActionResult Index()
    {
        return Ok();
    }
}