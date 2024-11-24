using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using ContractClaimSystemMvc.Models;
using ContractClaimSystemMvc.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Linq;
using System;
using System.IO;
using System.Threading.Tasks;

namespace ContractClaimSystemMvc.Controllers
{
    public class ClaimsController : Controller
    {
        private readonly ApiService _apiService;

        public ClaimsController(ApiService apiService)
        {
            _apiService = apiService;
        }

        // Get JWT Token from session
        private string GetJwtToken()
        {
            return HttpContext.Session.GetString("JWToken");
        }

        // Check if the user is authenticated
        private bool IsAuthenticated()
        {
            return !string.IsNullOrEmpty(GetJwtToken());
        }

        // Extract user role from the JWT token
        private string GetUserRole(string token)
        {
            var handler = new JwtSecurityTokenHandler();
            if (handler.CanReadToken(token))
            {
                var jwtToken = handler.ReadJwtToken(token);
                var roleClaim = jwtToken.Claims.FirstOrDefault(c =>
                    c.Type == "role" || c.Type == "roles" ||
                    c.Type == "http://schemas.microsoft.com/ws/2008/06/identity/claims/role");
                return roleClaim?.Value;
            }
            return null;
        }

        // Get all claims for the logged-in user or admin
        [HttpGet]
        public async Task<IActionResult> ViewClaims()
        {
            if (!IsAuthenticated())
            {
                return RedirectToAction("Login", "Account");
            }

            var token = GetJwtToken();
            var userRole = GetUserRole(token);

            // Fetch all claims (can be filtered by role if needed)
            var claims = await _apiService.GetClaimsAsync();

            // If the user is an admin, show all claims. Otherwise, filter by their userId.
            if (userRole != "Admin")
            {
                claims = claims.Where(c => c.UserId == Convert.ToInt32(HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier))).ToList();
            }

            return View(claims); // Return the claims to the ViewClaims view
        }

        // View to create a new claim
        [HttpGet]
        public IActionResult Create()
        {
            if (!IsAuthenticated())
            {
                return RedirectToAction("Login", "Account");
            }

            // Initialize the claim model with default values
            var claim = new TblClaim
            {
                HourlyRate = 0.00m, // Set a default value for HourlyRate
                TotalPayment = 0.00m // Set a default value for TotalPayment
            };

            return View(claim); // Return Create view with initialized claim
        }

        // Create a new claim (POST method)
        [HttpPost]
        public async Task<IActionResult> Create(TblClaim newClaim, IFormFile uploadedFile)
        {
            if (!IsAuthenticated())
            {
                return RedirectToAction("Login", "Account");
            }

            if (ModelState.IsValid)
            {
                // Check if a file was uploaded
                if (uploadedFile != null && uploadedFile.Length > 0)
                {
                    // Generate a unique file name to avoid overwriting
                    var fileName = Path.GetFileNameWithoutExtension(uploadedFile.FileName);
                    var fileExtension = Path.GetExtension(uploadedFile.FileName);
                    var uniqueFileName = $"{fileName}_{Guid.NewGuid()}{fileExtension}";

                    // Define the file path
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", uniqueFileName);

                    // Ensure the directory exists
                    var directory = Path.GetDirectoryName(filePath);
                    if (!Directory.Exists(directory))
                    {
                        Directory.CreateDirectory(directory);
                    }

                    // Save the uploaded file to the specified path
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await uploadedFile.CopyToAsync(stream);
                    }

                    // Set the file path in the claim object
                    newClaim.UploadedFile = $"/uploads/{uniqueFileName}"; // Store the relative path to the file
                    newClaim.UploadedFileName = uploadedFile.FileName; // Optionally store the original file name
                }

                // Assign UserId from session (ensure the user is logged in and the Id is available)
                newClaim.UserId = Convert.ToInt32(HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier));

                // Calculate Total Payment (example)
                newClaim.TotalPayment = newClaim.HoursWorked * newClaim.HourlyRate;

                // Call the API to create the claim
                await _apiService.CreateClaimAsync(newClaim);

                // Set success message in TempData and redirect
                TempData["SuccessMessage"] = "Claim submitted successfully!";
                return RedirectToAction("ViewClaims");
            }

            return View(newClaim); // If model state is invalid, return to the create view with the model
        }

        // View to delete a claim (GET method)
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            if (!IsAuthenticated())
            {
                return RedirectToAction("Login", "Account");
            }

            var claim = (await _apiService.GetClaimsAsync()).FirstOrDefault(c => c.ClaimId == id);
            if (claim != null)
            {
                return View(claim); // Return the claim to the Delete view
            }

            return RedirectToAction("ViewClaims"); // If no claim found, redirect to ViewClaims
        }

        // Confirm deletion of a claim (POST method)
        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (!IsAuthenticated())
            {
                return RedirectToAction("Login", "Account");
            }

            await _apiService.DeleteClaimAsync(id); // Call the API to delete the claim
            return RedirectToAction("ViewClaims"); // Redirect to claims list after deletion
        }
    }
}
