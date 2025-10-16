using System;
using FractalDataWorks.Abstractions;

namespace FractalDataWorks.Abstractions.Tests;

public class IGenericCommandTests
{
    private class TestCommand : IGenericCommand
    {
        public Guid CommandId { get; init; }
        public DateTime CreatedAt { get; init; }
        public string CommandType { get; init; } = string.Empty;
    }

    [Fact]
    public void IGenericCommand_CanBeImplemented()
    {
        var commandId = Guid.NewGuid();
        var createdAt = DateTime.UtcNow;
        var command = new TestCommand
        {
            CommandId = commandId,
            CreatedAt = createdAt,
            CommandType = "TestCommand"
        };

        command.CommandId.ShouldBe(commandId);
        command.CreatedAt.ShouldBe(createdAt);
        command.CommandType.ShouldBe("TestCommand");
    }

    [Fact]
    public void IGenericCommand_CommandId_CanBeSet()
    {
        var expectedId = Guid.NewGuid();
        var command = new TestCommand { CommandId = expectedId };

        command.CommandId.ShouldBe(expectedId);
    }

    [Fact]
    public void IGenericCommand_CreatedAt_CanBeSet()
    {
        var expectedTime = new DateTime(2025, 10, 12, 10, 30, 0, DateTimeKind.Utc);
        var command = new TestCommand { CreatedAt = expectedTime };

        command.CreatedAt.ShouldBe(expectedTime);
    }

    [Fact]
    public void IGenericCommand_CommandType_CanBeSet()
    {
        var command = new TestCommand { CommandType = "CustomCommand" };

        command.CommandType.ShouldBe("CustomCommand");
    }

    [Fact]
    public void IGenericCommand_AllProperties_CanBeSetTogether()
    {
        var id = Guid.NewGuid();
        var time = DateTime.UtcNow;
        var type = "ComplexCommand";

        var command = new TestCommand
        {
            CommandId = id,
            CreatedAt = time,
            CommandType = type
        };

        command.CommandId.ShouldBe(id);
        command.CreatedAt.ShouldBe(time);
        command.CommandType.ShouldBe(type);
    }
}
