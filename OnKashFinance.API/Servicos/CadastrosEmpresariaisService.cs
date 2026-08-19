using Microsoft.EntityFrameworkCore;
using OnKashFinance.API.Autenticacao;
using OnKashFinance.API.Dados;
using OnKashFinance.API.DTOs;
using OnKashFinance.API.Modelos;

namespace OnKashFinance.API.Servicos;

public class CadastrosEmpresariaisService
{
    private readonly OnKashDbContext _db;
    private readonly UsuarioAtualService _usuarioAtual;

    public CadastrosEmpresariaisService(
        OnKashDbContext db,
        UsuarioAtualService usuarioAtual)
    {
        _db = db;
        _usuarioAtual = usuarioAtual;
    }

    private Guid ObterEmpresa()
    {
        if (!_usuarioAtual.EhEmpresarial())
            throw new UnauthorizedAccessException(
                "Função exclusiva do financeiro empresarial.");

        return _usuarioAtual.ExigirEmpresaId();
    }

    // =========================================================
    // CONTAS
    // =========================================================

    public async Task<List<ContaEmpresarialResposta>>
    ListarContasAsync()
    {
        var empresaId = ObterEmpresa();

        var contas = await _db.ContasEmpresariais
            .AsNoTracking()
            .Where(x => x.EmpresaId == empresaId)
            .OrderBy(x => x.Nome)
            .ToListAsync();

        var resposta = new List<ContaEmpresarialResposta>();

        foreach (var conta in contas)
        {
            var movimentosOrigem = await _db.LancamentosEmpresariais
                .Where(x =>
                    x.EmpresaId == empresaId &&
                    x.ContaId == conta.Id &&
                    !x.Cancelado)
                .SumAsync(x =>
                    x.Tipo == TipoLancamentoEmpresarial.RECEITA
                        ? x.Valor
                        : -x.Valor);

            var transferenciasEntrada = await _db.LancamentosEmpresariais
                .Where(x =>
                    x.EmpresaId == empresaId &&
                    x.Tipo == TipoLancamentoEmpresarial.TRANSFERENCIA &&
                    x.ContaDestinoId == conta.Id &&
                    !x.Cancelado)
                .SumAsync(x => x.Valor);

            resposta.Add(new ContaEmpresarialResposta
            {
                Id = conta.Id,
                Nome = conta.Nome,
                Tipo = conta.Tipo,
                SaldoInicial = conta.SaldoInicial,
                SaldoAtual =
                    conta.SaldoInicial +
                    movimentosOrigem +
                    transferenciasEntrada,
                Ativo = conta.Ativo
            });
        }

        return resposta;
    }

    public async Task<ContaEmpresarialResposta>
        CriarContaAsync(
            CriarContaEmpresarialRequest request)
    {
        var empresaId = ObterEmpresa();

        var conta = new ContaEmpresarial
        {
            EmpresaId = empresaId,
            Nome = request.Nome.Trim(),
            Tipo = request.Tipo.Trim(),
            SaldoInicial = request.SaldoInicial,
            Ativo = true
        };

        _db.ContasEmpresariais.Add(conta);

        await _db.SaveChangesAsync();

        return new ContaEmpresarialResposta
        {
            Id = conta.Id,
            Nome = conta.Nome,
            Tipo = conta.Tipo,
            SaldoInicial = conta.SaldoInicial,
            SaldoAtual = conta.SaldoInicial,
            Ativo = conta.Ativo
        };
    }

    // =========================================================
    // CATEGORIAS
    // =========================================================

    public async Task<List<CategoriaEmpresarialResposta>>
    ListarCategoriasAsync()
    {
        var empresaId = ObterEmpresa();

        return await _db.CategoriasEmpresariais
            .AsNoTracking()
            .Where(x =>
                x.EmpresaId == null ||
                x.EmpresaId == empresaId)
            .OrderBy(x => x.Tipo)
            .ThenBy(x => x.Nome)
            .Select(x =>
                new CategoriaEmpresarialResposta
                {
                    Id = x.Id,
                    Nome = x.Nome,
                    Tipo = x.Tipo,
                    Padrao = x.Padrao,
                    Ativo = x.Ativo
                })
            .ToListAsync();
    }

    public async Task<CategoriaEmpresarialResposta>
        CriarCategoriaAsync(
            CriarCategoriaEmpresarialRequest request)
    {
        var empresaId = ObterEmpresa();

        if (string.IsNullOrWhiteSpace(request.Nome))
            throw new InvalidOperationException(
                "O nome da categoria é obrigatório.");

        var categoria = new CategoriaEmpresarial
        {
            EmpresaId = empresaId,
            Nome = request.Nome.Trim(),
            Tipo = request.Tipo,
            Padrao = false,
            Ativo = true
        };

        _db.CategoriasEmpresariais.Add(categoria);

        await _db.SaveChangesAsync();

        return new CategoriaEmpresarialResposta
        {
            Id = categoria.Id,
            Nome = categoria.Nome,
            Tipo = categoria.Tipo,
            Padrao = categoria.Padrao,
            Ativo = categoria.Ativo
        };
    }

    // =========================================================
    // CLIENTES
    // =========================================================

