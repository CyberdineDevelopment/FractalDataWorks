using Microsoft.Extensions.DependencyInjection;
using FractalDataWorks.Services;

namespace FractalDataWorks.Services.Tests;

/// <summary>
/// Tests for ServiceRegistrationOptions covering all pathways for 100% coverage.
/// </summary>
public class ServiceRegistrationOptionsTests
{
    [Fact]
    public void Constructor_InitializesWithDefaultValues()
    {
        var options = new ServiceRegistrationOptions();

        options.Lifetime.ShouldBe(ServiceLifetime.Transient);
        options.RegisterAsPrimary.ShouldBeTrue();
        options.ConfigurationSection.ShouldBe(string.Empty);
    }

    [Fact]
    public void Lifetime_CanBeSetAndRetrieved()
    {
        var options = new ServiceRegistrationOptions();

        options.Lifetime = ServiceLifetime.Singleton;

        options.Lifetime.ShouldBe(ServiceLifetime.Singleton);
    }

    [Fact]
    public void RegisterAsPrimary_CanBeSetAndRetrieved()
    {
        var options = new ServiceRegistrationOptions();

        options.RegisterAsPrimary = false;

        options.RegisterAsPrimary.ShouldBeFalse();
    }

    [Fact]
    public void ConfigurationSection_CanBeSetAndRetrieved()
    {
        var options = new ServiceRegistrationOptions();

        options.ConfigurationSection = "TestSection";

        options.ConfigurationSection.ShouldBe("TestSection");
    }

    [Fact]
    public void AllProperties_CanBeInitializedViaObjectInitializer()
    {
        var options = new ServiceRegistrationOptions
        {
            Lifetime = ServiceLifetime.Scoped,
            RegisterAsPrimary = false,
            ConfigurationSection = "MySection"
        };

        options.Lifetime.ShouldBe(ServiceLifetime.Scoped);
        options.RegisterAsPrimary.ShouldBeFalse();
        options.ConfigurationSection.ShouldBe("MySection");
    }

    [Fact]
    public void Lifetime_DefaultsToTransient()
    {
        var options = new ServiceRegistrationOptions();

        options.Lifetime.ShouldBe(ServiceLifetime.Transient);
    }

    [Fact]
    public void RegisterAsPrimary_DefaultsToTrue()
    {
        var options = new ServiceRegistrationOptions();

        options.RegisterAsPrimary.ShouldBeTrue();
    }
}
