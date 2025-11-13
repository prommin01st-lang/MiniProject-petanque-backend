using System.ComponentModel.DataAnnotations;

namespace JWTdemo.Models
{
    public class CreateArticleDto
    {
        [Required]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Content { get; set; } = string.Empty; // 👈 เนื้อหา (Markdown)

        public bool IsPublished { get; set; } = false; // 👈 (Optional) สร้างเป็นฉบับร่าง หรือ เผยแพร่เลย
    }
}