    public async Task<List<ClienteResposta>>
    ListarClientesAsync()
    {
        var empresaId = ObterEmpresa();

        return await _db.Clientes
            .AsNoTracking()
            .Where(x => x.EmpresaId == empresaId)
            .OrderBy(x => x.NomeRazaoSocial)
            .Select(x =>
                new ClienteResposta
                {
                    Id = x.Id,
                    NomeRazaoSocial = x.NomeRazaoSocial,
                    CpfCnpj = x.CpfCnpj,
                    Telefone = x.Telefone,
                    Email = x.Email,
                    Observacao = x.Observacao,
                    Ativo = x.Ativo
                })
            .ToListAsync();
    }

    public async Task<ClienteResposta>
        CriarClienteAsync(
            CriarClienteRequest request)
    {
        var empresaId = ObterEmpresa();

        if (string.IsNullOrWhiteSpace(request.NomeRazaoSocial))
            throw new InvalidOperationException(
                "O nome ou razão social do cliente é obrigatório.");

        var cliente = new Cliente
        {
            EmpresaId = empresaId,
            NomeRazaoSocial = request.NomeRazaoSocial.Trim(),
            CpfCnpj = request.CpfCnpj,
            Telefone = request.Telefone,
            Email = request.Email,
            Observacao = request.Observacao,
            Ativo = true
        };

        _db.Clientes.Add(cliente);

        await _db.SaveChangesAsync();

        return new ClienteResposta
        {
            Id = cliente.Id,
            NomeRazaoSocial = cliente.NomeRazaoSocial,
            CpfCnpj = cliente.CpfCnpj,
            Telefone = cliente.Telefone,
            Email = cliente.Email,
            Observacao = cliente.Observacao,
            Ativo = cliente.Ativo
        };
    }

    public async Task AtualizarClienteAsync(
        Guid id,
        AtualizarClienteRequest request)
    {
        var empresaId = ObterEmpresa();

        var cliente = await _db.Clientes
            .FirstOrDefaultAsync(x =>
                x.Id == id &&
                x.EmpresaId == empresaId);

        if (cliente is null)
            throw new KeyNotFoundException(
                "Cliente não encontrado.");

        if (string.IsNullOrWhiteSpace(request.NomeRazaoSocial))
            throw new InvalidOperationException(
                "O nome ou razão social do cliente é obrigatório.");

        cliente.NomeRazaoSocial =
            request.NomeRazaoSocial.Trim();

        cliente.CpfCnpj = request.CpfCnpj;
        cliente.Telefone = request.Telefone;
        cliente.Email = request.Email;
        cliente.Observacao = request.Observacao;
        cliente.Ativo = request.Ativo;

        await _db.SaveChangesAsync();
    }

    // =========================================================
    // FORNECEDORES
    // =========================================================

    public async Task<List<FornecedorResposta>>
    ListarFornecedoresAsync()
    {
        var empresaId = ObterEmpresa();

        return await _db.Fornecedores
            .AsNoTracking()
            .Where(x => x.EmpresaId == empresaId)
            .OrderBy(x => x.NomeRazaoSocial)
            .Select(x =>
                new FornecedorResposta
                {
                    Id = x.Id,
                    NomeRazaoSocial = x.NomeRazaoSocial,
                    CpfCnpj = x.CpfCnpj,
                    Telefone = x.Telefone,
                    Email = x.Email,
                    Observacao = x.Observacao,
                    Ativo = x.Ativo
                })
            .ToListAsync();
    }

    public async Task<FornecedorResposta>
        CriarFornecedorAsync(
            CriarFornecedorRequest request)
    {
        var empresaId = ObterEmpresa();

        if (string.IsNullOrWhiteSpace(request.NomeRazaoSocial))
            throw new InvalidOperationException(
                "O nome ou razão social do fornecedor é obrigatório.");

        var fornecedor = new Fornecedor
        {
            EmpresaId = empresaId,
            NomeRazaoSocial = request.NomeRazaoSocial.Trim(),
            CpfCnpj = request.CpfCnpj,
            Telefone = request.Telefone,
            Email = request.Email,
            Observacao = request.Observacao,
            Ativo = true
        };

        _db.Fornecedores.Add(fornecedor);

        await _db.SaveChangesAsync();

        return new FornecedorResposta
        {
            Id = fornecedor.Id,
            NomeRazaoSocial = fornecedor.NomeRazaoSocial,
            CpfCnpj = fornecedor.CpfCnpj,
            Telefone = fornecedor.Telefone,
            Email = fornecedor.Email,
            Observacao = fornecedor.Observacao,
            Ativo = fornecedor.Ativo
        };
    }

    public async Task AtualizarFornecedorAsync(
        Guid id,
        AtualizarFornecedorRequest request)
    {
        var empresaId = ObterEmpresa();

        var fornecedor = await _db.Fornecedores
            .FirstOrDefaultAsync(x =>
                x.Id == id &&
                x.EmpresaId == empresaId);

        if (fornecedor is null)
            throw new KeyNotFoundException(
                "Fornecedor não encontrado.");

        if (string.IsNullOrWhiteSpace(request.NomeRazaoSocial))
            throw new InvalidOperationException(
                "O nome ou razão social do fornecedor é obrigatório.");

        fornecedor.NomeRazaoSocial =
            request.NomeRazaoSocial.Trim();

        fornecedor.CpfCnpj = request.CpfCnpj;
        fornecedor.Telefone = request.Telefone;
        fornecedor.Email = request.Email;
        fornecedor.Observacao = request.Observacao;
        fornecedor.Ativo = request.Ativo;

        await _db.SaveChangesAsync();
    }
}