using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
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
    public class EmojiReactionService  : IEmojiReactionService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly UserManager<User> _userManager;

        public EmojiReactionService(ApplicationDbContext context,UserManager<User> userManager)
        {
            _dbContext = context;
            _userManager = userManager;
        }

        public async Task<EmojiReaction> AddEmojiReaction(int messageId, string userId, string emoji)
        {
            var existingReaction = await _dbContext.EmojiReactions
                                    .FirstOrDefaultAsync(er => er.MessageId == messageId && er.UserId == userId);

            if (existingReaction != null)
            {
                existingReaction.Emoji = emoji;
                _dbContext.SaveChanges();
                return existingReaction;
            }
            else
            {
                var reaction = new EmojiReaction
                {
                    MessageId = messageId,
                    UserId = userId,
                    Emoji = emoji
                };

                _dbContext.EmojiReactions.Add(reaction);
                await _dbContext.SaveChangesAsync(); // Use asynchronous SaveChanges

                return reaction;
            }
        }

        public List<EmojiReactionResponseDTO> GetEmojiReactionsForMessage(int messageId)
        {
            var reactions = _dbContext.EmojiReactions
                            .Where(reaction => reaction.MessageId == messageId)
                            .ToList();

            // Create a list of EmojiReactionWithUser that includes user information
            var reactionsWithUser = new List<EmojiReactionResponseDTO>();

            foreach (var reaction in reactions)
            {
                var user = _userManager.FindByIdAsync(reaction.UserId).Result; // Assuming UserManager is used for user management

                if (user != null)
                {
                    var reactionWithUser = new EmojiReactionResponseDTO
                    {
                        Id = reaction.Id,
                        MessageId = reaction.MessageId,
                        UserId = reaction.UserId,
                        Emoji = reaction.Emoji,
                        UserName = user.FirstName + ' ' +  user.LastName,
                        
                    };

                    reactionsWithUser.Add(reactionWithUser);
                }
            }

            return reactionsWithUser;
        }

    }
}
