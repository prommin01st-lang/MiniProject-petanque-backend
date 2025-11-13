using System.ComponentModel.DataAnnotations;

namespace JWTdemo.Entities
{
    public class UserNotificationStatus
    {
        [Key]
        public int Id { get; set; } // Auto-increment ID

        // เชื่อมโยงไปที่ตาราง Users (จาก AuthService เราเห็นว่า User.id เป็น Guid)
        public Guid UserId { get; set; }
        public User User { get; set; }

        // เชื่อมโยงไปที่ตาราง Notifications
        public string NotificationId { get; set; }
        public Notification Notification { get; set; }

        public bool IsRead { get; set; } = false;
    }
}