using FractalDataWorks.Services.Authentication.Abstractions;
using FractalDataWorks.Services.Authentication.Abstractions.Methods;
using FractalDataWorks.Services.Authentication.Abstractions.Security;
using Shouldly;

namespace FractalDataWorks.Services.Authentication.Abstractions.Tests;

// NOTE: These tests verify TypeCollection behavior when NO concrete implementations exist.
// When implementations are added (e.g., Auth0, Entra), additional tests should verify
// that the collections contain those implementations.

public sealed class AuthenticationMethodsTests
{
    [Fact]
    public void AuthenticationMethods_All_WhenNoImplementations_ReturnsEmptyArray()
    {
        // Act
        var all = AuthenticationMethods.All();

        // Assert
        all.ShouldNotBeNull();
        all.ShouldBeEmpty();
    }

    [Fact]
    public void AuthenticationMethods_Name_WhenNoImplementations_ReturnsEmpty()
    {
        // Act
        var method = AuthenticationMethods.Name("NonExistent");

        // Assert
        method.ShouldNotBeNull();
        method.Name.ShouldBe(string.Empty);
    }
}

public sealed class AuthenticationProtocolsTests
{
    [Fact]
    public void AuthenticationProtocols_All_WhenNoImplementations_ReturnsEmptyArray()
    {
        // Act
        var all = AuthenticationProtocols.All();

        // Assert
        all.ShouldNotBeNull();
        all.ShouldBeEmpty();
    }

    [Fact]
    public void AuthenticationProtocols_Name_WhenNoImplementations_ReturnsEmpty()
    {
        // Act
        var protocol = AuthenticationProtocols.Name("NonExistent");

        // Assert
        protocol.ShouldNotBeNull();
        protocol.Name.ShouldBe(string.Empty);
    }
}

public sealed class AuthenticationFlowsTests
{
    [Fact]
    public void AuthenticationFlows_All_WhenNoImplementations_ReturnsEmptyArray()
    {
        // Act
        var all = AuthenticationFlows.All();

        // Assert
        all.ShouldNotBeNull();
        all.ShouldBeEmpty();
    }

    [Fact]
    public void AuthenticationFlows_Name_WhenNoImplementations_ReturnsEmpty()
    {
        // Act
        var flow = AuthenticationFlows.Name("NonExistent");

        // Assert
        flow.ShouldNotBeNull();
        flow.Name.ShouldBe(string.Empty);
    }
}

public sealed class TokenTypesTests
{
    [Fact]
    public void TokenTypes_All_WhenNoImplementations_ReturnsEmptyArray()
    {
        // Act
        var all = TokenTypes.All();

        // Assert
        all.ShouldNotBeNull();
        all.ShouldBeEmpty();
    }

    [Fact]
    public void TokenTypes_Name_WhenNoImplementations_ReturnsEmpty()
    {
        // Act
        var token = TokenTypes.Name("NonExistent");

        // Assert
        token.ShouldNotBeNull();
        token.Name.ShouldBe(string.Empty);
    }
}
