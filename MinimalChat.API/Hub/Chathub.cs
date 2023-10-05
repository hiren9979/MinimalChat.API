using Microsoft.AspNetCore.SignalR;
using Minimal_chat_application.Model;

namespace MinimalChat.API.Hubs
{
    public class ChatHub : Hub
    {
        public async Task SendMessage(Message message)
        {
            try
            {
                // Your message handling logic
                await Clients.All.SendAsync("ReceiveMessage", message);
            }
            catch (Exception ex)
            {
                // Handle exceptions and log errors
                Console.WriteLine($"Error in SendMessage: {ex.Message}");
            }
        }

        public async Task EditMessage(string content,string id)
        {
            try
            {
                // Your message handling logic
                await Clients.All.SendAsync("EditMessage", id,content);
            }
            catch (Exception ex)
            {
                // Handle exceptions and log errors
                Console.WriteLine($"Error in EditMessage: {ex.Message}");
            }
        }

        public async Task DeleteMessage(string id)
        {
            try
            {
                // Your message handling logic
                await Clients.All.SendAsync("DeleteMessage", id);
            }
            catch (Exception ex)
            {
                // Handle exceptions and log errors
                Console.WriteLine($"Error in DeleteMessage: {ex.Message}");
            }
        }

    }
}