using MinimalChat.Domain.DTO;
using MinimalChat.Domain.Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MinimalChat.Domain.Interface
{
    public interface IEmojiReactionService
    {
        Task<EmojiReaction> AddEmojiReaction(int messageId, string userId, string emoji);
        List<EmojiReactionResponseDTO> GetEmojiReactionsForMessage(int messageId);
    }
}
