using JWTdemo.Entities;
using JWTdemo.Models;
// ✍️ 1. Import DbContext ของคุณ (อาจจะต้องเปลี่ยนถ้าชื่อไม่ตรง)
using JWTdemo.Data; // (หรือที่อยู่ของ UserDbContext.cs) 
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore; // 👈 1. Import EF
using System.Linq;                   // 👈 2. Import LINQ
using System.Collections.Generic;    // 👈 3. Import List


namespace JWTdemo.Services
{
    public class NotificationService : INotificationService
    {
        // 2. ✍️ Inject "UserDbContext" เข้ามา (เหมือน AuthService)
        private readonly UserDbContext _context;

        public NotificationService(UserDbContext context)
        {
            _context = context;
        }

        // 3. ✍️ นี่คือ Logic การสร้าง Noti
        // 👇 1. แก้ไข Method นี้
        public async Task<Notification> CreateNotificationAsync(CreateNotificationDto request)
        {
            var newNotification = new Notification
            {
                Title = request.Title,
                Subtitle = request.Subtitle,
                AvatarType = request.AvatarType,
                AvatarValue = request.AvatarValue
            };

            // 2. ✍️ บันทึกลงตาราง Notifications (อันเดิม)
            _context.Notifications.Add(newNotification);

            // 3. ⭐️ (เพิ่มโค้ดที่ขาดหายไป) ⭐️
            // วน Loop User ทุกคนที่จะส่งหา
            foreach (var userId in request.TargetUserIds)
            {
                var userStatus = new UserNotificationStatus
                {
                    UserId = Guid.Parse(userId),
                    NotificationId = newNotification.Id, // 👈 เชื่อมกับ Noti ใหม่
                    IsRead = false // 👈 ตั้งค่าว่า "ยังไม่อ่าน"
                };

                // 4. ✍️ เพิ่ม "สถานะ" ลงในตาราง UserNotificationStatus
                _context.UserNotificationStatus.Add(userStatus);
            }
            // ⭐️ (จบส่วนที่เพิ่ม) ⭐️

            // 5. ✍️ บันทึกการเปลี่ยนแปลง (ทั้ง 2 ตาราง) ลง DB ทีเดียว
            await _context.SaveChangesAsync();

            return newNotification;
        }


        // 1. ✍️ (เพิ่ม Method ใหม่นี้เข้าไป)
        public async Task<List<NotificationDto>> GetNotificationsAsync(Guid userId)
        {
            var notifications = await _context.UserNotificationStatus
                // 2. ✍️ กรองหา User คนนี้
                .Where(uns => uns.UserId == userId)

                // 3. ✍️ JOIN ตาราง Noti (ต้นฉบับ)
                .Include(uns => uns.Notification)

                // 4. ✍️ เรียงจากใหม่ไปเก่า
                .OrderByDescending(uns => uns.Notification.CreatedAt)

                // 5. ✍️ แปลงข้อมูลเป็น DTO (กล่องส่งกลับ)
                .Select(uns => new NotificationDto
                {
                    Id = uns.NotificationId,
                    Title = uns.Notification.Title,
                    Subtitle = uns.Notification.Subtitle,
                    CreatedAt = uns.Notification.CreatedAt,
                    IsRead = uns.IsRead, // 👈 เอาสถานะ "อ่าน" ของ User คนนี้มา

                    // 6. ✍️ Logic แปลง AvatarType/Value
                    AvatarImage = uns.Notification.AvatarType == "image" ? uns.Notification.AvatarValue : null,
                    AvatarIcon = uns.Notification.AvatarType == "icon" ? uns.Notification.AvatarValue : null,
                    AvatarText = uns.Notification.AvatarType == "text" ? uns.Notification.AvatarValue : null
                })
                .ToListAsync();

            return notifications;
        }

