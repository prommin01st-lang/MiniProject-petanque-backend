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
    public class ArticleService : IArticleService
    {
        private readonly UserDbContext _context;

        public ArticleService(UserDbContext context)
        {
            _context = context;
        }

        // 1. (Admin) ดึงทั้งหมด
        public async Task<IEnumerable<Article>> GetAllArticlesAsync()
        {
            return await _context.Articles
                .Include(a => a.Author) // 👈 (Optional) ดึงข้อมูลผู้เขียนมาด้วย
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();
        }

        // 2. (User) ดึงเฉพาะที่เผยแพร่แล้ว
        public async Task<IEnumerable<Article>> GetPublishedArticlesAsync()
        {
            return await _context.Articles
                .Include(a => a.Author)
                .Where(a => a.PublishedAt != null) // 👈 กรองเฉพาะที่เผยแพร่
                .OrderByDescending(a => a.PublishedAt)
                .ToListAsync();
        }

        // 3. (User) ดึงบทความเดียว
        public async Task<Article?> GetArticleByIdAsync(int id)
        {
            return await _context.Articles
                .Include(a => a.Author)
                .FirstOrDefaultAsync(a => a.Id == id && a.PublishedAt != null);
        }

        // 4. (Admin) สร้างบทความ
        public async Task<Article> CreateArticleAsync(CreateArticleDto dto, Guid authorUserId)
        {
            var article = new Article
            {
                Title = dto.Title,
                Content = dto.Content,
                AuthorUserId = authorUserId,
                CreatedAt = DateTime.UtcNow,
                PublishedAt = dto.IsPublished ? DateTime.UtcNow : null // 👈 ตั้งค่าเผยแพร่
            };

            _context.Articles.Add(article);
            await _context.SaveChangesAsync();
            return article;
        }

        // 5. (Admin) อัปเดตบทความ
        public async Task<bool> UpdateArticleAsync(int id, UpdateArticleDto dto, Guid authorUserId)
        {
            // (Admin สามารถแก้บทความของใครก็ได้, ถ้าอยากให้แก้แค่ของตัวเอง ให้เพิ่ม .Where(a => a.AuthorUserId == authorUserId))
            var article = await _context.Articles.FindAsync(id);

            if (article == null) return false;

            if (dto.Title != null) article.Title = dto.Title;
            if (dto.Content != null) article.Content = dto.Content;

            // Logic การเผยแพร่
            if (dto.IsPublished.HasValue)
            {
                if (dto.IsPublished.Value && article.PublishedAt == null)
                {
                    // ถ้าสั่ง "เผยแพร่" (จากร่าง)
                    article.PublishedAt = DateTime.UtcNow;
                }
                else if (!dto.IsPublished.Value)
                {
                    // ถ้าสั่ง "ยกเลิกเผยแพร่" (กลับเป็นร่าง)
                    article.PublishedAt = null;
                }
            }

            await _context.SaveChangesAsync();
            return true;
        }

        // 6. (Admin) ลบบทความ
        public async Task<bool> DeleteArticleAsync(int id, Guid authorUserId)
        {
            // (Admin สามารถลบบทความของใครก็ได้)
            var article = await _context.Articles.FindAsync(id);
            if (article == null) return false;

            _context.Articles.Remove(article);
            await _context.SaveChangesAsync();
            return true;
        }


        public async Task<bool> ToggleLikeArticleAsync(int articleId, Guid userId)
        {
            // 1. ค้นหา "Like" ที่มีอยู่ (ว่า User นี้เคยกด Like บทความนี้หรือยัง)
            var existingLike = await _context.ArticleLikes
                .FirstOrDefaultAsync(al => al.ArticleId == articleId && al.UserId == userId);

            if (existingLike == null)
            {
                // 2. ถ้าไม่เคย Like -> "กด Like" (สร้างใหม่)
                var newLike = new ArticleLike
                {
                    ArticleId = articleId,
                    UserId = userId
                };
                _context.ArticleLikes.Add(newLike);
                await _context.SaveChangesAsync();
                return true; // (สถานะใหม่: Liked)
            }
            else
            {
                // 3. ถ้าเคย Like -> "Unlike" (ลบทิ้ง)
                _context.ArticleLikes.Remove(existingLike);
                await _context.SaveChangesAsync();
                return false; // (สถานะใหม่: Unliked)
            }
        }

        // 8. (Logic: เช็คสถานะ Like)
        public async Task<LikeStatusDto> GetArticleLikeStatusAsync(int articleId, Guid? userId)
        {
            // 1. นับ Like ทั้งหมดของบทความนี้
            var likeCount = await _context.ArticleLikes
                .CountAsync(al => al.ArticleId == articleId);

            bool isLikedByMe = false;

            // 2. ถ้า User ล็อกอิน (userId ไม่ใช่ null)
            if (userId.HasValue)
            {
                // ให้เช็คว่า User คนนี้ Like หรือยัง
                isLikedByMe = await _context.ArticleLikes
                    .AnyAsync(al => al.ArticleId == articleId && al.UserId == userId.Value);
            }

            return new LikeStatusDto
            {
                LikeCount = likeCount,
                IsLikedByCurrentUser = isLikedByMe
            };
        }
    }
}