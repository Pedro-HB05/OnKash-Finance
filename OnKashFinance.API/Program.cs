using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using OnKashFinance.Api.Data;
using OnKashFinance.Api.Entities;
using OnKashFinance.Api.OpenApi;
using OnKashFinance.Api.Services;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer<BearerSecuritySchemeTransformer>();
});

builder.Services.AddDbContext<OnKashDbContext>(options =>
{
    options.UseNpgsql(
        builder.Configuration.GetConnectionString(
            "DefaultConnection"
        )
    );
});

builder.Services.AddScoped<
    IPasswordHasher<User>,
    PasswordHasher<User>
>();

builder.Services.AddScoped<JwtService>();

builder.Services.AddScoped<OrganizationAccessService>();

var allowedOrigins = builder.Configuration["Cors:AllowedOrigins"]?
    .Split(
        ';',
        StringSplitOptions.RemoveEmptyEntries |
        StringSplitOptions.TrimEntries
    )
    ?? ["http://localhost:5173"];

builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        policy
            .WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var jwtKey = builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException(
        "JWT Key não configurada."
    );

var jwtIssuer =
    builder.Configuration["Jwt:Issuer"];

var jwtAudience =
    builder.Configuration["Jwt:Audience"];

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
                        Encoding.UTF8.GetBytes(jwtKey)
                    ),

                ClockSkew = TimeSpan.Zero
            };
    });

var app = builder.Build();

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

app.MapGet(
    "/",
    () => Results.Redirect("/swagger")
);

app.UseHttpsRedirection();

app.UseCors("Frontend");

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();