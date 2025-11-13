using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JWTdemo.Entities
{
    public class Article
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Content { get; set; } = string.Empty; // 👈 1. [สำคัญ] นี่คือที่เก็บ "Markdown"

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? PublishedAt { get; set; } // 👈 (Optional) วันที่เผยแพร่ (ถ้าเป็น null = ฉบับร่าง)

        // 2. ✍️ Foreign Key ไปยัง "ผู้เขียน" (Admin/User)
        public Guid AuthorUserId { get; set; }

        [ForeignKey("AuthorUserId")]
        public virtual User Author { get; set; } = null!;
    }
}