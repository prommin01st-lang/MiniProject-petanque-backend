using System;

namespace JWTdemo.Models
{
    // นี่คือหน้าตาของ Noti 1 อัน ที่จะส่งกลับไปให้ React
    public class NotificationDto
    {
        public string Id { get; set; } // Id ของ Notification (ไม่ใช่ Id ของ Status)
        public string Title { get; set; }
        public string Subtitle { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsRead { get; set; } // 👈 สถานะ "อ่าน" ของ User คนนี้

        // 3 properties ที่จะแปลงให้ React ใช้งานง่าย
        public string? AvatarImage { get; set; }
        public string? AvatarIcon { get; set; }
        public string? AvatarText { get; set; }
    }
}