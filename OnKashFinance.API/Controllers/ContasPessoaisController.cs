using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnKashFinance.API.DTOs;
using OnKashFinance.API.Servicos;

namespace OnKashFinance.API.Controllers;

[ApiController]
[Authorize]
[Route("api/pessoal/contas")]
public class ContasPessoaisController : ControllerBase
{
    private readonly PessoalService _service;

    public ContasPessoaisController(
        PessoalService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> Listar()
    {
        var contas =
            await _service.ListarContasAsync();

        return Ok(contas);
    }

    [HttpPost]
    public async Task<IActionResult> Criar(
        CriarContaPessoalRequest request)
    {
        var conta =
            await _service.CriarContaAsync(request);

        return Ok(conta);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Atualizar(
        Guid id,
        AtualizarContaPessoalRequest request)
    {
        await _service.AtualizarContaAsync(
            id,
            request
        );

        return NoContent();
    }
}