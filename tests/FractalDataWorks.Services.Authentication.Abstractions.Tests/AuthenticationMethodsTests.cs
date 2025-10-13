using FractalDataWorks.Services.Authentication.Abstractions;
using Shouldly;

namespace FractalDataWorks.Services.Authentication.Abstractions.Tests;

public sealed class AuthenticationMethodsTests
{
    [Fact]
    public void OAuth2_ShouldHaveCorrectProperties()
    {
        // Act
        var method = AuthenticationMethods.OAuth2;

        // Assert
        method.ShouldNotBeNull();
        method.Name.ShouldBe("OAuth2");
        method.SupportsTokenRefresh.ShouldBeTrue();
    }

    [Fact]
    public void OpenIDConnect_ShouldHaveCorrectProperties()
    {
        // Act
        var method = AuthenticationMethods.OpenIDConnect;

        // Assert
        method.ShouldNotBeNull();
        method.Name.ShouldBe("OpenIDConnect");
    }

    [Fact]
    public void JWT_ShouldHaveCorrectProperties()
    {
        // Act
        var method = AuthenticationMethods.JWT;

        // Assert
        method.ShouldNotBeNull();
        method.Name.ShouldBe("JWT");
    }

    [Fact]
    public void BearerToken_ShouldHaveCorrectProperties()
    {
        // Act
        var method = AuthenticationMethods.BearerToken;

        // Assert
        method.ShouldNotBeNull();
        method.Name.ShouldBe("BearerToken");
    }

    [Fact]
    public void ApiKey_ShouldHaveCorrectProperties()
    {
        // Act
        var method = AuthenticationMethods.ApiKey;

        // Assert
        method.ShouldNotBeNull();
        method.Name.ShouldBe("ApiKey");
    }

    [Fact]
    public void Certificate_ShouldHaveCorrectProperties()
    {
        // Act
        var method = AuthenticationMethods.Certificate;

        // Assert
        method.ShouldNotBeNull();
        method.Name.ShouldBe("Certificate");
    }

    [Fact]
    public void ManagedIdentity_ShouldHaveCorrectProperties()
    {
        // Act
        var method = AuthenticationMethods.ManagedIdentity;

        // Assert
        method.ShouldNotBeNull();
        method.Name.ShouldBe("ManagedIdentity");
    }

    [Fact]
    public void All_ShouldContainAllMethods()
    {
        // Act
        var all = AuthenticationMethods.All;

        // Assert
        all.ShouldNotBeNull();
        all.ShouldContain(AuthenticationMethods.OAuth2);
        all.ShouldContain(AuthenticationMethods.OpenIDConnect);
        all.ShouldContain(AuthenticationMethods.JWT);
        all.ShouldContain(AuthenticationMethods.BearerToken);
        all.ShouldContain(AuthenticationMethods.ApiKey);
        all.ShouldContain(AuthenticationMethods.Certificate);
        all.ShouldContain(AuthenticationMethods.ManagedIdentity);
    }
}

public sealed class AuthenticationProtocolsTests
{
    [Fact]
    public void OAuth2Protocol_ShouldHaveCorrectProperties()
    {
        // Act
        var protocol = AuthenticationProtocols.OAuth2Protocol;

        // Assert
        protocol.ShouldNotBeNull();
        protocol.Name.ShouldBe("OAuth2Protocol");
    }

    [Fact]
    public void OpenIDConnectProtocol_ShouldHaveCorrectProperties()
    {
        // Act
        var protocol = AuthenticationProtocols.OpenIDConnectProtocol;

        // Assert
        protocol.ShouldNotBeNull();
        protocol.Name.ShouldBe("OpenIDConnectProtocol");
    }

    [Fact]
    public void All_ShouldContainAllProtocols()
    {
        // Act
        var all = AuthenticationProtocols.All;

        // Assert
        all.ShouldNotBeNull();
        all.ShouldContain(AuthenticationProtocols.OAuth2Protocol);
        all.ShouldContain(AuthenticationProtocols.OpenIDConnectProtocol);
    }
}

public sealed class AuthenticationFlowsTests
{
    [Fact]
    public void AuthorizationCodeFlow_ShouldHaveCorrectProperties()
    {
        // Act
        var flow = AuthenticationFlows.AuthorizationCodeFlow;

        // Assert
        flow.ShouldNotBeNull();
        flow.Name.ShouldBe("AuthorizationCodeFlow");
    }

    [Fact]
    public void ClientCredentialsFlow_ShouldHaveCorrectProperties()
    {
        // Act
        var flow = AuthenticationFlows.ClientCredentialsFlow;

        // Assert
        flow.ShouldNotBeNull();
        flow.Name.ShouldBe("ClientCredentialsFlow");
    }

    [Fact]
    public void DeviceCodeFlow_ShouldHaveCorrectProperties()
    {
        // Act
        var flow = AuthenticationFlows.DeviceCodeFlow;

        // Assert
        flow.ShouldNotBeNull();
        flow.Name.ShouldBe("DeviceCodeFlow");
    }

    [Fact]
    public void ImplicitFlow_ShouldHaveCorrectProperties()
    {
        // Act
        var flow = AuthenticationFlows.ImplicitFlow;

        // Assert
        flow.ShouldNotBeNull();
        flow.Name.ShouldBe("ImplicitFlow");
    }

    [Fact]
    public void InteractiveFlow_ShouldHaveCorrectProperties()
    {
        // Act
        var flow = AuthenticationFlows.InteractiveFlow;

        // Assert
        flow.ShouldNotBeNull();
        flow.Name.ShouldBe("InteractiveFlow");
    }

    [Fact]
    public void All_ShouldContainAllFlows()
    {
        // Act
        var all = AuthenticationFlows.All;

        // Assert
        all.ShouldNotBeNull();
        all.ShouldContain(AuthenticationFlows.AuthorizationCodeFlow);
        all.ShouldContain(AuthenticationFlows.ClientCredentialsFlow);
        all.ShouldContain(AuthenticationFlows.DeviceCodeFlow);
        all.ShouldContain(AuthenticationFlows.ImplicitFlow);
        all.ShouldContain(AuthenticationFlows.InteractiveFlow);
    }
}

public sealed class TokenTypesTests
{
    [Fact]
    public void AccessToken_ShouldHaveCorrectProperties()
    {
        // Act
        var token = TokenTypes.AccessToken;

        // Assert
        token.ShouldNotBeNull();
        token.Name.ShouldBe("AccessToken");
    }

    [Fact]
    public void RefreshToken_ShouldHaveCorrectProperties()
    {
        // Act
        var token = TokenTypes.RefreshToken;

        // Assert
        token.ShouldNotBeNull();
        token.Name.ShouldBe("RefreshToken");
    }

    [Fact]
    public void IdToken_ShouldHaveCorrectProperties()
    {
        // Act
        var token = TokenTypes.IdToken;

        // Assert
        token.ShouldNotBeNull();
        token.Name.ShouldBe("IdToken");
    }

    [Fact]
    public void BearerToken_ShouldHaveCorrectProperties()
    {
        // Act
        var token = TokenTypes.BearerToken;

        // Assert
        token.ShouldNotBeNull();
        token.Name.ShouldBe("BearerToken");
    }

    [Fact]
    public void All_ShouldContainAllTokenTypes()
    {
        // Act
        var all = TokenTypes.All;

        // Assert
        all.ShouldNotBeNull();
        all.ShouldContain(TokenTypes.AccessToken);
        all.ShouldContain(TokenTypes.RefreshToken);
        all.ShouldContain(TokenTypes.IdToken);
        all.ShouldContain(TokenTypes.BearerToken);
    }
}
