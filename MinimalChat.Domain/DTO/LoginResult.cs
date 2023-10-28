using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinimalChat.Domain.DTO
{
    public class LoginResult
    {
        public bool Succeeded { get; set; }
        public string Token { get; set; }
        public object Profile { get; set; }
        public string Error { get; set; } // You can include an error message property.
    }

}
