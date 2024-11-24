using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using ContractClaimSystemMvc.Models;
using ContractClaimSystemMvc.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace ContractClaimSystemMvc.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApiService _apiService;

        public AccountController(ApiService apiService)
        {
            _apiService = apiService;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        private string GetUserRole(string token)
        {
            var handler = new JwtSecurityTokenHandler();

            if (handler.CanReadToken(token))
            {
                var jwtToken = handler.ReadJwtToken(token);
                var roleClaim = jwtToken.Claims.FirstOrDefault(c =>
                    c.Type == "role" || c.Type == "roles" ||
                    c.Type == "http://schemas.microsoft.com/ws/2008/06/identity/claims/role");

                return roleClaim?.Value; // Return the role from the token
            }

            return null; // Default to "User" if no role is found
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var token = await _apiService.LoginAsync(model);
                    HttpContext.Session.SetString("JWToken", token); // Store token in session

                    var userRole = GetUserRole(token); // Extract user role from token
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, model.Username),
                        new Claim(ClaimTypes.Role, userRole)
                    };

                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, claimsPrincipal);

                    if (userRole == "Admin")
                    {
                        return RedirectToAction("AdminDashboard", "admin");
                    }
                    else
                    {
                        return RedirectToAction("UserDashboard", "user");
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(string.Empty, "Login failed: " + ex.Message);
                }
            }

            return View(model);
        }
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var result = await _apiService.RegisterAsync(model);
                if (result)
                {
                    return RedirectToAction("Login");
                }
                ModelState.AddModelError("", "Registration failed. Please try again.");
            }
            return View(model);
        }
        public async Task<IActionResult> Logout()
        {
            HttpContext.Session.Remove("JWToken"); // Clear the token
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}