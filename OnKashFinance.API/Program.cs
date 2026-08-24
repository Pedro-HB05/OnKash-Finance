using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Npgsql.NameTranslation;
using OnKashFinance.API.Autenticacao;
using OnKashFinance.API.Dados;
using OnKashFinance.API.Erros;
using OnKashFinance.API.Modelos;
using OnKashFinance.API.OpenApi;
using OnKashFinance.API.Servicos;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// =========================================================
// CONTROLLERS / JSON
// =========================================================

builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(
            new JsonStringEnumConverter()
        );
    });

// =========================================================
// OPENAPI / SWAGGER
// =========================================================

builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer<
        BearerSecuritySchemeTransformer>();

    options.AddOperationTransformer<
        AuthOperationTransformer>();
});

// =========================================================
// BANCO DE DADOS
// =========================================================

var connectionString =
    builder.Configuration.GetConnectionString(
        "DefaultConnection"
    )
    ?? throw new InvalidOperationException(
        "ConnectionString 'DefaultConnection' não configurada."
    );

// IMPORTANTE:
// Essa instância é criada UMA VEZ e reutilizada em todos
// os mapeamentos do Entity Framework/Npgsql.
var nomesExatos =
    new NpgsqlNullNameTranslator();

builder.Services.AddDbContext<OnKashDbContext>(
    options =>
    {
        options.UseNpgsql(
            connectionString,
            npgsqlOptions =>
            {
                npgsqlOptions.MapEnum<
                    TipoContaUsuario>(
                    "tipo_conta_usuario",
                    nameTranslator: nomesExatos
                );

                npgsqlOptions.MapEnum<
                    TipoCategoria>(
                    "tipo_categoria",
                    nameTranslator: nomesExatos
                );

                npgsqlOptions.MapEnum<
                    TipoLancamentoPessoal>(
                    "tipo_lancamento_pessoal",
                    nameTranslator: nomesExatos
                );

                npgsqlOptions.MapEnum<
                    StatusFatura>(
                    "status_fatura",
                    nameTranslator: nomesExatos
                );

                npgsqlOptions.MapEnum<
                    TipoLancamentoEmpresarial>(
                    "tipo_lancamento_empresarial",
                    nameTranslator: nomesExatos
                );

                npgsqlOptions.MapEnum<
                    StatusContaPagar>(
                    "status_conta_pagar",
                    nameTranslator: nomesExatos
                );

                npgsqlOptions.MapEnum<
                    StatusContaReceber>(
                    "status_conta_receber",
                    nameTranslator: nomesExatos
                );

                npgsqlOptions.MapEnum<
                    PerfilEmpresa>(
                    "perfil_empresa",
                    nameTranslator: nomesExatos
                );
            }
        );
    }
);

// =========================================================
// PASSWORD HASHER
// =========================================================

builder.Services.AddScoped<
    IPasswordHasher<Usuario>,
    PasswordHasher<Usuario>
>();

// =========================================================
// HTTP CONTEXT
// =========================================================

builder.Services.AddHttpContextAccessor();

// =========================================================
// SERVIÇOS
// =========================================================

builder.Services.AddScoped<JwtService>();

builder.Services.AddScoped<
    UsuarioAtualService>();

builder.Services.AddScoped<
    AutenticacaoService>();

builder.Services.AddScoped<
    PessoalService>();

builder.Services.AddScoped<
    CartaoService>();

builder.Services.AddScoped<
    CadastrosEmpresariaisService>();

builder.Services.AddScoped<
    FinanceiroEmpresarialService>();

builder.Services.AddScoped<
    EmpresaUsuariosService>();

builder.Services.AddScoped<
    DashboardService>();
builder.Services.AddScoped<PlanejamentoPessoalService>();
builder.Services.AddScoped<InteligenciaFinanceiraService>();
builder.Services.AddScoped<EmailService>();

builder.Services.AddRateLimiter(options =>
{
    options.AddPolicy("verificacao-email", context =>
        RateLimitPartition.GetFixedWindowLimiter(
            context.Connection.RemoteIpAddress?.ToString() ?? "desconhecido",
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 6,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0
            }));
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
});

// =========================================================
// CORS
// =========================================================

var allowedOrigins =
    builder.Configuration[
        "Cors:AllowedOrigins"
    ]?
    .Split(
        new[] { ',', ';' },
        StringSplitOptions.RemoveEmptyEntries |
        StringSplitOptions.TrimEntries
    )
    ?? new[]
    {
        "http://localhost:3000",
        "http://localhost:5173"
    };

builder.Services.AddCors(options =>
{
    options.AddPolicy(
        "Frontend",
        policy =>
        {
            if (allowedOrigins.Contains("*"))
            {
                policy
                    .AllowAnyOrigin()
                    .AllowAnyHeader()
                    .AllowAnyMethod();
            }
            else
            {
                policy
                    .WithOrigins(allowedOrigins)
                    .AllowAnyHeader()
                    .AllowAnyMethod();
            }
        }
    );
});

// =========================================================
// JWT
// =========================================================

var jwtKey =
    builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException(
        "Jwt:Key não configurada."
    );

var jwtIssuer =
    builder.Configuration["Jwt:Issuer"]
    ?? throw new InvalidOperationException(
        "Jwt:Issuer não configurado."
    );

var jwtAudience =
    builder.Configuration["Jwt:Audience"]
    ?? throw new InvalidOperationException(
        "Jwt:Audience não configurado."
    );

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme =
            JwtBearerDefaults.AuthenticationScheme;

        options.DefaultChallengeScheme =
            JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters =
            new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,

                ValidIssuer = jwtIssuer,
                ValidAudience = jwtAudience,

                IssuerSigningKey =
                    new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(
                            jwtKey
                        )
                    ),

                ClockSkew = TimeSpan.Zero
            };
    });

builder.Services.AddAuthorization();

// =========================================================
// APP
// =========================================================

var app = builder.Build();

// =========================================================
// TRATAMENTO GLOBAL DE ERROS
// =========================================================

app.UseMiddleware<
    TratamentoErrosMiddleware>();

// =========================================================
// SWAGGER - DESENVOLVIMENTO
// =========================================================

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint(
            "/openapi/v1.json",
            "OnKash Finance API v1"
        );

        options.DocumentTitle =
            "OnKash Finance API";
    });
}

// =========================================================
// ROTA RAIZ
// =========================================================

app.MapGet(
    "/",
    () =>
        app.Environment.IsDevelopment()
            ? Results.Redirect("/swagger")
            : Results.Ok(
                new
                {
                    nome = "OnKash Finance API",
                    status = "online"
                }
            )
);

// =========================================================
// PIPELINE
// =========================================================

app.UseHttpsRedirection();

app.UseCors("Frontend");

app.UseAuthentication();

app.UseAuthorization();

app.UseRateLimiter();

app.MapControllers();

app.Run();
