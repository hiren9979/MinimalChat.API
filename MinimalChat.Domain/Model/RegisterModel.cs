using System.ComponentModel.DataAnnotations;

namespace Minimal_chat_application.Model
{
    public class RegisterModel
    {
        [Key]
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }

        [Required]
        public string FirstName { get; set; }

        [Required]
        public string LastName { get; set; }
    }
}
