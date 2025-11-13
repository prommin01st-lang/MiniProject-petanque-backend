using System.ComponentModel.DataAnnotations;

namespace JWTdemo.Entities
{
    public class Notification
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString(); // UUID/GUID

        [Required]
        public string Title { get; set; }

        [Required]
        public string Subtitle { get; set; }

        [Required]
        public string AvatarType { get; set; } // "icon", "image", "text"

        [Required]
        public string AvatarValue { get; set; } // "bx-user", "/img.png", "MG"

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}