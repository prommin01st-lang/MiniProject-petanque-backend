using JWTdemo.Models;
using JWTdemo.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace JWTdemo.Controllers
{
    [ApiController]
    [Route("api/[controller]")] // 👈 Path หลัก: /api/Finance
    [Authorize] // 👈 [สำคัญ] ทุกฟังก์ชันในนี้ "ต้องล็อกอิน"
    public class FinanceController : ControllerBase
    {
        private readonly IFinanceService _financeService;

        public FinanceController(IFinanceService financeService)
        {
            _financeService = financeService;
        }

        // 1. [GET] /api/Finance (ดึงประวัติ "ของฉัน" ทั้งหมด)
        [HttpGet]
        public async Task<IActionResult> GetMyTransactions(
            [FromQuery] int pageNumber = 1, 
            [FromQuery] int pageSize = 10,
            [FromQuery] string? type = null, // 👈 [ใหม่]
            [FromQuery] DateTime? startDate = null, // 👈 [ใหม่]
            [FromQuery] DateTime? endDate = null)  // 👈 [ใหม่]
        {
            var userId = GetCurrentUserId();
            
            // ✍️ [แก้ไข] ส่ง Parameters ใหม่เข้าไปใน Service
            var transactions = await _financeService.GetMyTransactionsAsync(
                userId, 
                pageNumber, 
                pageSize, 
                type, 
                startDate, 
                endDate);
                
            return Ok(transactions);
        }
        // 2. [GET] /api/Finance/summary (ดึง "ยอดสรุป" ของฉัน)
        [HttpGet("summary")]
        public async Task<IActionResult> GetMySummary()
        {
            var userId = GetCurrentUserId();
            var summary = await _financeService.GetSummaryAsync(userId);
            return Ok(summary);
        }

        // 3. [POST] /api/Finance (สร้างรายการใหม่)
        [HttpPost]
        public async Task<IActionResult> CreateTransaction([FromBody] CreateTransactionDto dto)
        {
            if (dto.Amount <= 0)
            {
                return BadRequest(new { message = "Amount must be greater than zero." });
            }
            
            var userId = GetCurrentUserId();
            var newTransaction = await _financeService.CreateTransactionAsync(dto, userId);
            return Ok(newTransaction);
        }

        // --- (Helper Function) ---
        private Guid GetCurrentUserId()
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdString, out Guid userId))
            {
                throw new InvalidOperationException("User ID not found in token.");
            }
            return userId;
        }
    }
}