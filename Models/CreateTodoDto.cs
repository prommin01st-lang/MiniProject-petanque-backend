using System.ComponentModel.DataAnnotations;

namespace JWTdemo.Models
{
    public class CreateTodoDto
    {
        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;
    }
}