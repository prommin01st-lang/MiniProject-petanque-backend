using JWTdemo.Data;
using JWTdemo.Entities;
using JWTdemo.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JWTdemo.Services
{
    public class MessageService : IMessageService
    {
        private readonly UserDbContext _context;
        private readonly INotificationService _notificationService;
        public MessageService(UserDbContext context, INotificationService notificationService)
        {
            _context = context;
            _notificationService = notificationService; // 👈 (เก็บไว้)
        }

        // 1. ดึง "ห้องแชท" ทั้งหมดของฉัน
        public async Task<IEnumerable<ConversationDto>> GetMyConversationsAsync(Guid userId)
        {
            var conversations = await _context.Conversations
                // 1. กรองห้องแชทที่มี "ฉัน"
                .Where(c => c.User1Id == userId || c.User2Id == userId)
                .Include(c => c.User1) // (Join User1)
                .Include(c => c.User2) // (Join User2)

                // 2. ✍️ [แก้ไข] เลือก (Project) ข้อมูลที่จำเป็นออกมาก่อน
                .Select(c => new
                {
                    Conversation = c,
                    // 3. ✍️ [แก้ไข] "ค้นหา" ข้อความล่าสุด (LastMsg)
                    LastMsg = c.Messages.OrderByDescending(m => m.SentAt).FirstOrDefault()
                })

                // 4. ✍️ [แก้ไข] เรียงลำดับ "ห้องแชท" (โดยใช้เวลาของ LastMsg)
                .OrderByDescending(x => x.LastMsg != null ? x.LastMsg.SentAt : x.Conversation.CreatedAt)

                // 5. ✍️ [แก้ไข] แปลง (Map) เป็น DTO ที่จะส่งกลับ
                .Select(x => new ConversationDto
                {
                    Id = x.Conversation.Id,

                    // (Logic หา "คู่สนทนา" - โค้ดเดิม)
                    OtherUserId = (x.Conversation.User1Id == userId) ? x.Conversation.User2Id : x.Conversation.User1Id,
                    OtherUsername = (x.Conversation.User1Id == userId) ? x.Conversation.User2.Username : x.Conversation.User1.Username,
                    OtherUserProfileImageUrl = (x.Conversation.User1Id == userId) ? x.Conversation.User2.ProfileImageUrl : x.Conversation.User1.ProfileImageUrl,

                    // (Logic ดึงข้อมูลจาก "LastMsg" ที่เราหาเจอ)
                    LastMessage = x.LastMsg != null ? x.LastMsg.Content : "No messages yet.",
                    LastMessageTimestamp = x.LastMsg != null ? x.LastMsg.SentAt : x.Conversation.CreatedAt,
                    IsLastMessageRead = x.LastMsg != null ? x.LastMsg.IsRead : true,
                    LastMessageSenderId = x.LastMsg != null ? x.LastMsg.SenderId : (Guid?)null
                })
                .ToListAsync();

            return conversations;
        }

        // 2. ดึง "ข้อความ" ทั้งหมดในห้องแชท
        public async Task<IEnumerable<MessageDto>> GetMessagesForConversationAsync(int conversationId, Guid userId)
        {
            // 1. เช็คสิทธิ์ว่า "ฉัน" อยู่ในห้องแชทนี้
            var conversation = await _context.Conversations
                .FirstOrDefaultAsync(c => c.Id == conversationId && (c.User1Id == userId || c.User2Id == userId));

            if (conversation == null)
            {
                return new List<MessageDto>(); // 👈 (ถ้าไม่อยู่ ให้ส่ง Array ว่าง)
            }

            // 2. [สำคัญ] มาร์คข้อความที่ "คนอื่น" ส่งมาว่า "อ่านแล้ว"
            var unreadMessages = _context.Messages
                .Where(m => m.ConversationId == conversationId && m.SenderId != userId && m.IsRead == false);

            await unreadMessages.ForEachAsync(m => m.IsRead = true);
            await _context.SaveChangesAsync();

            // 3. ดึงข้อความทั้งหมด
            return await _context.Messages
                .Where(m => m.ConversationId == conversationId)
                .OrderBy(m => m.SentAt) // 👈 เรียงจากเก่าไปใหม่
                .Select(m => new MessageDto
                {
                    Id = m.Id,
                    Content = m.Content,
                    SentAt = m.SentAt,
                    IsRead = m.IsRead,
                    SenderId = m.SenderId // 👈 (Frontend จะใช้ ID นี้เทียบว่าใครส่ง)
                })
                .ToListAsync();
        }

        // 3. ส่งข้อความ
        public async Task<MessageDto?> SendMessageAsync(SendMessageDto dto, Guid senderId)
        {
            // 1. ค้นหา "ห้องแชท" ที่มี (User A + User B) หรือ (User B + User A)
            var conversation = await _context.Conversations
                .FirstOrDefaultAsync(c =>
                    (c.User1Id == senderId && c.User2Id == dto.RecipientId) ||
                    (c.User1Id == dto.RecipientId && c.User2Id == senderId));

            // 2. ถ้า "ห้องแชท" ไม่มีอยู่ -> สร้างใหม่
            if (conversation == null)
            {
                // (เช็คก่อนว่าผู้รับมีตัวตน)
                if (!await _context.Users.AnyAsync(u => u.id == dto.RecipientId))
                {
                    return null; // 👈 (ผู้รับไม่มีตัวตน)
                }

                conversation = new Conversation
                {
                    User1Id = senderId,
                    User2Id = dto.RecipientId,
                    CreatedAt = DateTime.UtcNow
                };
                _context.Conversations.Add(conversation);
                // (EF Core จะ Update 'conversation.Id' ให้อัตโนมัติหลัง Save)
            }

            // 3. สร้าง "ข้อความ"
            var message = new Message
            {
                Conversation = conversation, // 👈 ผูกกับห้องแชท
                SenderId = senderId, // 👈 ระบุผู้ส่ง
                Content = dto.Content,
                SentAt = DateTime.UtcNow,
                IsRead = false // 👈 (ยังไม่อ่าน)
            };

            _context.Messages.Add(message);
            await _context.SaveChangesAsync();

            try
            {
                // 4.1 ดึง "ผู้ส่ง" (Sender) เพื่อเอา Username
                var sender = await _context.Users.FindAsync(senderId);
                if (sender != null)
                {
                    // 4.2 สร้าง DTO สำหรับ Notification
                    var notiDto = new CreateNotificationDto
                    {
                        Title = $"New message from {sender.Username}",
                        Subtitle = dto.Content.Length > 50
                                    ? dto.Content.Substring(0, 50) + "..."
                                    : dto.Content,
                        AvatarType = "icon", // (หรือใช้ sender.ProfileImageUrl ถ้าต้องการ)
                        AvatarValue = "bx-message-dots", // 👈 (ไอคอน Message)
                        TargetUserIds = new List<string> { dto.RecipientId.ToString() } // 👈 ยิงไปหา "ผู้รับ"
                    };

                    // 4.3 สั่งยิง Noti!
                    await _notificationService.CreateNotificationAsync(notiDto);
                }
            }
            catch (Exception ex)
            {
                // (ถ้า Noti ล่ม ก็ไม่เป็นไร Comment ยังคงสร้างสำเร็จ)
                Console.WriteLine($"Failed to send message notification: {ex.Message}");
            }
            
            // 4. ส่ง MessageDto กลับไป (เพื่อให้ Frontend แสดงผลทันที)
            return new MessageDto
            {
                Id = message.Id,
                Content = message.Content,
                SentAt = message.SentAt,
                IsRead = message.IsRead,
                SenderId = message.SenderId
            };
        }
    }
}