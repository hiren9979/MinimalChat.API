using Azure.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Minimal_chat_application.Context;
using Minimal_chat_application.Model;
using MinimalChat.API.Hubs;
using System;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
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

        public MessageController(UserManager<User> userManager, ApplicationDbContext context, IHubContext<ChatHub> hubContext)
        {
            _userManager = userManager;
            _context = context;
            _hubContext = hubContext;
        }

        [HttpPost("SendMessages")]
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

            // Check if the receiver exists
            var receiver = await _userManager.FindByIdAsync(sendMessageModel.ReceiverId);
            if (receiver == null)
            {
                return BadRequest(new { error = "Receiver user not found" });
            }

            var message = new Message
            {
                SenderId = senderId,
                ReceiverId = sendMessageModel.ReceiverId,
                Content = sendMessageModel.Content,
                Timestamp = DateTime.Now
            };

            // Broadcast the message via SignalR


            _context.Messages.Add(message);
            await _context.SaveChangesAsync();
            message.Id = message.Id;
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

        //Edit message with messageId
        [HttpPost("EditMessage/{messageId}")]
        [Authorize]

        public async Task<IActionResult> EditMessage(int messageId, [FromBody] EditMessageModel editMessageModel)
        {

            // Check if the message with the given messageId exists.
            var message = await _context.Messages.FindAsync(messageId);
            var loginUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (message.SenderId != loginUserId)
            {
                return Unauthorized(new { error = "Unauthorized access - Try to edit message send by you not others" });
            }

            if (message == null)
            {
                return NotFound(new { error = "Message not found" });
            }

            // Update the message content.
            message.Content = editMessageModel.Content;

            // Save the changes to the database.
            await _context.SaveChangesAsync();

            // Broadcast the message via SignalR
            await _hubContext.Clients.All.SendAsync("EditMessage", messageId, editMessageModel.Content);


            return Ok(new
            {
                messageId = message.Id,
                content = message.Content,
                timestamp = message.Timestamp
            });
        }

        //Delete message
        [HttpDelete("DeleteMessage/{messageId}")]
        [Authorize]
        public async Task<IActionResult> DeleteMessage(int messageId)
        {
            // Your code here to fetch and delete the message with the given messageId.

            // Check if the message with the given messageId exists.
            var message = await _context.Messages.FindAsync(messageId);

            var loginUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (message.SenderId != loginUserId)
            {
                return Unauthorized(new { error = "Unauthorized access - Try to delete message send by you not others" });
            }

            if (message == null)
            {
                return NotFound(new { error = "Message not found" });
            }

            // Check if the user making the request is the sender of the message.
            var senderId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (senderId != message.SenderId)
            {
                return Unauthorized(new { error = "Unauthorized access" });
            }

            // Remove the message from the database.
            _context.Messages.Remove(message);
            await _context.SaveChangesAsync();

            // Broadcast the message via SignalR
            await _hubContext.Clients.All.SendAsync("DeleteMesage", messageId);


            return Ok(new
            {
                messageDeleted = true
            });
        }

        //Fetch conversation history
        [HttpGet("ConversationHistory")]
        public async Task<IActionResult> GetConversationHistory(
        /* [FromBody] FetchConverstionModel fetchConverstionModel*/
        [FromQuery] string receiverId,
        [FromQuery] string? sort,
        [FromQuery] DateTime? time,
        [FromQuery] int? count

        )
        {
            // Get the current user's ID from the token
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            // Check if the specified user exists
            var chattingUser = await _userManager.FindByIdAsync(receiverId);
            if (chattingUser == null)
            {
                return NotFound(new { error = "Receiver user not found" });
            }

            // Define the query for fetching messages
            var query = _context.Messages
                .Where(m => (m.SenderId == currentUserId && m.ReceiverId == chattingUser.Id)
                            || (m.SenderId == chattingUser.Id && m.ReceiverId == currentUserId))
                .AsQueryable();


            // Apply sorting based on timestamp
            if (sort == "desc")
            {
                query = query.OrderByDescending(m => m.Timestamp);
            }
            else
            {
                query = query.OrderBy(m => m.Timestamp);
            }

            TimeZoneInfo sourceTimeZone = TimeZoneInfo.Utc;

            // Define the target time zone (Indian Standard Time)
            TimeZoneInfo targetTimeZone = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");

            // Convert the time to IST
            DateTime istTime = TimeZoneInfo.ConvertTimeFromUtc(time.Value, targetTimeZone);

            // Apply filtering based on the 'before' timestamp
            if (time.HasValue)
            {

                query = query.Where(m => m.Timestamp < istTime);
            }
            else
            {
                query = query.Where(m => m.Timestamp < DateTime.Now);
            }

            if (!count.HasValue || count > 20)
            {
                count = 20;
            }

            var messages = await query
                            .OrderByDescending(m => m.Timestamp)
                            .Take(20)
                            .ToListAsync();

            //Prepare response
            var response = new
            {
                messages = messages.Select(m => new
                {
                    id = m.Id,
                    senderId = m.SenderId,
                    receiverId = m.ReceiverId,
                    content = m.Content,
                    timestamp = m.Timestamp
                })
            };

            return Ok(response);
        }


        [HttpGet("SearchConversations")]
        [Authorize]
        public async Task<IActionResult> SearchConversations([FromQuery] string query, [FromQuery] string receiverId)
        {
            // Get the current user's ID from the token
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrWhiteSpace(query))
            {
                return BadRequest(new { error = "Invalid query parameter" });
            }

            // Define the query for searching messages
            var queryNormalized = query.ToLowerInvariant(); // Normalize query for case-insensitive search
            var messages = await _context.Messages
                .Where(m =>
                    ((m.SenderId == currentUserId && m.ReceiverId == receiverId)
                    || (m.SenderId == receiverId && m.ReceiverId == currentUserId))
                    && m.Content.ToLower().Contains(queryNormalized))
                .OrderByDescending(m => m.Timestamp)
                .ToListAsync();

            // Prepare the response
            var response = new
            {
                messages = messages.Select(m => new
                {
                    id = m.Id,
                    senderId = m.SenderId,
                    receiverId = m.ReceiverId,
                    content = m.Content,
                    timestamp = m.Timestamp
                })
            };

            return Ok(response);
        }


    }

}