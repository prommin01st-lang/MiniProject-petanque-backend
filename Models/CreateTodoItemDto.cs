using System;
using System.ComponentModel.DataAnnotations;
namespace JWTdemo.Models
{
    public class CreateTodoItemDto
    {
        [Required]
        public string Title { get; set; } = string.Empty;
        [Required]
        public int CategoryId { get; set; } // 👈 ต้องบอกว่าสร้างใน "บอร์ด" ไหน
        public string? Priority { get; set; }
        public DateTime? Deadline { get; set; }
    }
}