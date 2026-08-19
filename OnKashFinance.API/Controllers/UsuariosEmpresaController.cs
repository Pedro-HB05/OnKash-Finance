using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnKashFinance.API.DTOs;
using OnKashFinance.API.Servicos;

namespace OnKashFinance.API.Controllers;

[ApiController]
[Authorize]
[Route("api/empresarial/usuarios")]
public class UsuariosEmpresaController
    : ControllerBase
{
    private readonly EmpresaUsuariosService _service;

    public UsuariosEmpresaController(
        EmpresaUsuariosService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> Listar()
    {
        var usuarios =
            await _service.ListarAsync();

        return Ok(usuarios);
    }

    [HttpPost]
    public async Task<IActionResult> Adicionar(
        AdicionarUsuarioEmpresaRequest request)
    {
        var usuario =
            await _service.AdicionarAsync(
                request
            );

        return Ok(usuario);
    }

    [HttpPut("{id:guid}/perfil")]
    public async Task<IActionResult>
        AtualizarPerfil(
            Guid id,
            AtualizarPerfilEmpresaRequest request)
    {
        await _service.AtualizarPerfilAsync(
            id,
            request
        );

        return NoContent();
    }

    [HttpPut("{id:guid}/permissoes")]
    public async Task<IActionResult>
        AtualizarPermissoes(
            Guid id,
            AtualizarPermissoesEmpresaRequest
                request)
    {
        await _service
            .AtualizarPermissoesAsync(
                id,
                request
            );

        return NoContent();
    }
}