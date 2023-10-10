using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Minimal_chat_application.Context;
using Minimal_chat_application.Model;
using MinimalChat.Domain.Interface;
using MinimalChat.Domain.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinimalChat.Data.Services
{
    public  class GroupChatService : IGroupChat
    {
        private readonly ApplicationDbContext _dbContext;

        public GroupChatService(ApplicationDbContext context)
        {
            _dbContext = context;
        }

        public async Task<GroupChat> CreateGroupChat(GroupChat model)
        {
            // Create a new GroupChatModel instance
            var newGroupChat = new GroupChat
            {
                // Populate the properties of the new group chat
                Name = model.Name,
                MemberIds = new List<int> { model.CreatorUserId } // Include the creator as a member
                // You can set other properties as needed
            };

            // Add the new group chat to the DbSet in your DbContext
            _dbContext.GroupChats.Add(newGroupChat);
            _dbContext.SaveChanges();

            return newGroupChat;
        }
        
    }
}
