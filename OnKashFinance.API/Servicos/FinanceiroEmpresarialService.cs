using Microsoft.EntityFrameworkCore;
using OnKashFinance.API.Autenticacao;
using OnKashFinance.API.Dados;
using OnKashFinance.API.DTOs;
using OnKashFinance.API.Modelos;
using System.Linq.Expressions;

namespace OnKashFinance.API.Servicos;

public class FinanceiroEmpresarialService
{
    private static readonly Expression<Func<
        LancamentoEmpresarial,
        LancamentoEmpresarialResposta>> SelecaoLancamento = x =>
        new LancamentoEmpresarialResposta
        {
            Id = x.Id,
            Tipo = x.Tipo,
            ContaId = x.ContaId,
            Conta = x.Conta.Nome,
            ContaDestinoId = x.ContaDestinoId,
            ContaDestino = x.ContaDestino != null
                ? x.ContaDestino.Nome : null,
            CategoriaId = x.CategoriaId,
            Categoria = x.Categoria != null ? x.Categoria.Nome : null,
            ClienteId = x.ClienteId,
            Cliente = x.Cliente != null ? x.Cliente.NomeRazaoSocial : null,
            FornecedorId = x.FornecedorId,
            Fornecedor = x.Fornecedor != null
                ? x.Fornecedor.NomeRazaoSocial : null,
            ContaPagarId = x.ContaPagarId,
            ContaReceberId = x.ContaReceberId,
            Descricao = x.Descricao,
            Valor = x.Valor,
            Data = x.Data,
            Observacao = x.Observacao,
            Cancelado = x.Cancelado,
            CriadoEm = x.CriadoEm
        };

    private readonly OnKashDbContext _db;
    private readonly UsuarioAtualService _usuarioAtual;

