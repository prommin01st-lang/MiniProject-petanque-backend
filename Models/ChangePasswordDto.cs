using System.ComponentModel.DataAnnotations;

namespace JWTdemo.Models
{
    public class ChangePasswordDto
    {
        [Required]
        public string OldPassword { get; set; }

        [Required]
        public string NewPassword { get; set; }
    }
}