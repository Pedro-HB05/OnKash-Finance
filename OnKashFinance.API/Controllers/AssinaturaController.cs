using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnKashFinance.API.DTOs;
using OnKashFinance.API.Servicos;

namespace OnKashFinance.API.Controllers;

[ApiController]
[Authorize]
[Route("api/assinatura")]
public class AssinaturaController : ControllerBase
{
    private readonly AssinaturaService _service;
    public AssinaturaController(AssinaturaService service) => _service = service;

    [HttpGet]
    public async Task<ActionResult<AssinaturaResumoResposta>> Obter() => Ok(await _service.ObterResumoAsync());

    [HttpPost("solicitar-upgrade")]
    public async Task<IActionResult> SolicitarUpgrade(SolicitarUpgradeRequest request)
    {
        await _service.SolicitarUpgradeAsync(request.Plano);
        return Ok(new { mensagem = "Interesse registrado. Avisaremos quando os planos pagos forem liberados." });
    }
}
