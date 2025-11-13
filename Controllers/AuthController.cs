using JWTdemo.Models;
using JWTdemo.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using JWTdemo.Services;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using JWTdemo.Data;

using Microsoft.AspNetCore.Http; // 👈 [เพิ่ม] Import นี้
using System.IO;                 // 👈 [เพิ่ม] Import นี้
using System.Linq;                // 👈 [เพิ่ม] Import นี้

namespace JWTdemo.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController(IAuthService authService, UserDbContext context) : ControllerBase
    {

        [HttpPost("register")]
        public async Task<ActionResult<User>> Register(UserDto request)
        {
            var user = await authService.RegisterAsync(request);
            if (user is null)
            {
                // อันเดิม Return เป็น BadRequest("Username already exists.")
                // return BadRequest("Username already exists.");
                // อันใหมม่ Return เป็น json
                return BadRequest(new { message = "Username already exists." });
            }

            return Ok(user);
        }

        [HttpPost("login")]
        public async Task<ActionResult<TokenResponseDto>> Login(UserDto request)
        {
            var result = await authService.LoginAsync(request);
            if (result is null)
            {
                return BadRequest("Invalid Username or Password.");
            }

            return Ok(result);
        }

        [HttpPost("refresh-token")]
        public async Task<ActionResult<TokenResponseDto>> RefreshToken(RefreshTokenRequestDto request)
        {
            var result = await authService.RefreshTokensAsync(request);
            if (result is null || result.AccessToken is null || result.RefreshToken is null)
            {
                return Unauthorized("Invalid refresh token.");
            }
            return Ok(result);
        }
        // อันเก่า Return เป็น Text
        // [Authorize]
        // [HttpGet]
        // public IActionResult AuthenticatedOnlyEndpoint()
        // {
        //     return Ok("You are authenticated");    
        // }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> AuthenticatedOnlyEndpoint() // ✍️ 1. เปลี่ยนเป็น async Task<IActionResult>
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // ตรวจสอบและแปลง ID จาก string เป็น Guid
            if (!Guid.TryParse(userIdString, out Guid userId))
            {
                return Unauthorized();
            }

            // 2. ✍️ ดึง User Entity ฉบับเต็มจาก Database
            var user = await context.Users.FindAsync(userId);

            if (user == null)
            {
                return NotFound();
            }

            // 3. ✍️ สร้าง Response Object จากข้อมูล Entity
            var userResponse = new
            {
                Id = user.id.ToString(),
                Username = user.Username,
                Role = user.Role, // เราดึง Role จาก Entity แทนที่จะดึงจาก Claims

                // 👇 4. ✍️ เพิ่ม Fields ใหม่จาก Entity
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,

                CreateAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt,

                ProfileImageUrl = user.ProfileImageUrl
            };

            return Ok(userResponse);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("admin-only")]
        public IActionResult AdminOnlyEndpoint()
        {
            return Ok("You are Admin");
        }

        [HttpGet("users")] // 👈 เราจะ GET ไปที่ /api/Auth/users
        [Authorize(Roles = "Admin")] // 👈 เฉพาะ Admin
        public async Task<IActionResult> GetAllUsers()
        {
            // 5. ✍️ สั่ง Service ให้ทำงาน
            var users = await authService.GetAllUsersAsync();

            return Ok(users);
        }

        // Update Profile and Change Password
        [HttpPut("update-profile")]
        [Authorize]
        public async Task<IActionResult> UpdateProfile([FromBody] ProfileUpdateDto request)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            // ถ้าดึง UserId จาก Token ไม่ได้ หรือแปลงไม่ได้ ให้ Unauthorized
            if (!Guid.TryParse(userIdString, out Guid userId)) return Unauthorized();

            var success = await authService.UpdateProfileAsync(userId, request);

            if (!success)
            {
                // ถ้า return false จาก Service มักจะเป็นเพราะ Username/Email ซ้ำ 
                return BadRequest(new { message = "Username or email is already taken, or failed to update." });
            }

            // 💡 สำคัญ: ถ้าอัปเดตสำเร็จ User Context ใน React ต้องถูก Refresh!
            return Ok(new { message = "Profile updated successfully." });
        }

        // ----------------------------------------------------
        // 2. ✍️ [เพิ่ม] Endpoint สำหรับเปลี่ยนรหัสผ่าน (POST)
        // ----------------------------------------------------
        [HttpPost("change-password")]
        [Authorize]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto request)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdString, out Guid userId)) return Unauthorized();

            var success = await authService.ChangePasswordAsync(userId, request);

            if (!success)
            {
                // ถ้า return false มักจะเป็นเพราะรหัสผ่านเก่าไม่ถูกต้อง
                return BadRequest(new { message = "Invalid old password." });
            }

            return Ok(new { message = "Password changed successfully. Please log in again." });
        }

        [HttpPut("users/{userId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateUser(string userId, [FromBody] UserUpdateRequest request)
        {
            if (!Guid.TryParse(userId, out Guid userGuid))
            {
                return BadRequest(new { message = "Invalid User ID format." });
            }

            // 💡 ป้องกัน Admin แก้ไข Role ของตัวเอง (Optional: ถ้าต้องการป้องกันอย่างเข้มงวด)
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userGuid.ToString().Equals(currentUserId, StringComparison.OrdinalIgnoreCase) && request.Role != null)
            {
                if (request.Role != User.FindFirstValue(ClaimTypes.Role))
                {
                    return BadRequest(new { message = "Admin cannot change their own role." });
                }
            }

            var success = await authService.UpdateUserAsync(userGuid, request);

            if (!success)
            {
                // ✍️ ถ้าการอัปเดตล้มเหลว (รวมถึงกรณีที่พยายามแก้ Admin)
                return BadRequest(new { message = "Update failed. Cannot modify Admin role or invalid request." });
            }

            return Ok(new { message = "User updated successfully." });
        }

        [HttpGet("stats")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetOverallStats()
        {
            var stats = await authService.GetOverallStatsAsync();
            return Ok(stats);
        }

        [HttpDelete("users/{userId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteUser(string userId)
        {
            if (!Guid.TryParse(userId, out Guid userGuid))
            {
                return BadRequest("Invalid User ID format.");
            }

            // 💡 ป้องกัน Admin ลบตัวเอง (Security Check)
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userGuid.ToString().Equals(currentUserId, StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest(new { message = "You cannot delete your own account." });
            }

            var success = await authService.DeleteUserAsync(userGuid);

            if (!success) return NotFound();

            return NoContent(); // 204 No Content สำหรับการลบที่สำเร็จ
        }

        [HttpPost("upload-picture")]
        [Authorize] // 👈 (ต้องล็อกอิน)
        public async Task<IActionResult> UploadProfilePicture(IFormFile file) // 👈 [สำคัญ] ต้องใช้ [FromForm]
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(new { message = "No file uploaded." });
            }

            // --- 7. ✍️ [เพิ่ม] การจำกัดขนาดไฟล์ (5 MB) ---
            const long maxFileSize = 5 * 1024 * 1024;
            if (file.Length > maxFileSize)
            {
                return BadRequest(new { message = "File size exceeds the 5MB limit." });
            }

            // --- 8. ✍️ [เพิ่ม] การจำกัดประเภทไฟล์ (นามสกุล) ---
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };

            if (string.IsNullOrEmpty(extension) || !allowedExtensions.Contains(extension))
            {
                return BadRequest(new { message = "Invalid file type. Only .jpg, .jpeg, .png, and .gif are allowed." });
            }

            // --- (โค้ดเดิม) ---
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdString, out Guid userId)) return Unauthorized();

            var newPath = await authService.UploadProfilePictureAsync(userId, file);

            if (newPath == null)
            {
                return NotFound(new { message = "User not found." });
            }

            // --- 9. ✍️ ส่ง Path ใหม่กลับไปให้ Frontend ---
            return Ok(new { profileImageUrl = newPath });
        }


        [HttpGet("chat-users")]
        [Authorize] // 👈 [สำคัญ] อนุญาตให้ "ทุกคนที่ล็อกอิน" (User & Admin)
        public async Task<IActionResult> GetChatUsers()
        {
            var userId = GetCurrentUserId(); // (Helper Function ที่คุณมีอยู่)
            var users = await authService.GetChatUsersAsync(userId);
            return Ok(users);
        }


        // --- (Helper Function) ---
        private Guid GetCurrentUserId()
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            
            if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out Guid userId))
            {
                // นี่ไม่ควรเกิดขึ้นถ้ามี [Authorize]
                throw new InvalidOperationException("User ID not found or invalid in token.");
            }
            return userId;
        }

    }
}