    public FinanceiroEmpresarialService(
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
    // CONTAS A PAGAR
    // =========================================================

    public async Task<List<ContaPagarResposta>>
        ListarContasPagarAsync()
    {
        var empresaId = ObterEmpresa();

        return await _db.ContasPagar
            .AsNoTracking()
            .Where(x => x.EmpresaId == empresaId)
            .OrderBy(x => x.Vencimento)
            .Select(x => new ContaPagarResposta
            {
                Id = x.Id,
                FornecedorId = x.FornecedorId,
                Fornecedor = x.Fornecedor != null
                    ? x.Fornecedor.NomeRazaoSocial
                    : null,
                Descricao = x.Descricao,
                Valor = x.Valor,
                Vencimento = x.Vencimento,
                DataPagamento = x.DataPagamento,
                CategoriaId = x.CategoriaId,
                ContaId = x.ContaId,
                Status = x.Status,
                Observacao = x.Observacao
            })
            .ToListAsync();
    }

    public async Task<ContaPagarResposta>
        CriarContaPagarAsync(
            CriarContaPagarRequest request)
    {
        var empresaId = ObterEmpresa();

        ValidarCamposFinanceiros(
            request.Descricao,
            request.Valor,
            request.Vencimento
        );

        await ValidarCategoriaAsync(
            empresaId,
            request.CategoriaId,
            TipoCategoria.SAIDA
        );

        await ValidarFornecedorAsync(
            empresaId,
            request.FornecedorId
        );

        var conta = new ContaPagar
        {
            EmpresaId = empresaId,
            FornecedorId = request.FornecedorId,
            Descricao = request.Descricao.Trim(),
            Valor = request.Valor,
            Vencimento = request.Vencimento,
            CategoriaId = request.CategoriaId,
            Observacao = request.Observacao,
            ContaId = null,
            DataPagamento = null,
            Status = StatusContaPagar.PENDENTE
        };

        _db.ContasPagar.Add(conta);

        await _db.SaveChangesAsync();

        return await ObterContaPagarRespostaAsync(
            empresaId,
            conta.Id
        );
    }

    public async Task<ContaPagarResposta>
        AtualizarContaPagarAsync(
            Guid id,
            AtualizarContaPagarRequest request)
    {
        var empresaId = ObterEmpresa();

        var conta = await _db.ContasPagar
            .FirstOrDefaultAsync(x =>
                x.Id == id &&
                x.EmpresaId == empresaId);

        if (conta is null)
            throw new KeyNotFoundException(
                "Conta a pagar não encontrada.");

        if (conta.Status == StatusContaPagar.PAGO)
            throw new InvalidOperationException(
                "Não é possível editar uma conta já paga.");

        if (conta.Status == StatusContaPagar.CANCELADO)
            throw new InvalidOperationException(
                "Não é possível editar uma conta cancelada.");

        ValidarCamposFinanceiros(
            request.Descricao,
            request.Valor,
            request.Vencimento
        );

        await ValidarCategoriaAsync(
            empresaId,
            request.CategoriaId,
            TipoCategoria.SAIDA
        );

        await ValidarFornecedorAsync(
            empresaId,
            request.FornecedorId
        );

        conta.FornecedorId = request.FornecedorId;
        conta.Descricao = request.Descricao.Trim();
        conta.Valor = request.Valor;
        conta.Vencimento = request.Vencimento;
        conta.CategoriaId = request.CategoriaId;
        conta.Observacao = request.Observacao;

        await _db.SaveChangesAsync();

        return await ObterContaPagarRespostaAsync(
            empresaId,
            conta.Id
        );
    }

    public async Task PagarContaAsync(
        Guid id,
        PagarContaRequest request)
    {
        var empresaId = ObterEmpresa();

        if (request.DataPagamento == default)
            throw new InvalidOperationException(
                "A data do pagamento é obrigatória.");

        var contaPagar =
            await _db.ContasPagar
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    x.EmpresaId == empresaId);

        if (contaPagar is null)
            throw new KeyNotFoundException(
                "Conta a pagar não encontrada.");

        if (contaPagar.Status ==
            StatusContaPagar.PAGO)
        {
            throw new InvalidOperationException(
                "Esta conta já foi paga.");
        }

        if (contaPagar.Status ==
            StatusContaPagar.CANCELADO)
        {
            throw new InvalidOperationException(
                "Esta conta está cancelada.");
        }

        var conta =
            await _db.ContasEmpresariais
                .FirstOrDefaultAsync(x =>
                    x.Id == request.ContaId &&
                    x.EmpresaId == empresaId &&
                    x.Ativo);

        if (conta is null)
            throw new InvalidOperationException(
                "Conta empresarial inválida.");

        var jaPossuiLancamento =
            await _db.LancamentosEmpresariais
                .AnyAsync(x =>
                    x.ContaPagarId == contaPagar.Id &&
                    !x.Cancelado);

        if (jaPossuiLancamento)
            throw new InvalidOperationException(
                "Esta conta já possui um lançamento de pagamento.");

        await using var transacao =
            await _db.Database
                .BeginTransactionAsync();

        try
        {
            contaPagar.ContaId = conta.Id;
            contaPagar.DataPagamento =
                request.DataPagamento;
            contaPagar.Status =
                StatusContaPagar.PAGO;

            var lancamento =
                new LancamentoEmpresarial
                {
                    EmpresaId = empresaId,
                    Tipo = TipoLancamentoEmpresarial.DESPESA,
                    ContaId = conta.Id,
                    CategoriaId = contaPagar.CategoriaId,
                    FornecedorId = contaPagar.FornecedorId,
                    ContaPagarId = contaPagar.Id,
                    Descricao = contaPagar.Descricao,
                    Valor = contaPagar.Valor,
                    Data = request.DataPagamento,
                    Observacao = contaPagar.Observacao,
                    Cancelado = false
                };

            _db.LancamentosEmpresariais
                .Add(lancamento);

            await _db.SaveChangesAsync();

            await transacao.CommitAsync();
        }
        catch
        {
            await transacao.RollbackAsync();
            throw;
        }
    }

    // =========================================================
    // CONTAS A RECEBER
    // =========================================================

    public async Task<List<ContaReceberResposta>>
        ListarContasReceberAsync()
    {
        var empresaId = ObterEmpresa();

        return await _db.ContasReceber
            .AsNoTracking()
            .Where(x => x.EmpresaId == empresaId)
            .OrderBy(x => x.Vencimento)
            .Select(x => new ContaReceberResposta
            {
                Id = x.Id,
                ClienteId = x.ClienteId,
                Cliente = x.Cliente != null
                    ? x.Cliente.NomeRazaoSocial
                    : null,
                Descricao = x.Descricao,
                Valor = x.Valor,
                Vencimento = x.Vencimento,
                DataRecebimento = x.DataRecebimento,
                CategoriaId = x.CategoriaId,
                ContaId = x.ContaId,
                Status = x.Status,
                Observacao = x.Observacao
            })
            .ToListAsync();
    }

