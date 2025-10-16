using FractalDataWorks.Collections.Attributes;
using FractalDataWorks.Services.Authentication.Abstractions;
using FractalDataWorks.Services.Authentication.Abstractions.Methods;
using FractalDataWorks.Services.Authentication.Abstractions.Security;
using Shouldly;

namespace FractalDataWorks.Services.Authentication.Abstractions.Tests;

// Test implementations
[TypeOption(typeof(AuthenticationMethods), "TestMethod1")]
internal sealed class TestAuthenticationMethod1 : AuthenticationMethodBase
{
    public TestAuthenticationMethod1() : base(1, "TestMethod1", false, true, false, "Bearer", 50) { }
}

[TypeOption(typeof(AuthenticationMethods), "TestMethod2")]
internal sealed class TestAuthenticationMethod2 : AuthenticationMethodBase
{
    public TestAuthenticationMethod2() : base(2, "TestMethod2", true, false, true, "ApiKey", 75) { }
}

[TypeOption(typeof(AuthenticationProtocols), "TestProtocol1")]
internal sealed class TestProtocol1 : AuthenticationProtocolBase
{
    public TestProtocol1() : base(1, "TestProtocol1", "1.0", true, true) { }
}

[TypeOption(typeof(AuthenticationProtocols), "TestProtocol2")]
internal sealed class TestProtocol2 : AuthenticationProtocolBase
{
    public TestProtocol2() : base(2, "TestProtocol2", "2.0", false, false) { }
}

[TypeOption(typeof(AuthenticationFlows), "TestFlow1")]
internal sealed class TestFlow1 : AuthenticationFlowBase
{
    public TestFlow1() : base(1, "TestFlow1", true, true, false) { }
}

[TypeOption(typeof(AuthenticationFlows), "TestFlow2")]
internal sealed class TestFlow2 : AuthenticationFlowBase
{
    public TestFlow2() : base(2, "TestFlow2", false, false, true) { }
}

[TypeOption(typeof(TokenTypes), "TestToken1")]
internal sealed class TestToken1 : TokenTypeBase
{
    public TestToken1() : base(1, "TestToken1", "JWT", false, true, 3600) { }
}

[TypeOption(typeof(TokenTypes), "TestToken2")]
internal sealed class TestToken2 : TokenTypeBase
{
    public TestToken2() : base(2, "TestToken2", "Opaque", true, false, null) { }
}

public sealed class AuthenticationMethodsTests
{
    [Fact]
    public void AuthenticationMethodBase_ShouldHaveCorrectProperties()
    {
        // Arrange & Act
        var method = new TestAuthenticationMethod1();

        // Assert
        method.Id.ShouldBe(1);
        method.Name.ShouldBe("TestMethod1");
        method.RequiresUserInteraction.ShouldBeFalse();
        method.SupportsTokenRefresh.ShouldBeTrue();
        method.SupportsMultiTenant.ShouldBeFalse();
        method.AuthenticationScheme.ShouldBe("Bearer");
        method.Priority.ShouldBe(50);
    }

    [Fact]
    public void AuthenticationMethods_ShouldContainTestMethods()
    {
        // Act
        var all = AuthenticationMethods.All();

        // Assert
        all.ShouldNotBeEmpty();
        all.ShouldContain(m => m.Name == "TestMethod1");
        all.ShouldContain(m => m.Name == "TestMethod2");
    }

    [Fact]
    public void AuthenticationMethods_Name_ShouldReturnCorrectMethod()
    {
        // Act
        var method = AuthenticationMethods.Name("TestMethod1");

        // Assert
        method.ShouldNotBeNull();
        method.Name.ShouldBe("TestMethod1");
        method.Id.ShouldBe(1);
    }

    [Fact]
    public void AuthenticationMethods_Id_ShouldReturnCorrectMethod()
    {
        // Act
        var method = AuthenticationMethods.Id(2);

        // Assert
        method.ShouldNotBeNull();
        method.Name.ShouldBe("TestMethod2");
    }
}

