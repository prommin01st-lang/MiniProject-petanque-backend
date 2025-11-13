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
    public class CommentService : ICommentService
    {
        private readonly UserDbContext _context;
        private readonly INotificationService _notificationService;
        public CommentService(UserDbContext context ,INotificationService notificationService)
        {
            _context = context;
            _notificationService = notificationService; // 👈 (เก็บไว้)
        }

        // 1. (Public) ดึง Comment ทั้งหมด
        public async Task<IEnumerable<CommentDto>> GetCommentsForArticleAsync(int articleId)
        {
            return await _context.ArticleComments
                .Include(c => c.User) // 👈 [สำคัญ] Join ตาราง User
                .Where(c => c.ArticleId == articleId)
                .OrderBy(c => c.CreatedAt) // 👈 เรียงจากเก่าไปใหม่
                .Select(c => new CommentDto // 👈 [สำคัญ] แปลงเป็น DTO
                {
                    Id = c.Id,
                    Content = c.Content,
                    CreatedAt = c.CreatedAt,
                    UserId = c.UserId,
                    Username = c.User.Username, // (ดึงจาก User ที่ Join มา)
                    ProfileImageUrl = c.User.ProfileImageUrl // (ดึงจาก User ที่ Join มา)
                })
                .ToListAsync();
        }

        // 2. (Auth) สร้าง Comment
        public async Task<CommentDto?> CreateCommentAsync(CreateCommentDto dto, Guid userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return null; 

            var comment = new ArticleComment
            {
                Content = dto.Content,
                ArticleId = dto.ArticleId,
                UserId = userId,
                CreatedAt = DateTime.UtcNow
            };

            _context.ArticleComments.Add(comment);
            await _context.SaveChangesAsync();

            // --- 👇 4. [เพิ่ม] Logic การยิง Notification ---
            try
            {
                // 4.1 ดึง "บทความ" เพื่อหา "เจ้าของ"
                var article = await _context.Articles.FindAsync(dto.ArticleId);

                // 4.2 [สำคัญ] ถ้ามีบทความ และ "คน Comment" ไม่ใช่ "เจ้าของบทความ"
                if (article != null && article.AuthorUserId != userId)
                {
                    // 4.3 สร้าง DTO สำหรับ Notification
                    var notiDto = new CreateNotificationDto
                    {
                        Title = $"{user.Username} commented on your post",
                        Subtitle = comment.Content.Length > 50 
                                    ? comment.Content.Substring(0, 50) + "..." 
                                    : comment.Content,
                        AvatarType = "icon",
                        AvatarValue = "bx-comment-dots", // 👈 (ไอคอน Comment)
                        TargetUserIds = new List<string> { article.AuthorUserId.ToString() } // 👈 ยิงไปหาเจ้าของบทความ
                    };
                    
                    // 4.4 สั่งยิง Noti!
                    await _notificationService.CreateNotificationAsync(notiDto);
                }
            }
            catch (Exception ex)
            {
                // (ถ้า Noti ล่ม ก็ไม่เป็นไร Comment ยังคงสร้างสำเร็จ)
                Console.WriteLine($"Failed to send comment notification: {ex.Message}");
            }
            // ---------------------------------------------

            // (ส่ง CommentDto กลับไป - โค้ดเดิม)
            return new CommentDto
            {
                Id = comment.Id,
                Content = comment.Content,
                CreatedAt = comment.CreatedAt,
                UserId = user.id,
                Username = user.Username,
                ProfileImageUrl = user.ProfileImageUrl
            };
        }

        // 3. (Auth/Admin) ลบ Comment
        public async Task<bool> DeleteCommentAsync(int commentId, Guid userId, bool isAdmin)
        {
            var comment = await _context.ArticleComments.FindAsync(commentId);
            if (comment == null) return false; // (ไม่เจอ Comment)

            // [สำคัญ] ตรวจสอบสิทธิ์
            // ถ้าไม่ใช่ Admin และ ไม่ใช่เจ้าของ Comment
            if (!isAdmin && comment.UserId != userId)
            {
                return false; // 👈 (ไม่มีสิทธิ์ลบ)
            }

            _context.ArticleComments.Remove(comment);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}