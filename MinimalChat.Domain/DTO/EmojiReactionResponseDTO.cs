using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinimalChat.Domain.DTO
{
    public class EmojiReactionResponseDTO
    {
        public int Id { get; set; }
        public int MessageId { get; set; }
        public string UserId { get; set; }
        public string Emoji { get; set; }
        public string UserName { get; set; }
    }

}
