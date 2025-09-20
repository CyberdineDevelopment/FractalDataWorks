using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using FractalDataWorks.Services.Authentication.Abstractions.Security;
using FractalDataWorks.ServiceTypes;

namespace FractalDataWorks.Services.Authentication.Abstractions;

/// <summary>
/// Service type definition for local authentication (username/password).
/// Provides traditional form-based authentication with local credential storage.
/// </summary>
public sealed class LocalAuthenticationType : 
    AuthenticationTypeBase<IAuthenticationService, IAuthenticationConfiguration, IAuthenticationServiceFactory<IAuthenticationService, IAuthenticationConfiguration>>
{
    /// <summary>
    /// Gets the singleton instance of the local authentication type.
    /// </summary>
    public static LocalAuthenticationType Instance { get; } = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="LocalAuthenticationType"/> class.
    /// </summary>
    private LocalAuthenticationType() : base(2, "Local", "Local Authentication")
    {
    }

    /// <inheritdoc/>
    public override string ProviderName => "Local Identity Store";

    /// <inheritdoc/>
    public override AuthenticationMethod Method => AuthenticationMethod.FormBased;

    /// <inheritdoc/>
    public override IReadOnlyList<string> SupportedProtocols => new[]
    {
        "HTTP Basic",
        "Form Authentication",
        "Cookie Authentication"
    };

    /// <inheritdoc/>
    public override IReadOnlyList<string> SupportedFlows => new[]
    {
        "username_password",
        "cookie_based"
    };

    /// <inheritdoc/>
    public override IReadOnlyList<string> SupportedTokenTypes => new[]
    {
        "JWT",
        "Session Cookie",
        "Authentication Cookie"
    };

    /// <inheritdoc/>
    public override bool SupportsMultiTenant => false; // Local auth is single-tenant by nature

    /// <inheritdoc/>
    public override bool SupportsTokenCaching => true;

    /// <inheritdoc/>
    public override bool SupportsTokenRefresh => false; // Traditional local auth doesn't use refresh tokens

    /// <inheritdoc/>
    public override int Priority => 30; // Lower priority compared to modern OAuth2 providers

    /// <inheritdoc/>
    public override Type FactoryType => typeof(IAuthenticationServiceFactory<IAuthenticationService, IAuthenticationConfiguration>);

    /// <inheritdoc/>
    public override void Register(IServiceCollection services)
    {
        // Register local authentication specific services
        // services.AddScoped<ILocalAuthenticationFactory, LocalAuthenticationFactory>();
        // services.AddScoped<LocalAuthenticationService>();
        // services.AddScoped<IPasswordHasher, BCryptPasswordHasher>();
        // services.AddScoped<IUserStore, LocalUserStore>();
    }

    /// <inheritdoc/>
    public override void Configure(IConfiguration configuration)
    {
        // Local authentication specific configuration
        // This could set password policies, lockout settings, etc.
    }
}