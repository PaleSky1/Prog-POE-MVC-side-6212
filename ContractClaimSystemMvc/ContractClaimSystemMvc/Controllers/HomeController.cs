using ContractClaimSystemMvc.Models;
using ContractClaimSystemMvc.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Net;

namespace ContractClaimSystemMvc.Controllers
{
    [Authorize(Roles = "Admin")] // This will enforce that all actions require authentication
    public class HomeController : Controller
    {
        private readonly ApiService _apiService;

        public HomeController(ApiService apiService)
        {
            _apiService = apiService;
        }

        public async Task<IActionResult> Index()
        {
            var users = await _apiService.GetUsersAsync();
            return View(users);
        }
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(TblUser user)
        {
            await _apiService.CreateUserAsync(user);
            return RedirectToAction("Index");
        }
        // New Delete action
        [HttpPost] // Ensure this is a POST request
        public async Task<IActionResult> Delete(string username)
        {
            try
            {
                await _apiService.DeleteUserAsync(username);
                return RedirectToAction("Index");
            }
            catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.Conflict)
            {
                ModelState.AddModelError(string.Empty, "This User cannot be deleted because " +
                    "there are claims associated with this user.");
                var users = await _apiService.GetUsersAsync();
                return View("Index", users); // Return to the Index view with the current list
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "An unexpected error occurred while trying " +
                    "to delete the User. " + ex.ToString());
                var avengers = await _apiService.GetUsersAsync();
                return View("Index", avengers); // Return to the Index view with the current list
            }
        }
    }
}