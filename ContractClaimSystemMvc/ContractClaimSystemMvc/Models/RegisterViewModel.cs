namespace ContractClaimSystemMvc.Models
{
    public class RegisterViewModel
    {
        public string Username { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string ConfirmPassword { get; set; }
        public string Role { get; set; } // Default this to "User" in the UI if not dynamic
    }
}

