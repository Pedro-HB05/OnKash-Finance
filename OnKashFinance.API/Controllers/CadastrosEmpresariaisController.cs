using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnKashFinance.API.DTOs;
using OnKashFinance.API.Servicos;

namespace OnKashFinance.API.Controllers;

[ApiController]
[Authorize]
[Route("api/empresarial")]
public class CadastrosEmpresariaisController
    : ControllerBase
{
    private readonly CadastrosEmpresariaisService
        _service;

    public CadastrosEmpresariaisController(
        CadastrosEmpresariaisService service)
    {
        _service = service;
    }

    // =====================================================
    // CONTAS
    // =====================================================

    [HttpGet("contas")]
    public async Task<IActionResult> ListarContas()
    {
        var contas =
            await _service.ListarContasAsync();

        return Ok(contas);
    }

    [HttpPost("contas")]
    public async Task<IActionResult> CriarConta(
        CriarContaEmpresarialRequest request)
    {
        var conta =
            await _service.CriarContaAsync(
                request
            );

        return Ok(conta);
    }

    // =====================================================
    // CATEGORIAS
    // =====================================================

    [HttpGet("categorias")]
    public async Task<IActionResult>
        ListarCategorias()
    {
        var categorias =
            await _service
                .ListarCategoriasAsync();

        return Ok(categorias);
    }

    [HttpPost("categorias")]
    public async Task<IActionResult>
        CriarCategoria(
            CriarCategoriaEmpresarialRequest
                request)
    {
        var categoria =
            await _service
                .CriarCategoriaAsync(request);

        return Ok(categoria);
    }

    // =====================================================
    // CLIENTES
    // =====================================================

    [HttpGet("clientes")]
    public async Task<IActionResult>
        ListarClientes()
    {
        var clientes =
            await _service.ListarClientesAsync();

        return Ok(clientes);
    }

    [HttpPost("clientes")]
    public async Task<IActionResult> CriarCliente(
        CriarClienteRequest request)
    {
        var cliente =
            await _service.CriarClienteAsync(
                request
            );

        return Ok(cliente);
    }

    [HttpPut("clientes/{id:guid}")]
    public async Task<IActionResult>
        AtualizarCliente(
            Guid id,
            AtualizarClienteRequest request)
    {
        await _service.AtualizarClienteAsync(
            id,
            request
        );

        return NoContent();
    }

    // =====================================================
    // FORNECEDORES
    // =====================================================

    [HttpGet("fornecedores")]
    public async Task<IActionResult>
        ListarFornecedores()
    {
        var fornecedores =
            await _service
                .ListarFornecedoresAsync();

        return Ok(fornecedores);
    }

    [HttpPost("fornecedores")]
    public async Task<IActionResult>
        CriarFornecedor(
            CriarFornecedorRequest request)
    {
        var fornecedor =
            await _service
                .CriarFornecedorAsync(request);

        return Ok(fornecedor);
    }

    [HttpPut("fornecedores/{id:guid}")]
    public async Task<IActionResult>
        AtualizarFornecedor(
            Guid id,
            AtualizarFornecedorRequest request)
    {
        await _service
            .AtualizarFornecedorAsync(
                id,
                request
            );

        return NoContent();
    }
}