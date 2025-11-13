using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JWTdemo.Entities
{
    // นี่คือ "ข้อความ"
    public class Message
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Content { get; set; } = string.Empty; // 👈 เนื้อหาข้อความ

        public DateTime SentAt { get; set; } = DateTime.UtcNow;

        public bool IsRead { get; set; } = false; // 👈 สถานะ "อ่านแล้ว"

        // --- (ความสัมพันธ์) ---

        // 1. ✍️ Foreign Key ไปยัง "ห้องแชท"
        public int ConversationId { get; set; }
        [ForeignKey("ConversationId")]
        public virtual Conversation Conversation { get; set; } = null!;

        // 2. ✍️ Foreign Key ไปยัง "ผู้ส่ง"
        public Guid SenderId { get; set; }
        [ForeignKey("SenderId")]
        public virtual User Sender { get; set; } = null!;
    }
}