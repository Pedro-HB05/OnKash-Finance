using Microsoft.EntityFrameworkCore;
using OnKashFinance.API.Dados;

namespace OnKashFinance.API.Seguranca;

public class RetencaoDadosService : BackgroundService
{
    private readonly IServiceScopeFactory _escopos;
    private readonly ILogger<RetencaoDadosService> _logger;
    public RetencaoDadosService(IServiceScopeFactory escopos, ILogger<RetencaoDadosService> logger)
    { _escopos = escopos; _logger = logger; }
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await using var escopo = _escopos.CreateAsyncScope();
                var db = escopo.ServiceProvider.GetRequiredService<OnKashDbContext>();
                var limiteAuditoria = DateTimeOffset.UtcNow.AddDays(-180);
                await db.AuditoriasOperacoes.Where(x => x.CriadoEm < limiteAuditoria).ExecuteDeleteAsync(stoppingToken);
                var limiteCodigo = DateTimeOffset.UtcNow.AddDays(-1);
                await db.Usuarios.Where(x => !x.EmailVerificado && x.CodigoVerificacaoExpiraEm < limiteCodigo)
                    .ExecuteUpdateAsync(x => x.SetProperty(u => u.CodigoVerificacaoEmail, (string?)null), stoppingToken);
            }
            catch (Exception ex) { _logger.LogError(ex, "Falha na rotina de retenção e descarte de dados."); }
            await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
        }
    }
}
