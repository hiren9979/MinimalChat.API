using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Minimal_chat_application.Context;
using Minimal_chat_application.Model;
using MinimalChat.Domain.DTO;
using MinimalChat.Domain.Interface;
using MinimalChat.Domain.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Threading.Tasks;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore.Infrastructure.Internal;

namespace MinimalChat.Data.Services
{
    public class GroupChatService : IGroupChat
    {
        private readonly UserManager<User> _userManager;
        private readonly ApplicationDbContext _dbContext;

        public GroupChatService(ApplicationDbContext context, UserManager<User> userManager)
        {
            _dbContext = context;
            _userManager = userManager;
        }

        public async Task<GroupDTO> CreateGroupChat(GroupChatDTO model, string currentUserId)
        {
            // Generate a random and unique GroupId(GUID)
            string uniqueGroupId = Guid.NewGuid().ToString();

            // Check if a group chat with the generated GroupId already exists
            if (_dbContext.GroupChats.Any(g => g.Id == uniqueGroupId) || _dbContext.GroupChats.Any(g => g.Name == model.Name))
            {
                return null;
            }



            var newGroupChat = new GroupChat
            {
                Name = model.Name,
                CreatorUserId = model.CreatorUserId,
                Id = uniqueGroupId,
               

            };
            newGroupChat.GroupMembers = new List<GroupMember>
            {
                new GroupMember
                {
                    UserId = model.CreatorUserId,
                    IsAdmin = true
                }
            };

            await _dbContext.GroupChats.AddAsync(newGroupChat);


            // Check if there are user IDs to add as members (you can keep this part for additional members)
            if (model.SelectedUserIds != null && model.SelectedUserIds.Any())
            {
                // Add the selected users as group members
                foreach (var userId in model.SelectedUserIds)
                {
                    if (!newGroupChat.GroupMembers.Any(gm => gm.UserId == userId))
                    {
                        var groupMember = new GroupMember
                        {
                            UserId = userId,
                            GroupChatId = newGroupChat.Id, // Set the GroupChatId
                            IsAdmin = userId == currentUserId
                        };
                        newGroupChat.GroupMembers.Add(groupMember);
                    }
                }
            }

            var groupdto = new GroupDTO
            {
                Id = uniqueGroupId,
                Name = model.Name,
            };

            try
            {
                await _dbContext.GroupChats.AddAsync(newGroupChat);
                await _dbContext.SaveChangesAsync();

                return groupdto;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw;
            }
         
        }


        public async Task<GroupChat> AddGroupMembers(string groupId, List<string> memberIds, string currentUserId)
        {
            // Get the group chat by its ID and eagerly load the GroupMembers and associated User entities
            var groupChat = await _dbContext.GroupChats
                .Include(g => g.GroupMembers)
                .ThenInclude(gm => gm.User)
                .FirstOrDefaultAsync(g => g.Id == groupId);

            memberIds.Add(currentUserId);

            if (groupChat == null)
            {
                Console.WriteLine("GroupId is Null Please provide GroupId for identify Group");
                // Handle the case when the group chat doesn't exist (e.g., return an error or throw an exception).
                return null;
            }

            // Retrieve the users based on their IDs

            // Add the selected users as group members
            foreach (var user in memberIds)
            {
                if (user != null)
                {

                    if (groupChat.GroupMembers == null)
                    {
                        groupChat.GroupMembers = new List<GroupMember>();
                    }

                    bool isAdmin = user == currentUserId;

                    groupChat.GroupMembers.Add(new GroupMember
                    {
                        GroupChatId = groupId,
                        UserId = user,
                        IsAdmin = isAdmin // You can set this value as needed
                    });
                }
                else
                {
                    return null; // You may want to return an error response here.
                }
            }

            try
            {
                await _dbContext.SaveChangesAsync();
                return groupChat;

            }
            catch(Exception ex)
            {
                throw ex;
            }

       
        }

        public async Task<GroupChat> EditGroupName(UpdateGroupNameDTO model)
        {
                var groupChat =  _dbContext.GroupChats.FirstOrDefault(g => g.Id == model.GroupId);
                if (groupChat == null)
                {
                    return null;
                }

                // Update the group name
                groupChat.Name = model.UpdatedGroupName;

                // Save the changes to the database
                await _dbContext.SaveChangesAsync();

                return groupChat;
            
        }

        public async Task<bool> DeleteGroupAsync(string groupId)
        {
            var groupChat = await _dbContext.GroupChats.FirstOrDefaultAsync(g => g.Id == groupId);

            if (groupChat == null)
            {
                Console.WriteLine("Group not found.");
                return false;
            }

            // Remove the group chat from the database
            _dbContext.GroupChats.Remove(groupChat);
            await _dbContext.SaveChangesAsync();

            return true;
        }

        public async Task<GroupChat> RemoveGroupMembers(string groupId, List<string> memberIds, string adminUserId)
        {
            var groupChat = await _dbContext.GroupChats
                .Include(g => g.GroupMembers)
                .ThenInclude(gm => gm.User)
                .FirstOrDefaultAsync(g => g.Id == groupId);

            if (groupChat == null)
            {
                Console.WriteLine("GroupId is Null. Please provide a valid GroupId to identify the group.");
                return null;
            }

            // Check if the user making the request is an admin of the group
            var isAdmin = groupChat.GroupMembers.Any(member => member.User.Id == adminUserId && member.IsAdmin);

            if (!isAdmin)
            {
                Console.WriteLine($"User {adminUserId} is not an admin of the group and cannot remove members.");
                return null;
            }

            // Retrieve the users based on their IDs
            var membersToRemove = await _userManager.Users.Where(u => memberIds.Contains(u.Id)).ToListAsync();

            if (membersToRemove.Any())
            {
                // Remove the selected users from the group
                foreach (var user in membersToRemove)
                {
                    var groupMember = groupChat.GroupMembers.FirstOrDefault(member => member.User.Id == user.Id);

                    if (groupMember != null)
                    {
                        groupChat.GroupMembers.Remove(groupMember);
                    }
                    else
                    {
                        Console.WriteLine($"User {user.Id} is not a member of the group.");
                        // Handle the case when a user is not a member (e.g., return an error or throw an exception).
                    }
                }

                await _dbContext.SaveChangesAsync();

                return groupChat;
            }

            // Handle the case when no valid members were found (e.g., return an error or throw an exception).
            return null;
        }

        public async Task<List<GroupChat>> GetAllGroupsAsync()
        {
            try
            {
                // Fetch all groups from the database
                var groups = await _dbContext.GroupChats.ToListAsync();
                return groups;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw;
            }
        }

        public async Task<List<GroupMemberDTO>> GetGroupMembersAsync(string groupId)
        {
            var groupChat = await _dbContext.GroupChats
                .Include(g => g.GroupMembers)
                .ThenInclude(gm => gm.User)
                .FirstOrDefaultAsync(g => g.Id == groupId);

            if (groupChat == null)
            {
                return null; // You may want to return an error response here.
            }

            if (groupChat.GroupMembers != null)
            {
                return groupChat.GroupMembers.Select(gm => new GroupMemberDTO
                {
                    UserId = gm.User.Id,
                    UserName = gm.User.UserName,
                    IsAdmin = gm.IsAdmin
                }).ToList();
            }

            return new List<GroupMemberDTO>();
        }


    }
}
