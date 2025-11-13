using JWTdemo.Entities;
using JWTdemo.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace JWTdemo.Services
{
    // ✍️ นี่คือ Interface ใหม่สำหรับระบบ 2 ตาราง
    public interface ITodoService
    {
        // --- Category (บอร์ด) ---
        Task<TodoListCategory> CreateCategoryAsync(CreateTodoCategoryDto dto, Guid userId);
        Task<IEnumerable<TodoListCategory>> GetMyCategoriesAsync(Guid userId);
        // (เรายังไม่ทำ Update/Delete Category ตอนนี้)

        // --- Item (รายการย่อย) ---
        Task<TodoItem?> CreateItemAsync(CreateTodoItemDto dto, Guid userId); // (แก้เป็น Nullable)
        Task<IEnumerable<TodoDto>> GetItemsForCategoryAsync(int categoryId, Guid userId);
        Task<bool> UpdateItemAsync(int itemId, UpdateTodoDto dto, Guid userId);
        Task<bool> DeleteItemAsync(int itemId, Guid userId);
    }
}