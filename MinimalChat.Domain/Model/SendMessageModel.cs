using System.ComponentModel.DataAnnotations;

namespace Minimal_chat_application.Model
{
    public class SendMessageModel
    {
        [Required]
        public string ReceiverId { get; set; }

        [Required]
        public string Content { get; set; }
    }

}
