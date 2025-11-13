namespace JWTdemo.Models
{
    // สำหรับ Widget สถิติ Admin
    public class UserStatsDto
    {
        public int TotalUsers { get; set; }
        public int TotalAdmins { get; set; }
        public int NewUsersThisMonth { get; set; } // Users created in last 30 days
    }
}