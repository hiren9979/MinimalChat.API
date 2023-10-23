using Microsoft.AspNetCore.Identity;
using MinimalChat.Domain.Model;

namespace Minimal_chat_application.Model
{
    public class User : IdentityUser
    {

        public string FirstName { get; set; }
        public string LastName { get; set; }

        public virtual ICollection<GroupMember>? GroupMembers { get; set; }

        public static string? FindFirst(string nameIdentifier)
        {
            throw new NotImplementedException();
        }
    }
}
