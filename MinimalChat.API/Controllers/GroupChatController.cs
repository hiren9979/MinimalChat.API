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

            if (createdGroupChat.Result == null)
            {
                // Handle the case when an error occurred during group chat creation.
                return Conflict("A group chat with the same ID or name already exists.");
                
            }

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

        [HttpPut("{groupId}/editGroupName")]
        public async Task<IActionResult> EditGroupNameAsync([FromBody] UpdateGroupNameDTO model)
        {

            var updatedGroupChat = await _groupChatService.EditGroupName(model);


            if (updatedGroupChat == null)
            {
                // Handle the case when the group chat doesn't exist or the update fails.
                return NotFound("Group chat not found or update failed.");
            }

            return Ok(updatedGroupChat);
        }

        [HttpDelete("{groupId}")]
        public async Task<IActionResult> DeleteGroup(string groupId)
        {
            var deleted = await _groupChatService.DeleteGroupAsync(groupId);

            if (deleted)
            {
                return Ok(new { message = "Group deleted successfully" }); // 200 OK with a custom message
            }

            return NotFound(new { error = "Group not found" }); // 404 Not Found with an error message
        }

    }
}
