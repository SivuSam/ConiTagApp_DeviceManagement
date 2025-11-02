using System.Diagnostics;
using DeviceManagement.Models;
using DeviceManagement.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using DeviceManagement.Data;
using Microsoft.EntityFrameworkCore;

namespace DeviceManagement.Controllers
{
    public class HomeController : Controller
    {
        private readonly INotificationService _notificationService;
       
        private readonly ApplicationDbContext _context;

        public HomeController(
            INotificationService notificationService,
         
            ApplicationDbContext context)
        {
            _notificationService = notificationService;
     
            _context = context;
        }

        [AllowAnonymous]
        public IActionResult Index()
        {
            return View();
        }

        [Authorize]


        public async Task<IActionResult> Dashboard()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            

            var myNotifications = await _context.Notifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();

            var profile = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);

            ViewBag.Leases = "Assert";
            ViewBag.Notifications = myNotifications;
            ViewBag.Profile = profile;

            return View();
        }



        
        [Authorize]
        public IActionResult Notifications()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var notifications = _context.Notifications
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .ToList();

            return View(notifications);
        }

        public IActionResult About() => View();
        public IActionResult Contact() => View();
        public IActionResult Services() => View();
    }
}
