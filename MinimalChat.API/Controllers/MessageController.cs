using Azure.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.IdentityModel.Tokens;
using Minimal_chat_application.Context;
using Minimal_chat_application.Model;
using MinimalChat.API.Hubs;
using MinimalChat.Data.Services;
using MinimalChat.Domain.Model;
using System;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Minimal_chat_application.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    // [Authorize] // Add the Authorize attribute to protect this controller
    public class MessageController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly IHubContext<ChatHub> _hubContext;
        private readonly MessageService _messageService;

        public MessageController(UserManager<User> userManager, ApplicationDbContext context, IHubContext<ChatHub> hubContext,MessageService messageSergice)
        {
            _userManager = userManager;
            _context = context;
            _hubContext = hubContext;
            _messageService = messageSergice;
        }

        [HttpPost("messages")]
        [Authorize]
        public async Task<IActionResult> SendMessage([FromBody] SendMessageModel sendMessageModel)
        {

            var senderId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrWhiteSpace(senderId))
            {
                return Unauthorized(new { error = "Unauthorized access" });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(new { error = "Validation failed", errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)) });
            }

            try
            {
                var message = await _messageService.SendMessage(senderId, sendMessageModel);
                if (message == null)
                {
                    return Conflict("Error while sending message");
                }


                await _hubContext.Clients.All.SendAsync("ReceiveMessage", message);


                return Ok(new
                {
                    messageId = message.Id,
                    senderId = message.SenderId,
                    receiverId = message.ReceiverId,
                    content = message.Content,
                    timestamp = message.Timestamp
                });
            }
            catch (Exception ex)
            {
                // Log the exception for debugging purposes
                Console.WriteLine(ex);

                // Return an appropriate error response
                return StatusCode(500, "An internal server error occurred.");
            }
        }

        [HttpPost("messages/{messageId}")]
        [Authorize]
        public async Task<IActionResult> EditMessage(int messageId, [FromBody] EditMessageModel editMessageModel)
        {
            if (messageId == null)
            {
                return BadRequest(new { error = "Invalid message ID" });
            }

            var loginUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrWhiteSpace(loginUserId))
            {
                return Unauthorized(new { error = "Unauthorized access" });
            }

            var editedMessage = await _messageService.EditMessage(messageId, loginUserId, editMessageModel);

            if (editedMessage == null)
            {
                return NotFound(new { error = "Message not found" });
            }

            return Ok(new
            {
                messageId = editedMessage.Id,
                content = editedMessage.Content,
                timestamp = editedMessage.Timestamp
            });
        }


        [HttpDelete("messages/{messageId}")]
        [Authorize]
        public async Task<IActionResult> DeleteMessage(int messageId)
        {
            if (messageId == null)
            {
                return BadRequest(new { error = "Invalid message ID" });
            }

            var loginUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrWhiteSpace(loginUserId))
            {
                return Unauthorized(new { error = "Unauthorized access" });
            }

            bool messageDeleted = await _messageService.DeleteMessage(messageId, loginUserId);

            if (!messageDeleted)
            {
                return NotFound(new { error = "Message not found or unauthorized" });
            }

            return Ok(new
            {
                messageDeleted = true
            });
        }



        [HttpGet("ConversationHistory")]
        [Authorize]
        public async Task<IActionResult> GetConversationHistory([FromQuery] ConversationHistoryRequestModel model)
        {
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrWhiteSpace(currentUserId))
            {
                return Unauthorized(new { error = "Unauthorized access" });
            }

            int recordCount = model.Count.HasValue ? model.Count.Value : 20; // Use count parameter or default to 20

            var response = await _messageService.GetConversationHistory(
                currentUserId,
                model.ReceiverId,
                model.Sort,
                model.Time,
                recordCount,
                model.IsGroup
            );

            if (response == null)
            {
                return NotFound(new { error = "Receiver user not found" });
            }

            return Ok(response);
        }




        [HttpGet("SearchConversations")]
        [Authorize]
        public async Task<IActionResult> SearchConversations([FromQuery] string query, [FromQuery] string receiverId)
        {
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrWhiteSpace(currentUserId))
            {
                return Unauthorized(new { error = "Unauthorized access" });
            }

            var response = await _messageService.SearchConversations(currentUserId, query, receiverId);

            if (response == null)
            {
                return BadRequest(new { error = "Invalid query parameter" });
            }

            return Ok(response);
        }


        [HttpGet("TaggedMessages")]
        public async Task<IActionResult> GetTaggedMessages()
        {
            var response = await _messageService.GetTaggedMessages();

            return Ok(response);
        }

            [HttpPost("AddEmojiReaction")]
            public IActionResult AddEmojiReaction([FromQuery]int messageId,[FromQuery] string emoji)
            {
                try
                {
                    _messageService.AddEmojiReaction(messageId, emoji);
                    return Ok(new { message = "Emoji reaction added successfully" });
                }   
                catch (Exception ex)
                {
                    return BadRequest(new { error = "Failed to add emoji reaction", message = ex.Message });
                }
            }


    }

}