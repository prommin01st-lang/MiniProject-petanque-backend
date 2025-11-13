using JWTdemo.Models;       // 👈 1. Import DTOs (CreateNotificationDto)
using JWTdemo.Services;   // 👈 2. Import Service (INotificationService)
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims; // 👈 3. Import Claims (สำหรับดึง UserId)
using System.Threading.Tasks;

namespace JWTdemo.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NotificationController : ControllerBase
    {
        // 4. ✍️ (เปลี่ยน) Inject Service (ไม่ใช่ DbContext)
        private readonly INotificationService _notificationService;

        // 5. ✍️ (เปลี่ยน) แก้ Constructor ให้รับ Service
        public NotificationController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        // --- Endpoint 1: สร้าง Notification (สำหรับ Admin) ---
        [HttpPost]
        [Authorize(Roles = "Admin")] // 👈 เฉพาะ Admin
        public async Task<IActionResult> CreateNotification([FromBody] CreateNotificationDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // 6. ✍️ (เปลี่ยน) สั่ง Service ให้ทำงาน (Service จะจัดการทั้ง 2 ตาราง)
            var newNotification = await _notificationService.CreateNotificationAsync(request);

            return Ok(newNotification); // ส่ง "ต้นฉบับ" กลับไป
        }

        // --- Endpoint 2: ดึง Notification (สำหรับ User ที่ล็อกอิน) ---
        [HttpGet]
        [Authorize] // 👈 อนุญาตให้ "ทุกคนที่ล็อกอิน"
        public async Task<IActionResult> GetNotifications()
        {
            // 7. ✍️ ดึง UserId ของ "ฉัน" (คนที่ยิง Token นี้มา)
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out Guid userId))
            {
                return Unauthorized("Invalid user ID.");
            }

            // 8. ✍️ สั่ง Service ให้ไปดึง Noti ของฉัน
            var notifications = await _notificationService.GetNotificationsAsync(userId);

            return Ok(notifications);
        }



        [HttpPost("read/{notificationId}")] // 👈 เราจะ POST ไปที่ /api/Notification/read/ID_NOTI
        [Authorize] // 👈 ทุกคนที่ล็อกอิน
        public async Task<IActionResult> MarkNotificationAsRead([FromRoute] string notificationId)
        {
            // 8. ✍️ ดึง UserId ของ "ฉัน" จาก Token
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdString, out Guid userId))
            {
                return Unauthorized("Invalid user ID.");
            }

            // 9. ✍️ สั่ง Service ให้ทำงาน
            var result = await _notificationService.MarkAsReadAsync(userId, notificationId);

            if (!result)
            {
                // โดยปกติ Logic ของเราจะ trả về true เสมอ แต่เผื่อไว้
                return NotFound("Notification not found for this user.");
            }

            return Ok(new { message = "Notification marked as read." });
        }

        [HttpDelete("{notificationId}")] // 👈 เราจะ DELETE ไปที่ /api/Notification/ID_NOTI
        [Authorize] // 👈 ทุกคนที่ล็อกอิน
        public async Task<IActionResult> DeleteNotification([FromRoute] string notificationId)
        {
            // 8. ✍️ ดึง UserId ของ "ฉัน" จาก Token
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdString, out Guid userId))
            {
                return Unauthorized("Invalid user ID.");
            }

            // 9. ✍️ สั่ง Service ให้ทำงาน
            var result = await _notificationService.DeleteNotificationAsync(userId, notificationId);

            if (!result)
            {
                return NotFound("Notification not found for this user.");
            }

            // เราใช้ 204 No Content สำหรับ Delete ที่สำเร็จ (เป็นมาตรฐานที่ดี)
            return NoContent(); // 👈 แปลว่า "ลบสำเร็จ และไม่มีเนื้อหาจะส่งกลับ"
        }


        [HttpPost("read-all")] // 👈 เราจะ POST ไปที่ /api/Notification/read-all
        [Authorize] // 👈 ทุกคนที่ล็อกอิน
        public async Task<IActionResult> MarkAllAsRead()
        {
            // 5. ✍️ ดึง UserId ของ "ฉัน" จาก Token
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdString, out Guid userId))
            {
                return Unauthorized("Invalid user ID.");
            }

            // 6. ✍️ สั่ง Service ให้ทำงาน
            await _notificationService.MarkAllAsReadAsync(userId);

            return Ok(new { message = "All notifications marked as read." });
        }


        [HttpGet("stats")]
        [Authorize(Roles = "Admin")] // 👈 เฉพาะ Admin
        public async Task<IActionResult> GetNotificationStats()
        {
            // 🎯 เรียก Service ที่ถูกต้อง (ตัวมันเอง)
            var stats = await _notificationService.GetNotificationStatsAsync();
            return Ok(stats);
        }

        [HttpGet("all")]
        [Authorize(Roles = "Admin")]
        // ✍️ รับค่า Query Parameters (ถ้าไม่ส่งมา ให้ใช้ค่า Default)
        public async Task<IActionResult> GetAllNotifications(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
        {
            var paginatedResult = await _notificationService.GetAllNotificationsAsync(pageNumber, pageSize);
            return Ok(paginatedResult);
        }
    }
}