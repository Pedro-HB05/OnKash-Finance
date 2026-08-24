using System.Net;
using System.Net.Mail;
using System.Net.Http.Json;
using System.Text.Encodings.Web;

namespace OnKashFinance.API.Servicos;

public class EmailService
{
    private readonly IConfiguration _configuracao;
    private readonly ILogger<EmailService> _logger;
    private readonly IHttpClientFactory _httpClientFactory;

    public EmailService(
        IConfiguration configuracao,
        ILogger<EmailService> logger,
        IHttpClientFactory httpClientFactory)
    {
        _configuracao = configuracao;
        _logger = logger;
        _httpClientFactory = httpClientFactory;
    }

    public async Task<bool> EnviarCodigoVerificacaoAsync(string nome, string email, string codigo)
    {
        var chaveBrevo = _configuracao["Email:BrevoApiKey"]?.Trim();
        if (!string.IsNullOrWhiteSpace(chaveBrevo))
            return await EnviarPelaBrevoAsync(nome, email, codigo, chaveBrevo);

        return await EnviarPorSmtpAsync(nome, email, codigo);
    }

    private async Task<bool> EnviarPelaBrevoAsync(string nome, string email, string codigo, string chaveApi)
    {
        var remetente = _configuracao["Email:Remetente"]?.Trim();
        var nomeRemetente = _configuracao["Email:NomeRemetente"] ?? "OnKash Finance";
        if (string.IsNullOrWhiteSpace(remetente))
        {
            _logger.LogError("Remetente não configurado para envio de verificação pela Brevo.");
            return false;
        }

        try
        {
            var cliente = _httpClientFactory.CreateClient();
            cliente.Timeout = TimeSpan.FromSeconds(10);
            cliente.DefaultRequestHeaders.Add("api-key", chaveApi);

            var resposta = await cliente.PostAsJsonAsync(
                "https://api.brevo.com/v3/smtp/email",
                new
                {
                    sender = new { name = nomeRemetente, email = remetente },
                    to = new[] { new { email, name = nome } },
                    subject = $"{codigo} é seu código de verificação OnKash",
                    htmlContent = CriarHtml(nome, codigo)
                });

            if (resposta.IsSuccessStatusCode)
                return true;

            var detalhe = await resposta.Content.ReadAsStringAsync();
            _logger.LogError(
                "Falha ao enviar verificação pela Brevo. Status={Status}; Resposta={Resposta}",
                (int)resposta.StatusCode,
                detalhe.Length > 500 ? detalhe[..500] : detalhe);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Falha na conexão HTTPS com a Brevo ao enviar verificação.");
            return false;
        }
    }

    private async Task<bool> EnviarPorSmtpAsync(string nome, string email, string codigo)
    {
        var host = _configuracao["Email:SmtpHost"]?.Trim();
        var usuario = _configuracao["Email:Usuario"]?.Trim();
        var senha = _configuracao["Email:Senha"]?.Replace(" ", "").Trim();
        var remetente = (_configuracao["Email:Remetente"] ?? usuario)?.Trim();
        if (string.IsNullOrWhiteSpace(host) || string.IsNullOrWhiteSpace(usuario) || string.IsNullOrWhiteSpace(senha) || string.IsNullOrWhiteSpace(remetente))
        { _logger.LogError("Configuração SMTP incompleta para verificação de e-mail."); return false; }
        try
        {
            using var cliente = new SmtpClient(host, _configuracao.GetValue("Email:Porta", 587))
            {
                EnableSsl = _configuracao.GetValue("Email:UsarSsl", true), UseDefaultCredentials = false,
                Credentials = new NetworkCredential(usuario, senha),
                DeliveryMethod = SmtpDeliveryMethod.Network,
                Timeout = 10_000
            };
            using var mensagem = new MailMessage
            {
                From = new MailAddress(remetente, _configuracao["Email:NomeRemetente"] ?? "OnKash Finance"),
                Subject = $"{codigo} é seu código de verificação OnKash", IsBodyHtml = true,
                Body = CriarHtml(nome, codigo)
            };
            mensagem.To.Add(email);
            await cliente.SendMailAsync(mensagem).WaitAsync(TimeSpan.FromSeconds(10));
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Falha SMTP ao enviar verificação. Host={Host}, Porta={Porta}, UsuarioConfigurado={UsuarioConfigurado}, RemetenteConfigurado={RemetenteConfigurado}.",
                host, _configuracao.GetValue("Email:Porta", 587), !string.IsNullOrWhiteSpace(usuario), !string.IsNullOrWhiteSpace(remetente));
            return false;
        }
    }

    private static string CriarHtml(string nome, string codigo)
    {
        var nomeSeguro = HtmlEncoder.Default.Encode(nome);
        return $"""<div style="font-family:Arial,sans-serif;max-width:560px;margin:auto;padding:32px;color:#15323a"><h1 style="color:#116a71">OnKash Finance</h1><p>Olá, {nomeSeguro}.</p><p>Use o código abaixo para confirmar seu e-mail:</p><div style="font-size:34px;font-weight:800;letter-spacing:10px;background:#eef8f7;padding:20px;text-align:center;border-radius:12px">{codigo}</div><p style="color:#667b80">O código é válido por 10 minutos. Se você não criou esta conta, ignore esta mensagem.</p></div>""";
    }
}
