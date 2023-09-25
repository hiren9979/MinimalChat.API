using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinimalChat.Domain.Model
{
    public class ApiResponse<T>
    {
        public string Message { get; set; }
        public T Data { get; set; }
        public int StatusCode { get; set; }
    }
}
