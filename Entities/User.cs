namespace JWTdemo.Entities
{
    public class User
    {
        public Guid id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string? RefreshToken { get; set; }
        public DateTime? RefreshTokenExpiryTime { get; set; }

        public string? Email { get; set; }

        public string? FirstName { get; set; } // 👈 ✍️ แก้ไข
        public string? LastName { get; set; } // 👈 ✍️ แก้ไข

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public string? ProfileImageUrl { get;  set; }
    

        
    }
}
