using Microsoft.EntityFrameworkCore;
using OnKashFinance.API.Autenticacao;
using OnKashFinance.API.Dados;
using OnKashFinance.API.DTOs;
using OnKashFinance.API.Modelos;

namespace OnKashFinance.API.Servicos;

public class PessoalService
{
    private readonly OnKashDbContext _db;
    private readonly UsuarioAtualService _usuarioAtual;

    public PessoalService(
        OnKashDbContext db,
        UsuarioAtualService usuarioAtual)
    {
        _db = db;
        _usuarioAtual = usuarioAtual;
    }

    private Guid ObterUsuario()
    {
        if (!_usuarioAtual.EhPessoal())
        {
            throw new UnauthorizedAccessException(
                "Esta função é exclusiva do financeiro pessoal.");
        }

        return _usuarioAtual.ObterUsuarioId();
    }

    // =========================================================
    // CONTAS
    // =========================================================

    public async Task<List<ContaPessoalResposta>>
        ListarContasAsync()
    {
        var usuarioId = ObterUsuario();

        var contas = await _db.ContasPessoais
            .AsNoTracking()
            .Where(x => x.UsuarioId == usuarioId)
            .OrderBy(x => x.Nome)
            .ToListAsync();

        var resposta = new List<ContaPessoalResposta>();

        foreach (var conta in contas)
        {
            var movimentos = await _db.LancamentosPessoais
                .Where(x =>
                    x.UsuarioId == usuarioId &&
                    x.ContaId == conta.Id &&
                    !x.Cancelado)
                .SumAsync(x =>
                    x.Tipo == TipoLancamentoPessoal.ENTRADA
                        ? x.Valor
                        : -x.Valor);

            resposta.Add(new ContaPessoalResposta
            {
                Id = conta.Id,
                Nome = conta.Nome,
                Tipo = conta.Tipo,
                SaldoInicial = conta.SaldoInicial,
                SaldoAtual =
                    conta.SaldoInicial + movimentos,
                Ativo = conta.Ativo
            });
        }

        return resposta;
    }

    public async Task<ContaPessoal>
        CriarContaAsync(
            CriarContaPessoalRequest request)
    {
        var usuarioId = ObterUsuario();

        if (string.IsNullOrWhiteSpace(request.Nome))
            throw new InvalidOperationException(
                "O nome da conta é obrigatório.");

        if (string.IsNullOrWhiteSpace(request.Tipo))
            throw new InvalidOperationException(
                "O tipo da conta é obrigatório.");

        var existe = await _db.ContasPessoais
            .AnyAsync(x =>
                x.UsuarioId == usuarioId &&
                x.Nome.ToLower() ==
                request.Nome.Trim().ToLower());

        if (existe)
        {
            throw new InvalidOperationException(
                "Já existe uma conta com este nome.");
        }

        var conta = new ContaPessoal
        {
            UsuarioId = usuarioId,
            Nome = request.Nome.Trim(),
            Tipo = request.Tipo.Trim(),
            SaldoInicial = request.SaldoInicial,
            Ativo = true
        };

        _db.ContasPessoais.Add(conta);

        await _db.SaveChangesAsync();

        return conta;
    }

    public async Task AtualizarContaAsync(
        Guid id,
        AtualizarContaPessoalRequest request)
    {
        var usuarioId = ObterUsuario();

        var conta = await _db.ContasPessoais
            .FirstOrDefaultAsync(x =>
                x.Id == id &&
                x.UsuarioId == usuarioId);

        if (conta is null)
            throw new KeyNotFoundException(
                "Conta não encontrada.");

        conta.Nome = request.Nome.Trim();
        conta.Tipo = request.Tipo.Trim();
        conta.Ativo = request.Ativo;

        await _db.SaveChangesAsync();
    }

    // =========================================================
    // CATEGORIAS
    // =========================================================

    public async Task<List<CategoriaPessoalResposta>>
        ListarCategoriasAsync()
    {
        var usuarioId = ObterUsuario();

        return await _db.CategoriasPessoais
            .AsNoTracking()
            .Where(x =>
                x.UsuarioId == null ||
                x.UsuarioId == usuarioId)
            .OrderBy(x => x.Tipo)
            .ThenBy(x => x.Nome)
            .Select(x =>
                new CategoriaPessoalResposta
                {
                    Id = x.Id,
                    Nome = x.Nome,
                    Tipo = x.Tipo,
                    Padrao = x.Padrao,
                    Ativo = x.Ativo
                })
            .ToListAsync();
    }

    public async Task<CategoriaPessoalResposta>
    CriarCategoriaAsync(
        CriarCategoriaPessoalRequest request)
    {
        var usuarioId = ObterUsuario();

        if (string.IsNullOrWhiteSpace(request.Nome))
        {
            throw new InvalidOperationException(
                "O nome da categoria é obrigatório.");
        }

        var categoria = new CategoriaPessoal
        {
            UsuarioId = usuarioId,
            Nome = request.Nome.Trim(),
            Tipo = request.Tipo,
            Padrao = false,
            Ativo = true
        };

        _db.CategoriasPessoais.Add(categoria);

        await _db.SaveChangesAsync();

        return new CategoriaPessoalResposta
        {
            Id = categoria.Id,
            Nome = categoria.Nome,
            Tipo = categoria.Tipo,
            Padrao = categoria.Padrao,
            Ativo = categoria.Ativo
        };
    }

    public async Task AtualizarCategoriaAsync(
        Guid id,
        AtualizarCategoriaPessoalRequest request)
    {
        var usuarioId = ObterUsuario();

        var categoria = await _db.CategoriasPessoais
            .FirstOrDefaultAsync(x =>
                x.Id == id &&
                x.UsuarioId == usuarioId &&
                !x.Padrao);

        if (categoria is null)
        {
            throw new KeyNotFoundException(
                "Categoria personalizada não encontrada.");
        }

        categoria.Nome = request.Nome.Trim();
        categoria.Tipo = request.Tipo;
        categoria.Ativo = request.Ativo;

        await _db.SaveChangesAsync();
    }

