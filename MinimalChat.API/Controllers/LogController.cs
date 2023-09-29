using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Minimal_chat_application.Context;
using Minimal_chat_application.Model;

namespace Minimal_chat_application.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LogController : ControllerBase
    { 

        private readonly ApplicationDbContext _context;
     
        public LogController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("GetLog")]
        [Authorize]
        public IActionResult GetLogs(DateTime? startTime, DateTime? endTime)
        {
            try
            {
                // Set default values if parameters are not provided
                startTime ??= DateTime.Now.AddMinutes(-5);
                endTime ??= DateTime.Now;

                if (startTime > endTime)
                {
                    return BadRequest(new { error = "Invalid request parameters: StartTime cannot be greater than EndTime." });
                }

                var logs = _context.Logs
                    .Where(log => log.Timestamp >= startTime && log.Timestamp <= endTime)
                    .ToList();

                if (logs.Count == 0)
                {
                    return NotFound(new { error = "No logs found" });
                }

                return Ok(logs);
            }
            catch (Exception ex)
            {
                // Handle exceptions and return an error response
                return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving log data.");
            }
        }
    }
}
