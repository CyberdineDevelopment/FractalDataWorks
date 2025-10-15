using FractalDataWorks.Services.Authentication.Abstractions;
using FractalDataWorks.Services.Authentication.Abstractions.Methods;
using FractalDataWorks.Services.Authentication.Abstractions.Security;
using Shouldly;

namespace FractalDataWorks.Services.Authentication.Abstractions.Tests;

public sealed class AuthenticationMethodsTests
{
    [Fact]
    public void AuthenticationMethods_All_ShouldReturnAllMethods()
    {
        // Act
        var all = AuthenticationMethods.All();

        // Assert
        all.ShouldNotBeEmpty();
        // Should contain OAuth2 method used by Entra
        all.ShouldContain(m => m.Name == "OAuth2");
    }

    [Fact]
    public void AuthenticationMethods_Name_ShouldReturnCorrectMethod()
    {
        // Act
        var method = AuthenticationMethods.Name("OAuth2");

        // Assert
        method.ShouldNotBeNull();
        method.Name.ShouldBe("OAuth2");
    }

    [Fact]
    public void AuthenticationMethods_Id_ShouldReturnMethod_WhenIdExists()
    {
        // Arrange - Get OAuth2 method used by Entra
        var oauth2 = AuthenticationMethods.Name("OAuth2");

        // Act
        var method = AuthenticationMethods.Id(oauth2.Id);

        // Assert
        method.ShouldNotBeNull();
        method.Name.ShouldBe("OAuth2");
    }

    [Fact]
    public void AuthenticationMethod_ShouldHaveValidProperties()
    {
        // Act
        var method = AuthenticationMethods.Name("OAuth2");

        // Assert
        method.ShouldNotBeNull();
        method.Name.ShouldBe("OAuth2");
        method.AuthenticationScheme.ShouldNotBeNullOrWhiteSpace();
    }
}

// NOTE: These tests verify the TypeCollection infrastructure by testing against
// EXISTING implementations from FractalDataWorks.Services.Authentication.Auth0 project.
// The source generators run during Authentication.Abstractions compilation and discover
// all [TypeOption] implementations from the Auth0 project via project reference.
// Test project references Abstractions (collections), Authentication, and Auth0 (implementations).

public sealed class AuthenticationProtocolsTests
{
    [Fact]
    public void AuthenticationProtocols_All_ShouldReturnAllProtocols()
    {
        // Act
        var all = AuthenticationProtocols.All();

        // Assert
        all.ShouldNotBeEmpty();
        // Should contain protocols used by Entra
        all.ShouldContain(p => p.Name == "OAuth2");
        all.ShouldContain(p => p.Name == "OpenIDConnect");
        all.ShouldContain(p => p.Name == "SAML2");
    }

    [Fact]
    public void AuthenticationProtocols_Name_ShouldReturnCorrectProtocol()
    {
        // Act
        var protocol = AuthenticationProtocols.Name("OAuth2");

        // Assert
        protocol.ShouldNotBeNull();
        protocol.Name.ShouldBe("OAuth2");
        protocol.Version.ShouldNotBeNullOrWhiteSpace();
    }

    [Fact]
    public void AuthenticationProtocols_Id_ShouldReturnProtocol_WhenIdExists()
    {
        // Arrange
        var oauth2 = AuthenticationProtocols.Name("OAuth2");

        // Act
        var protocol = AuthenticationProtocols.Id(oauth2.Id);

        // Assert
        protocol.ShouldNotBeNull();
        protocol.Name.ShouldBe("OAuth2");
    }
}

public sealed class AuthenticationFlowsTests
{
    [Fact]
    public void AuthenticationFlows_All_ShouldReturnAllFlows()
    {
        // Act
        var all = AuthenticationFlows.All();

        // Assert
        all.ShouldNotBeEmpty();
        // Should contain flows used by Entra
        all.ShouldContain(f => f.Name == "AuthorizationCode");
        all.ShouldContain(f => f.Name == "ClientCredentials");
        all.ShouldContain(f => f.Name == "Interactive");
    }

    [Fact]
    public void AuthenticationFlows_Name_ShouldReturnCorrectFlow()
    {
        // Act
        var flow = AuthenticationFlows.Name("ClientCredentials");

        // Assert
        flow.ShouldNotBeNull();
        flow.Name.ShouldBe("ClientCredentials");
    }

    [Fact]
    public void AuthenticationFlows_Id_ShouldReturnFlow_WhenIdExists()
    {
        // Arrange
        var clientCreds = AuthenticationFlows.Name("ClientCredentials");

        // Act
        var flow = AuthenticationFlows.Id(clientCreds.Id);

        // Assert
        flow.ShouldNotBeNull();
        flow.Name.ShouldBe("ClientCredentials");
    }
}

public sealed class TokenTypesTests
{
    [Fact]
    public void TokenTypes_All_ShouldReturnAllTokens()
    {
        // Act
        var all = TokenTypes.All();

        // Assert
        all.ShouldNotBeEmpty();
        // Should contain token types used by Entra
        all.ShouldContain(t => t.Name == "AccessToken");
        all.ShouldContain(t => t.Name == "IdToken");
        all.ShouldContain(t => t.Name == "RefreshToken");
    }

    [Fact]
    public void TokenTypes_Name_ShouldReturnCorrectToken()
    {
        // Act
        var token = TokenTypes.Name("AccessToken");

        // Assert
        token.ShouldNotBeNull();
        token.Name.ShouldBe("AccessToken");
        token.Format.ShouldNotBeNullOrWhiteSpace();
    }

    [Fact]
    public void TokenTypes_Id_ShouldReturnToken_WhenIdExists()
    {
        // Arrange
        var accessToken = TokenTypes.Name("AccessToken");

        // Act
        var token = TokenTypes.Id(accessToken.Id);

        // Assert
        token.ShouldNotBeNull();
        token.Name.ShouldBe("AccessToken");
    }
}
