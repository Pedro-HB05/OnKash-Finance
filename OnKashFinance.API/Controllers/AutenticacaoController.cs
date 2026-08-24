using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using OnKashFinance.API.DTOs;
using OnKashFinance.API.Servicos;

namespace OnKashFinance.API.Controllers;

[ApiController]
[Route("api")]
public class AutenticacaoController : ControllerBase
{
    private readonly AutenticacaoService _service;

    public AutenticacaoController(
        AutenticacaoService service)
    {
        _service = service;
    }

    [AllowAnonymous]
    [EnableRateLimiting("cadastro")]
    [HttpPost("cadastro")]
    public async Task<IActionResult> Cadastrar(
        CadastroRequest request)
    {
        var resposta =
            await _service.CadastrarAsync(request);

        return Created(string.Empty, resposta);
    }

    [AllowAnonymous]
    [EnableRateLimiting("login")]
    [HttpPost("login")]
    public async Task<ActionResult<LoginResposta>> Login(
        LoginRequest request)
    {
        var resposta =
            await _service.LoginAsync(request);

        return Ok(resposta);
    }

    [AllowAnonymous]
    [EnableRateLimiting("verificacao-email")]
    [HttpPost("verificar-email")]
    public async Task<IActionResult> VerificarEmail(VerificarEmailRequest request)
    {
        await _service.VerificarEmailAsync(request);
        return Ok(new { mensagem = "E-mail verificado com sucesso." });
    }

    [AllowAnonymous]
    [EnableRateLimiting("verificacao-email")]
    [HttpPost("reenviar-codigo-email")]
    public async Task<IActionResult> ReenviarCodigo(ReenviarCodigoEmailRequest request)
    {
        await _service.ReenviarCodigoAsync(request);
        return Ok(new { mensagem = "Se houver um cadastro pendente, um novo código será enviado." });
    }
}
