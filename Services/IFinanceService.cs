using JWTdemo.Entities;
using JWTdemo.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace JWTdemo.Services
{
    public interface IFinanceService
    {
        // (ดึงประวัติทั้งหมด)
        Task<PaginatedResultDto<Transaction>> GetMyTransactionsAsync(
            Guid userId, 
            int pageNumber, 
            int pageSize,
            string? type, // 👈 [ใหม่] "Income" หรือ "Expense"
            DateTime? startDate, // 👈 [ใหม่] วันที่เริ่มต้น
            DateTime? endDate // 👈 [ใหม่] วันที่สิ้นสุด
        );        // (สร้างรายการใหม่)
        Task<Transaction> CreateTransactionAsync(CreateTransactionDto dto, Guid userId);

        // (คำนวณสรุป)
        Task<FinanceSummaryDto> GetSummaryAsync(Guid userId);
    }
}