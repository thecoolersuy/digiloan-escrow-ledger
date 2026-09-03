using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace DigiLoan.Api.Extensions;

public class BearerSecuritySchemeTransformer : IOpenApiDocumentTransformer
{

    private readonly IAuthenticationSchemeProvider _authenticationSchemeProvider;

    public BearerSecuritySchemeTransformer(IAuthenticationSchemeProvider authenticationSchemeProvider)
    {
        _authenticationSchemeProvider = authenticationSchemeProvider;
    }
    public async Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
    {
        var authenticationSchemes = await _authenticationSchemeProvider.GetAllSchemesAsync();
        if (authenticationSchemes.Any(authScheme => authScheme.Name == "Bearer"))
        {
            var requirements = new Dictionary<string, IOpenApiSecurityScheme>
            {
                ["Bearer"] = new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.Http,
                    Scheme = "Bearer",
                    In = ParameterLocation.Header,
                    BearerFormat = "JWT"
                }
            };
            document.Components ??= new OpenApiComponents();
            document.Components.SecuritySchemes = requirements;
        }


    }
}

