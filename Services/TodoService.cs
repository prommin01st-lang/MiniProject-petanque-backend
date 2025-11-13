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
    public class TodoService : ITodoService
    {
        private readonly UserDbContext _context;

        public TodoService(UserDbContext context)
        {
            _context = context;
        }

        // === CATEGORY ===

        public async Task<TodoListCategory> CreateCategoryAsync(CreateTodoCategoryDto dto, Guid userId)
        {
            var category = new TodoListCategory
            {
                Title = dto.Title,
                UserId = userId
            };
            _context.TodoListCategories.Add(category);
            await _context.SaveChangesAsync();
            return category;
        }

        public async Task<IEnumerable<TodoListCategory>> GetMyCategoriesAsync(Guid userId)
        {
            return await _context.TodoListCategories
                .Where(c => c.UserId == userId)
                .OrderBy(c => c.Title)
                .ToListAsync();
        }

        // === ITEM ===

        public async Task<TodoItem?> CreateItemAsync(CreateTodoItemDto dto, Guid userId)
        {
            var categoryExists = await _context.TodoListCategories
                .AnyAsync(c => c.Id == dto.CategoryId && c.UserId == userId);
            
            if (!categoryExists)
            {
                return null; // (ถ้าไม่ใช่เจ้าของ หรือ Category ไม่มีอยู่)
            }

            var todoItem = new TodoItem
            {
                Title = dto.Title,
                TodoListCategoryId = dto.CategoryId,
                Priority = dto.Priority,
                Deadline = dto.Deadline,
                IsCompleted = false,
                CreatedAt = DateTime.UtcNow
            };

            _context.TodoItems.Add(todoItem);
            await _context.SaveChangesAsync();
            return todoItem;
        }

        public async Task<IEnumerable<TodoDto>> GetItemsForCategoryAsync(int categoryId, Guid userId)
        {
            return await _context.TodoItems
                .Where(t => t.TodoListCategoryId == categoryId && t.TodoListCategory.UserId == userId)
                .OrderBy(t => t.IsCompleted) 
                .ThenByDescending(t => t.CreatedAt)
                .Select(t => new TodoDto 
                {
                    Id = t.Id,
                    Title = t.Title,
                    IsCompleted = t.IsCompleted,
                    CreatedAt = t.CreatedAt,
                    Priority = t.Priority,
                    Deadline = t.Deadline
                })
                .ToListAsync();
        }

        public async Task<bool> UpdateItemAsync(int itemId, UpdateTodoDto dto, Guid userId)
        {
            var todoItem = await _context.TodoItems
                .FirstOrDefaultAsync(t => t.Id == itemId && t.TodoListCategory.UserId == userId);

            if (todoItem == null) return false; 

            if (dto.Title != null) todoItem.Title = dto.Title;
            if (dto.IsCompleted.HasValue) todoItem.IsCompleted = dto.IsCompleted.Value;
            if (dto.Priority != null) todoItem.Priority = dto.Priority;
            if (dto.Deadline.HasValue) todoItem.Deadline = dto.Deadline.Value;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteItemAsync(int itemId, Guid userId)
        {
            var todoItem = await _context.TodoItems
                .FirstOrDefaultAsync(t => t.Id == itemId && t.TodoListCategory.UserId == userId);

            if (todoItem == null) return false; 

            _context.TodoItems.Remove(todoItem);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}