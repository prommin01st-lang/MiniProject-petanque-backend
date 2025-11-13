namespace JWTdemo.Models
{
    public class GlobalSearchResultDto
    {
        public string Type { get; set; } = string.Empty; // 👈 "User", "Article", "Todo"
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty; // 👈 Path ที่จะ Link ไป (เช่น /admin/users/...)
    }
}