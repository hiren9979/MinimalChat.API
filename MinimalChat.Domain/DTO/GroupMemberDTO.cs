using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinimalChat.Domain.DTO
{
    public class GroupMemberDTO
    {
        public string UserName { get; set; }
        public string UserId { get; set; }

        public bool IsAdmin {  get; set; }
    }

}
