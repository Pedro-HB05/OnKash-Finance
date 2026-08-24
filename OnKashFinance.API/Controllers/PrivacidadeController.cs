using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnKashFinance.API.DTOs;
using OnKashFinance.API.Servicos;

namespace OnKashFinance.API.Controllers;

[ApiController, Authorize]
[Route("api/privacidade")]
public class PrivacidadeController : ControllerBase
{
    private readonly PrivacidadeService _service;
    public PrivacidadeController(PrivacidadeService service) => _service = service;

    [HttpGet]
    public async Task<IActionResult> Obter() => Ok(await _service.ObterResumoAsync());
    [HttpPost("aceites")]
    public async Task<IActionResult> Aceitar() { await _service.RegistrarAceiteAtualAsync(); return NoContent(); }
    [HttpPost("solicitacoes")]
    public async Task<IActionResult> Solicitar(SolicitarDireitoRequest request) => Ok(await _service.SolicitarDireitoAsync(request));
    [HttpPut("perfil")]
    public async Task<IActionResult> Corrigir(CorrigirPerfilRequest request) { await _service.CorrigirPerfilAsync(request.Nome); return NoContent(); }
    [HttpGet("exportacao")]
    public async Task<IActionResult> Exportar() => File(await _service.ExportarDadosAsync(), "application/json", $"dados-onkash-{DateTime.UtcNow:yyyyMMdd}.json");
}
