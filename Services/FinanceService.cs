using JWTdemo.Data;
using JWTdemo.Entities;
using JWTdemo.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JWTdemo.Services
{
    public class FinanceService : IFinanceService
    {
        private readonly UserDbContext _context;

        public FinanceService(UserDbContext context)
        {
            _context = context;
        }

        // 1. สร้างรายการ
        public async Task<Transaction> CreateTransactionAsync(CreateTransactionDto dto, Guid userId)
        {
            var transaction = new Transaction
            {
                Title = dto.Title,
                Amount = dto.Amount,
                Type = dto.Type,
                UserId = userId,
                Date = DateTime.UtcNow
            };

            _context.Transactions.Add(transaction);
            await _context.SaveChangesAsync();
            return transaction;
        }

        // 2. ดึงประวัติ
        public async Task<PaginatedResultDto<Transaction>> GetMyTransactionsAsync(
        Guid userId, 
        int pageNumber, 
        int pageSize,
        string? type, // 👈 [ใหม่]
        DateTime? startDate, // 👈 [ใหม่]
        DateTime? endDate)  // 👈 [ใหม่]
    {
        // 1. สร้าง Base Query
        IQueryable<Transaction> query = _context.Transactions
            .Where(t => t.UserId == userId);
        
        // 2. ✍️ [เพิ่ม] Logic การกรองตาม Type (Income/Expense)
        if (!string.IsNullOrEmpty(type))
        {
            query = query.Where(t => t.Type == type);
        }

        // 3. ✍️ [เพิ่ม] Logic การกรองตามช่วงวันที่
        if (startDate.HasValue)
        {
            query = query.Where(t => t.Date >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            // (เราบวก 1 วันเพื่อรวมสิ้นวันนั้นๆ ด้วย)
            query = query.Where(t => t.Date < endDate.Value.AddDays(1)); 
        }

        // 4. นับจำนวน "ทั้งหมด" (หลังจากการกรอง)
        var totalCount = await query.CountAsync();

        // 5. ดึงข้อมูล "เฉพาะหน้า" นั้นๆ
        var items = await query
            .OrderByDescending(t => t.Date)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        // 6. ส่งผลลัพธ์กลับ
        return new PaginatedResultDto<Transaction>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

        // 3. ⭐️ [การคำนวณ] สรุปยอด
        public async Task<FinanceSummaryDto> GetSummaryAsync(Guid userId)
        {
            // 3.1 คำนวณ "รายรับ" ทั้งหมด
            var totalIncome = await _context.Transactions
                .Where(t => t.UserId == userId && t.Type == "Income")
                .SumAsync(t => t.Amount);

            // 3.2 คำนวณ "รายจ่าย" ทั้งหมด
            var totalExpense = await _context.Transactions
                .Where(t => t.UserId == userId && t.Type == "Expense")
                .SumAsync(t => t.Amount);

            return new FinanceSummaryDto
            {
                TotalIncome = totalIncome,
                TotalExpense = totalExpense,
                Balance = totalIncome - totalExpense // 👈 (ยอดคงเหลือ)
            };
        }
    }
}