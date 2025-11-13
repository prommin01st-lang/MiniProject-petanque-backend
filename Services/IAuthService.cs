using JWTdemo.Entities;
using JWTdemo.Models;

namespace JWTdemo.Services
{
    public interface IAuthService
    {
        Task<User?> RegisterAsync(UserDto request);
        Task<TokenResponseDto?> LoginAsync(UserDto request);
        Task<TokenResponseDto?> RefreshTokensAsync(RefreshTokenRequestDto request);
        Task<IEnumerable<UserListDto>> GetAllUsersAsync();
        Task<bool> UpdateUserAsync(Guid userId, UserUpdateRequest request);

        // ✍️ เพิ่มสัญญาสำหรับ Update Profile
        Task<bool> UpdateProfileAsync(Guid userId, ProfileUpdateDto request);

        // ✍️ เพิ่มสัญญาสำหรับ Change Password
        Task<bool> ChangePasswordAsync(Guid userId, ChangePasswordDto request);

        Task<UserStatsDto> GetOverallStatsAsync(); // 👈 เพิ่ม

        Task<bool> DeleteUserAsync(Guid userId);

        Task<string?> UploadProfilePictureAsync(Guid userId, IFormFile file);
        
        Task<IEnumerable<UserOptionDto>> GetChatUsersAsync(Guid currentUserId);
    }
}