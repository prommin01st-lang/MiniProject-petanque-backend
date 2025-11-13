namespace JWTdemo.Models
{
    public class UpdateArticleDto
    {
        public string? Title { get; set; }
        public string? Content { get; set; }
        public bool? IsPublished { get; set; }
    }
}