using System.Net;
using System.Text.Json;

namespace ChatApp.Middleware
{
    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionMiddleware> _logger;

        public GlobalExceptionMiddleware(RequestDelegate next,
                                         ILogger<GlobalExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception: {Message}", ex.Message);
                await HandleExceptionAsync(context, ex);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception ex)
        {
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

            if (context.Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                context.Response.ContentType = "application/json";
                return context.Response.WriteAsync(JsonSerializer.Serialize(new
                {
                    error = "An unexpected error occurred.",
                    detail = ex.Message
                }));
            }

            context.Response.Redirect("/Home/Error");
            return Task.CompletedTask;
        }
    }
}
