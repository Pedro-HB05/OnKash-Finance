using System.Net;
using System.Net.Mail;
using System.Text.Encodings.Web;

namespace OnKashFinance.API.Servicos;

public class EmailService
{
    private readonly IConfiguration _configuracao;
    private readonly ILogger<EmailService> _logger;
    public EmailService(IConfiguration configuracao, ILogger<EmailService> logger)
    { _configuracao = configuracao; _logger = logger; }

    public async Task<bool> EnviarCodigoVerificacaoAsync(string nome, string email, string codigo)
    {
        var host = _configuracao["Email:SmtpHost"];
        var usuario = _configuracao["Email:Usuario"];
        var senha = _configuracao["Email:Senha"];
        var remetente = _configuracao["Email:Remetente"] ?? usuario;
        if (string.IsNullOrWhiteSpace(host) || string.IsNullOrWhiteSpace(usuario) || string.IsNullOrWhiteSpace(senha) || string.IsNullOrWhiteSpace(remetente))
        { _logger.LogError("Configuração SMTP incompleta para verificação de e-mail."); return false; }
        try
        {
            using var cliente = new SmtpClient(host, _configuracao.GetValue("Email:Porta", 587))
            {
                EnableSsl = _configuracao.GetValue("Email:UsarSsl", true), UseDefaultCredentials = false,
                Credentials = new NetworkCredential(usuario, senha), DeliveryMethod = SmtpDeliveryMethod.Network
            };
            var nomeSeguro = HtmlEncoder.Default.Encode(nome);
            using var mensagem = new MailMessage
            {
                From = new MailAddress(remetente, _configuracao["Email:NomeRemetente"] ?? "OnKash Finance"),
                Subject = $"{codigo} é seu código de verificação OnKash", IsBodyHtml = true,
                Body = $"""<div style="font-family:Arial,sans-serif;max-width:560px;margin:auto;padding:32px;color:#15323a"><h1 style="color:#116a71">OnKash Finance</h1><p>Olá, {nomeSeguro}.</p><p>Use o código abaixo para confirmar seu e-mail:</p><div style="font-size:34px;font-weight:800;letter-spacing:10px;background:#eef8f7;padding:20px;text-align:center;border-radius:12px">{codigo}</div><p style="color:#667b80">O código é válido por 10 minutos. Se você não criou esta conta, ignore esta mensagem.</p></div>"""
            };
            mensagem.To.Add(email); await cliente.SendMailAsync(mensagem); return true;
        }
        catch (Exception ex) { _logger.LogError(ex, "Falha ao enviar código de verificação para {Email}.", email); return false; }
    }
}
