using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinimalChat.Domain.DTO
{
    public class SendMessageToGroupDTO
    {
        public string GroupId { get; set; }
        public string MessageText { get; set; }
    }

}
