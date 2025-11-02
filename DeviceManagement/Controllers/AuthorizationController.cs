using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using DeviceManagement.Models;
using DeviceManagement.ViewModels;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json;

namespace DeviceManagement.Controllers
{
    public class AuthorizationController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly HttpClient _httpClient;

        public AuthorizationController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _httpClient = new HttpClient();
        }

        // LOGIN
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public async Task<IActionResult> Login(LoginViewModel loginModel)
        {
            if (ModelState.IsValid)
            {
                ApplicationUser user = await _userManager.FindByEmailAsync(loginModel.Email);
                if (user != null)
                {

                    var result = await _signInManager.PasswordSignInAsync(user,
                        loginModel.Password, isPersistent: loginModel.RememberMe, false);
                    if (result.Succeeded)
                    {
                        return RedirectToAction("Index", "Home");
                    }
                }
            }
            ModelState.AddModelError("", "Invalid username or password");
            return View(loginModel);
        }

        // REGISTER
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            bool isStudent = !string.IsNullOrWhiteSpace(model.StudentNumber);

            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                FullName = model.FullName,
                Address = model.Address,
                CellphoneNumber = model.CellphoneNumber,
                Gender = model.Gender,
                CompanyName = isStudent ? "null" : model.CompanyName,
                StudentNumber = isStudent ? model.StudentNumber : "30",
                BusinessNumber = isStudent ? "null" : model.BusinessNumber,
                DateOfBirth = model.DateOfBirth,
                SAIdNumber = model.SAIdNumber
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                string role = isStudent ? "Student" : "Business";

                if (!await _roleManager.RoleExistsAsync(role))
                    await _roleManager.CreateAsync(new IdentityRole(role));

                await _userManager.AddToRoleAsync(user, role);
                await _signInManager.SignInAsync(user, isPersistent: false);

                // ✅ After successful registration, sync user to Oracle APEX
                await SyncUserToMobileApi(user, role);

                return RedirectToAction("Index", "Home");
            }

            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error.Description);

            return View(model);
        }

        // LOGOUT
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        // FORGOT PASSWORD
        [HttpGet]
        [AllowAnonymous]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> ForgotPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null || !(await _userManager.IsEmailConfirmedAsync(user)))
                return RedirectToAction(nameof(ForgotPasswordConfirmation));

            // TODO: Email sending logic

            return RedirectToAction(nameof(ForgotPasswordConfirmation));
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        // RESET PASSWORD
        [HttpGet]
        [AllowAnonymous]
        public IActionResult ResetPassword(string token = null, string email = null)
        {
            if (token == null || email == null)
                return BadRequest("A token and email must be supplied for password reset.");

            var model = new ResetPasswordViewModel { Token = token, Email = email };
            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
                return RedirectToAction(nameof(ResetPasswordConfirmation));

            var result = await _userManager.ResetPasswordAsync(user, model.Token, model.Password);

            if (result.Succeeded)
                return RedirectToAction(nameof(ResetPasswordConfirmation));

            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error.Description);

            return View(model);
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ResetPasswordConfirmation()
        {
            return View();
        }

        // 🔁 Helper: Redirect to returnUrl if it's local
        private IActionResult RedirectToLocal(string returnUrl)
        {
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction("Index", "Home");
        }

        // =====================================
        // ✅ Helper: Sync newly registered user
        // =====================================
        private async Task SyncUserToMobileApi(ApplicationUser user, string role)
        {
            try
            {
                var apiUrl = $"{Request.Scheme}://{Request.Host}/api/mobileapi/sync-user";

                var apiUserDto = new
                {
                    name = user.FullName?.Split(' ')[0] ?? user.FullName,
                    surname = user.FullName?.Split(' ').Length > 1 ? user.FullName.Split(' ')[1] : "",
                    username = user.Email,
                    password = "N/A",
                    role = role,
                    location = user.Address,
                    active = "Y"
                };

                var json = JsonConvert.SerializeObject(apiUserDto);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(apiUrl, content);
                if (!response.IsSuccessStatusCode)
                {
                    var responseBody = await response.Content.ReadAsStringAsync();
                    System.Diagnostics.Debug.WriteLine($"❌ Sync failed: {response.StatusCode} - {responseBody}");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("✅ User synced successfully to Oracle APEX API.");
                }
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"⚠️ Error syncing user: {ex.Message}");
            }
        }
    }
}
