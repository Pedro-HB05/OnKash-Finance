using Microsoft.AspNetCore.Identity;
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

    public AutenticacaoService(
        OnKashDbContext db,
        IPasswordHasher<Usuario> passwordHasher,
        JwtService jwtService)
    {
        _db = db;
        _passwordHasher = passwordHasher;
        _jwtService = jwtService;
    }

    public async Task<Guid> CadastrarAsync(CadastroRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Nome))
            throw new InvalidOperationException("O nome é obrigatório.");

        if (string.IsNullOrWhiteSpace(request.Email))
            throw new InvalidOperationException("O e-mail é obrigatório.");

        if (string.IsNullOrWhiteSpace(request.Senha))
            throw new InvalidOperationException("A senha é obrigatória.");

        if (request.Senha.Length < 6)
            throw new InvalidOperationException(
                "A senha deve possuir pelo menos 6 caracteres.");

        var email = request.Email
            .Trim()
            .ToLowerInvariant();

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
            var usuario = new Usuario
            {
                Nome = request.Nome.Trim(),
                Email = email,
                TipoConta = request.TipoConta,
                Ativo = true
            };

            usuario.SenhaHash =
                _passwordHasher.HashPassword(
                    usuario,
                    request.Senha
                );

            _db.Usuarios.Add(usuario);

            if (request.TipoConta ==
                TipoContaUsuario.EMPRESARIAL)
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

            return usuario.Id;
        }
        catch
        {
            await transacao.RollbackAsync();
            throw;
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
                x => x.Email.ToLower() == email
            );

        if (usuario is null || !usuario.Ativo)
        {
            throw new UnauthorizedAccessException(
                "E-mail ou senha inválidos.");
        }

        var resultado = _passwordHasher.VerifyHashedPassword(
            usuario,
            usuario.SenhaHash,
            request.Senha
        );

        if (resultado == PasswordVerificationResult.Failed)
        {
            throw new UnauthorizedAccessException(
                "E-mail ou senha inválidos.");
        }

        Guid? empresaId = null;
        PerfilEmpresa? perfil = null;

        if (usuario.TipoConta ==
            TipoContaUsuario.EMPRESARIAL)
        {
            var vinculo = await _db.EmpresaUsuarios
                .AsNoTracking()
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
        }

        var token = _jwtService.GerarToken(
            usuario,
            empresaId,
            perfil
        );

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