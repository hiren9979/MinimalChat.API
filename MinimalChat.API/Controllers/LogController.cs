using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Minimal_chat_application.Context;
using Minimal_chat_application.Model;
using MinimalChat.Data.Services;
using MinimalChat.Domain.Interface;

namespace Minimal_chat_application.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LogController : ControllerBase
    { 

        private readonly ApplicationDbContext _context;
        private readonly ILogService _logService;

        public LogController(ApplicationDbContext context,ILogService logService)
        {
            _context = context;
            _logService = logService;
        }


        [HttpGet("GetLog")]
        [Authorize]
        public IActionResult GetLogs([FromQuery ]DateTime? startTime, [FromQuery] DateTime? endTime)
        {
            try
            {
                // Set default values if parameters are not provided
                startTime ??= DateTime.Now.AddMinutes(-5);
                endTime ??= DateTime.Now;

                var logs = _logService.GetLogs(startTime.Value, endTime.Value);

                if (logs == null)
                {
                    return BadRequest(new { error = "Invalid request parameters: StartTime cannot be greater than EndTime." });
                }

                if (logs.Count == 0)
                {
                    return Ok(new { message = "No logs found" });
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