        public async Task<bool> MarkAsReadAsync(Guid userId, string notificationId)
        {
            // 3. ✍️ ค้นหา "สถานะ" (ซองจดหมาย) ที่ตรงกับ User และ Noti นี้
            var status = await _context.UserNotificationStatus
                .FirstOrDefaultAsync(uns =>
                    uns.UserId == userId &&
                    uns.NotificationId == notificationId);

            // 4. ✍️ ถ้าไม่เจอ (อาจจะลบไปแล้ว) หรืออ่านไปแล้ว ก็ไม่ต้องทำอะไร
            if (status == null || status.IsRead)
            {
                // คืนค่า true เพราะถือว่า "สำเร็จ" (มันถูกอ่านไปแล้ว)
                return true;
            }

            // 5. ✍️ อัปเดตสถานะ
            status.IsRead = true;

            // 6. ✍️ บันทึกการเปลี่ยนแปลง
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> DeleteNotificationAsync(Guid userId, string notificationId)
        {
            // 3. ✍️ ค้นหา "สถานะ" (ซองจดหมาย) ที่ตรงกัน
            var status = await _context.UserNotificationStatus
                .FirstOrDefaultAsync(uns =>
                    uns.UserId == userId &&
                    uns.NotificationId == notificationId);

            // 4. ✍️ ถ้าไม่เจอ ก็ไม่ต้องทำอะไร
            if (status == null)
            {
                // คืนค่า true เพราะถือว่า "สำเร็จ" (มันไม่มีให้ลบ)
                return true;
            }

            // 5. ✍️ สั่งลบ "สถานะ" นี้ออกจากตาราง
            _context.UserNotificationStatus.Remove(status);

            // 6. ✍️ บันทึกการเปลี่ยนแปลง
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> MarkAllAsReadAsync(Guid userId)
        {
            // 3. ✍️ ค้นหา Noti "ทั้งหมดที่ยังไม่อ่าน" ของ User คนนี้
            // เราจะใช้ ExecuteUpdateAsync (ของ EF Core 7+) ซึ่งเร็วกว่าการ
            // ดึงมาวน Loop มากครับ
            await _context.UserNotificationStatus
                .Where(uns => uns.UserId == userId && !uns.IsRead) // 👈 หาอันที่ยังไม่อ่าน
                .ExecuteUpdateAsync(s => s.SetProperty(b => b.IsRead, true)); // 👈 อัปเดตทั้งหมดเป็น true

            // (EF Core จะสร้าง SQL UPDATE ... WHERE ... ให้เราอัตโนมัติ)

            return true;
        }

        public async Task<NotificationStatsDto> GetNotificationStatsAsync()
        {
            // เราจะอ่านจากตาราง "สถานะ" (ใบส่งของ)
            var totalSent = await _context.UserNotificationStatus.CountAsync();

            var totalUnread = await _context.UserNotificationStatus
                .CountAsync(uns => uns.IsRead == false); // 👈 นับเฉพาะที่ยังไม่อ่าน

            return new NotificationStatsDto
            {
                TotalSent = totalSent,
                TotalUnread = totalUnread,
                TotalRead = totalSent - totalUnread // 👈 (อ่านแล้ว = ทั้งหมด - ยังไม่อ่าน)
            };
        }

        public async Task<PaginatedResultDto<Notification>> GetAllNotificationsAsync(int pageNumber, int pageSize)
        {
            // 1. นับจำนวน "ทั้งหมด" ก่อน
            var totalCount = await _context.Notifications.CountAsync();

            // 2. ดึงข้อมูล "เฉพาะหน้า" นั้นๆ
            var items = await _context.Notifications
                .OrderByDescending(n => n.CreatedAt)
                .Skip((pageNumber - 1) * pageSize) // 👈 (หน้า 1 = ข้าม 0)
                .Take(pageSize) // 👈 (ดึงมา 10 รายการ)
                .ToListAsync();

            // 3. ส่งผลลัพธ์กลับ
            return new PaginatedResultDto<Notification>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }
    }
}