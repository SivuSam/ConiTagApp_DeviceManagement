using DeviceManagement.Models;
using DeviceManagement.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;

namespace DeviceManagement.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly INotificationService _notificationService;

        public AccountController(UserManager<ApplicationUser> userManager, INotificationService notificationService)
        {
            _userManager = userManager;
            _notificationService = notificationService;
        }

        // Profile + Notifications
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            var notifications = await _notificationService.GetUserNotificationsAsync(user.Id);
            ViewData["Notifications"] = notifications;

            return View(user);
        }

        // Change password (AJAX)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordModel model)
        {
            if (model.NewPassword != model.ConfirmPassword)
                return Json(new { success = false, message = "New password and confirmation do not match." });

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Json(new { success = false, message = "User not found." });

            var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
            if (result.Succeeded) return Json(new { success = true });

            return Json(new { success = false, message = string.Join(" ", result.Errors.Select(e => e.Description)) });
        }
    }

    public class ChangePasswordModel
    {
        public string CurrentPassword { get; set; }
        public string NewPassword { get; set; }
        public string ConfirmPassword { get; set; }
    }
}
