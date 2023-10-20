using MinimalChat.Domain.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinimalChat.Domain.DTO
{
    public class AdminResponse
    {
        public bool IsSuccessful { get; set; }
        public string Message { get; set; }
        public GroupChat GroupChat { get; set; }
    }
}
