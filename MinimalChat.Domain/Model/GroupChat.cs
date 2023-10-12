using Microsoft.EntityFrameworkCore;
using Minimal_chat_application.Model;
using System.ComponentModel.DataAnnotations;

namespace MinimalChat.Domain.Model
{
    public class GroupChat
    {
        [Key]
        public string? Id { get; set; }
        [Required]
        public string Name { get; set; }

        // Navigation property for GroupMembers

        public ICollection<GroupMember>? GroupMembers { get; set; } // Use List instead of ICollection

        public string CreatorUserId { get; set; }

        // Navigation property for Messages
        public List<Message>? Messages { get; set; } 

    }
}