    public async Task<ContaReceberResposta>
        CriarContaReceberAsync(
            CriarContaReceberRequest request)
    {
        var empresaId = ObterEmpresa();

        ValidarCamposFinanceiros(
            request.Descricao,
            request.Valor,
            request.Vencimento
        );

        await ValidarCategoriaAsync(
            empresaId,
            request.CategoriaId,
            TipoCategoria.ENTRADA
        );

        await ValidarClienteAsync(
            empresaId,
            request.ClienteId
        );

        var conta = new ContaReceber
        {
            EmpresaId = empresaId,
            ClienteId = request.ClienteId,
            Descricao = request.Descricao.Trim(),
            Valor = request.Valor,
            Vencimento = request.Vencimento,
            CategoriaId = request.CategoriaId,
            Observacao = request.Observacao,
            ContaId = null,
            DataRecebimento = null,
            Status = StatusContaReceber.PENDENTE
        };

        _db.ContasReceber.Add(conta);

        await _db.SaveChangesAsync();

        return await ObterContaReceberRespostaAsync(
            empresaId,
            conta.Id
        );
    }

    public async Task<ContaReceberResposta>
        AtualizarContaReceberAsync(
            Guid id,
            AtualizarContaReceberRequest request)
    {
        var empresaId = ObterEmpresa();

        var conta =
            await _db.ContasReceber
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    x.EmpresaId == empresaId);

        if (conta is null)
            throw new KeyNotFoundException(
                "Conta a receber não encontrada.");

        if (conta.Status ==
            StatusContaReceber.RECEBIDO)
        {
            throw new InvalidOperationException(
                "Não é possível editar uma conta já recebida.");
        }

        if (conta.Status ==
            StatusContaReceber.CANCELADO)
        {
            throw new InvalidOperationException(
                "Não é possível editar uma conta cancelada.");
        }

        ValidarCamposFinanceiros(
            request.Descricao,
            request.Valor,
            request.Vencimento
        );

        await ValidarCategoriaAsync(
            empresaId,
            request.CategoriaId,
            TipoCategoria.ENTRADA
        );

        await ValidarClienteAsync(
            empresaId,
            request.ClienteId
        );

        conta.ClienteId = request.ClienteId;
        conta.Descricao = request.Descricao.Trim();
        conta.Valor = request.Valor;
        conta.Vencimento = request.Vencimento;
        conta.CategoriaId = request.CategoriaId;
        conta.Observacao = request.Observacao;

        await _db.SaveChangesAsync();

