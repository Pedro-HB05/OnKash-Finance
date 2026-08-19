using Microsoft.EntityFrameworkCore;
using OnKashFinance.API.Autenticacao;
using OnKashFinance.API.Dados;
using OnKashFinance.API.DTOs;
using OnKashFinance.API.Modelos;

namespace OnKashFinance.API.Servicos;

public class EmpresaUsuariosService
{
    private readonly OnKashDbContext _db;
    private readonly UsuarioAtualService _usuarioAtual;

    public EmpresaUsuariosService(
        OnKashDbContext db,
        UsuarioAtualService usuarioAtual)
    {
        _db = db;
        _usuarioAtual = usuarioAtual;
    }

    private async Task<Guid> ObterEmpresaAdministradorAsync()
    {
        if (!_usuarioAtual.EhEmpresarial())
        {
            throw new UnauthorizedAccessException(
                "Função exclusiva do financeiro empresarial.");
        }

        var usuarioId = _usuarioAtual.ObterUsuarioId();
        var empresaId = _usuarioAtual.ExigirEmpresaId();

        var administrador = await _db.EmpresaUsuarios
            .AnyAsync(x =>
                x.UsuarioId == usuarioId &&
                x.EmpresaId == empresaId &&
                x.Ativo &&
                x.Perfil == PerfilEmpresa.ADMINISTRADOR);

        if (!administrador)
        {
            throw new UnauthorizedAccessException(
                "Somente administradores podem gerenciar usuários.");
        }

        return empresaId;
    }

    public async Task<List<UsuarioEmpresaResposta>> ListarAsync()
    {
        var empresaId = await ObterEmpresaAdministradorAsync();

        return await _db.EmpresaUsuarios
            .AsNoTracking()
            .Where(x => x.EmpresaId == empresaId)
            .OrderBy(x => x.Usuario.Nome)
            .Select(x => new UsuarioEmpresaResposta
            {
                EmpresaUsuarioId = x.Id,
                UsuarioId = x.UsuarioId,
                Nome = x.Usuario.Nome,
                Email = x.Usuario.Email,
                Perfil = x.Perfil,
                Ativo = x.Ativo,

                Permissoes = x.Permissoes == null
                    ? null
                    : new AtualizarPermissoesEmpresaRequest
                    {
                        Dashboard = x.Permissoes.Dashboard,
                        Lancamentos = x.Permissoes.Lancamentos,
                        Contas = x.Permissoes.Contas,
                        Clientes = x.Permissoes.Clientes,
                        Fornecedores = x.Permissoes.Fornecedores,
                        ContasPagar = x.Permissoes.ContasPagar,
                        ContasReceber = x.Permissoes.ContasReceber,
                        Categorias = x.Permissoes.Categorias,
                        Relatorios = x.Permissoes.Relatorios,
                        Usuarios = x.Permissoes.Usuarios
                    }
            })
            .ToListAsync();
    }

    public async Task<EmpresaUsuario> AdicionarAsync(
        AdicionarUsuarioEmpresaRequest request)
    {
        var empresaId = await ObterEmpresaAdministradorAsync();

        var usuario = await _db.Usuarios
            .FirstOrDefaultAsync(x =>
                x.Id == request.UsuarioId &&
                x.Ativo);

        if (usuario is null)
        {
            throw new KeyNotFoundException(
                "Usuário não encontrado.");
        }

        if (usuario.TipoConta != TipoContaUsuario.EMPRESARIAL)
        {
            throw new InvalidOperationException(
                "Somente usuários empresariais podem ser adicionados.");
        }

        var existe = await _db.EmpresaUsuarios
            .AnyAsync(x =>
                x.EmpresaId == empresaId &&
                x.UsuarioId == usuario.Id);

        if (existe)
        {
            throw new InvalidOperationException(
                "Este usuário já pertence à empresa.");
        }

        var vinculo = new EmpresaUsuario
        {
            EmpresaId = empresaId,
            UsuarioId = usuario.Id,
            Perfil = request.Perfil,
            Ativo = true
        };

        _db.EmpresaUsuarios.Add(vinculo);

        var administrador =
            request.Perfil == PerfilEmpresa.ADMINISTRADOR;

        var permissoes = new PermissaoEmpresa
        {
            EmpresaUsuario = vinculo,

            Dashboard = administrador,
            Lancamentos = administrador,
            Contas = administrador,
            Clientes = administrador,
            Fornecedores = administrador,
            ContasPagar = administrador,
            ContasReceber = administrador,
            Categorias = administrador,
            Relatorios = administrador,
            Usuarios = administrador
        };

        _db.PermissoesEmpresa.Add(permissoes);

        await _db.SaveChangesAsync();

        return vinculo;
    }

