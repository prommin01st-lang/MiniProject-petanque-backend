using System.ComponentModel.DataAnnotations;

namespace JWTdemo.Models
{
    // DTO สำหรับการรับข้อมูลที่ผู้ใช้ต้องการแก้ไข (เช่น Username/Email ใหม่)
    public class ProfileUpdateDto
    {
        // เราจะอนุญาตให้อัปเดต Username ได้
        [Required]
        public string NewUsername { get; set; }
        public string? Email { get; set; } // 👈 ✍️ เพิ่ม Email
        public string? FirstName { get; set; } // 👈 ✍️ เพิ่ม FirstName
        public string? LastName { get; set; } // 👈 ✍️ เพิ่ม LastName
    }
}