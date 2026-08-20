using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnKashFinance.API.DTOs;
using OnKashFinance.API.Modelos;
using OnKashFinance.API.Servicos;

namespace OnKashFinance.API.Controllers;

[ApiController]
[Authorize]
[Route("api/empresarial")]
public class FinanceiroEmpresarialController
    : ControllerBase
{
    private readonly FinanceiroEmpresarialService
        _service;

    public FinanceiroEmpresarialController(
        FinanceiroEmpresarialService service)
    {
        _service = service;
    }

    // =====================================================
    // CONTAS A PAGAR
    // =====================================================

    [HttpGet("contas-pagar")]
    public async Task<IActionResult>
        ListarContasPagar()
    {
        var contas =
            await _service
                .ListarContasPagarAsync();

        return Ok(contas);
    }

    [HttpPost("contas-pagar")]
    public async Task<IActionResult>
        CriarContaPagar(
            CriarContaPagarRequest request)
    {
        var conta =
            await _service
                .CriarContaPagarAsync(request);

        return Ok(conta);
    }

    [HttpPut("contas-pagar/{id:guid}")]
    public async Task<IActionResult>
        AtualizarContaPagar(
            Guid id,
            AtualizarContaPagarRequest request)
    {
        var conta =
            await _service
                .AtualizarContaPagarAsync(
                    id,
                    request
                );

        return Ok(conta);
    }

    [HttpPost("contas-pagar/{id:guid}/pagar")]
    public async Task<IActionResult>
        PagarConta(
            Guid id,
            PagarContaRequest request)
    {
        await _service.PagarContaAsync(
            id,
            request
        );

        return NoContent();
    }

    // =====================================================
    // CONTAS A RECEBER
    // =====================================================

    [HttpGet("contas-receber")]
    public async Task<IActionResult>
        ListarContasReceber()
    {
        var contas =
            await _service
                .ListarContasReceberAsync();

        return Ok(contas);
    }

    [HttpPost("contas-receber")]
    public async Task<IActionResult>
        CriarContaReceber(
            CriarContaReceberRequest request)
    {
        var conta =
            await _service
                .CriarContaReceberAsync(request);

        return Ok(conta);
    }

    [HttpPut("contas-receber/{id:guid}")]
    public async Task<IActionResult>
        AtualizarContaReceber(
            Guid id,
            AtualizarContaReceberRequest request)
    {
        var conta =
            await _service
                .AtualizarContaReceberAsync(
                    id,
                    request
                );

        return Ok(conta);
    }

    [HttpPost(
        "contas-receber/{id:guid}/receber")]
    public async Task<IActionResult>
        ReceberConta(
            Guid id,
            ReceberContaRequest request)
    {
        await _service.ReceberContaAsync(
            id,
            request
        );

        return NoContent();
    }

    // =====================================================
    // LANÇAMENTOS
    // =====================================================

    [HttpGet("lancamentos")]
    public async Task<IActionResult> ListarLancamentos(
        [FromQuery] DateOnly? dataInicial,
        [FromQuery] DateOnly? dataFinal,
        [FromQuery] TipoLancamentoEmpresarial? tipo,
        [FromQuery] Guid? contaId,
        [FromQuery] Guid? categoriaId,
        [FromQuery] bool incluirCancelados = false)
    {
        var lancamentos = await _service.ListarLancamentosAsync(
            dataInicial, dataFinal, tipo, contaId, categoriaId,
            incluirCancelados);

        return Ok(lancamentos);
    }

    [HttpPost("lancamentos")]
    public async Task<IActionResult>
        CriarLancamento(
            CriarLancamentoEmpresarialRequest
                request)
    {
        var lancamento =
            await _service
                .CriarLancamentoAsync(request);

        return Ok(lancamento);
    }

    [HttpDelete("lancamentos/{id:guid}")]
    public async Task<IActionResult>
        CancelarLancamento(Guid id)
    {
        await _service
            .CancelarLancamentoAsync(id);

        return NoContent();
    }
}
