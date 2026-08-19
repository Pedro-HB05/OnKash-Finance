using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnKashFinance.API.DTOs;
using OnKashFinance.API.Servicos;

namespace OnKashFinance.API.Controllers;

[ApiController]
[Authorize]
[Route("api/pessoal/categorias")]
public class CategoriasPessoaisController : ControllerBase
{
    private readonly PessoalService _service;

    public CategoriasPessoaisController(
        PessoalService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> Listar()
    {
        var categorias =
            await _service.ListarCategoriasAsync();

        return Ok(categorias);
    }

    [HttpPost]
    public async Task<IActionResult> Criar(
        CriarCategoriaPessoalRequest request)
    {
        var categoria =
            await _service.CriarCategoriaAsync(request);

        return Ok(categoria);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Atualizar(
        Guid id,
        AtualizarCategoriaPessoalRequest request)
    {
        await _service.AtualizarCategoriaAsync(
            id,
            request
        );

        return NoContent();
    }
}