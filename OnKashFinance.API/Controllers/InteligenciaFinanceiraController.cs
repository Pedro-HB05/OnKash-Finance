using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnKashFinance.API.DTOs;
using OnKashFinance.API.Servicos;

namespace OnKashFinance.API.Controllers;

[ApiController, Authorize]
[Route("api/{ambiente:regex(^(pessoal|empresarial)$)}/inteligencia")]
public class InteligenciaFinanceiraController : ControllerBase
{
    private readonly InteligenciaFinanceiraService _service;
    public InteligenciaFinanceiraController(InteligenciaFinanceiraService service) => _service = service;
    private static bool Pessoal(string ambiente) => ambiente.Equals("pessoal", StringComparison.OrdinalIgnoreCase);

    [HttpPost("importacoes")]
    public async Task<IActionResult> Importar(string ambiente, ImportarExtratoRequest request) => Ok(await _service.ImportarAsync(Pessoal(ambiente), request));
    [HttpGet("projecao")]
    public async Task<IActionResult> Projecao(string ambiente, [FromQuery] int dias = 90) => Ok(await _service.ProjetarAsync(Pessoal(ambiente), dias));
    [HttpGet("dre")]
    public async Task<IActionResult> Dre(string ambiente, [FromQuery] DateOnly inicio, [FromQuery] DateOnly fim)
    { if (Pessoal(ambiente)) return BadRequest(new { mensagem = "DRE disponível para contas empresariais." }); return Ok(await _service.ObterDreAsync(inicio, fim)); }
    [HttpGet("alertas")]
    public async Task<IActionResult> Alertas(string ambiente)
    { if (Pessoal(ambiente)) return BadRequest(); return Ok(await _service.AlertasEmpresariaisAsync()); }
    [HttpGet("lancamentos/{lancamentoId:guid}/anexos")]
    public async Task<IActionResult> Anexos(string ambiente, Guid lancamentoId) => Ok(await _service.ListarAnexosAsync(Pessoal(ambiente), lancamentoId));
    [HttpPost("lancamentos/{lancamentoId:guid}/anexos")]
    [RequestSizeLimit(5_500_000)]
    public async Task<IActionResult> AdicionarAnexo(string ambiente, Guid lancamentoId, IFormFile arquivo) { await _service.AdicionarAnexoAsync(Pessoal(ambiente), lancamentoId, arquivo); return NoContent(); }
    [HttpGet("anexos/{id:guid}/arquivo")]
    public async Task<IActionResult> Baixar(string ambiente, Guid id) { var item = await _service.ObterAnexoAsync(Pessoal(ambiente), id); return File(item.Conteudo, item.TipoConteudo, item.NomeArquivo); }
    [HttpDelete("anexos/{id:guid}")]
    public async Task<IActionResult> Excluir(string ambiente, Guid id) { await _service.ExcluirAnexoAsync(Pessoal(ambiente), id); return NoContent(); }
}
