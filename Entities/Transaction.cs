using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JWTdemo.Entities
{
    public class Transaction
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Title { get; set; } = string.Empty;

        // 1. ✍️ [สำคัญ] "จำนวนเงิน" (เราใช้ decimal เพื่อความแม่นยำ)
        [Column(TypeName = "decimal(18, 2)")]
        public decimal Amount { get; set; }

        // 2. ✍️ "ประเภท" (รายรับ หรือ รายจ่าย)
        [Required]
        public string Type { get; set; } = "Expense"; // ( "Income" | "Expense" )

        public DateTime Date { get; set; } = DateTime.UtcNow;

        // 3. ✍️ "เจ้าของ" ธุรกรรม
        public Guid UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;
    }
}