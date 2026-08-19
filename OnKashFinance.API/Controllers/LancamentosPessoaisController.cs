using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnKashFinance.API.DTOs;
using OnKashFinance.API.Servicos;

namespace OnKashFinance.API.Controllers;

[ApiController]
[Authorize]
[Route("api/pessoal/lancamentos")]
public class LancamentosPessoaisController : ControllerBase
{
    private readonly PessoalService _service;

    public LancamentosPessoaisController(
        PessoalService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> Listar(
        [FromQuery] DateOnly? inicio,
        [FromQuery] DateOnly? fim)
    {
        var lancamentos =
            await _service.ListarLancamentosAsync(
                inicio,
                fim
            );

        return Ok(lancamentos);
    }

    [HttpPost]
    public async Task<IActionResult> Criar(
        CriarLancamentoPessoalRequest request)
    {
        var lancamento =
            await _service
                .CriarLancamentoAsync(request);

        return Ok(lancamento);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Atualizar(
        Guid id,
        AtualizarLancamentoPessoalRequest request)
    {
        await _service.AtualizarLancamentoAsync(
            id,
            request
        );

        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Cancelar(
        Guid id)
    {
        await _service
            .CancelarLancamentoAsync(id);

        return NoContent();
    }
}