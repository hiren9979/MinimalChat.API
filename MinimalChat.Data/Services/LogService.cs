using Microsoft.AspNetCore.Mvc;
using Minimal_chat_application.Context;
using Minimal_chat_application.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinimalChat.Data.Services
{
    public class LogService
    {
        private readonly ApplicationDbContext _context;

        public LogService(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<LogModel> GetLogs(DateTime startTime, DateTime endTime)
        {

            if (startTime > endTime)
            {
                return null;
            }

            var logs = _context.Logs
                     .Where(log => log.Timestamp >= startTime && log.Timestamp <= endTime)
                     .ToList();



            return logs;
        }
    }
}
