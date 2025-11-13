namespace JWTdemo.Models
{
    public class UserListDto
    {
        public string Id { get; set; } // ใช้ string เพื่อความสะดวกใน Frontend
        public string Username { get; set; }
        public string Role { get; set; } // Role เดียว (Admin, User)
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime CreatedAt { get; set; } // สำหรับ Member Since
    }
}