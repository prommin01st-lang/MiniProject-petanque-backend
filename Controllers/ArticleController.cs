using JWTdemo.Models;
using JWTdemo.Services;
using Microsoft.AspNetCore.Authorization; // 👈 [สำคัญ]
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace JWTdemo.Controllers
{
    [ApiController]
    [Route("api/[controller]")] // 👈 Path หลัก: /api/Article
    public class ArticleController : ControllerBase
    {
        private readonly IArticleService _articleService;

        public ArticleController(IArticleService articleService)
        {
            _articleService = articleService;
        }

        // --- 1. Endpoints สำหรับ "User ทั่วไป" (Public) ---
        // (เราจะอนุญาตให้ทุกคนที่ล็อกอินแล้ว [Authorize] อ่านได้)
        // (ถ้าอยากให้คน "ไม่ล็อกอิน" อ่านได้ ให้ใช้ [AllowAnonymous])

        // [GET] /api/Article/public (ดึงบทความที่ "เผยแพร่" แล้วทั้งหมด)
        [HttpGet("public")]
        [AllowAnonymous] // 👈 (อนุญาตให้ทุกคน แม้ไม่ได้ล็อกอิน)
        public async Task<IActionResult> GetPublishedArticles()
        {
            var articles = await _articleService.GetPublishedArticlesAsync();
            return Ok(articles);
        }

        // [GET] /api/Article/public/{id} (ดึงบทความเดียว)
        [HttpGet("public/{id}")]
        [AllowAnonymous] // 👈 (อนุญาตให้ทุกคน)
        public async Task<IActionResult> GetArticleById(int id)
        {
            var article = await _articleService.GetArticleByIdAsync(id);
            if (article == null) return NotFound();
            return Ok(article);
        }

        // --- 2. Endpoints สำหรับ "Admin" ---

        // [GET] /api/Article/admin (ดึงบทความ "ทั้งหมด" รวมฉบับร่าง)
        [HttpGet("admin")]
        [Authorize(Roles = "Admin")] // 👈 เฉพาะ Admin
        public async Task<IActionResult> GetAllArticles()
        {
            var articles = await _articleService.GetAllArticlesAsync();
            return Ok(articles);
        }

        // [POST] /api/Article (สร้างบทความใหม่)
        [HttpPost]
        [Authorize(Roles = "Admin")] // 👈 เฉพาะ Admin
        public async Task<IActionResult> CreateArticle([FromBody] CreateArticleDto dto)
        {
            var userId = GetCurrentUserId();
            var newArticle = await _articleService.CreateArticleAsync(dto, userId);
            return Ok(newArticle);
        }

        // [PUT] /api/Article/{id} (อัปเดตบทความ)
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")] // 👈 เฉพาะ Admin
        public async Task<IActionResult> UpdateArticle(int id, [FromBody] UpdateArticleDto dto)
        {
            var userId = GetCurrentUserId(); // (เผื่อใช้เช็คสิทธิ์ในอนาคต)
            var success = await _articleService.UpdateArticleAsync(id, dto, userId);
            if (!success) return NotFound();
            return Ok(new { message = "Article updated." });
        }

        // [DELETE] /api/Article/{id} (ลบบทความ)
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")] // 👈 เฉพาะ Admin
        public async Task<IActionResult> DeleteArticle(int id)
        {
            var userId = GetCurrentUserId();
            var success = await _articleService.DeleteArticleAsync(id, userId);
            if (!success) return NotFound();
            return NoContent(); // (204 ลบสำเร็จ)
        }


        // --- (Helper Function) ---
        private Guid GetCurrentUserId()
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out Guid userId))
            {
                // นี่ไม่ควรเกิดขึ้นถ้ามี [Authorize]
                throw new InvalidOperationException("User ID not found in token.");
            }
            return userId;
        }


        // 1. [POST] /api/Article/{id}/like (สำหรับ "กด" Like/Unlike)
        [HttpPost("{id}/like")]
        [Authorize] // 👈 [สำคัญ] ต้องล็อกอินเท่านั้น
        public async Task<IActionResult> ToggleLike(int id)
        {
            var userId = GetCurrentUserId(); // (ดึง ID จาก Token)
            var newStatus = await _articleService.ToggleLikeArticleAsync(id, userId);
            
            // ส่งสถานะใหม่กลับไป (true = Like, false = Unlike)
            return Ok(new { isLiked = newStatus });
        }

        // 2. [GET] /api/Article/{id}/like-status (สำหรับ "เช็ค" สถานะ)
        [HttpGet("{id}/like-status")]
        [AllowAnonymous] // 👈 (อนุญาตให้ทุกคน (แม้ไม่ล็อกอิน) ดู "จำนวน" Like ได้)
        public async Task<IActionResult> GetLikeStatus(int id)
        {
            // (ลองดึง ID, ถ้ามี Token)
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            Guid? userId = Guid.TryParse(userIdString, out Guid parsedId) ? parsedId : null;

            var status = await _articleService.GetArticleLikeStatusAsync(id, userId);
            return Ok(status);
        }
    }
}