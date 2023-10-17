using Microsoft.EntityFrameworkCore;
using Minimal_chat_application.Model;
using MinimalChat.Domain.Model;
using System.ComponentModel.DataAnnotations;

namespace MinimalChat.Domain.DTO

{
    public class GroupDTO
    {
        public string? Id { get; set; }
       
        public string Name { get; set; }


    }
}
