using System.ComponentModel.DataAnnotations;

namespace JWTdemo.Models
{
    public class CreateCommentDto
    {
        [Required]
        public string Content { get; set; } = string.Empty;

        [Required]
        public int ArticleId { get; set; } // 👈 Comment นี้สำหรับบทความไหน
    }
}