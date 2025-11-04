namespace BankApi.Middleware;
using BankApi.Services;

public class SessionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly RedisService _redis;

    public SessionMiddleware(RequestDelegate next, RedisService redis)
    {
        _next = next;
        _redis = redis;
    }
    public async Task InvokeAsync(HttpContext context)
    {    
        var isAuth = context.User?.Identity?.IsAuthenticated ?? false;

        if (isAuth)
        {   
            var userId = context.User!.FindFirst("id")?.Value;
           
            if (!string.IsNullOrEmpty(userId))
            {
                var key = $"session:{userId}";
                var exists = await _redis.GetStringAsync(key);

                if (exists == null)
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    await context.Response.WriteAsync("Session expired");
                    return;
                }

                await _redis.SetStringAsync(key, "online", TimeSpan.FromMinutes(15));
            }
        }
        await _next(context);
    }

}
