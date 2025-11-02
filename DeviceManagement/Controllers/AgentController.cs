using Microsoft.AspNetCore.Mvc;

namespace DeviceManagement.Controllers
{
    public class AgentController : Controller
    {
        private readonly IAssetService _assetService;
        public AgentController(IAssetService assetService)
        {
            _assetService = assetService;

        }
        public async Task<IActionResult> Index()
        {
            var assets = await _assetService.GetAllAsync();
            return View(assets);
        }

    }
}
