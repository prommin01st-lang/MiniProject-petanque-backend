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
    [Route("api/todo")] 
    [Authorize] 
    public class TodoController : ControllerBase
    {
        private readonly ITodoService _todoService;

        public TodoController(ITodoService todoService)
        {
            _todoService = todoService;
        }

        // [GET] /api/todo/categories (ดึง "บอร์ด" ของฉันทั้งหมด)
        [HttpGet("categories")]
        public async Task<IActionResult> GetMyCategories()
        {
            var userId = GetCurrentUserId();
            var categories = await _todoService.GetMyCategoriesAsync(userId);
            return Ok(categories);
        }
        
        // [POST] /api/todo/categories (สร้าง "บอร์ด" ใหม่)
        [HttpPost("categories")]
        public async Task<IActionResult> CreateCategory([FromBody] CreateTodoCategoryDto dto)
        {
            var userId = GetCurrentUserId();
            var newCategory = await _todoService.CreateCategoryAsync(dto, userId);
            return Ok(newCategory);
        }

        // [GET] /api/todo/items/{categoryId} (ดึง "รายการย่อย" ในบอร์ดนี้)
        [HttpGet("items/{categoryId}")]
        public async Task<IActionResult> GetItemsForCategory(int categoryId)
        {
            var userId = GetCurrentUserId();
            var items = await _todoService.GetItemsForCategoryAsync(categoryId, userId);
            return Ok(items);
        }

        // [POST] /api/todo/items (สร้าง "รายการย่อย" ใหม่)
        [HttpPost("items")]
        public async Task<IActionResult> CreateItem([FromBody] CreateTodoItemDto dto)
        {
            var userId = GetCurrentUserId();
            var newItem = await _todoService.CreateItemAsync(dto, userId);
            if (newItem == null) return BadRequest("Invalid Category ID or permission denied.");
            return Ok(newItem);
        }
        
        // [PUT] /api/todo/items/{itemId} (อัปเดต "รายการย่อย")
        [HttpPut("items/{itemId}")]
        public async Task<IActionResult> UpdateItem(int itemId, [FromBody] UpdateTodoDto dto)
        {
            var userId = GetCurrentUserId();
            var success = await _todoService.UpdateItemAsync(itemId, dto, userId);
            if (!success) return NotFound();
            return Ok(new { message = "Todo item updated." });
        }

        // [DELETE] /api/todo/items/{itemId} (ลบ "รายการย่อย")
        [HttpDelete("items/{itemId}")]
        public async Task<IActionResult> DeleteItem(int itemId)
        {
            var userId = GetCurrentUserId();
            var success = await _todoService.DeleteItemAsync(itemId, userId);
            if (!success) return NotFound();
            return NoContent();
        }

        // --- (Helper Function) ---
        private Guid GetCurrentUserId()
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out Guid userId))
            {
                throw new InvalidOperationException("User ID not found in token.");
            }
            return userId;
        }
    }
}