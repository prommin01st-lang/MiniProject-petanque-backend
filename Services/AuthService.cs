using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Azure;
using JWTdemo.Data;
using JWTdemo.Entities;
using JWTdemo.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.VisualBasic;
using System.IO;



namespace JWTdemo.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _webHostEnvironment;


        public AuthService(UserDbContext context, IConfiguration configuration, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _configuration = configuration;
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task<TokenResponseDto?> LoginAsync(UserDto request)
        {

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == request.Username);

            if (user is null)
            {
                return null;
            }

            if (new PasswordHasher<User>().VerifyHashedPassword(user, user.PasswordHash, request.Password)
             == PasswordVerificationResult.Failed)
            {
                return null;
            }


            return await CreateTokenResponse(user);
        }

        private async Task<TokenResponseDto> CreateTokenResponse(User user)
        {
            return new TokenResponseDto
            {
                AccessToken = CreateToken(user),
                RefreshToken = await GenerateAndSaveRefreshTokenAsync(user)
            };
        }

        public async Task<User?> RegisterAsync(UserDto request)
        {
            if (await _context.Users.AnyAsync(u => u.Username == request.Username))
            {
                return null;
            }

            var user = new User();
            var hashedPassword = new PasswordHasher<User>()
                .HashPassword(user, request.Password);

            user.Username = request.Username;
            user.PasswordHash = hashedPassword;  // Simplified for demonstration

            // 👇 1. ✍️ [เพิ่ม] กำหนด Role เป็น 'User' อัตโนมัติ
            user.Role = "User";

            // 👇 2. ✍️ [เพิ่ม] กำหนดเวลาสมัคร (สำหรับ Field ที่เราเพิ่มไว้)
            user.CreatedAt = DateTime.UtcNow;

            _context.Users.Add(user);

            await _context.SaveChangesAsync();
            return user;
        }

        public async Task<TokenResponseDto?> RefreshTokensAsync(RefreshTokenRequestDto request)
        {
            var user = await ValidateRefreshTokenAsync(request.UserId, request.RefreshToken);
            if (user is null)
            {
                return null;
            }

            return await CreateTokenResponse(user);
        }

        private async Task<User?> ValidateRefreshTokenAsync(Guid userId, string refreshToken)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user is null || user.RefreshToken != refreshToken
                || user.RefreshTokenExpiryTime <= DateTime.UtcNow
            )
            {
                return null;
            }

            return user;
        }

        private string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        private async Task<string> GenerateAndSaveRefreshTokenAsync(User user)
        {
            var refreshToken = GenerateRefreshToken();
            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
            await _context.SaveChangesAsync();
            return refreshToken;
        }

        private string CreateToken(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.NameIdentifier, user.id.ToString()),
                new Claim(ClaimTypes.Role, user.Role)
            };


            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_configuration.GetValue<string>("AppSettings:Token")!));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512);

            var tokenDescription = new JwtSecurityToken(
                issuer: _configuration.GetValue<string>("AppSettings:Issuer"),
                audience: _configuration.GetValue<string>("AppSettings:Audience"),

                claims: claims,
                expires: DateTime.Now.AddDays(1),
                signingCredentials: creds
                );

            return new JwtSecurityTokenHandler().WriteToken(tokenDescription);

        }

        public async Task<bool> UpdateProfileAsync(Guid userId, ProfileUpdateDto request)
        {
            var user = await _context.Users.FindAsync(userId);

            if (user is null) return false;

            // 1. ✍️ [ตรวจสอบ Username ซ้ำ] 
            // ถ้า Username ที่ส่งมาไม่เหมือนอันเดิม และไปซ้ำกับคนอื่น
            if (user.Username != request.NewUsername && await _context.Users.AnyAsync(u => u.Username == request.NewUsername))
            {
                return false; // Username ใหม่ถูกใช้งานแล้ว
            }

            // 2. ✍️ [ตรวจสอบ Email ซ้ำ] 
            // ถ้า Email ถูกส่งมา (ไม่ null) และไม่เหมือนอันเดิม และไปซ้ำกับคนอื่น
            if (!string.IsNullOrEmpty(request.Email) && user.Email != request.Email && await _context.Users.AnyAsync(u => u.Email == request.Email))
            {
                return false; // Email ใหม่ถูกใช้งานแล้ว
            }

            // 3. ✍️ [อัปเดตข้อมูล]
            user.Username = request.NewUsername;
            user.Email = request.Email;
            user.FirstName = request.FirstName;
            user.LastName = request.LastName;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ChangePasswordAsync(Guid userId, ChangePasswordDto request)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user is null) return false;

            var passwordHasher = new PasswordHasher<User>();

            // 2. ✍️ ตรวจสอบรหัสผ่านเก่าก่อน
            if (passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.OldPassword)
                == PasswordVerificationResult.Failed)
            {
                return false; // รหัสผ่านเก่าไม่ถูกต้อง
            }

            // 3. ✍️ Hash รหัสผ่านใหม่และบันทึก
            user.PasswordHash = passwordHasher.HashPassword(user, request.NewPassword);

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<UserListDto>> GetAllUsersAsync()
        {
            // ✍️ [แก้ไข] ให้ดึงข้อมูลและ map ไปยัง UserListDto
            return await _context.Users
                .Select(u => new UserListDto
                {
                    Id = u.id.ToString(),
                    Username = u.Username,
                    Role = u.Role,
                    Email = u.Email,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    CreatedAt = u.CreatedAt
                })
                .OrderBy(u => u.Username)
                .ToListAsync();
        }

        public async Task<bool> UpdateUserAsync(Guid userId, UserUpdateRequest request)
        {
            var user = await _context.Users.FindAsync(userId);

            if (user == null) return false;

            if (user.Role == "Admin")
            {
                if (request.Role != null && request.Role != "Admin")
                {
                    return false;
                }
            }

            // อัปเดต Fields
            if (request.FirstName != null) user.FirstName = request.FirstName;
            if (request.LastName != null) user.LastName = request.LastName;

            if (user.Role == "Admin" && request.Role != null && request.Role != "Admin")
            {
                return false; // ป้องกันการเปลี่ยน Role จาก Admin เป็น User
            }

            // อัปเดต Role สำหรับ User ทั่วไป
            if (user.Role != "Admin" && request.Role != null)
            {
                user.Role = request.Role;
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<UserStatsDto> GetOverallStatsAsync()
        {
            var totalUsers = await _context.Users.CountAsync();
            var totalAdmins = await _context.Users.CountAsync(u => u.Role == "Admin");

            var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30);
            var newUsers = await _context.Users.CountAsync(u => u.CreatedAt >= thirtyDaysAgo);

            return new UserStatsDto
            {
                TotalUsers = totalUsers,
                TotalAdmins = totalAdmins,
                NewUsersThisMonth = newUsers
            };
        }

        public async Task<bool> DeleteUserAsync(Guid userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return false;

            // 1. 🚨 [สำคัญ] ลบข้อมูลที่เชื่อมโยงก่อน (Prevent Foreign Key Error)
            // ลบสถานะ Notification ของ User คนนี้ทั้งหมด
            await _context.UserNotificationStatus
                .Where(uns => uns.UserId == userId)
                .ExecuteDeleteAsync();

            // 2. ลบ User หลัก
            _context.Users.Remove(user);

            // 3. บันทึกการเปลี่ยนแปลง
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<string?> UploadProfilePictureAsync(Guid userId, IFormFile file)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return null;

            // 1. สร้าง Path ที่จะเก็บไฟล์ (โค้ดเดิม)
            string uploadsFolder = Path.Combine(_webHostEnvironment.ContentRootPath, "wwwroot", "uploads", "profiles");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            // 2. ✍️ [แก้ไข] ลบไฟล์เก่า (โดยไม่สนใจนามสกุล)
            // (ค้นหาไฟล์ทั้งหมดที่ชื่อขึ้นต้นด้วย UserID)
            string searchPattern = $"{userId}.*"; // (เช่น 7e1e808b-....*)
            var oldFiles = Directory.GetFiles(uploadsFolder, searchPattern);

            foreach (var oldFile in oldFiles)
            {
                File.Delete(oldFile); // 👈 ลบไฟล์เก่าทั้งหมดที่เจอ (เช่น .png, .jpg, .gif)
            }

            // (ลบ Logic การลบไฟล์เก่าแบบเดิม ที่ใช้ user.ProfileImageUrl ทิ้ง)
            // if (!string.IsNullOrEmpty(user.ProfileImageUrl)) { ... }

            // 3. สร้างชื่อไฟล์ใหม่ (โค้ดเดิม)
            var extension = Path.GetExtension(file.FileName);
            var uniqueFileName = $"{userId}{extension}"; // (เช่น 7e1e808b-....png)
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            // 4. บันทึกไฟล์ลง Disk (โค้ดเดิม)
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            // 5. สร้าง Path ที่ Browser จะใช้เรียก (Web Path)
            var webPath = $"/uploads/profiles/{uniqueFileName}";

            // 6. อัปเดต Database
            user.ProfileImageUrl = webPath;
            await _context.SaveChangesAsync();

            return webPath; // ส่ง Path ใหม่กลับไป
        }

        public async Task<IEnumerable<UserOptionDto>> GetChatUsersAsync(Guid currentUserId)
        {
            // (เราใช้ UserOptionDto (อันเดิม) ที่มีแค่ Id, Username)
            return await _context.Users
                .Where(u => u.id != currentUserId) // 👈 [สำคัญ] ดึงทุกคนที่ไม่ใช่ "ฉัน"
                .Select(u => new UserOptionDto
                {
                    Id = u.id,
                    Username = u.Username
                    // (เราไม่จำเป็นต้องส่งรูป ProfileImageUrl มาในช่องค้นหา)
                })
                .OrderBy(u => u.Username)
                .ToListAsync();
        }

    }
}
