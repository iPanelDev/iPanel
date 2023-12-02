using iPanel.Core.Interaction;
using Xunit;

namespace iPanel.Tests;

public class ParseInputTests
{
    [Theory]
    [InlineData("a", "a")]
    [InlineData("a b", "a", "b")]
    [InlineData("a  b", "a", "b")]
    [InlineData("a b ", "a", "b")]
    [InlineData("a  b ", "a", "b")]
    public void ShouldSplitBySpace(string input, params string[] expected)
    {
        Assert.True(InputReader.Parse(input, out var result));
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("test \"")]
    [InlineData("test \"aa")]
    [InlineData("test aa\"")]
    public void ShouldReturnFalseWhenColonsAreNotClosed(string input)
    {
        Assert.False(InputReader.Parse(input, out _));
    }

    [Theory]
    [InlineData("test \"\"", "test")]
    [InlineData("test \"abc\"", "test", "abc")]
    [InlineData("test \"a b c\"", "test", "a b c")]
    public void ShouldReturnTrueWhenColonsAreClosed(string input, params string[] expected)
    {
        Assert.True(InputReader.Parse(input, out var result));
        Assert.Equal(expected, result);
    }
}
