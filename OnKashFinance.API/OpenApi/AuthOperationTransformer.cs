using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace OnKashFinance.API.OpenApi;

public sealed class AuthOperationTransformer
    : IOpenApiOperationTransformer
{
    public Task TransformAsync(
        OpenApiOperation operation,
        OpenApiOperationTransformerContext context,
        CancellationToken cancellationToken)
    {
        var permiteAnonimo =
            context.Description
                .ActionDescriptor
                .EndpointMetadata
                .OfType<AllowAnonymousAttribute>()
                .Any();

        if (permiteAnonimo)
        {
            return Task.CompletedTask;
        }

        var exigeAutenticacao =
            context.Description
                .ActionDescriptor
                .EndpointMetadata
                .OfType<AuthorizeAttribute>()
                .Any();

        if (!exigeAutenticacao)
        {
            return Task.CompletedTask;
        }

        operation.Security ??=
            new List<OpenApiSecurityRequirement>();

        operation.Security.Add(
            new OpenApiSecurityRequirement
            {
                [
                    new OpenApiSecuritySchemeReference(
                        "Bearer",
                        context.Document
                    )
                ] = []
            }
        );

        return Task.CompletedTask;
    }
}