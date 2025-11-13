using System.ComponentModel.DataAnnotations;
namespace JWTdemo.Models
{
    public class CreateTodoCategoryDto
    {
        [Required]
        public string Title { get; set; } = string.Empty;
    }
}