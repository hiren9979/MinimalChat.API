using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MinimalChat.Domain.Model
{
    public class GroupMember
    {
        public bool IsAdmin { get; set; }

       public string GroupId { get; set; }
       public GroupChat GroupChat { get; set; }

        // Foreign key to reference GroupChat

        public string UserId { get; set; }
    }
}
