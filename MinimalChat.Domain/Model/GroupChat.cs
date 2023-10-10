using Microsoft.EntityFrameworkCore;
using Minimal_chat_application.Model;
using System.ComponentModel.DataAnnotations;

namespace MinimalChat.Domain.Model
{
    public class GroupChat
    {
        [Key]
        [Required]
        public string Name { get; set; }

        // Navigation property for GroupMembers
        public List<GroupMember> Members { get; set; } 

        public int CreatorUserId { get; set; }

        // Navigation property for Messages
        public List<Message> Messages { get; set; } 

    }
}
