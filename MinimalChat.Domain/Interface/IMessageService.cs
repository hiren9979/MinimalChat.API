using Minimal_chat_application.Model;
using MinimalChat.Domain.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinimalChat.Domain.Interface
{
    public interface IMessageService
    {
        Task<Message> SendMessage(string senderId, SendMessageModel sendMessageModel);
        Task<Message> EditMessage(int messageId, string loginUserId, EditMessageModel editMessageModel);
        Task<bool> DeleteMessage(int messageId, string loginUserId);
        Task<object> GetConversationHistory(string currentUserId, string receiverId, string sort, DateTime? time, int? count, bool isGroup);
        List<EmojiReactionResponseDTO> GetEmojiReactionsForMessage(int messageId);
        Task<object> SearchConversations(string currentUserId, string query, string receiverId);
        Task<object> GetTaggedMessages();
        Task AddEmojiReaction(int messageId, string emoji);
    }

}
