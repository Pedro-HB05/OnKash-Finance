namespace OnKashFinance.API.Seguranca;

public class CabecalhosSegurancaMiddleware
{
    private readonly RequestDelegate _next;
    public CabecalhosSegurancaMiddleware(RequestDelegate next) => _next = next;
    public async Task InvokeAsync(HttpContext context)
    {
        context.Response.OnStarting(() =>
        {
            var h = context.Response.Headers;
            h.TryAdd("X-Content-Type-Options", "nosniff");
            h.TryAdd("X-Frame-Options", "DENY");
            h.TryAdd("Referrer-Policy", "no-referrer");
            h.TryAdd("Permissions-Policy", "camera=(), microphone=(), geolocation=(), payment=()");
            h.TryAdd("Content-Security-Policy", "default-src 'none'; frame-ancestors 'none'; base-uri 'none'; form-action 'none'");
            return Task.CompletedTask;
        });
        await _next(context);
    }
}
