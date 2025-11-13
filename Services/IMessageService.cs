using JWTdemo.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace JWTdemo.Services
{
    public interface IMessageService
    {
        // 1. ดึง "ห้องแชท" ทั้งหมดของฉัน
        Task<IEnumerable<ConversationDto>> GetMyConversationsAsync(Guid userId);

        // 2. ดึง "ข้อความ" ทั้งหมดในห้องแชท (พร้อมมาร์คว่าอ่านแล้ว)
        Task<IEnumerable<MessageDto>> GetMessagesForConversationAsync(int conversationId, Guid userId);

        // 3. ส่งข้อความ
        Task<MessageDto?> SendMessageAsync(SendMessageDto dto, Guid senderId);
    }
}