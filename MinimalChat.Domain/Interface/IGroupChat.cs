using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MinimalChat.Domain.Model;

namespace MinimalChat.Domain.Interface
{
    public interface IGroupChat
    {
        Task<GroupChat> CreateGroupChat(GroupChat groupChatCreateModel);
    }
}