    public async Task AtualizarPerfilAsync(
        Guid empresaUsuarioId,
        AtualizarPerfilEmpresaRequest request)
    {
        var empresaId = await ObterEmpresaAdministradorAsync();

        var vinculo = await _db.EmpresaUsuarios
            .Include(x => x.Permissoes)
            .FirstOrDefaultAsync(x =>
                x.Id == empresaUsuarioId &&
                x.EmpresaId == empresaId);

        if (vinculo is null)
        {
            throw new KeyNotFoundException(
                "Usuário da empresa não encontrado.");
        }

        vinculo.Perfil = request.Perfil;
        vinculo.Ativo = request.Ativo;

        if (request.Perfil == PerfilEmpresa.ADMINISTRADOR)
        {
            vinculo.Permissoes ??= new PermissaoEmpresa
            {
                EmpresaUsuarioId = vinculo.Id
            };

            vinculo.Permissoes.Dashboard = true;
            vinculo.Permissoes.Lancamentos = true;
            vinculo.Permissoes.Contas = true;
            vinculo.Permissoes.Clientes = true;
            vinculo.Permissoes.Fornecedores = true;
            vinculo.Permissoes.ContasPagar = true;
            vinculo.Permissoes.ContasReceber = true;
            vinculo.Permissoes.Categorias = true;
            vinculo.Permissoes.Relatorios = true;
            vinculo.Permissoes.Usuarios = true;
        }

        await _db.SaveChangesAsync();
    }

    public async Task AtualizarPermissoesAsync(
        Guid empresaUsuarioId,
        AtualizarPermissoesEmpresaRequest request)
    {
        var empresaId = await ObterEmpresaAdministradorAsync();

        var vinculo = await _db.EmpresaUsuarios
            .Include(x => x.Permissoes)
            .FirstOrDefaultAsync(x =>
                x.Id == empresaUsuarioId &&
                x.EmpresaId == empresaId);

        if (vinculo is null)
        {
            throw new KeyNotFoundException(
                "Usuário da empresa não encontrado.");
        }

        if (vinculo.Perfil == PerfilEmpresa.ADMINISTRADOR)
        {
            throw new InvalidOperationException(
                "Administradores possuem acesso completo.");
        }

        var permissoes = vinculo.Permissoes;

        if (permissoes is null)
        {
            permissoes = new PermissaoEmpresa
            {
                EmpresaUsuarioId = vinculo.Id
            };

            _db.PermissoesEmpresa.Add(permissoes);
        }

        permissoes.Dashboard = request.Dashboard;
        permissoes.Lancamentos = request.Lancamentos;
        permissoes.Contas = request.Contas;
        permissoes.Clientes = request.Clientes;
        permissoes.Fornecedores = request.Fornecedores;
        permissoes.ContasPagar = request.ContasPagar;
        permissoes.ContasReceber = request.ContasReceber;
        permissoes.Categorias = request.Categorias;
        permissoes.Relatorios = request.Relatorios;
        permissoes.Usuarios = request.Usuarios;

        await _db.SaveChangesAsync();
    }
}