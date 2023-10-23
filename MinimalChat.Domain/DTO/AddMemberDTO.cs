using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinimalChat.Domain.DTO
{
    public class AddMemberDTO
    {
        public string GroupId { get; set; }
        public List<string> MemberIds { get; set; }
    }

}
