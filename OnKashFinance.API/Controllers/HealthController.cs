using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnKashFinance.Api.Data;

namespace OnKashFinance.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    private readonly OnKashDbContext _context;

    public HealthController(OnKashDbContext context)
    {
        _context = context;
    }

    [HttpGet("database")]
    public async Task<IActionResult> CheckDatabase()
    {
        var canConnect = await _context.Database.CanConnectAsync();

        if (!canConnect)
        {
            return StatusCode(500, new
            {
                message = "Não foi possível conectar ao banco de dados."
            });
        }

        return Ok(new
        {
            message = "Conexão com o Supabase realizada com sucesso."
        });
    }
}