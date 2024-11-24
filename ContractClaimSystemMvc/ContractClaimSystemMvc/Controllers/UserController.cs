using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ContractClaimSystemMvc.Models;
using ContractClaimSystemMvc.Services;
using System.Threading.Tasks;

namespace ContractClaimSystemMvc.Controllers
{
    public class UserController : Controller
    {
        private readonly ApiService _apiService;

        public UserController(ApiService apiService)
        {
            _apiService = apiService;
        }

        // Admin only: Get all users
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ViewUsers()
        {
            // Fetch all users from the API
            var users = await _apiService.GetUsersAsync();  // Assuming the API service has a method to get users
            return View(users);  // Pass the list of users to the view
        }
    }
}
