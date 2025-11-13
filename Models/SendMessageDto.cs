using System;
using System.ComponentModel.DataAnnotations;

namespace JWTdemo.Models
{
    public class SendMessageDto
    {
        [Required]
        public Guid RecipientId { get; set; } // 👈 ID ของ "ผู้รับ"

        [Required]
        public string Content { get; set; } = string.Empty;
    }
}