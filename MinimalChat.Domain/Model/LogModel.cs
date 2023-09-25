using System;

namespace Minimal_chat_application.Model // Use your actual namespace
{
    public class LogModel
    {
        public int Id { get; set; } // Add appropriate properties for your log entries
        public string IpAddress { get; set; }
        public string RequestBody { get; set; }
        public DateTime Timestamp { get; set; }
        public string Username { get; set; }
    }
}
