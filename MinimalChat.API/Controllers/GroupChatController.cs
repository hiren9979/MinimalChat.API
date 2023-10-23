using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MinimalChat.Data.Services;
using MinimalChat.Domain.DTO;
using MinimalChat.Domain.Model;
using System.Text.Json.Serialization;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

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
        public async Task<IActionResult> CreateGroupChat([FromBody] GroupChatDTO model)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            try
            {
                var createdGroupChat = await _groupChatService.CreateGroupChat(model, currentUserId);

                if (createdGroupChat == null)
                {
                    return Conflict("A group chat with the same ID or name already exists.");
                }

                return Ok(createdGroupChat);
            }
            catch (Exception ex)
            {
                // Log the exception for debugging purposes
                Console.WriteLine(ex);

                // Return an appropriate error response
                return StatusCode(500, "An internal server error occurred.");
            }
        }

        [HttpPost("AddGroupMembers")]
        public async Task<IActionResult> AddGroupMembers([FromBody] AddMemberDTO addMemberDTO)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var updatedGroupChat = await _groupChatService.AddGroupMembers(addMemberDTO.GroupId, addMemberDTO.MemberIds, currentUserId);

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

        [HttpPut("editGroupName")]
        public async Task<IActionResult> EditGroupNameAsync([FromQuery] UpdateGroupNameDTO model)
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

        [HttpDelete("{groupId}/remove-members")]
        public async Task<IActionResult> RemoveGroupMembers(string groupId, [FromQuery] RemoveGroupMembersDTO removeMembersDTO)
        {
            var groupChat = await _groupChatService.RemoveGroupMembers(groupId, removeMembersDTO.MemberIds, removeMembersDTO.AdminUserId);

            if (groupChat != null)
            {
                var options = new JsonSerializerOptions
                {
                    ReferenceHandler = ReferenceHandler.Preserve
                };

                return Ok(JsonSerializer.Serialize(groupChat, options));
            }

            return NotFound(new { error = "Error while remove member from group" }); // You may want to return an error response here.
        }

        [HttpGet("GetAllGroups")]
        [Authorize]
        public async Task<IActionResult> GetAllGroups()
        {
            try
            {
                // Retrieve the current user's ID (you can use your authentication mechanism)
                var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                var groups = await _groupChatService.GetAllGroupsAsync(currentUserId);
                if (groups != null)
                {
                    var groupData = groups.Select(group => new
                    {
                        Id = group.Id,
                        Name = group.Name,
                        CreatorUserId = group.CreatorUserId,
                        // Add other properties as needed
                    }).ToList();

                    return Ok(groupData);
                }

                return NotFound(new { error = "No groups found" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "An error occurred while processing your request" });
            }
        }

        [HttpGet("fetchGroupMembers")]
        public async Task<IActionResult> GetGroupMembers(string groupId)
        {
            try
            {
                var groupMembers = await _groupChatService.GetGroupMembersAsync(groupId);

                if (groupMembers != null)
                {
                    //var options = new JsonSerializerOptions
                    //{
                    //    ReferenceHandler = ReferenceHandler.Preserve,
                    //    // Other options as needed
                    //};

                    //var json = JsonSerializer.Serialize(groupMembers, options);

                    return Ok(groupMembers);
                }
                else
                {
                    return NotFound($"Group with ID {groupId} not found.");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }

        [HttpPost("{groupId}/makeUserAdmin")]
        [Authorize]
        public async Task<IActionResult> MakeUserAdminInGroup(string groupId, [FromQuery] string userId)
        {
            try
            {
                // Retrieve the current user's ID (you can use your authentication mechanism)
                var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                var result = await _groupChatService.MakeUserAdminInGroup(groupId, userId, currentUserId);

                if (result == null)
                {
                    return BadRequest(new { error = "Failed to make the user admin" });
                }

                return Ok(new { message = "User made admin successfully" });
            }
            catch (Exception ex)
            {
                // Log the exception
                return StatusCode(500, new { error = "An error occurred while making the user admin" });
            }
        }

        [HttpGet("{groupId}/GetUsersNotInGroup")]
        [Authorize]
        public async Task<IActionResult> GetUsersNotInGroup(string groupId)
        {
            try
            {
                // Call the service method to get users not in the group
                var usersNotInGroup = await _groupChatService.GetUsersNotInGroupAsync(groupId);

                // Return only UserId and UserName
                var userDTOs = usersNotInGroup.Select(user => new
                {
                    UserId = user.UserId,
                    UserName = user.UserName,
                }).ToList();

                return Ok(userDTOs);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "An error occurred while fetching users not in the group" });
            }
        }

        [HttpPost("SendMessageToGroupMembers")]
        public async Task<IActionResult> SendMessageToGroupMembers([FromQuery] string GroupId, [FromQuery] string MessageText)
        {
            try
            {
                var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
               var message =  await _groupChatService.SendMessageToGroupMembers(GroupId,MessageText,currentUserId);
                if (message != null)
                {
                    return Ok(new
                    {
                        newMessage = new
                        {
                            id = message.Id,
                            senderId = message.SenderId,
                            receiverId = message.ReceiverId,
                            content = message.Content,
                            timestamp = message.Timestamp
                        }
                    });
                }
                else
                {
                    return StatusCode(400, new { error = "Failed to send the message." });
                }
            }
            catch (Exception ex)
            {
                // Handle any exceptions and return an appropriate error response.
                return StatusCode(500, new { error = "An error occurred while sending messages." });
            }
        }
    }
}

