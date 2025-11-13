namespace JWTdemo.Models
{
    public class UpdateTodoDto
    {
        public string? Title { get; set; }
        public bool? IsCompleted { get; set; }
        public string? Priority { get; set; }
        public DateTime? Deadline { get; set; }
    }
}