using Microsoft.IdentityModel.Tokens;
using OnKashFinance.API.Modelos;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace OnKashFinance.API.Autenticacao;

public class JwtService
{
    private readonly IConfiguration _configuration;

    public JwtService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string GerarToken(
        Usuario usuario,
        Guid? empresaId = null,
        PerfilEmpresa? perfil = null)
    {
        var jwt = _configuration.GetSection("Jwt");

        var chave = jwt["Key"]
            ?? throw new InvalidOperationException(
                "A chave JWT não foi configurada.");

        var issuer = jwt["Issuer"]
            ?? throw new InvalidOperationException(
                "O Issuer do JWT não foi configurado.");

        var audience = jwt["Audience"]
            ?? throw new InvalidOperationException(
                "O Audience do JWT não foi configurado.");

        var expirationMinutes = int.TryParse(
            jwt["ExpirationMinutes"],
            out var minutos)
                ? minutos
                : 120;

        var claims = new List<Claim>
        {
            new(
                ClaimTypes.NameIdentifier,
                usuario.Id.ToString()
            ),

            new(
                ClaimTypes.Name,
                usuario.Nome
            ),

            new(
                ClaimTypes.Email,
                usuario.Email
            ),

            new(
                "tipo_conta",
                usuario.TipoConta.ToString()
            )
        };

        if (empresaId.HasValue)
        {
            claims.Add(
                new Claim(
                    "empresa_id",
                    empresaId.Value.ToString()
                )
            );
        }

        if (perfil.HasValue)
        {
            claims.Add(
                new Claim(
                    "perfil_empresa",
                    perfil.Value.ToString()
                )
            );

            claims.Add(
                new Claim(
                    ClaimTypes.Role,
                    perfil.Value.ToString()
                )
            );
        }

        var securityKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(chave)
        );

        var credentials = new SigningCredentials(
            securityKey,
            SecurityAlgorithms.HmacSha256
        );

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: DateTime.UtcNow.AddMinutes(
                expirationMinutes
            ),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler()
            .WriteToken(token);
    }
}