using System;

namespace JWTdemo.Models
{
    // DTO นี้จะแสดง Comment พร้อมข้อมูลผู้เขียน (Username, รูป)
    public class CommentDto
    {
        public int Id { get; set; }
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        
        // ข้อมูลผู้เขียน (Author)
        public Guid UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string? ProfileImageUrl { get; set; }
    }
}