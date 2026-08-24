using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnKashFinance.API.DTOs;
using OnKashFinance.API.Servicos;

namespace OnKashFinance.API.Controllers;

[ApiController]
[Authorize]
[Route("api/pessoal/planejamento")]
public class PlanejamentoPessoalController : ControllerBase
{
    private readonly PlanejamentoPessoalService _service;
    public PlanejamentoPessoalController(PlanejamentoPessoalService service) => _service = service;

    [HttpGet("orcamentos")]
    public async Task<IActionResult> Orcamentos([FromQuery] DateOnly? mes) => Ok(await _service.ListarOrcamentosAsync(mes));
    [HttpPost("orcamentos")]
    public async Task<IActionResult> SalvarOrcamento(SalvarOrcamentoRequest request) { await _service.SalvarOrcamentoAsync(request); return NoContent(); }
    [HttpDelete("orcamentos/{id:guid}")]
    public async Task<IActionResult> ExcluirOrcamento(Guid id) { await _service.ExcluirOrcamentoAsync(id); return NoContent(); }

    [HttpGet("recorrencias")]
    public async Task<IActionResult> Recorrencias() => Ok(await _service.ListarRecorrenciasAsync());
    [HttpPost("recorrencias")]
    public async Task<IActionResult> CriarRecorrencia(SalvarRecorrenciaRequest request) { await _service.SalvarRecorrenciaAsync(null, request); return NoContent(); }
    [HttpPut("recorrencias/{id:guid}")]
    public async Task<IActionResult> AtualizarRecorrencia(Guid id, SalvarRecorrenciaRequest request) { await _service.SalvarRecorrenciaAsync(id, request); return NoContent(); }
    [HttpDelete("recorrencias/{id:guid}")]
    public async Task<IActionResult> ExcluirRecorrencia(Guid id) { await _service.ExcluirRecorrenciaAsync(id); return NoContent(); }

    [HttpGet("alertas")]
    public async Task<IActionResult> Alertas() => Ok(await _service.ListarAlertasAsync());
}
