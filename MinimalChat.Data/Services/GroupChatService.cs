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
        
    }
}
