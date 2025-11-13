using System.ComponentModel.DataAnnotations;

namespace JWTdemo.Models
{
    public class CreateTransactionDto
    {
        [Required]
        public string Title { get; set; } = string.Empty;

        [Required]
        public decimal Amount { get; set; } // 👈 จำนวนเงิน (ห้ามเป็น 0)

        [Required]
        public string Type { get; set; } = "Expense"; // ( "Income" | "Expense" )
    }
}