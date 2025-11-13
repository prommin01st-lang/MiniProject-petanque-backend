using JWTdemo.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace JWTdemo.Services
{
    public interface ICommentService
    {
        // (Public) ดึง Comment ทั้งหมดของบทความ (ArticleId)
        Task<IEnumerable<CommentDto>> GetCommentsForArticleAsync(int articleId);

        // (Auth) สร้าง Comment
        Task<CommentDto?> CreateCommentAsync(CreateCommentDto dto, Guid userId);

        // (Auth/Admin) ลบ Comment
        Task<bool> DeleteCommentAsync(int commentId, Guid userId, bool isAdmin);
    }
}