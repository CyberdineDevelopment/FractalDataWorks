using FractalDataWorks.Messages;
using FractalDataWorks.Results;

namespace FractalDataWorks.Results.Tests;

/// <summary>
/// Tests for the non-generic GenericResult class.
/// </summary>
public class GenericResultTests
{
    #region Success Tests

    [Fact]
    public void Success_WithNoMessage_CreatesSuccessResult()
    {
        // Act
        var result = GenericResult.Success();

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.IsFailure.ShouldBeFalse();
        result.Error.ShouldBeFalse();
        result.IsEmpty.ShouldBeTrue();
        result.CurrentMessage.ShouldBeNull();
        result.Messages.ShouldBeEmpty();
    }

    [Fact]
    public void Success_WithStringMessage_CreatesSuccessResultWithMessage()
    {
        // Arrange
        const string message = "Operation completed successfully";

        // Act
        var result = GenericResult.Success(message);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.IsFailure.ShouldBeFalse();
        result.IsEmpty.ShouldBeFalse();
        result.CurrentMessage.ShouldBe(message);
        result.Messages.Count.ShouldBe(1);
        result.Messages[0].Message.ShouldBe(message);
    }

    [Fact]
    public void Success_WithIGenericMessage_CreatesSuccessResultWithMessage()
    {
        // Arrange
        var message = new RecMessage("Success message");

        // Act
        var result = GenericResult.Success(message);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.IsEmpty.ShouldBeFalse();
        result.CurrentMessage.ShouldBe(message.Message);
        result.Messages.Count.ShouldBe(1);
        result.Messages[0].ShouldBe(message);
    }

    [Fact]
    public void Success_WithGenericMessage_CreatesSuccessResultWithMessage()
    {
        // Arrange
        var message = new RecMessage("Generic success");

        // Act
        var result = GenericResult.Success<RecMessage>(message);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.CurrentMessage.ShouldBe(message.Message);
        result.Messages[0].ShouldBe(message);
    }

    [Fact]
    public void Success_WithMessageCollection_CreatesSuccessResultWithMultipleMessages()
    {
        // Arrange
        var messages = new List<IGenericMessage>
        {
            new RecMessage("Message 1"),
            new RecMessage("Message 2"),
            new RecMessage("Message 3")
        };

        // Act
        var result = GenericResult.Success(messages);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.IsEmpty.ShouldBeFalse();
        result.Messages.Count.ShouldBe(3);
        result.CurrentMessage.ShouldBe("Message 3"); // LIFO - last message
        result.Messages[0].Message.ShouldBe("Message 1");
        result.Messages[1].Message.ShouldBe("Message 2");
        result.Messages[2].Message.ShouldBe("Message 3");
    }

    [Fact]
    public void Success_WithParamsMessages_CreatesSuccessResultWithMultipleMessages()
    {
        // Arrange
        var msg1 = new RecMessage("Message 1");
        var msg2 = new RecMessage("Message 2");

        // Act
        var result = GenericResult.Success(msg1, msg2);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Messages.Count.ShouldBe(2);
        result.CurrentMessage.ShouldBe("Message 2");
    }

    #endregion

    #region Failure Tests

    [Fact]
    public void Failure_WithStringMessage_CreatesFailureResultWithMessage()
    {
        // Arrange
        const string message = "Operation failed";

        // Act
        var result = GenericResult.Failure(message);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBeTrue();
        result.IsEmpty.ShouldBeFalse();
        result.CurrentMessage.ShouldBe(message);
        result.Messages.Count.ShouldBe(1);
    }

    [Fact]
    public void Failure_WithIGenericMessage_CreatesFailureResultWithMessage()
    {
        // Arrange
        var message = new RecMessage("Failure message");

        // Act
        var result = GenericResult.Failure(message);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.IsFailure.ShouldBeTrue();
        result.CurrentMessage.ShouldBe(message.Message);
        result.Messages[0].ShouldBe(message);
    }

    [Fact]
    public void Failure_WithGenericMessage_CreatesFailureResultWithMessage()
    {
        // Arrange
        var message = new RecMessage("Generic failure");

        // Act
        var result = GenericResult.Failure<RecMessage>(message);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.CurrentMessage.ShouldBe(message.Message);
    }

    [Fact]
    public void Failure_WithMessageCollection_CreatesFailureResultWithMultipleMessages()
    {
        // Arrange
        var messages = new List<IGenericMessage>
        {
            new RecMessage("Error 1"),
            new RecMessage("Error 2")
        };

        // Act
        var result = GenericResult.Failure(messages);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Messages.Count.ShouldBe(2);
        result.CurrentMessage.ShouldBe("Error 2");
    }

    [Fact]
    public void Failure_WithParamsMessages_CreatesFailureResultWithMultipleMessages()
    {
        // Arrange
        var msg1 = new RecMessage("Error 1");
        var msg2 = new RecMessage("Error 2");
        var msg3 = new RecMessage("Error 3");

        // Act
        var result = GenericResult.Failure(msg1, msg2, msg3);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Messages.Count.ShouldBe(3);
        result.CurrentMessage.ShouldBe("Error 3");
    }

    #endregion

    #region Property Tests

    [Fact]
    public void IsFailure_IsOppositeOfIsSuccess()
    {
        // Arrange & Act
        var success = GenericResult.Success();
        var failure = GenericResult.Failure("Error");

        // Assert
        success.IsFailure.ShouldBe(!success.IsSuccess);
        failure.IsFailure.ShouldBe(!failure.IsSuccess);
    }

    [Fact]
    public void Error_IsOppositeOfIsSuccess()
    {
        // Arrange & Act
        var success = GenericResult.Success();
        var failure = GenericResult.Failure("Error");

        // Assert
        success.Error.ShouldBe(!success.IsSuccess);
        failure.Error.ShouldBe(!failure.IsSuccess);
    }

    [Fact]
    public void CurrentMessage_ReturnsLastMessage_LIFO()
    {
        // Arrange
        var messages = new[]
        {
            new RecMessage("First"),
            new RecMessage("Second"),
            new RecMessage("Third")
        };

        // Act
        var result = GenericResult.Success(messages);

        // Assert
        result.CurrentMessage.ShouldBe("Third");
    }

    [Fact]
    public void CurrentMessage_ReturnsNull_WhenNoMessages()
    {
        // Act
        var result = GenericResult.Success();

        // Assert
        result.CurrentMessage.ShouldBeNull();
    }

    [Fact]
    public void Messages_ReturnsReadOnlyCollection()
    {
        // Arrange
        var result = GenericResult.Success("Test");

        // Act & Assert
        result.Messages.ShouldBeAssignableTo<IReadOnlyList<IGenericMessage>>();
    }

    #endregion
}