        return await ObterContaReceberRespostaAsync(
            empresaId,
            conta.Id
        );
    }

    public async Task ReceberContaAsync(
        Guid id,
        ReceberContaRequest request)
    {
        var empresaId = ObterEmpresa();

        if (request.DataRecebimento == default)
            throw new InvalidOperationException(
                "A data do recebimento é obrigatória.");

        var contaReceber =
            await _db.ContasReceber
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    x.EmpresaId == empresaId);

        if (contaReceber is null)
            throw new KeyNotFoundException(
                "Conta a receber não encontrada.");

        if (contaReceber.Status ==
            StatusContaReceber.RECEBIDO)
        {
            throw new InvalidOperationException(
                "Esta conta já foi recebida.");
        }

        if (contaReceber.Status ==
            StatusContaReceber.CANCELADO)
        {
            throw new InvalidOperationException(
                "Esta conta está cancelada.");
        }

        var conta =
            await _db.ContasEmpresariais
                .FirstOrDefaultAsync(x =>
                    x.Id == request.ContaId &&
                    x.EmpresaId == empresaId &&
                    x.Ativo);

        if (conta is null)
            throw new InvalidOperationException(
                "Conta empresarial inválida.");

        var jaPossuiLancamento =
            await _db.LancamentosEmpresariais
                .AnyAsync(x =>
                    x.ContaReceberId == contaReceber.Id &&
                    !x.Cancelado);

        if (jaPossuiLancamento)
            throw new InvalidOperationException(
                "Esta conta já possui um lançamento de recebimento.");

        await using var transacao =
            await _db.Database
                .BeginTransactionAsync();

        try
        {
            contaReceber.ContaId = conta.Id;
            contaReceber.DataRecebimento =
                request.DataRecebimento;
            contaReceber.Status =
                StatusContaReceber.RECEBIDO;

            var lancamento =
                new LancamentoEmpresarial
                {
                    EmpresaId = empresaId,
                    Tipo = TipoLancamentoEmpresarial.RECEITA,
                    ContaId = conta.Id,
                    CategoriaId = contaReceber.CategoriaId,
                    ClienteId = contaReceber.ClienteId,
                    ContaReceberId = contaReceber.Id,
                    Descricao = contaReceber.Descricao,
                    Valor = contaReceber.Valor,
                    Data = request.DataRecebimento,
                    Observacao = contaReceber.Observacao,
                    Cancelado = false
                };

            _db.LancamentosEmpresariais
                .Add(lancamento);

            await _db.SaveChangesAsync();

            await transacao.CommitAsync();
        }
        catch
        {
            await transacao.RollbackAsync();
            throw;
        }
    }

    // =========================================================
    // MÉTODOS AUXILIARES
    // =========================================================

    private static void ValidarCamposFinanceiros(
        string descricao,
        decimal valor,
        DateOnly vencimento)
    {
        if (string.IsNullOrWhiteSpace(descricao))
            throw new InvalidOperationException(
                "A descrição é obrigatória.");

        if (valor <= 0)
            throw new InvalidOperationException(
                "O valor deve ser maior que zero.");

        if (vencimento == default)
            throw new InvalidOperationException(
                "O vencimento é obrigatório.");
    }

    private async Task ValidarCategoriaAsync(
        Guid empresaId,
        Guid categoriaId,
        TipoCategoria tipoEsperado)
    {
        var categoria =
            await _db.CategoriasEmpresariais
                .AsNoTracking()
                .FirstOrDefaultAsync(x =>
                    x.Id == categoriaId &&
                    x.Ativo &&
                    (
                        x.EmpresaId == null ||
                        x.EmpresaId == empresaId
                    ));

        if (categoria is null)
            throw new InvalidOperationException(
                "Categoria empresarial inválida.");

        if (categoria.Tipo != tipoEsperado)
        {
            throw new InvalidOperationException(
                tipoEsperado == TipoCategoria.ENTRADA
                    ? "A categoria deve ser do tipo ENTRADA."
                    : "A categoria deve ser do tipo SAIDA."
            );
        }
    }

    private async Task ValidarFornecedorAsync(
        Guid empresaId,
        Guid? fornecedorId)
    {
        if (!fornecedorId.HasValue)
            return;

        var existe =
            await _db.Fornecedores
                .AnyAsync(x =>
                    x.Id == fornecedorId.Value &&
                    x.EmpresaId == empresaId &&
                    x.Ativo);

        if (!existe)
            throw new InvalidOperationException(
                "Fornecedor inválido.");
    }

    private async Task ValidarClienteAsync(
        Guid empresaId,
        Guid? clienteId)
    {
        if (!clienteId.HasValue)
            return;

        var existe =
            await _db.Clientes
                .AnyAsync(x =>
                    x.Id == clienteId.Value &&
                    x.EmpresaId == empresaId &&
                    x.Ativo);

        if (!existe)
            throw new InvalidOperationException(
                "Cliente inválido.");
    }

    private async Task<ContaPagarResposta>
        ObterContaPagarRespostaAsync(
            Guid empresaId,
            Guid id)
    {
        var resposta =
            await _db.ContasPagar
                .AsNoTracking()
                .Where(x =>
                    x.Id == id &&
                    x.EmpresaId == empresaId)
                .Select(x =>
                    new ContaPagarResposta
                    {
                        Id = x.Id,
                        FornecedorId = x.FornecedorId,
                        Fornecedor =
                            x.Fornecedor != null
                                ? x.Fornecedor.NomeRazaoSocial
                                : null,
                        Descricao = x.Descricao,
                        Valor = x.Valor,
                        Vencimento = x.Vencimento,
                        DataPagamento = x.DataPagamento,
                        CategoriaId = x.CategoriaId,
                        ContaId = x.ContaId,
                        Status = x.Status,
                        Observacao = x.Observacao
                    })
                .FirstOrDefaultAsync();

        return resposta
            ?? throw new KeyNotFoundException(
                "Conta a pagar não encontrada.");
    }

    private async Task<ContaReceberResposta>
        ObterContaReceberRespostaAsync(
            Guid empresaId,
            Guid id)
    {
        var resposta =
            await _db.ContasReceber
                .AsNoTracking()
                .Where(x =>
                    x.Id == id &&
                    x.EmpresaId == empresaId)
                .Select(x =>
                    new ContaReceberResposta
                    {
                        Id = x.Id,
                        ClienteId = x.ClienteId,
                        Cliente =
                            x.Cliente != null
                                ? x.Cliente.NomeRazaoSocial
                                : null,
                        Descricao = x.Descricao,
                        Valor = x.Valor,
                        Vencimento = x.Vencimento,
                        DataRecebimento = x.DataRecebimento,
                        CategoriaId = x.CategoriaId,
                        ContaId = x.ContaId,
                        Status = x.Status,
                        Observacao = x.Observacao
                    })
                .FirstOrDefaultAsync();

        return resposta
            ?? throw new KeyNotFoundException(
                "Conta a receber não encontrada.");
    }

    // =========================================================
    // LANÇAMENTOS
    // =========================================================

    public async Task<LancamentoEmpresarialResposta>
        CriarLancamentoAsync(
            CriarLancamentoEmpresarialRequest request)
    {
        var empresaId = ObterEmpresa();

        var contaExiste =
            await _db.ContasEmpresariais
                .AnyAsync(x =>
                    x.Id == request.ContaId &&
                    x.EmpresaId == empresaId);

        if (!contaExiste)
            throw new InvalidOperationException(
                "Conta inválida.");

        if (request.Tipo ==
            TipoLancamentoEmpresarial.TRANSFERENCIA)
        {
            if (!request.ContaDestinoId.HasValue)
                throw new InvalidOperationException(
                    "A conta de destino é obrigatória.");

            if (request.ContaDestinoId ==
                request.ContaId)
            {
                throw new InvalidOperationException(
                    "As contas de origem e destino devem ser diferentes.");
            }

            var destinoExiste =
                await _db.ContasEmpresariais
                    .AnyAsync(x =>
                        x.Id == request.ContaDestinoId &&
                        x.EmpresaId == empresaId);

            if (!destinoExiste)
                throw new InvalidOperationException(
                    "Conta de destino inválida.");
        }

        var lancamento =
            new LancamentoEmpresarial
            {
                EmpresaId = empresaId,
                Tipo = request.Tipo,
                ContaId = request.ContaId,
                ContaDestinoId = request.ContaDestinoId,
                CategoriaId = request.CategoriaId,
                ClienteId = request.ClienteId,
                FornecedorId = request.FornecedorId,
                Descricao = request.Descricao.Trim(),
                Valor = request.Valor,
                Data = request.Data,
                Observacao = request.Observacao,
                Cancelado = false
            };

        _db.LancamentosEmpresariais
            .Add(lancamento);

        await _db.SaveChangesAsync();

        return await ObterLancamentoRespostaAsync(
            empresaId,
            lancamento.Id
        );
    }

    public async Task<List<LancamentoEmpresarialResposta>>
        ListarLancamentosAsync(
            DateOnly? dataInicial,
            DateOnly? dataFinal,
            TipoLancamentoEmpresarial? tipo,
            Guid? contaId,
            Guid? categoriaId,
            bool incluirCancelados)
    {
        if (dataInicial.HasValue && dataFinal.HasValue &&
            dataInicial.Value > dataFinal.Value)
        {
            throw new InvalidOperationException(
                "A data inicial não pode ser maior que a data final.");
        }

        var empresaId = ObterEmpresa();

        var consulta = _db.LancamentosEmpresariais
            .AsNoTracking()
            .Where(x => x.EmpresaId == empresaId);

        if (!incluirCancelados)
            consulta = consulta.Where(x => !x.Cancelado);
        if (dataInicial.HasValue)
            consulta = consulta.Where(x => x.Data >= dataInicial.Value);
        if (dataFinal.HasValue)
            consulta = consulta.Where(x => x.Data <= dataFinal.Value);
        if (tipo.HasValue)
            consulta = consulta.Where(x => x.Tipo == tipo.Value);
        if (contaId.HasValue)
            consulta = consulta.Where(x => x.ContaId == contaId.Value);
        if (categoriaId.HasValue)
            consulta = consulta.Where(x => x.CategoriaId == categoriaId.Value);

        return await consulta
            .OrderByDescending(x => x.Data)
            .ThenByDescending(x => x.CriadoEm)
            .Select(SelecaoLancamento)
            .ToListAsync();
    }

    private async Task<LancamentoEmpresarialResposta>
        ObterLancamentoRespostaAsync(Guid empresaId, Guid id)
    {
        var resposta = await _db.LancamentosEmpresariais
            .AsNoTracking()
            .Where(x => x.Id == id && x.EmpresaId == empresaId)
            .Select(SelecaoLancamento)
            .FirstOrDefaultAsync();

        return resposta ?? throw new KeyNotFoundException(
            "Lançamento não encontrado.");
    }


    public async Task CancelarLancamentoAsync(
        Guid id)
    {
        var empresaId = ObterEmpresa();

        var lancamento =
            await _db.LancamentosEmpresariais
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    x.EmpresaId == empresaId &&
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
