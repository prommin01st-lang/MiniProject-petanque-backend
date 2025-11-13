namespace JWTdemo.Models
{
    // DTO สำหรับสถิติ Notification รวม (ของทุกคน)
    public class NotificationStatsDto
    {
        public int TotalSent { get; set; } // จำนวน Noti ทั้งหมดที่ส่งไป
        public int TotalUnread { get; set; } // จำนวนที่ "ยังไม่อ่าน"
        public int TotalRead { get; set; } // จำนวนที่ "อ่านแล้ว"
    }
}