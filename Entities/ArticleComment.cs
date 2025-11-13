using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JWTdemo.Entities
{
    // นี่คือตารางเก็บ "ความคิดเห็น"
    public class ArticleComment
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Content { get; set; } = string.Empty; // 👈 เนื้อหา Comment

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // 1. ✍️ Foreign Key ไปยัง "บทความ"
        public int ArticleId { get; set; }

        [ForeignKey("ArticleId")]
        public virtual Article Article { get; set; } = null!;

        // 2. ✍️ Foreign Key ไปยัง "ผู้ใช้" (คนที่ Comment)
        public Guid UserId { get; set; }
        
        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;
    }
}