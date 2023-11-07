using Minimal_chat_application.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinimalChat.Domain.Interface
{
    public interface ILogService
    {
        List<LogModel> GetLogs(DateTime startTime, DateTime endTime);
    }

}
