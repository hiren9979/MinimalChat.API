using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Minimal_chat_application.Context;
using Minimal_chat_application.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace MinimalChat.Data.Services
{
    public class MessageService
    {
        private readonly UserManager<User> _userManager;
        private readonly ApplicationDbContext _context;


        public MessageService(UserManager<User> userManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;

        }

        public async Task<Message> SendMessage(string senderId, SendMessageModel sendMessageModel)
        {
            // Check if the receiver exists
            var receiver = await _userManager.FindByIdAsync(sendMessageModel.ReceiverId);
            if (receiver == null)
            {
                throw new ArgumentException("Receiver user not found");
            }

            var message = new Message
            {
                SenderId = senderId,
                ReceiverId = sendMessageModel.ReceiverId,
                Content = sendMessageModel.Content,
                Timestamp = DateTime.Now
            };

            _context.Messages.Add(message);
            await _context.SaveChangesAsync();
            message.Id = message.Id;
            return message;
        }

        public async Task<Message> EditMessage(int messageId, string loginUserId, EditMessageModel editMessageModel)
        {
            var message = await _context.Messages.FindAsync(messageId);

            if (message == null)
            {
                return null;
            }

            if (message.SenderId != loginUserId)
            {
                return null; // Handle unauthorized access here
            }

            message.Content = editMessageModel.Content;

            await _context.SaveChangesAsync();

            return message;
        }

        public async Task<bool> DeleteMessage(int messageId, string loginUserId)
        {
            var message = await _context.Messages.FindAsync(messageId);

            if (message == null)
            {
                return false;
            }

            if (message.SenderId != loginUserId)
            {
                return false; // Handle unauthorized access here
            }

            _context.Messages.Remove(message);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<object> GetConversationHistory(string currentUserId, string receiverId, string sort, DateTime? time, int? count, bool isGroup)
        {
            User chattingUser = null;
            string chattingUserId = receiverId;

            if (!isGroup)
            {
                chattingUser = await _userManager.FindByIdAsync(receiverId);
                if (chattingUser == null)
                {
                    return null; // Handle the case where the receiver user is not found
                }
            }

            var query = isGroup
                         ? _context.Messages
                             .Where(m => m.ReceiverId == receiverId)
                             .AsQueryable()
                         : _context.Messages
                             .Where(m => (m.SenderId == currentUserId && m.ReceiverId == chattingUserId)
                                         || (m.SenderId == chattingUserId && m.ReceiverId == currentUserId))
                             .AsQueryable();

            if (sort == "desc")
            {
                query = query.OrderByDescending(m => m.Timestamp);
            }
            else
            {
                query = query.OrderBy(m => m.Timestamp);
            }

            TimeZoneInfo sourceTimeZone = TimeZoneInfo.Utc;
            TimeZoneInfo targetTimeZone = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");

            DateTime istTime = TimeZoneInfo.ConvertTimeFromUtc(time ?? DateTime.UtcNow, targetTimeZone);

            query = query.Where(m => m.Timestamp < istTime);

            count = count.HasValue && count <= 20 ? count : 20;

            var messages = await query
                            .OrderByDescending(m => m.Timestamp)
                            .Take(count.Value)
                            .ToListAsync();

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

            return response;
        }

        public async Task<object> SearchConversations(string currentUserId, string query, string receiverId)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return null; // Handle the case where the query parameter is invalid
            }

            var queryNormalized = query.ToLowerInvariant(); // Normalize query for case-insensitive search

            var messages = await _context.Messages
                .Where(m =>
                    ((m.SenderId == currentUserId && m.ReceiverId == receiverId)
                    || (m.SenderId == receiverId && m.ReceiverId == currentUserId))
                    && m.Content.ToLower().Contains(queryNormalized))
                .OrderByDescending(m => m.Timestamp)
                .ToListAsync();

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

            return response;
        }

        public async Task<object> GetTaggedMessages()
        {
            var taggedMessages = await _context.Messages
                .Where(m => m.Content.StartsWith("[[]]"))
                .OrderByDescending(m => m.Timestamp)
                .ToListAsync();

            var userIds = taggedMessages
                .SelectMany(m => new[] { m.SenderId, m.ReceiverId })
                .Distinct();

            var users = await _userManager.Users
                .Where(u => userIds.Contains(u.Id))
                .ToDictionaryAsync(u => u.Id);

            var response = new
            {
                messages = taggedMessages.Select(m => new
                {
                    id = m.Id,
                    sender = users.ContainsKey(m.SenderId) ? new
                    {
                        id = users[m.SenderId].Id,
                        userName = users[m.SenderId].UserName,
                        email = users[m.SenderId].Email
                        // Add other user properties you want to include
                    } : null,
                    receiver = users.ContainsKey(m.ReceiverId) ? new
                    {
                        id = users[m.ReceiverId].Id,
                        userName = users[m.ReceiverId].UserName,
                        // Add other user properties you want to include
                    } : null,
                    content = m.Content,
                    timestamp = m.Timestamp
                })
            };

            return response;
        }


    }
}
