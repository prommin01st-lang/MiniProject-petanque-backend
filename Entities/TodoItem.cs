using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JWTdemo.Entities
{
    // นี่คือ "รายการย่อย" (เช่น "Build API")
    public class TodoItem
    {
        [Key]
        public int Id { get; set; } 

        [Required]
        public string Title { get; set; } = string.Empty;

        public bool IsCompleted { get; set; } = false;

        // --- 👇 1. [เพิ่ม] Fields ใหม่ตามที่คุณต้องการ ---
        public string? Priority { get; set; } // (เช่น "High", "Medium", "Low")
        public DateTime? Deadline { get; set; } // (วันที่สิ้นสุด / Est. Time)
        // ---------------------------------------------

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // 2. ✍️ Foreign Key ไปยัง "ตารางแม่" (Category)
        public int TodoListCategoryId { get; set; }

        [ForeignKey("TodoListCategoryId")]
        public virtual TodoListCategory TodoListCategory { get; set; } = null!;
        
        // (เราไม่ต้องมี UserId ที่นี่แล้ว เพราะ Category มี UserId อยู่)
    }
}