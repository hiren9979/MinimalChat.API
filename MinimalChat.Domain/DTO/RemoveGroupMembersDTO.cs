using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinimalChat.Domain.DTO
{
    public class RemoveGroupMembersDTO
    {
        public List<string> MemberIds { get; set; }
        public string AdminUserId { get; set; }
    }
}
