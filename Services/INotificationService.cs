using JWTdemo.Entities;
using JWTdemo.Models; // เราจะสร้าง DTO นี้ในข้อ 2
using System.Threading.Tasks;

namespace JWTdemo.Services
{
    // "รายการคำสั่ง" ที่ Service นี้ทำได้
    public interface INotificationService
    {
        // คำสั่งที่ 1: สร้าง Noti (สำหรับ Admin)
        Task<Notification> CreateNotificationAsync(CreateNotificationDto request);

        Task<List<NotificationDto>> GetNotificationsAsync(Guid userId);

        Task<bool> MarkAsReadAsync(Guid userId, string notificationId);

        Task<bool> DeleteNotificationAsync(Guid userId, string notificationId);

        Task<bool> MarkAllAsReadAsync(Guid userId);

        Task<NotificationStatsDto> GetNotificationStatsAsync();

         
        // Task<IEnumerable<Notification>> GetAllNotificationsAsync(); <-- ไม่รองรับ Paginattion

        Task<PaginatedResultDto<Notification>> GetAllNotificationsAsync(int pageNumber, int pageSize);
    }
}