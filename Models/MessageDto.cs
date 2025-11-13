using System;

namespace JWTdemo.Models
{
    public class MessageDto
    {
        public int Id { get; set; }
        public string Content { get; set; } = string.Empty;
        public DateTime SentAt { get; set; }
        public bool IsRead { get; set; }
        public Guid SenderId { get; set; } // 👈 ID ของผู้ส่ง
    }
}