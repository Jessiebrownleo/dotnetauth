namespace DotnetAuthentication.Middleware;

public class RateLimitingMiddleware
{
    private readonly RequestDelegate _next;
    private static readonly Dictionary<string, (int Count, DateTime WindowStart)> _requestCounts = new();
    private static readonly SemaphoreSlim _semaphore = new(1, 1);
    private const int MaxRequests = 100; // Per minute
    private readonly TimeSpan _window = TimeSpan.FromMinutes(1);

    public RateLimitingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var ip = context.Connection.RemoteIpAddress?.ToString();
        if (ip == null)
        {
            await _next(context);
            return;
        }

        await _semaphore.WaitAsync();
        try
        {
            if (!_requestCounts.TryGetValue(ip, out var data) || data.WindowStart.Add(_window) < DateTime.UtcNow)
            {
                _requestCounts[ip] = (1, DateTime.UtcNow);
            }
            else if (data.Count >= MaxRequests)
            {
                context.Response.StatusCode = 429; // Too Many Requests
                await context.Response.WriteAsync("Rate limit exceeded. Try again later.");
                return;
            }
            else
            {
                _requestCounts[ip] = (data.Count + 1, data.WindowStart);
            }
        }
        finally
        {
            _semaphore.Release();
        }

        await _next(context);
    }
}