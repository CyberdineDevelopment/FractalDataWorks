using FractalDataWorks.Configuration;

namespace FractalDataWorks.Configuration.Tests;

public class ConfigurationChangeTypesTests
{
    [Fact]
    public void Added_ShouldHaveCorrectProperties()
    {
        // Arrange & Act
        var added = new Added();

        // Assert
        added.Id.ShouldBe(1);
        added.Name.ShouldBe("Added");
        added.Description.ShouldBe("A configuration was added");
    }

    [Fact]
    public void Updated_ShouldHaveCorrectProperties()
    {
        // Arrange & Act
        var updated = new Updated();

        // Assert
        updated.Id.ShouldBe(2);
        updated.Name.ShouldBe("Updated");
        updated.Description.ShouldBe("A configuration was updated");
    }

    [Fact]
    public void Deleted_ShouldHaveCorrectProperties()
    {
        // Arrange & Act
        var deleted = new Deleted();

        // Assert
        deleted.Id.ShouldBe(3);
        deleted.Name.ShouldBe("Deleted");
        deleted.Description.ShouldBe("A configuration was deleted");
    }

    [Fact]
    public void Reloaded_ShouldHaveCorrectProperties()
    {
        // Arrange & Act
        var reloaded = new Reloaded();

        // Assert
        reloaded.Id.ShouldBe(4);
        reloaded.Name.ShouldBe("Reloaded");
        reloaded.Description.ShouldBe("The configuration source was reloaded");
    }
}

public class ConfigurationSourceTypesTests
{
    [Fact]
    public void FileConfigurationSource_ShouldHaveCorrectProperties()
    {
        // Arrange & Act
        var fileSource = new FileConfigurationSource();

        // Assert
        fileSource.Id.ShouldBe(1);
        fileSource.Name.ShouldBe("FileConfigurationSource");
        fileSource.Description.ShouldBe("Configuration from a file");
    }

    [Fact]
    public void Environment_ShouldHaveCorrectProperties()
    {
        // Arrange & Act
        var env = new FractalDataWorks.Configuration.Environment();

        // Assert
        env.Id.ShouldBe(2);
        env.Name.ShouldBe("Environment");
        env.Description.ShouldBe("Configuration from environment variables");
    }

    [Fact]
    public void Database_ShouldHaveCorrectProperties()
    {
        // Arrange & Act
        var db = new Database();

        // Assert
        db.Id.ShouldBe(3);
        db.Name.ShouldBe("Database");
        db.Description.ShouldBe("Configuration from a database");
    }

    [Fact]
    public void Remote_ShouldHaveCorrectProperties()
    {
        // Arrange & Act
        var remote = new Remote();

        // Assert
        remote.Id.ShouldBe(4);
        remote.Name.ShouldBe("Remote");
        remote.Description.ShouldBe("Configuration from a remote service");
    }

    [Fact]
    public void Memory_ShouldHaveCorrectProperties()
    {
        // Arrange & Act
        var memory = new Memory();

        // Assert
        memory.Id.ShouldBe(5);
        memory.Name.ShouldBe("Memory");
        memory.Description.ShouldBe("Configuration from memory/cache");
    }

    [Fact]
    public void CommandLine_ShouldHaveCorrectProperties()
    {
        // Arrange & Act
        var cmdLine = new CommandLine();

        // Assert
        cmdLine.Id.ShouldBe(6);
        cmdLine.Name.ShouldBe("CommandLine");
        cmdLine.Description.ShouldBe("Configuration from command line arguments");
    }

    [Fact]
    public void Custom_ShouldHaveCorrectProperties()
    {
        // Arrange & Act
        var custom = new Custom();

        // Assert
        custom.Id.ShouldBe(7);
        custom.Name.ShouldBe("Custom");
        custom.Description.ShouldBe("Custom configuration source");
    }
}
