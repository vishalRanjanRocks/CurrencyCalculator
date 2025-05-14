namespace CurrencyExchange.Infrastructure.Models
{
    public class LoginRequest
    {
        public string? Username { get; set; }
        public string? Password { get; set; }
        public string? Role { get; set; } // Admin or User
    }
}
