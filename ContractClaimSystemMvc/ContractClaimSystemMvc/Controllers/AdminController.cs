using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ContractClaimSystemMvc.Controllers
{
    public class AdminController : Controller
    {
        [Authorize(Roles = "Admin")] // Only allow users with the Admin role
        public IActionResult AdminDashboard()
        {
            return View();
        }
    }
}
