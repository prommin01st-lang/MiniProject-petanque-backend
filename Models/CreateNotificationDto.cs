using System.ComponentModel.DataAnnotations;
using System.Collections.Generic; 

namespace JWTdemo.Models
{
    // นี่คือข้อมูลที่ React Form จะต้องส่งมา
    public class CreateNotificationDto
    {
        [Required]
        public string Title { get; set; }

        [Required]
        public string Subtitle { get; set; }

        [Required]
        public string AvatarType { get; set; } // "icon", "image", "text"

        [Required]
        public string AvatarValue { get; set; }

        [Required]
        public List<string> TargetUserIds { get; set; }
    }
}