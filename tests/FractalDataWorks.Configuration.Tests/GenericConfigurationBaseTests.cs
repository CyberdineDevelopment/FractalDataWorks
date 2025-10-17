using FractalDataWorks;
using FluentValidation.Results;
using FractalDataWorks.Configuration.Abstractions;

namespace FractalDataWorks.Configuration.Tests;

public class GenericConfigurationBaseTests
{
    private class TestConfiguration : GenericConfigurationBase
    {
        public override string SectionName => "TestSection";

        public bool InitializeDefaultsCalled { get; private set; }
        public bool OnConfigurationLoadedCalled { get; private set; }

        public void CallInitializeDefaults() => InitializeDefaults();
        public void CallOnConfigurationLoaded() => OnConfigurationLoaded();

        protected override void InitializeDefaults()
        {
            InitializeDefaultsCalled = true;
            base.InitializeDefaults();
        }

        protected override void OnConfigurationLoaded()
        {
            OnConfigurationLoadedCalled = true;
            base.OnConfigurationLoaded();
        }
    }

    [Fact]
    public void Constructor_ShouldInitializeDefaultValues()
    {
        // Act
        var config = new TestConfiguration();

        // Assert
        config.Id.ShouldBe(0);
        config.Name.ShouldBe(string.Empty);
        config.IsEnabled.ShouldBeTrue();
        config.SectionName.ShouldBe("TestSection");
    }

    [Fact]
    public void Id_CanBeSetAndRetrieved()
    {
        // Arrange
        var config = new TestConfiguration();

        // Act
        config.Id = 123;

        // Assert
        config.Id.ShouldBe(123);
    }

    [Fact]
    public void Name_CanBeSetAndRetrieved()
    {
        // Arrange
        var config = new TestConfiguration();

        // Act
        config.Name = "Test Name";

        // Assert
        config.Name.ShouldBe("Test Name");
    }

    [Fact]
    public void IsEnabled_CanBeSetAndRetrieved()
    {
        // Arrange
        var config = new TestConfiguration();

        // Act
        config.IsEnabled = false;

        // Assert
        config.IsEnabled.ShouldBeFalse();
    }

    [Fact]
    public void Validate_DefaultImplementation_ReturnsSuccessWithValidResult()
    {
        // Arrange
        var config = new TestConfiguration();

        // Act
        var result = config.Validate();

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.IsValid.ShouldBeTrue();
        result.Value.Errors.ShouldBeEmpty();
    }

    [Fact]
    public void InitializeDefaults_DefaultImplementation_DoesNothing()
    {
        // Arrange
        var config = new TestConfiguration();

        // Act
        config.CallInitializeDefaults();

        // Assert
        config.InitializeDefaultsCalled.ShouldBeTrue();
    }

    [Fact]
    public void OnConfigurationLoaded_DefaultImplementation_DoesNothing()
    {
        // Arrange
        var config = new TestConfiguration();

        // Act
        config.CallOnConfigurationLoaded();

        // Assert
        config.OnConfigurationLoadedCalled.ShouldBeTrue();
    }
}
