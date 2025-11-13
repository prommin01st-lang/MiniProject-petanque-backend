using JWTdemo.Entities;
using JWTdemo.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace JWTdemo.Services
{
    public interface IArticleService
    {
        // (สำหรับ Admin - ดึงทั้งหมดรวมฉบับร่าง)
        Task<IEnumerable<Article>> GetAllArticlesAsync(); 
        
        // (สำหรับ User - ดึงเฉพาะที่เผยแพร่แล้ว)
        Task<IEnumerable<Article>> GetPublishedArticlesAsync(); 

        // (สำหรับ User - ดึงบทความเดียว)
        Task<Article?> GetArticleByIdAsync(int id); 

        // (สำหรับ Admin - สร้าง)
        Task<Article> CreateArticleAsync(CreateArticleDto dto, Guid authorUserId);

        // (สำหรับ Admin - อัปเดต)
        Task<bool> UpdateArticleAsync(int id, UpdateArticleDto dto, Guid authorUserId);

        // (สำหรับ Admin - ลบ)
        Task<bool> DeleteArticleAsync(int id, Guid authorUserId);

        Task<bool> ToggleLikeArticleAsync(int articleId, Guid userId);

        // (สำหรับ "เช็ค" ข้อมูลตอนโหลดหน้า)
        Task<LikeStatusDto> GetArticleLikeStatusAsync(int articleId, Guid? userId);
    }
}