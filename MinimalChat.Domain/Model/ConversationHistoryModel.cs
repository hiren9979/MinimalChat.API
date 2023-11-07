using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinimalChat.Domain.Model
{
    public class ConversationHistoryRequestModel
    {
        public string ReceiverId { get; set; }
        public string? Sort { get; set; }
        public DateTime? Time { get; set; }
        public int? Count { get; set; }
        public bool IsGroup { get; set; }
    }

}
