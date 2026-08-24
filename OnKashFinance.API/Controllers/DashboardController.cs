using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnKashFinance.API.Servicos;

namespace OnKashFinance.API.Controllers;

[ApiController]
[Authorize]
[Route("api/dashboard")]
public class DashboardController : ControllerBase
{
    private readonly DashboardService _service;

    public DashboardController(
        DashboardService service)
    {
        _service = service;
    }

    [HttpGet("pessoal")]
    public async Task<IActionResult> Pessoal(
        [FromQuery] DateOnly? inicio,
        [FromQuery] DateOnly? fim)
    {
        var dashboard =
            await _service.ObterPessoalAsync(
                inicio,
                fim);

        return Ok(dashboard);
    }

    [HttpGet("empresarial")]
    public async Task<IActionResult> Empresarial(
        [FromQuery] DateOnly? inicio,
        [FromQuery] DateOnly? fim)
    {
        var dashboard =
            await _service.ObterEmpresarialAsync(
                inicio,
                fim);

        return Ok(dashboard);
    }
}