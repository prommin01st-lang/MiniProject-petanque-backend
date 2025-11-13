using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JWTdemo.Entities
{
    // นี่คือตาราง "สะพาน" ที่เก็บว่า "ใคร" (UserId) Like "บทความไหน" (ArticleId)
    public class ArticleLike
    {
        [Key]
        public int Id { get; set; }

        // 1. ✍️ Foreign Key ไปยัง "บทความ"
        public int ArticleId { get; set; }

        [ForeignKey("ArticleId")]
        public virtual Article Article { get; set; } = null!;

        // 2. ✍️ Foreign Key ไปยัง "ผู้ใช้"
        public Guid UserId { get; set; }
        
        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;
    }
}