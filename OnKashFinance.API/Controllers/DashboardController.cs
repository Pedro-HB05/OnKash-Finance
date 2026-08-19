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
    public async Task<IActionResult> Pessoal()
    {
        var dashboard =
            await _service.ObterPessoalAsync();

        return Ok(dashboard);
    }

    [HttpGet("empresarial")]
    public async Task<IActionResult> Empresarial()
    {
        var dashboard =
            await _service
                .ObterEmpresarialAsync();

        return Ok(dashboard);
    }
}