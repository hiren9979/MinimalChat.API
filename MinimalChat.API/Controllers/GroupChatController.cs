using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MinimalChat.Data.Services;
using MinimalChat.Domain.Model;

namespace MinimalChat.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GroupChatController : ControllerBase
    {
        private readonly GroupChatService _groupChatService;

        public GroupChatController(GroupChatService groupChatService)
        {
            _groupChatService = groupChatService;
        }

        [HttpPost]
        public IActionResult CreateGroupChat([FromBody] GroupChat model)
        {
            var createdGroupChat = _groupChatService.CreateGroupChat(model);
            return Ok(createdGroupChat);
        }
    }
}
