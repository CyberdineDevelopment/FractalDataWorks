using Microsoft.Extensions.DependencyInjection;
using FractalDataWorks.Services.Abstractions;

namespace FractalDataWorks.Services.Tests;

/// <summary>
/// Tests for ServiceLifetimes and lifetime option classes for 100% coverage.
/// </summary>
public class ServiceLifetimesTests
{
    [Fact]
    public void Transient_ReturnsTransientLifetime()
    {
        var lifetime = ServiceLifetimes.Transient;

        lifetime.ShouldNotBeNull();
        lifetime.Name.ShouldBe("Transient");
        lifetime.Id.ShouldBe(1);
        lifetime.EnumValue.ShouldBe(ServiceLifetime.Transient);
        lifetime.Description.ShouldBe("New instance created each time");
    }

    [Fact]
    public void Scoped_ReturnsScopedLifetime()
    {
        var lifetime = ServiceLifetimes.Scoped;

        lifetime.ShouldNotBeNull();
        lifetime.Name.ShouldBe("Scoped");
        lifetime.Id.ShouldBe(2);
        lifetime.EnumValue.ShouldBe(ServiceLifetime.Scoped);
        lifetime.Description.ShouldBe("New instance per scope/request");
    }

    [Fact]
    public void Singleton_ReturnsSingletonLifetime()
    {
        var lifetime = ServiceLifetimes.Singleton;

        lifetime.ShouldNotBeNull();
        lifetime.Name.ShouldBe("Singleton");
        lifetime.Id.ShouldBe(3);
        lifetime.EnumValue.ShouldBe(ServiceLifetime.Singleton);
        lifetime.Description.ShouldBe("Single instance for application lifetime");
    }

    [Fact]
    public void ByName_WithTransient_ReturnsTransient()
    {
        var lifetime = ServiceLifetimes.ByName("transient");

        lifetime.ShouldBe(ServiceLifetimes.Transient);
    }

    [Fact]
    public void ByName_WithScoped_ReturnsScoped()
    {
        var lifetime = ServiceLifetimes.ByName("scoped");

        lifetime.ShouldBe(ServiceLifetimes.Scoped);
    }

    [Fact]
    public void ByName_WithSingleton_ReturnsSingleton()
    {
        var lifetime = ServiceLifetimes.ByName("singleton");

        lifetime.ShouldBe(ServiceLifetimes.Singleton);
    }

    [Fact]
    public void ByName_IsCaseInsensitive()
    {
        var transient1 = ServiceLifetimes.ByName("TRANSIENT");
        var transient2 = ServiceLifetimes.ByName("Transient");
        var transient3 = ServiceLifetimes.ByName("TrAnSiEnT");

        transient1.ShouldBe(ServiceLifetimes.Transient);
        transient2.ShouldBe(ServiceLifetimes.Transient);
        transient3.ShouldBe(ServiceLifetimes.Transient);
    }

    [Fact]
    public void ByName_WithNull_ReturnsNull()
    {
        var lifetime = ServiceLifetimes.ByName(null);

        lifetime.ShouldBeNull();
    }

    [Fact]
    public void ByName_WithEmpty_ReturnsNull()
    {
        var lifetime = ServiceLifetimes.ByName(string.Empty);

        lifetime.ShouldBeNull();
    }

    [Fact]
    public void ByName_WithWhitespace_ReturnsNull()
    {
        var lifetime = ServiceLifetimes.ByName("   ");

        lifetime.ShouldBeNull();
    }

    [Fact]
    public void ByName_WithUnknownName_ReturnsNull()
    {
        var lifetime = ServiceLifetimes.ByName("Unknown");

        lifetime.ShouldBeNull();
    }

    [Fact]
    public void Transient_IsSingleton()
    {
        var lifetime1 = ServiceLifetimes.Transient;
        var lifetime2 = ServiceLifetimes.Transient;

        lifetime1.ShouldBeSameAs(lifetime2);
    }

    [Fact]
    public void Scoped_IsSingleton()
    {
        var lifetime1 = ServiceLifetimes.Scoped;
        var lifetime2 = ServiceLifetimes.Scoped;

        lifetime1.ShouldBeSameAs(lifetime2);
    }

    [Fact]
    public void Singleton_IsSingleton()
    {
        var lifetime1 = ServiceLifetimes.Singleton;
        var lifetime2 = ServiceLifetimes.Singleton;

        lifetime1.ShouldBeSameAs(lifetime2);
    }
}
