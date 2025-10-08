using System;
using System.Collections.Generic;
using Moq;
using FractalDataWorks.Services;
using FractalDataWorks.Services.Abstractions;

namespace FractalDataWorks.Services.Tests;

/// <summary>
/// Tests for FactoryRegistration covering all pathways for 100% coverage.
/// </summary>
public class FactoryRegistrationTests
{
    [Fact]
    public void Constructor_InitializesWithDefaultValues()
    {
        var registration = new FactoryRegistration();

        registration.Factory.ShouldBeNull();
        registration.Lifetime.ShouldBe(ServiceLifetimes.Scoped);
        registration.TypeName.ShouldBe(string.Empty);
        registration.Priority.ShouldBe(50);
        registration.Metadata.ShouldNotBeNull();
        registration.Metadata.Count.ShouldBe(0);
    }

    [Fact]
    public void Factory_CanBeSetAndRetrieved()
    {
        var registration = new FactoryRegistration();
        var factory = new Mock<IServiceFactory>().Object;

        registration.Factory = factory;

        registration.Factory.ShouldBe(factory);
    }

    [Fact]
    public void Lifetime_CanBeSetAndRetrieved()
    {
        var registration = new FactoryRegistration();
        var lifetime = ServiceLifetimes.Singleton;

        registration.Lifetime = lifetime;

        registration.Lifetime.ShouldBe(lifetime);
    }

    [Fact]
    public void TypeName_CanBeSetAndRetrieved()
    {
        var registration = new FactoryRegistration();
        var typeName = "TestService";

        registration.TypeName = typeName;

        registration.TypeName.ShouldBe(typeName);
    }

    [Fact]
    public void Priority_CanBeSetAndRetrieved()
    {
        var registration = new FactoryRegistration();
        var priority = 100;

        registration.Priority = priority;

        registration.Priority.ShouldBe(priority);
    }

    [Fact]
    public void Metadata_CanBeSetAndRetrieved()
    {
        var registration = new FactoryRegistration();
        var metadata = new Dictionary<string, object> { ["key"] = "value" };

        registration.Metadata = metadata;

        registration.Metadata.ShouldBe(metadata);
        registration.Metadata["key"].ShouldBe("value");
    }

    [Fact]
    public void Metadata_UsesOrdinalStringComparer()
    {
        var registration = new FactoryRegistration();

        registration.Metadata.Comparer.ShouldBe(StringComparer.Ordinal);
    }

    [Fact]
    public void AllProperties_CanBeInitializedViaObjectInitializer()
    {
        var factory = new Mock<IServiceFactory>().Object;
        var lifetime = ServiceLifetimes.Transient;
        var metadata = new Dictionary<string, object> { ["test"] = 123 };

        var registration = new FactoryRegistration
        {
            Factory = factory,
            Lifetime = lifetime,
            TypeName = "TestType",
            Priority = 75,
            Metadata = metadata
        };

        registration.Factory.ShouldBe(factory);
        registration.Lifetime.ShouldBe(lifetime);
        registration.TypeName.ShouldBe("TestType");
        registration.Priority.ShouldBe(75);
        registration.Metadata.ShouldBe(metadata);
    }

    [Fact]
    public void Lifetime_DefaultsToScoped()
    {
        var registration = new FactoryRegistration();

        registration.Lifetime.ShouldBe(ServiceLifetimes.Scoped);
    }

    [Fact]
    public void Priority_DefaultsTo50()
    {
        var registration = new FactoryRegistration();

        registration.Priority.ShouldBe(50);
    }
}
