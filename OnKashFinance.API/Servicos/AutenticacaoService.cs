using System.Net.Mail;
using Microsoft.AspNetCore.Identity;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using OnKashFinance.API.Autenticacao;
using OnKashFinance.API.Dados;
using OnKashFinance.API.DTOs;
using OnKashFinance.API.Modelos;

namespace OnKashFinance.API.Servicos;

public class AutenticacaoService
{
    private readonly OnKashDbContext _db;
    private readonly IPasswordHasher<Usuario> _passwordHasher;
    private readonly JwtService _jwtService;
    private readonly EmailService _emailService;
    private readonly string _chaveVerificacao;
    private readonly IHttpContextAccessor _http;

    public AutenticacaoService(
        OnKashDbContext db,
        IPasswordHasher<Usuario> passwordHasher,
        JwtService jwtService,
        EmailService emailService,
        IConfiguration configuracao,
        IHttpContextAccessor http)
    {
        _db = db;
        _passwordHasher = passwordHasher;
        _jwtService = jwtService;
        _emailService = emailService;
        _http = http;
        _chaveVerificacao = configuracao["EmailVerification:HashKey"]
            ?? configuracao["Jwt:Key"]
            ?? throw new InvalidOperationException("Chave de verificação não configurada.");
    }

    public async Task<CadastroResposta> CadastrarAsync(CadastroRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Nome))
            throw new InvalidOperationException("O nome é obrigatório.");

        if (string.IsNullOrWhiteSpace(request.Email))
            throw new InvalidOperationException("O e-mail é obrigatório.");

        if (string.IsNullOrWhiteSpace(request.Senha))
            throw new InvalidOperationException("A senha é obrigatória.");

        if (request.Senha.Length < 8)
            throw new InvalidOperationException(
                "A senha deve possuir pelo menos 8 caracteres.");

        if (!request.AceitouTermos)
            throw new InvalidOperationException("É necessário ler e aceitar os Termos de Uso e a Política de Privacidade.");

        var email = request.Email
            .Trim()
            .ToLowerInvariant();

        ValidarEmail(email);

        var emailExiste = await _db.Usuarios
            .AnyAsync(x => x.Email.ToLower() == email);

        if (emailExiste)
            throw new InvalidOperationException(
                "Já existe um usuário com este e-mail.");

        if (request.TipoConta == TipoContaUsuario.EMPRESARIAL &&
            string.IsNullOrWhiteSpace(request.NomeEmpresa))
        {
            throw new InvalidOperationException(
                "O nome da empresa é obrigatório.");
        }

        await using var transacao =
            await _db.Database.BeginTransactionAsync();

        try
        {
            var codigo = RandomNumberGenerator.GetInt32(0, 1_000_000).ToString("D6");
            var usuario = new Usuario
            {
                Id = Guid.NewGuid(),
                Nome = request.Nome.Trim(),
                Email = email,
                TipoConta = request.TipoConta,
                Ativo = true,
                EmailVerificado = false,
                CodigoVerificacaoEmail = HashCodigo(codigo),
                CodigoVerificacaoExpiraEm = DateTimeOffset.UtcNow.AddMinutes(10)
            };

            usuario.SenhaHash =
                _passwordHasher.HashPassword(
                    usuario,
                    request.Senha);

            _db.Usuarios.Add(usuario);
            _db.AceitesLegais.Add(PrivacidadeService.CriarAceite(usuario.Id, _http.HttpContext));
            _db.AssinaturasUsuario.Add(new AssinaturaUsuario
            {
                Usuario = usuario,
                Plano = "GRATUITO",
                Status = "ATIVA"
            });

            if (request.TipoConta == TipoContaUsuario.EMPRESARIAL)
            {
                var empresa = new Empresa
                {
                    Nome = request.NomeEmpresa!.Trim(),
                    Ativo = true
                };

                var empresaUsuario = new EmpresaUsuario
                {
                    Empresa = empresa,
                    Usuario = usuario,
                    Perfil = PerfilEmpresa.ADMINISTRADOR,
                    Ativo = true
                };

                var permissoes = new PermissaoEmpresa
                {
                    EmpresaUsuario = empresaUsuario,
                    Dashboard = true,
                    Lancamentos = true,
                    Contas = true,
                    Clientes = true,
                    Fornecedores = true,
                    ContasPagar = true,
                    ContasReceber = true,
                    Categorias = true,
                    Relatorios = true,
                    Usuarios = true
                };

                _db.Empresas.Add(empresa);
                _db.EmpresaUsuarios.Add(empresaUsuario);
                _db.PermissoesEmpresa.Add(permissoes);
            }

            await _db.SaveChangesAsync();
            await transacao.CommitAsync();

            var enviado = await _emailService.EnviarCodigoVerificacaoAsync(
                usuario.Nome, usuario.Email, codigo);

            if (!enviado)
            {
                usuario.CodigoVerificacaoExpiraEm = DateTimeOffset.UtcNow;
                await _db.SaveChangesAsync();
            }

            return new CadastroResposta
            {
                UsuarioId = usuario.Id,
                Email = usuario.Email,
                EmailEnviado = enviado,
                Mensagem = enviado
                    ? "Enviamos um código de verificação para seu e-mail."
                    : "Conta criada. Solicite um novo código para validar o e-mail."
            };
        }
        catch
        {
            await transacao.RollbackAsync();
            throw;
        }
    }


    public async Task VerificarEmailAsync(VerificarEmailRequest request)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var codigo = new string(request.Codigo.Where(char.IsDigit).ToArray());
        if (codigo.Length != 6)
            throw new InvalidOperationException("Informe o código de seis dígitos.");

        var usuario = await _db.Usuarios
            .FirstOrDefaultAsync(x => x.Email.ToLower() == email);

        if (usuario is null || usuario.EmailVerificado ||
            string.IsNullOrWhiteSpace(usuario.CodigoVerificacaoEmail))
            throw new InvalidOperationException("Código inválido ou conta já verificada.");

        if (!usuario.CodigoVerificacaoExpiraEm.HasValue ||
            usuario.CodigoVerificacaoExpiraEm <= DateTimeOffset.UtcNow)
            throw new InvalidOperationException("O código expirou. Solicite um novo código.");

        var esperado = Encoding.UTF8.GetBytes(usuario.CodigoVerificacaoEmail);
        var informado = Encoding.UTF8.GetBytes(HashCodigo(codigo));
        if (!CryptographicOperations.FixedTimeEquals(esperado, informado))
            throw new InvalidOperationException("Código de verificação inválido.");

        usuario.EmailVerificado = true;
        usuario.CodigoVerificacaoEmail = null;
        usuario.CodigoVerificacaoExpiraEm = null;
        await _db.SaveChangesAsync();
    }

    public async Task ReenviarCodigoAsync(ReenviarCodigoEmailRequest request)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var usuario = await _db.Usuarios
            .FirstOrDefaultAsync(x => x.Email.ToLower() == email);

        // Resposta neutra evita revelar quais e-mails possuem conta.
        if (usuario is null || usuario.EmailVerificado) return;

        if (usuario.CodigoVerificacaoExpiraEm > DateTimeOffset.UtcNow.AddMinutes(9))
            throw new InvalidOperationException("Aguarde um minuto antes de solicitar outro código.");

        var codigo = RandomNumberGenerator.GetInt32(0, 1_000_000).ToString("D6");
        usuario.CodigoVerificacaoEmail = HashCodigo(codigo);
        usuario.CodigoVerificacaoExpiraEm = DateTimeOffset.UtcNow.AddMinutes(10);
        await _db.SaveChangesAsync();

        if (!await _emailService.EnviarCodigoVerificacaoAsync(usuario.Nome, usuario.Email, codigo))
        {
            usuario.CodigoVerificacaoExpiraEm = DateTimeOffset.UtcNow;
            await _db.SaveChangesAsync();
            throw new InvalidOperationException(
                "Não foi possível enviar o e-mail agora. Tente novamente em instantes.");
        }
    }

    private string HashCodigo(string codigo)
    {
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_chaveVerificacao));
        return Convert.ToHexString(hmac.ComputeHash(Encoding.UTF8.GetBytes(codigo)))[..6];
    }

    private static void ValidarEmail(string email)
    {
        try
        {
            var endereco = new MailAddress(email);

            var dominiosPermitidos = new[]
            {
                "gmail.com",
                "hotmail.com",
                "outlook.com",
                "yahoo.com",
                "yahoo.com.br"
            };

            var dominio = endereco.Host.ToLowerInvariant();

            if (!dominiosPermitidos.Contains(dominio))
            {
                throw new InvalidOperationException(
                    "Utilize um e-mail Gmail, Hotmail, Outlook ou Yahoo.");
            }
        }
        catch (FormatException)
        {
            throw new InvalidOperationException(
                "E-mail inválido.");
        }
    }


    public async Task<LoginResposta> LoginAsync(
        LoginRequest request)
    {
        var email = request.Email
            .Trim()
            .ToLowerInvariant();

        var usuario = await _db.Usuarios
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.Email.ToLower() == email);

        if (usuario is null || !usuario.Ativo)
        {
            throw new UnauthorizedAccessException(
                "E-mail ou senha inválidos.");
        }

        var resultado =
            _passwordHasher.VerifyHashedPassword(
                usuario,
                usuario.SenhaHash,
                request.Senha);

        if (resultado == PasswordVerificationResult.Failed)
        {
            throw new UnauthorizedAccessException(
                "E-mail ou senha inválidos.");
        }

        // Contas antigas não possuíam código pendente e continuam acessando normalmente.
        if (!usuario.EmailVerificado &&
            !string.IsNullOrWhiteSpace(usuario.CodigoVerificacaoEmail))
        {
            throw new UnauthorizedAccessException(
                "E-mail ainda não verificado. Confirme o código enviado antes de entrar.");
        }

        Guid? empresaId = null;
        PerfilEmpresa? perfil = null;
        PermissaoEmpresa? permissoes = null;

        if (usuario.TipoConta == TipoContaUsuario.EMPRESARIAL)
        {
            var vinculo = await _db.EmpresaUsuarios
                .AsNoTracking()
                .Include(x => x.Permissoes)
                .Where(x =>
                    x.UsuarioId == usuario.Id &&
                    x.Ativo &&
                    x.Empresa.Ativo)
                .OrderBy(x => x.CriadoEm)
                .FirstOrDefaultAsync();

            if (vinculo is null)
            {
                throw new UnauthorizedAccessException(
                    "O usuário não possui empresa ativa.");
            }

            empresaId = vinculo.EmpresaId;
            perfil = vinculo.Perfil;
            permissoes = vinculo.Permissoes;
        }

        var token = _jwtService.GerarToken(
            usuario,
            empresaId,
            perfil,
            permissoes);

        return new LoginResposta
        {
            Token = token,
            UsuarioId = usuario.Id,
            Nome = usuario.Nome,
            Email = usuario.Email,
            TipoConta = usuario.TipoConta,
            EmpresaId = empresaId,
            Perfil = perfil
        };
    }
}
