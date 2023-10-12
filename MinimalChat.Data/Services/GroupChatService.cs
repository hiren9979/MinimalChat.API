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

namespace MinimalChat.Data.Services
{
    public  class GroupChatService : IGroupChat
    {
        private readonly UserManager<User> _userManager;
        private readonly ApplicationDbContext _dbContext;

        public GroupChatService(ApplicationDbContext context,UserManager<User> userManager)
        {
            _dbContext = context;
            _userManager = userManager;
        }

        public async Task<GroupChat> CreateGroupChat(GroupChatDTO model)
        {

            //var creatorUser = await _userManager.FindByIdAsync(model.CreatorUserId);
            //if (creatorUser == null)
            //{
            //    Console.WriteLine("The creator user does not exist.");
            //    return null;
            //}
            // Create a new GroupChatModel instance
            var newGroupChat = new GroupChat
            {
                Name = model.Name,
                CreatorUserId = model.CreatorUserId, // Set the creator user
                Id = model.Id
            };

            _dbContext.GroupChats.Add(newGroupChat);
            _dbContext.SaveChanges();

            return newGroupChat;
        }

        public async Task<GroupChat> AddGroupMembers(string groupId, List<string> memberIds)
        {
            // Get the group chat by its ID and eagerly load the GroupMembers and associated User entities
            var groupChat = await _dbContext.GroupChats
                .Include(g => g.GroupMembers)
                .ThenInclude(gm => gm.User)
                .FirstOrDefaultAsync(g => g.Id == groupId);



            if (groupChat == null)
            {
                Console.WriteLine("GroupId is Null Please provide GroupId for identify Group");
                // Handle the case when the group chat doesn't exist (e.g., return an error or throw an exception).
                return null;
            }

            // Retrieve the users based on their IDs
            var membersToAdd = await _userManager.Users.Where(u => memberIds.Contains(u.Id)).ToListAsync();

            if (membersToAdd.Any())
            {
                // Add the selected users as group members
                foreach (var user in membersToAdd)
                {
                    if (user != null)
                    {
                        if (!groupChat.GroupMembers.Any(member => member.User.Id == user.Id))
                        {
                            if (groupChat.GroupMembers == null)
                            {
                                groupChat.GroupMembers = new List<GroupMember>();
                            }

                            groupChat.GroupMembers.Add(new GroupMember
                            {
                                User = user,
                                IsAdmin = false // You can set this value as needed
                            });
                        }
                        else
                        {
                            Console.WriteLine($"User {user.Id} is already a member of the group.");
                            // Handle the case when a user is already a member (e.g., return an error or throw an exception).
                            return null; // You may want to return an error response here.
                        }
                    }
                }

                await _dbContext.SaveChangesAsync();

                return groupChat;
            }

            // Handle the case when no valid members were found (e.g., return an error or throw an exception).
            return null;
        }

    }
}
