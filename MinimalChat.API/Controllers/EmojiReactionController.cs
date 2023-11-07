using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Minimal_chat_application.Context;
using MinimalChat.Data.Services;
using MinimalChat.Domain.DTO;
using MinimalChat.Domain.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Minimal_chat_application.Model;
using MinimalChat.API.Hubs;
using MinimalChat.Domain.Interface;

namespace MinimalChat.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmojiReactionController : ControllerBase
     {
        private readonly ApplicationDbContext _dbContext;
        private readonly IEmojiReactionService _emojiReactionService;
        private readonly IHubContext<ChatHub> _hubContext;


        public EmojiReactionController(ApplicationDbContext context,IEmojiReactionService emojiReactionService, IHubContext<ChatHub> hubContext)
        {
            _dbContext = context;
            _emojiReactionService = emojiReactionService;
            _hubContext = hubContext;
        }

        [HttpPost("AddEmoji")]
        public  async Task<IActionResult> AddEmojiReaction([FromBody] EmojiReactionDTO request)
        {
            // Validate and sanitize the input if necessary

                var reaction = await  _emojiReactionService.AddEmojiReaction(request.MessageId, request.UserId, request.Emoji);

            if (reaction == null)
            {
                return NotFound(new { error = "There is no message like this..." });
            }

            await _hubContext.Clients.All.SendAsync("ReceiveEmoji", reaction);

            return Ok(reaction);
                
        }

        [HttpGet("GetEmojiReaction/{messageId}")]
        public IActionResult GetEmojiReactionsForMessage(int messageId)
        {
            var reactions = _emojiReactionService.GetEmojiReactionsForMessage(messageId);
            return Ok(reactions);
        }


    }
}
