using Todo.Api;
using Xunit;

namespace Todo.UnitTests;

public class TodoValidatorTests
{
    [Theory]
    [InlineData("Buy milk")]
    [InlineData("  padded  ")]
    [InlineData("x")]
    public void IsValidTitle_AcceptsReasonableTitles(string title)
    {
        Assert.True(TodoValidator.IsValidTitle(title));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void IsValidTitle_RejectsBlankTitles(string? title)
    {
        Assert.False(TodoValidator.IsValidTitle(title));
    }

    [Fact]
    public void IsValidTitle_RejectsOverlongTitles()
    {
        var tooLong = new string('a', TodoValidator.MaxTitleLength + 1);
        Assert.False(TodoValidator.IsValidTitle(tooLong));
    }

    [Fact]
    public void IsValidTitle_AcceptsTitleAtMaxLength()
    {
        var exact = new string('a', TodoValidator.MaxTitleLength);
        Assert.True(TodoValidator.IsValidTitle(exact));
    }

    [Fact]
    public void Normalize_TrimsSurroundingWhitespace()
    {
        Assert.Equal("Buy milk", TodoValidator.Normalize("   Buy milk   "));
    }

    [Fact]
    public void Normalize_ThrowsOnInvalidTitle()
    {
        Assert.Throws<ArgumentException>(() => TodoValidator.Normalize("   "));
    }
}
