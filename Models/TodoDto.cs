namespace JWTdemo.Models
{
    public class TodoDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public bool IsCompleted { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? Priority { get; set; } 
        public DateTime? Deadline { get; set; }
    }
}