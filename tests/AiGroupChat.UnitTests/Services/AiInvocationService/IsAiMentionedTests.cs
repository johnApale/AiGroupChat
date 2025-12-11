namespace AiGroupChat.UnitTests.Services.AiInvocationService;

public class IsAiMentionedTests : AiInvocationServiceTestBase
{
    [Fact]
    public void WithAiMentionAtStart_ReturnsTrue()
    {
        // Arrange
        string content = "@ai how are you";

        // Act
        bool result = AiInvocationService.IsAiMentioned(content);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void WithAiMentionUppercase_ReturnsTrue()
    {
        // Arrange
        string content = "@AI help me";

        // Act
        bool result = AiInvocationService.IsAiMentioned(content);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void WithAiMentionMixedCase_ReturnsTrue()
    {
        // Arrange
        string content = "@Ai what is this";

        // Act
        bool result = AiInvocationService.IsAiMentioned(content);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void WithAiMentionOnly_ReturnsTrue()
    {
        // Arrange
        string content = "@ai";

        // Act
        bool result = AiInvocationService.IsAiMentioned(content);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void WithAiMentionOnlyUppercase_ReturnsTrue()
    {
        // Arrange
        string content = "@AI";

        // Act
        bool result = AiInvocationService.IsAiMentioned(content);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void WithAiMentionWithLeadingSpaces_ReturnsTrue()
    {
        // Arrange
        string content = "  @ai test";

        // Act
        bool result = AiInvocationService.IsAiMentioned(content);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void WithAiMentionWithLeadingTabs_ReturnsTrue()
    {
        // Arrange
        string content = "\t@ai test";

        // Act
        bool result = AiInvocationService.IsAiMentioned(content);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void WithAiMentionWithMultipleLeadingWhitespace_ReturnsTrue()
    {
        // Arrange
        string content = "   \t  @ai test";

        // Act
        bool result = AiInvocationService.IsAiMentioned(content);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void WithAiMentionInMiddle_ReturnsFalse()
    {
        // Arrange
        string content = "hello @ai there";

        // Act
        bool result = AiInvocationService.IsAiMentioned(content);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void WithAiMentionAtEnd_ReturnsFalse()
    {
        // Arrange
        string content = "hello @ai";

        // Act
        bool result = AiInvocationService.IsAiMentioned(content);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void WithSimilarPrefix_ReturnsFalse()
    {
        // Arrange
        string content = "@aiden hello";

        // Act
        bool result = AiInvocationService.IsAiMentioned(content);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void WithAiWithoutSpace_ReturnsFalse()
    {
        // Arrange
        string content = "@aihelp";

        // Act
        bool result = AiInvocationService.IsAiMentioned(content);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void WithNoMention_ReturnsFalse()
    {
        // Arrange
        string content = "hello world";

        // Act
        bool result = AiInvocationService.IsAiMentioned(content);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void WithEmptyString_ReturnsFalse()
    {
        // Arrange
        string content = "";

        // Act
        bool result = AiInvocationService.IsAiMentioned(content);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void WithWhitespaceOnly_ReturnsFalse()
    {
        // Arrange
        string content = "   ";

        // Act
        bool result = AiInvocationService.IsAiMentioned(content);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void WithAtSymbolOnly_ReturnsFalse()
    {
        // Arrange
        string content = "@";

        // Act
        bool result = AiInvocationService.IsAiMentioned(content);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void WithPartialAiMention_ReturnsFalse()
    {
        // Arrange
        string content = "@a";

        // Act
        bool result = AiInvocationService.IsAiMentioned(content);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void WithDifferentMention_ReturnsFalse()
    {
        // Arrange
        string content = "@bob hello";

        // Act
        bool result = AiInvocationService.IsAiMentioned(content);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void WithAiInText_ReturnsFalse()
    {
        // Arrange
        string content = "ai is great";

        // Act
        bool result = AiInvocationService.IsAiMentioned(content);

        // Assert
        Assert.False(result);
    }
}