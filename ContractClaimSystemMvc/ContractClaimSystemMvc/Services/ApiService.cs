using ContractClaimSystemMvc.Models;
using System.Text.Json;

namespace ContractClaimSystemMvc.Services
{
    public class ApiService
    {
        private readonly HttpClient _httpClient;
        public ApiService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }
        public async Task<List<TblUser>> GetUsersAsync()
        {
            var response = await _httpClient.GetAsync("/users");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<List<TblUser>>();
        }
        public async Task<List<TblClaim>> GetClaimsAsync()
        {
            var response = await _httpClient.GetAsync("/claims");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<List<TblClaim>>();
        }
        public async Task CreateUserAsync(TblUser newUser)
        {
            var response = await _httpClient.PostAsJsonAsync("/users", newUser);
            response.EnsureSuccessStatusCode();
        }

        public async Task CreateClaimAsync(TblClaim newClaim)
        {
            var response = await _httpClient.PostAsJsonAsync("/claims", newClaim);
            response.EnsureSuccessStatusCode();
        }
        public async Task DeleteClaimAsync(int claimId)
        {
            // Assuming you have an API endpoint to delete the claim by ID
            var response = await _httpClient.DeleteAsync($"api/claims/{claimId}");

            if (!response.IsSuccessStatusCode)
            {
                // Handle failure (you can throw an exception or return a result indicating failure)
                throw new Exception("Error deleting claim.");
            }
        }
        // Delete method for TblAvenger
        public async Task DeleteUserAsync(string username)
        {
            var response = await _httpClient.DeleteAsync($"/users/{username}");
            response.EnsureSuccessStatusCode();
        }

        public async Task<bool> RegisterAsync(RegisterViewModel registerModel)
        {
            var response = await _httpClient.PostAsJsonAsync("/register", new
            {
                registerModel.Username,
                registerModel.Email,
                registerModel.Password,
                Role = registerModel.Role
            });

            return response.IsSuccessStatusCode;
        }

        // Method to log in a user
        public async Task<string> LoginAsync(LoginViewModel userLoginDto)
        {
            var response = await _httpClient.PostAsJsonAsync("/login", userLoginDto);

            // Ensure the response is successful
            response.EnsureSuccessStatusCode();

            // Read the response as a string
            var jsonString = await response.Content.ReadAsStringAsync();

            // Deserialize the response to a dictionary
            var result = JsonSerializer.Deserialize<Dictionary<string, string>>(jsonString);

            // Check if the token key exists (note the lowercase 'token')
            if (result != null && result.TryGetValue("token", out var token))
            {
                return token; // Return the token from the response
            }

            throw new Exception("Token not found in response"); // Throw an exception if the token is not found
        }
    }
}