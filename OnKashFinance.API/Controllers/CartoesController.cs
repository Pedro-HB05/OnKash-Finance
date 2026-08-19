using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnKashFinance.API.DTOs;
using OnKashFinance.API.Servicos;

namespace OnKashFinance.API.Controllers;

[ApiController]
[Authorize]
[Route("api/pessoal/cartoes")]
public class CartoesController : ControllerBase
{
    private readonly CartaoService _service;

    public CartoesController(
        CartaoService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> Listar()
    {
        var cartoes =
            await _service.ListarAsync();

        return Ok(cartoes);
    }

    [HttpPost]
    public async Task<IActionResult> Criar(
        CriarCartaoRequest request)
    {
        var cartao =
            await _service.CriarAsync(request);

        return Ok(cartao);
    }

    [HttpPost("compras")]
    public async Task<IActionResult> CriarCompra(
        CriarCompraCartaoRequest request)
    {
        var compra =
            await _service.CriarCompraAsync(
                request
            );

        return Ok(compra);
    }

    [HttpDelete("compras/{id:guid}")]
    public async Task<IActionResult> CancelarCompra(
        Guid id)
    {
        await _service.CancelarCompraAsync(id);

        return NoContent();
    }

    [HttpGet("faturas")]
    public async Task<IActionResult> ListarFaturas(
        [FromQuery] Guid? cartaoId)
    {
        var faturas =
            await _service.ListarFaturasAsync(
                cartaoId
            );

        return Ok(faturas);
    }

    [HttpPost("faturas/{id:guid}/pagar")]
    public async Task<IActionResult> PagarFatura(
        Guid id,
        PagarFaturaRequest request)
    {
        await _service.PagarFaturaAsync(
            id,
            request
        );

        return NoContent();
    }
}