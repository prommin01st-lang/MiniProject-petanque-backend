using JWTdemo.Models;
using JWTdemo.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace JWTdemo.Controllers
{
    [ApiController]
    [Route("api/[controller]")] // 👈 Path หลัก: /api/Message
    [Authorize] // 👈 [สำคัญ] ทุกฟังก์ชันในนี้ "ต้องล็อกอิน"
    public class MessageController : ControllerBase
    {
        private readonly IMessageService _messageService;

        public MessageController(IMessageService messageService)
        {
            _messageService = messageService;
        }

        // 1. [GET] /api/Message (ดึง "ห้องแชท" ทั้งหมดของฉัน)
        [HttpGet]
        public async Task<IActionResult> GetMyConversations()
        {
            var userId = GetCurrentUserId();
            var conversations = await _messageService.GetMyConversationsAsync(userId);
            return Ok(conversations);
        }

        // 2. [GET] /api/Message/{conversationId} (ดึง "ข้อความ" ในห้อง)
        [HttpGet("{conversationId}")]
        public async Task<IActionResult> GetMessages(int conversationId)
        {
            var userId = GetCurrentUserId();
            var messages = await _messageService.GetMessagesForConversationAsync(conversationId, userId);
            return Ok(messages);
        }

        // 3. [POST] /api/Message (ส่งข้อความ)
        [HttpPost]
        public async Task<IActionResult> SendMessage([FromBody] SendMessageDto dto)
        {
            var userId = GetCurrentUserId();
            
            // (ป้องกันการส่งหาตัวเอง)
            if (dto.RecipientId == userId)
            {
                return BadRequest(new { message = "Cannot send message to yourself." });
            }

            var newMessage = await _messageService.SendMessageAsync(dto, userId);

            if (newMessage == null)
            {
                return BadRequest(new { message = "Recipient user not found." });
            }

            return Ok(newMessage);
        }

        // --- (Helper Function) ---
        private Guid GetCurrentUserId()
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdString, out Guid userId))
            {
                throw new InvalidOperationException("User ID not found in token.");
            }
            return userId;
        }
    }
}