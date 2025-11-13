using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JWTdemo.Entities
{
    // นี่คือ "บอร์ด" หรือ "ตารางใหญ่" (เช่น "Course Work")
    public class TodoListCategory
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Title { get; set; } = string.Empty;
        
        // 1. ✍️ Foreign Key ไปยัง "เจ้าของ" บอร์ดนี้
        public Guid UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;

        // 2. ✍️ ความสัมพันธ์ (1 Category มีหลาย Item)
        public virtual ICollection<TodoItem> TodoItems { get; set; } = new List<TodoItem>();
    }
}