using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Minimal_chat_application.Context;
using Minimal_chat_application.Model; // Update the namespace to your actual model namespace

public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;

    public RequestLoggingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, ApplicationDbContext _context)
    {
            var request = context.Request;

            // Enable rewinding the request body so it can be read multiple times
            context.Request.EnableBuffering();

            // Read the request body
            string requestBody;
            using (var reader = new StreamReader(request.Body, Encoding.UTF8, true, 1024, true))
            {
                requestBody = await reader.ReadToEndAsync();
            }

            // Reset the custom request body stream position to the beginning
            context.Request.Body.Position = 0;

            // Log the request details
            var requestLog = new LogModel
            {
                IpAddress = context.Connection.RemoteIpAddress?.ToString(),
                Timestamp = DateTime.Now,
                Username = context.User.Identity.IsAuthenticated ? context.User.Identity.Name : string.Empty,
                RequestBody = requestBody,
            };

            // Check the content type before accessing the Form property
            if (context.Request.HasFormContentType)
            {
                // Access the Form property if the request has a form content type
                var formData = context.Request.Form;
                // You can access form fields here if needed
            }

            // Log this information in the database using log Repository     
            // Continue processing the request
       

            try
            {
                // Log this information in the database using log Repository
                await _context.AddAsync(requestLog);
                await _context.SaveChangesAsync();
            await _next(context);
        }
            catch (Exception ex)
            {
            await context.Response.WriteAsync($"Internal Server Error: {ex.Message}");
            }
    }
       
}

public static class RequestLoggingMiddlewareExtensions
{
    public static IApplicationBuilder UseRequestLoggingMiddleware(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<RequestLoggingMiddleware>();
    }
}