using JWTdemo.Data;
using JWTdemo.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JWTdemo.Services
{
    public class SearchService : ISearchService
    {
        private readonly UserDbContext _context;

        public SearchService(UserDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<GlobalSearchResultDto>> SearchAsync(string query, Guid userId, bool isAdmin)
        {
            var results = new List<GlobalSearchResultDto>();

            // (1) ✍️ ค้นหา Users (ถ้าเป็น Admin)
            if (isAdmin)
            {
                var users = await _context.Users
                    .Where(u => u.Username.Contains(query) || (u.Email != null && u.Email.Contains(query)))
                    .Take(5)
                    .Select(u => new GlobalSearchResultDto
                    {
                        Type = "User",
                        Title = u.Username,
                        Description = u.Email ?? "No Email",
                        Url = $"/admin/user-management" // 👈 (Link ไปหน้าจัดการ User)
                    })
                    .ToListAsync();
                results.AddRange(users);

                var draftArticles = await _context.Articles
                    .Include(a => a.Author)
                    .Where(a => a.PublishedAt == null && // 👈 (ค้นหาเฉพาะ Drafts)
                               (EF.Functions.Like(a.Title, $"%{query}%")))
                    .Take(5)
                    .Select(a => new GlobalSearchResultDto
                    {
                        Type = "Admin (Draft)",
                        Title = a.Title,
                        Description = $"By: {a.Author.Username} (Draft)",
                        Url = $"/admin/articles/edit/{a.Id}" // 👈 (Link ไปหน้า Admin Edit)
                    })
                    .ToListAsync();
                results.AddRange(draftArticles);

                // (3.3) ✍️ [เพิ่ม] ค้นหา Notification History
                var adminNotifications = await _context.Notifications
                    .Where(n => EF.Functions.Like(n.Title, $"%{query}%") || EF.Functions.Like(n.Subtitle, $"%{query}%"))
                    .OrderByDescending(n => n.CreatedAt)
                    .Take(5)
                    .Select(n => new GlobalSearchResultDto
                    {
                        Type = "Admin (Notification)",
                        Title = n.Title,
                        Description = n.Subtitle,
                        Url = $"/admin/notifications" // 👈 (Link ไปหน้า Admin Noti)
                    })
                    .ToListAsync();
                results.AddRange(adminNotifications);
            }

            // (2) ✍️ ค้นหา Articles (Public)
            var articles = await _context.Articles
                .Where(a => a.PublishedAt != null && (a.Title.Contains(query) || a.Content.Contains(query)))
                .Take(5)
                .Select(a => new GlobalSearchResultDto
                {
                    Type = "Article",
                    Title = a.Title,
                    Description = a.Content.Length > 100 ? a.Content.Substring(0, 100) + "..." : a.Content,
                    Url = $"/articles/{a.Id}" // 👈 (Link ไปหน้าอ่าน)
                })
                .ToListAsync();
            results.AddRange(articles);

            // (3) ✍️ ค้นหา To-Do Items (เฉพาะของตัวเอง)
            var todos = await _context.TodoItems
                .Where(t =>
                    t.TodoListCategory.UserId == userId &&
                    (
                        t.Title.Contains(query) ||
                        t.TodoListCategory.Title.Contains(query)
                    )
                )
                .Take(10)
                .Select(t => new GlobalSearchResultDto
                {
                    Type = "Todo",
                    Title = t.Title,
                    Description = $"In list: {t.TodoListCategory.Title}",
                    Url = "/todos"
                })
                .ToListAsync();
            results.AddRange(todos);


            var conversations = await _context.Conversations
                .Include(c => c.User1) // 👈 (Join User1)
                .Include(c => c.User2) // 👈 (Join User2)
                
                // (4.1 ดึงห้องแชทที่ "ฉัน" (userId) อยู่)
                .Where(c => c.User1Id == userId || c.User2Id == userId)
                
                // (4.2 และ "คู่สนทนา" (Other User) มีชื่อตรงกับคำค้น)
                .Where(c =>
                    (c.User1Id == userId && EF.Functions.Like(c.User2.Username, $"%{query}%")) || // (ถ้าฉันคือ User1, ให้ค้นหา User2)
                    (c.User2Id == userId && EF.Functions.Like(c.User1.Username, $"%{query}%"))    // (ถ้าฉันคือ User2, ให้ค้นหา User1)
                )
                .Take(5)
                .Select(c => new GlobalSearchResultDto
                {
                    Type = "Message",
                    // ✍️ (แสดงชื่อ "คู่สนทนา" เป็น Title)
                    Title = (c.User1Id == userId) ? c.User2.Username : c.User1.Username,
                    Description = "Go to conversation...",
                    Url = "/message" // 👈 (Link ไปหน้า Message หลัก)
                })
                .ToListAsync();
            
            results.AddRange(conversations);


            // (เรียงผลลัพธ์ตาม Type)
            return results.OrderBy(r => r.Type);
        }
    }
}