using System.Collections.Generic;

namespace JWTdemo.Models
{
    // DTO นี้จะใช้สำหรับทุกตารางที่ต้องการแบ่งหน้า
    public class PaginatedResultDto<T>
    {
        public List<T> Items { get; set; } = new List<T>();
        public int TotalCount { get; set; } // 👈 จำนวนรายการทั้งหมด
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
    }
}