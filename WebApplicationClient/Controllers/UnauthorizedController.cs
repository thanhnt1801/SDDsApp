using Microsoft.AspNetCore.Mvc;

namespace WebApplicationClient.Controllers
{
    public class UnauthorizedController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