public sealed class AuthenticationProtocolsTests
{
    [Fact]
    public void AuthenticationProtocolBase_ShouldHaveCorrectProperties()
    {
        // Arrange & Act
        var protocol = new TestProtocol1();

        // Assert
        protocol.Id.ShouldBe(1);
        protocol.Name.ShouldBe("TestProtocol1");
        protocol.Version.ShouldBe("1.0");
        protocol.RequiresSecureTransport.ShouldBeTrue();
        protocol.SupportsTokens.ShouldBeTrue();
    }

    [Fact]
    public void AuthenticationProtocols_ShouldContainTestProtocols()
    {
        // Act
        var all = AuthenticationProtocols.All();

        // Assert
        all.ShouldNotBeEmpty();
        all.ShouldContain(p => p.Name == "TestProtocol1");
        all.ShouldContain(p => p.Name == "TestProtocol2");
    }

    [Fact]
    public void AuthenticationProtocols_Name_ShouldReturnCorrectProtocol()
    {
        // Act
        var protocol = AuthenticationProtocols.Name("TestProtocol2");

        // Assert
        protocol.ShouldNotBeNull();
        protocol.Version.ShouldBe("2.0");
        protocol.RequiresSecureTransport.ShouldBeFalse();
    }
}

public sealed class AuthenticationFlowsTests
{
    [Fact]
    public void AuthenticationFlowBase_ShouldHaveCorrectProperties()
    {
        // Arrange & Act
        var flow = new TestFlow1();

        // Assert
        flow.Id.ShouldBe(1);
        flow.Name.ShouldBe("TestFlow1");
        flow.RequiresUserInteraction.ShouldBeTrue();
        flow.SupportsRefreshTokens.ShouldBeTrue();
        flow.IsServerToServer.ShouldBeFalse();
    }

    [Fact]
    public void AuthenticationFlows_ShouldContainTestFlows()
    {
        // Act
        var all = AuthenticationFlows.All();

        // Assert
        all.ShouldNotBeEmpty();
        all.ShouldContain(f => f.Name == "TestFlow1");
        all.ShouldContain(f => f.Name == "TestFlow2");
    }

    [Fact]
    public void AuthenticationFlows_Id_ShouldReturnCorrectFlow()
    {
        // Act
        var flow = AuthenticationFlows.Id(2);

        // Assert
        flow.ShouldNotBeNull();
        flow.Name.ShouldBe("TestFlow2");
        flow.IsServerToServer.ShouldBeTrue();
    }
}

public sealed class TokenTypesTests
{
    [Fact]
    public void TokenTypeBase_ShouldHaveCorrectProperties()
    {
        // Arrange & Act
        var token = new TestToken1();

        // Assert
        token.Id.ShouldBe(1);
        token.Name.ShouldBe("TestToken1");
        token.Format.ShouldBe("JWT");
        token.CanBeRefreshed.ShouldBeFalse();
        token.ContainsUserIdentity.ShouldBeTrue();
        token.TypicalLifetimeSeconds.ShouldBe(3600);
    }

    [Fact]
    public void TokenTypes_ShouldContainTestTokens()
    {
        // Act
        var all = TokenTypes.All();

        // Assert
        all.ShouldNotBeEmpty();
        all.ShouldContain(t => t.Name == "TestToken1");
        all.ShouldContain(t => t.Name == "TestToken2");
    }

    [Fact]
    public void TokenTypes_Name_ShouldReturnCorrectToken()
    {
        // Act
        var token = TokenTypes.Name("TestToken2");

        // Assert
        token.ShouldNotBeNull();
        token.Format.ShouldBe("Opaque");
        token.CanBeRefreshed.ShouldBeTrue();
        token.TypicalLifetimeSeconds.ShouldBeNull();
    }
}
