namespace JWTdemo.Models
{
    public class FinanceSummaryDto
    {
        public decimal TotalIncome { get; set; }
        public decimal TotalExpense { get; set; }
        public decimal Balance { get; set; } // 👈 ยอดคงเหลือ (Income - Expense)
    }
}