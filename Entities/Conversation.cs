using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JWTdemo.Entities
{
    // นี่คือ "ห้องแชท" (1 ห้องมี 2 คน)
    public class Conversation
    {
        [Key]
        public int Id { get; set; }

        // --- (ผู้เข้าร่วม) ---
        [Required]
        public Guid User1Id { get; set; }
        [ForeignKey("User1Id")]
        public virtual User User1 { get; set; } = null!;

        [Required]
        public Guid User2Id { get; set; }
        [ForeignKey("User2Id")]
        public virtual User User2 { get; set; } = null!;
        // --------------------

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // ความสัมพันธ์ (1 ห้อง มีหลายข้อความ)
        public virtual ICollection<Message> Messages { get; set; } = new List<Message>();
    }
}