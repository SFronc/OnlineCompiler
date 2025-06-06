using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Writers;
using OnlineCompiler.Data;

namespace OnlineCompiler.ApiControllers
{
    public class ApiKeyMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ApiKeyMiddleware> _logger;

        public ApiKeyMiddleware(RequestDelegate next, ILogger<ApiKeyMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context, IServiceProvider serviceProvider)
        {
            if (context.Request.Path.StartsWithSegments("/swagger") ||
               context.Request.Path.StartsWithSegments("/api-docs") ||
               !context.Request.Path.StartsWithSegments("/api"))
            {
                await _next(context);
                return;
            }



            if (!context.Request.Headers.TryGetValue("X-API-Username", out var usernameHeader) ||
                !context.Request.Headers.TryGetValue("X-API-Key", out var apiKeyHeader))
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("API key or username missing");
                return;
            }

            var username = usernameHeader.ToString();
            var apiKey = apiKeyHeader.ToString();

            Console.WriteLine($"Trying log with data: {username}, key: {apiKey}");

            using var scope = serviceProvider.CreateScope();

            var _dbContext = scope.ServiceProvider.GetRequiredService<DataBaseContext>();

            var user = await _dbContext.User
                .FirstOrDefaultAsync(u => u.Username == username && u.ApiKey == apiKey);

            
            if (user == null)
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("Invalid API key or username");
                return;
            }

            context.Items["User"] = user;
            await _next(context);
        }
    }
}