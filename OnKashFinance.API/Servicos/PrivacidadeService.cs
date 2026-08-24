using System.Text.Json;
using System.Text.Encodings.Web;
using Microsoft.EntityFrameworkCore;
using OnKashFinance.API.Autenticacao;
using OnKashFinance.API.Dados;
using OnKashFinance.API.DTOs;
using OnKashFinance.API.Modelos;

namespace OnKashFinance.API.Servicos;

public class PrivacidadeService
{
    private readonly OnKashDbContext _db;
    private readonly UsuarioAtualService _usuarioAtual;
    private readonly IHttpContextAccessor _http;
    private readonly EmailService _email;

    public PrivacidadeService(OnKashDbContext db, UsuarioAtualService usuarioAtual, IHttpContextAccessor http, EmailService email)
    { _db = db; _usuarioAtual = usuarioAtual; _http = http; _email = email; }

    public async Task<PrivacidadeResumoResposta> ObterResumoAsync()
    {
        var usuarioId = _usuarioAtual.ObterUsuarioId();
        var aceite = await _db.AceitesLegais.AsNoTracking().Where(x => x.UsuarioId == usuarioId)
            .OrderByDescending(x => x.AceitoEm).FirstOrDefaultAsync();
        var solicitacoes = await _db.SolicitacoesPrivacidade.AsNoTracking().Where(x => x.UsuarioId == usuarioId)
            .OrderByDescending(x => x.CriadoEm).Take(30)
            .Select(x => new SolicitacaoPrivacidadeResposta(x.Protocolo, x.Tipo, x.Status, x.Detalhes, x.CriadoEm, x.ConcluidoEm)).ToListAsync();
        return new PrivacidadeResumoResposta
        {
            AceiteAtual = aceite?.PoliticaPrivacidadeVersao == GovernancaPrivacidade.VersaoAtual && aceite.TermosUsoVersao == GovernancaPrivacidade.VersaoAtual,
            AceitoEm = aceite?.AceitoEm,
            Solicitacoes = solicitacoes
        };
    }

    public async Task RegistrarAceiteAtualAsync()
    {
        var usuarioId = _usuarioAtual.ObterUsuarioId();
        if (await _db.AceitesLegais.AnyAsync(x => x.UsuarioId == usuarioId && x.PoliticaPrivacidadeVersao == GovernancaPrivacidade.VersaoAtual && x.TermosUsoVersao == GovernancaPrivacidade.VersaoAtual)) return;
        _db.AceitesLegais.Add(CriarAceite(usuarioId, _http.HttpContext));
        await _db.SaveChangesAsync();
    }

    public static AceiteLegal CriarAceite(Guid usuarioId, HttpContext? contexto) => new()
    {
        UsuarioId = usuarioId,
        PoliticaPrivacidadeVersao = GovernancaPrivacidade.VersaoAtual,
        TermosUsoVersao = GovernancaPrivacidade.VersaoAtual,
        AceitoEm = DateTimeOffset.UtcNow,
        EnderecoIp = Limitar(contexto?.Connection.RemoteIpAddress?.ToString(), 64),
        AgenteUsuario = Limitar(contexto?.Request.Headers.UserAgent.ToString(), 300)
    };

    public async Task<SolicitacaoPrivacidadeResposta> SolicitarDireitoAsync(SolicitarDireitoRequest request)
    {
        var tipo = request.Tipo.Trim().ToUpperInvariant();
        if (!GovernancaPrivacidade.TiposSolicitacao.Contains(tipo))
            throw new InvalidOperationException("Selecione um direito válido.");
        if (request.Detalhes?.Length > 2000) throw new InvalidOperationException("Os detalhes devem possuir no máximo 2.000 caracteres.");
        var agora = DateTimeOffset.UtcNow;
        var item = new SolicitacaoPrivacidade
        {
            UsuarioId = _usuarioAtual.ObterUsuarioId(), Tipo = tipo,
            Detalhes = string.IsNullOrWhiteSpace(request.Detalhes) ? null : request.Detalhes.Trim(),
            Protocolo = $"LGPD-{agora:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..8].ToUpperInvariant()}",
            Status = "RECEBIDA", CriadoEm = agora, AtualizadoEm = agora
        };
        _db.SolicitacoesPrivacidade.Add(item);
        await _db.SaveChangesAsync();
        var emailTitular = _usuarioAtual.ObterEmail();
        var seguro = HtmlEncoder.Default;
        var resumo = $"""<div style="font-family:Arial,sans-serif;max-width:600px;margin:auto;padding:28px"><h1 style="color:#116a71">Solicitação LGPD recebida</h1><p>Protocolo: <strong>{seguro.Encode(item.Protocolo)}</strong></p><p>Tipo: {seguro.Encode(item.Tipo)}</p><p>Registrada em {item.CriadoEm:dd/MM/yyyy HH:mm} UTC.</p><p>Você pode acompanhar o status em Privacidade e dados.</p></div>""";
        await _email.EnviarMensagemTransacionalAsync(_usuarioAtual.ObterNome(), emailTitular, $"Recebemos sua solicitação {item.Protocolo}", resumo);
        var alerta = $"""<div style="font-family:Arial,sans-serif;max-width:600px;margin:auto;padding:28px"><h1 style="color:#116a71">Nova solicitação de privacidade</h1><p>Protocolo: <strong>{seguro.Encode(item.Protocolo)}</strong></p><p>Titular: {seguro.Encode(emailTitular)}</p><p>Tipo: {seguro.Encode(item.Tipo)}</p><p>Detalhes: {seguro.Encode(item.Detalhes ?? "Não informado")}</p></div>""";
        await _email.EnviarMensagemTransacionalAsync("OnKash Finance", "onkashfinance@gmail.com", $"Ação necessária: {item.Protocolo}", alerta);
        return new(item.Protocolo, item.Tipo, item.Status, item.Detalhes, item.CriadoEm, null);
    }

