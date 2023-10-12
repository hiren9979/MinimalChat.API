using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MinimalChat.Data.Services;
using MinimalChat.Domain.DTO;
using MinimalChat.Domain.Model;
using System.Text.Json.Serialization;
using System.Text.Json;

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

        [HttpPost("CreateGroup")]
        public IActionResult CreateGroupChat([FromBody] GroupChatDTO model)
        {
            var createdGroupChat = _groupChatService.CreateGroupChat(model);
            return Ok(createdGroupChat);
        }

        [HttpPost("{groupId}/add-members")]
        public async Task<IActionResult> AddGroupMembers(string groupId, [FromBody] List<string> memberIds)
        {
            var updatedGroupChat = await _groupChatService.AddGroupMembers(groupId, memberIds);

            if (updatedGroupChat == null)
            {
                Console.WriteLine("User is already exist in this group");
                return BadRequest("User is already a member of this group.");

            }
            else
            {
                Console.WriteLine("Successfully Added !!!");
            }

            // Use JsonSerializerOptions with ReferenceHandler.Preserve to handle object cycles
            var options = new JsonSerializerOptions
            {
                ReferenceHandler = ReferenceHandler.Preserve
            };

            // Serialize the group chat with the Preserve reference handling
            var serializedGroupChat = JsonSerializer.Serialize(updatedGroupChat, options);

            return Ok(serializedGroupChat);
        }

    }
}
