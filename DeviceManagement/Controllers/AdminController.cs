using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using DeviceManagement.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using DeviceManagement.ViewModels;
using Microsoft.EntityFrameworkCore;
using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;

namespace DeviceManagement.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ReportService _reportService;
        public AdminController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, ReportService reportService)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _reportService = reportService;
        }

        public IActionResult Index()
        {
            var users = _userManager.Users.ToList();

            var model = users.Select(u => new UserViewModel
            {
                Id = u.Id,
                UserName = u.UserName,
                Email = u.Email,
                FullName = u.FullName,
                Address = u.Address,
                CellphoneNumber = u.CellphoneNumber,
                Gender = u.Gender,
                CompanyName = u.CompanyName,
                StudentNumber = u.StudentNumber,
                BusinessNumber = u.BusinessNumber,
                DateOfBirth = u.DateOfBirth,
                SAIdNumber = u.SAIdNumber,
                MonthlyEarnings = u.MonthlyEarnings,
               
                NotificationsCount = u.Notifications?.Count ?? 0,
                LockoutEnd = u.LockoutEnd
            }).ToList();

            return View(model);
        }
        

        public async Task<IActionResult> Details(string id)
        {
            if (string.IsNullOrEmpty(id))
                return NotFound();

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound();

            var model = new UserViewModel
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                FullName = user.FullName,
                Address = user.Address,
                CellphoneNumber = user.CellphoneNumber,
                Gender = user.Gender,
                CompanyName = user.CompanyName,
                StudentNumber = user.StudentNumber,
                BusinessNumber = user.BusinessNumber,
                DateOfBirth = user.DateOfBirth,
                SAIdNumber = user.SAIdNumber,
                MonthlyEarnings = user.MonthlyEarnings,
                
                NotificationsCount = user.Notifications?.Count ?? 0,
                LockoutEnd = user.LockoutEnd
            };

            return View(model);
        }
        public async Task<IActionResult> REPORT()
        {
            byte[] pdfBytes = await _reportService.GenerateUsersAssetsReportPdfAsync();

            string fileName = $"UsersAssetsReport_{DateTime.Now:yyyyMMddHHmmss}.pdf";

            return File(pdfBytes, "application/pdf", fileName);
        }

        // Lock user account
        [HttpPost]
        public async Task<IActionResult> LockUser(string id)
        {
            if (string.IsNullOrEmpty(id))
                return BadRequest("User ID is required.");

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound();

            // Lock account for 100 years (effectively permanent lock)
            await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.UtcNow.AddYears(100));

            return RedirectToAction(nameof(Index));
        }

        // Unlock user account
        [HttpPost]
        public async Task<IActionResult> UnlockUser(string id)
        {
            if (string.IsNullOrEmpty(id))
                return BadRequest("User ID is required.");

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound();

            await _userManager.SetLockoutEndDateAsync(user, null);  // Unlock user

            return RedirectToAction(nameof(Index));
        }

        // Delete user account
        [HttpPost]
        public async Task<IActionResult> DeleteUser(string id)
        {
            if (string.IsNullOrEmpty(id))
                return BadRequest("User ID is required.");

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound();

            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
            {
                ModelState.AddModelError("", "Failed to delete user.");
                return RedirectToAction(nameof(Index));
            }

            return RedirectToAction(nameof(Index));
        }

        // Show edit form for user (GET)
        public async Task<IActionResult> EditUser(string id)
        {
            if (string.IsNullOrEmpty(id))
                return NotFound();

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound();

            var model = new EditUserViewModel
            {
                Id = user.Id,
                FullName = user.FullName,
                Address = user.Address,
                CellphoneNumber = user.CellphoneNumber,
                Gender = user.Gender,
                CompanyName = user.CompanyName
            };

            return View(model);
        }

        // Handle edit form submission (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditUser(EditUserViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userManager.FindByIdAsync(model.Id);
            if (user == null)
                return NotFound();

            // Update editable fields
            user.FullName = model.FullName;
            user.Address = model.Address;
            user.CellphoneNumber = model.CellphoneNumber;
            user.Gender = model.Gender;
            user.CompanyName = model.CompanyName;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                foreach (var err in result.Errors)
                    ModelState.AddModelError("", err.Description);
                return View(model);
            }

            return RedirectToAction(nameof(Index));
        }
    }

    // ViewModel for displaying user info
   
}