    public async Task CorrigirPerfilAsync(string nome)
    {
        nome = nome.Trim();
        if (nome.Length is < 2 or > 150) throw new InvalidOperationException("Informe um nome válido, com até 150 caracteres.");
        var usuarioId = _usuarioAtual.ObterUsuarioId();
        var usuario = await _db.Usuarios.FirstAsync(x => x.Id == usuarioId);
        usuario.Nome = nome; usuario.AtualizadoEm = DateTimeOffset.UtcNow;
        await _db.SaveChangesAsync();
    }

    public async Task<byte[]> ExportarDadosAsync()
    {
        var usuarioId = _usuarioAtual.ObterUsuarioId();
        var usuario = await _db.Usuarios.AsNoTracking().Where(x => x.Id == usuarioId)
            .Select(x => new { x.Id, x.Nome, x.Email, TipoConta = x.TipoConta.ToString(), x.Ativo, x.EmailVerificado, x.CriadoEm, x.AtualizadoEm }).FirstAsync();
        object dadosFinanceiros;
        if (_usuarioAtual.EhPessoal())
        {
            dadosFinanceiros = new
            {
                contas = await _db.ContasPessoais.AsNoTracking().Where(x => x.UsuarioId == usuarioId).ToListAsync(),
                categorias = await _db.CategoriasPessoais.AsNoTracking().Where(x => x.UsuarioId == usuarioId).ToListAsync(),
                cartoes = await _db.CartoesPessoais.AsNoTracking().Where(x => x.UsuarioId == usuarioId).ToListAsync(),
                lancamentos = await _db.LancamentosPessoais.AsNoTracking().Where(x => x.UsuarioId == usuarioId).ToListAsync(),
                orcamentos = await _db.OrcamentosPessoais.AsNoTracking().Where(x => x.UsuarioId == usuarioId).ToListAsync(),
                recorrencias = await _db.LancamentosRecorrentesPessoais.AsNoTracking().Where(x => x.UsuarioId == usuarioId).ToListAsync()
            };
        }
        else
        {
            var empresaId = _usuarioAtual.ExigirEmpresaId();
            dadosFinanceiros = new
            {
                vinculo = await _db.EmpresaUsuarios.AsNoTracking().Where(x => x.UsuarioId == usuarioId && x.EmpresaId == empresaId)
                    .Select(x => new { x.EmpresaId, Empresa = x.Empresa.Nome, Perfil = x.Perfil.ToString(), x.Ativo, x.CriadoEm }).FirstOrDefaultAsync(),
                permissoes = await _db.PermissoesEmpresa.AsNoTracking().Where(x => x.EmpresaUsuario.UsuarioId == usuarioId && x.EmpresaUsuario.EmpresaId == empresaId)
                    .Select(x => new { x.Dashboard, x.Lancamentos, x.Contas, x.Clientes, x.Fornecedores, x.ContasPagar, x.ContasReceber, x.Categorias, x.Relatorios, x.Usuarios }).FirstOrDefaultAsync()
            };
        }
        var aceites = await _db.AceitesLegais.AsNoTracking().Where(x => x.UsuarioId == usuarioId)
            .Select(x => new { x.PoliticaPrivacidadeVersao, x.TermosUsoVersao, x.AceitoEm }).ToListAsync();
        var solicitacoes = await _db.SolicitacoesPrivacidade.AsNoTracking().Where(x => x.UsuarioId == usuarioId)
            .Select(x => new { x.Protocolo, x.Tipo, x.Status, x.Detalhes, x.CriadoEm, x.ConcluidoEm }).ToListAsync();
        return JsonSerializer.SerializeToUtf8Bytes(new
        {
            exportadoEm = DateTimeOffset.UtcNow, controlador = "Pedro Henrique Benvento — OnKash Finance",
            usuario, dadosFinanceiros, aceites, solicitacoes
        }, new JsonSerializerOptions { WriteIndented = true });
    }

    private static string? Limitar(string? valor, int limite) => string.IsNullOrWhiteSpace(valor) ? null : valor.Length <= limite ? valor : valor[..limite];
}
