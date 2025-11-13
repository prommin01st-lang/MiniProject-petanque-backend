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
    [Route("api/[controller]")] // 👈 Path หลัก: /api/Comment
    public class CommentController : ControllerBase
    {
        private readonly ICommentService _commentService;

        public CommentController(ICommentService commentService)
        {
            _commentService = commentService;
        }

        // 1. [GET] /api/Comment/{articleId} (ดึง Comment ทั้งหมด)
        [HttpGet("{articleId}")]
        [AllowAnonymous] // 👈 (อนุญาตให้ทุกคนอ่าน Comment ได้)
        public async Task<IActionResult> GetComments(int articleId)
        {
            var comments = await _commentService.GetCommentsForArticleAsync(articleId);
            return Ok(comments);
        }

        // 2. [POST] /api/Comment (สร้าง Comment)
        [HttpPost]
        [Authorize] // 👈 (ต้องล็อกอินถึงจะ Comment ได้)
        public async Task<IActionResult> CreateComment([FromBody] CreateCommentDto dto)
        {
            var userId = GetCurrentUserId();
            var newComment = await _commentService.CreateCommentAsync(dto, userId);
            
            if (newComment == null) return BadRequest("User not found.");

            return Ok(newComment); // (ส่ง Comment ใหม่กลับไป)
        }

        // 3. [DELETE] /api/Comment/{commentId} (ลบ Comment)
        [HttpDelete("{commentId}")]
        [Authorize] // 👈 (ต้องล็อกอิน)
        public async Task<IActionResult> DeleteComment(int commentId)
        {
            var userId = GetCurrentUserId();
            bool isAdmin = User.IsInRole("Admin"); // 👈 เช็คว่าเป็น Admin หรือไม่

            var success = await _commentService.DeleteCommentAsync(commentId, userId, isAdmin);
            
            if (!success) return Forbid(); // (403 Forbidden - ไม่มีสิทธิ์ลบ)

            return NoContent(); // (204 ลบสำเร็จ)
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