    // =========================================================
    // LANÇAMENTOS
    // =========================================================

    public async Task<List<LancamentoPessoalResposta>>
        ListarLancamentosAsync(
            DateOnly? inicio = null,
            DateOnly? fim = null)
    {
        var usuarioId = ObterUsuario();

        var query = _db.LancamentosPessoais
            .AsNoTracking()
            .Where(x => x.UsuarioId == usuarioId);

        if (inicio.HasValue)
            query = query.Where(
                x => x.Data >= inicio.Value);

        if (fim.HasValue)
            query = query.Where(
                x => x.Data <= fim.Value);

        return await query
            .OrderByDescending(x => x.Data)
            .ThenByDescending(x => x.CriadoEm)
            .Select(x =>
                new LancamentoPessoalResposta
                {
                    Id = x.Id,
                    ContaId = x.ContaId,
                    Conta = x.Conta.Nome,
                    CategoriaId = x.CategoriaId,
                    Categoria =
                        x.Categoria != null
                            ? x.Categoria.Nome
                            : null,
                    Tipo = x.Tipo,
                    Descricao = x.Descricao,
                    Valor = x.Valor,
                    Data = x.Data,
                    Observacao = x.Observacao,
                    Cancelado = x.Cancelado
                })
            .ToListAsync();
    }

    public async Task<LancamentoPessoalResposta>
    CriarLancamentoAsync(
        CriarLancamentoPessoalRequest request)
    {
        var usuarioId = ObterUsuario();

        if (request.Valor <= 0)
            throw new InvalidOperationException(
                "O valor deve ser maior que zero.");

        if (string.IsNullOrWhiteSpace(request.Descricao))
            throw new InvalidOperationException(
                "A descrição é obrigatória.");

        var conta = await _db.ContasPessoais
            .AsNoTracking()
            .FirstOrDefaultAsync(x =>
                x.Id == request.ContaId &&
                x.UsuarioId == usuarioId &&
                x.Ativo);

        if (conta is null)
            throw new InvalidOperationException(
                "Conta inválida.");

        if (!request.CategoriaId.HasValue)
        {
            throw new InvalidOperationException(
                "A categoria é obrigatória.");
        }

        var categoria = await _db.CategoriasPessoais
            .AsNoTracking()
            .FirstOrDefaultAsync(x =>
                x.Id == request.CategoriaId.Value &&
                x.Ativo &&
                (x.UsuarioId == null ||
                 x.UsuarioId == usuarioId));

        if (categoria is null)
            throw new InvalidOperationException(
                "Categoria inválida.");

        if (categoria.Tipo.ToString() != request.Tipo.ToString())
        {
            throw new InvalidOperationException(
                "O tipo da categoria não corresponde ao lançamento.");
        }

        var lancamento = new LancamentoPessoal
        {
            UsuarioId = usuarioId,
            ContaId = request.ContaId,
            CategoriaId = request.CategoriaId,
            Tipo = request.Tipo,
            Descricao = request.Descricao.Trim(),
            Valor = request.Valor,
            Data = request.Data,
            Observacao = request.Observacao,
            Cancelado = false
        };

        _db.LancamentosPessoais.Add(lancamento);

        await _db.SaveChangesAsync();

        return new LancamentoPessoalResposta
        {
            Id = lancamento.Id,
            ContaId = lancamento.ContaId,
            Conta = conta.Nome,
            CategoriaId = lancamento.CategoriaId,
            Categoria = categoria.Nome,
            Tipo = lancamento.Tipo,
            Descricao = lancamento.Descricao,
            Valor = lancamento.Valor,
            Data = lancamento.Data,
            Observacao = lancamento.Observacao,
            Cancelado = lancamento.Cancelado
        };
    }

    public async Task AtualizarLancamentoAsync(
        Guid id,
        AtualizarLancamentoPessoalRequest request)
    {
        var usuarioId = ObterUsuario();

        var lancamento = await _db.LancamentosPessoais
            .FirstOrDefaultAsync(x =>
                x.Id == id &&
                x.UsuarioId == usuarioId &&
                !x.Cancelado &&
                x.FaturaId == null);

        if (lancamento is null)
            throw new KeyNotFoundException(
                "Lançamento não encontrado.");

        var contaExiste = await _db.ContasPessoais
            .AnyAsync(x =>
                x.Id == request.ContaId &&
                x.UsuarioId == usuarioId);

        if (!contaExiste)
            throw new InvalidOperationException(
                "Conta inválida.");

        lancamento.ContaId = request.ContaId;
        lancamento.CategoriaId = request.CategoriaId;
        lancamento.Tipo = request.Tipo;
        lancamento.Descricao =
            request.Descricao.Trim();
        lancamento.Valor = request.Valor;
        lancamento.Data = request.Data;
        lancamento.Observacao =
            request.Observacao;

        await _db.SaveChangesAsync();
    }

    public async Task CancelarLancamentoAsync(
    Guid id)
    {
        var usuarioId = ObterUsuario();

        var lancamento = await _db.LancamentosPessoais
            .FirstOrDefaultAsync(x =>
                x.Id == id &&
                x.UsuarioId == usuarioId &&
                !x.Cancelado);

        if (lancamento is null)
            throw new KeyNotFoundException(
                "Lançamento não encontrado.");

        lancamento.Cancelado = true;
        lancamento.CanceladoEm =
            DateTimeOffset.UtcNow;

        await _db.SaveChangesAsync();
    }
}