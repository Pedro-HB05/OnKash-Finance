using System.Net;
using System.Text.Json;

namespace OnKashFinance.API.Erros;

public class TratamentoErrosMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<TratamentoErrosMiddleware> _logger;

    public TratamentoErrosMiddleware(
        RequestDelegate next,
        ILogger<TratamentoErrosMiddleware> logger)
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
        catch (UnauthorizedAccessException ex)
        {
            await ResponderErro(
                context,
                HttpStatusCode.Unauthorized,
                ex.Message
            );
        }
        catch (KeyNotFoundException ex)
        {
            await ResponderErro(
                context,
                HttpStatusCode.NotFound,
                ex.Message
            );
        }
        catch (InvalidOperationException ex)
        {
            await ResponderErro(
                context,
                HttpStatusCode.BadRequest,
                ex.Message
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Erro inesperado na aplicação."
            );

            await ResponderErro(
                context,
                HttpStatusCode.InternalServerError,
                "Ocorreu um erro interno no servidor."
            );
        }
    }

    private static async Task ResponderErro(
        HttpContext context,
        HttpStatusCode statusCode,
        string mensagem)
    {
        context.Response.StatusCode = (int)statusCode;

        context.Response.ContentType =
            "application/json";

        var resposta = new
        {
            status = (int)statusCode,
            mensagem
        };

        var json = JsonSerializer.Serialize(resposta);

        await context.Response.WriteAsync(json);
    }
}