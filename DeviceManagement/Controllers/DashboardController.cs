using Microsoft.AspNetCore.Mvc;

namespace DeviceManagement.Controllers
{
    public class DashboardController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
