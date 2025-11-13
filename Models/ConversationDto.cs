using System;

namespace JWTdemo.Models
{
    // DTO นี้จะแสดง "ห้องแชท" 1 ห้อง
    public class ConversationDto
    {
        public int Id { get; set; } // ID ของห้องแshท
        
        // ข้อมูลของ "คู่สนทนา" (ไม่ใช่ตัวเรา)
        public Guid OtherUserId { get; set; } 
        public string OtherUsername { get; set; } = string.Empty;
        public string? OtherUserProfileImageUrl { get; set; }
        
        // ข้อความล่าสุด (สำหรับ Preview)
        public string? LastMessage { get; set; }
        public DateTime? LastMessageTimestamp { get; set; }
        public bool IsLastMessageRead { get; set; }

        public Guid? LastMessageSenderId { get; set; }
    }
}