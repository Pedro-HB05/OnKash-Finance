using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
    [HttpPost("cadastro")]
    public async Task<IActionResult> Cadastrar(
        CadastroRequest request)
    {
        var usuarioId =
            await _service.CadastrarAsync(request);

        return Created(
            string.Empty,
            new
            {
                usuarioId,
                mensagem = "Cadastro realizado com sucesso."
            });
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult<LoginResposta>> Login(
        LoginRequest request)
    {
        var resposta =
            await _service.LoginAsync(request);

        return Ok(resposta);
    }
}