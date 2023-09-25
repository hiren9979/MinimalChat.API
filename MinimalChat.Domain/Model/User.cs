using Microsoft.AspNetCore.Identity;

namespace Minimal_chat_application.Model
{
    public class User : IdentityUser
    {

        public string FirstName { get; set; }
        public string LastName { get; set; }
    }
}
