using FluentValidation;

namespace FractalDataWorks.Services.Authentication.Auth0.Configuration;

/// <summary>
/// Validator for Auth0 configuration.
/// </summary>
public sealed class Auth0ConfigurationValidator : AbstractValidator<Auth0Configuration>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Auth0ConfigurationValidator"/> class.
    /// </summary>
    public Auth0ConfigurationValidator()
    {
        RuleFor(x => x.Domain)
            .NotEmpty()
            .WithMessage("Auth0 domain is required");

        RuleFor(x => x.ClientId)
            .NotEmpty()
            .WithMessage("Auth0 client ID is required");

        RuleFor(x => x.ClientSecret)
            .NotEmpty()
            .WithMessage("Auth0 client secret is required");

        RuleFor(x => x.Audience)
            .NotEmpty()
            .WithMessage("Auth0 audience is required");

        RuleFor(x => x.TokenCacheDurationSeconds)
            .GreaterThan(0)
            .WithMessage("Token cache duration must be greater than 0");
    }
}
