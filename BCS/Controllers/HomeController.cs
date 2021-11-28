using Microsoft.AspNetCore.Mvc;

namespace HMTSolution.BCS.Controllers
{
    [Route("")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class HomeController : Controller
    {
        [HttpGet("")]
        public RedirectResult Get()
        {
            return Redirect("swagger");
        }
    }
}
