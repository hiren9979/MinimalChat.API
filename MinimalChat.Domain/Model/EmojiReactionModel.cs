using Minimal_chat_application.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinimalChat.Domain.Model
{
   
        public class EmojiReaction
        {
            public int Id { get; set; }
            public int MessageId { get; set; }
            public string UserId { get; set; }
            public string Emoji { get; set; }
            public Message Message { get; set; }
    }
}
