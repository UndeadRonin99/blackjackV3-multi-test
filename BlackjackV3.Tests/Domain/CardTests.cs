using Xunit;
using BlackjackV3.Domain;

namespace BlackjackV3.Tests.Domain;

/// <summary>
/// Tests for the Card class.
/// </summary>
public class CardTests
{
    [Fact]
    public void Card_AceValue_Returns11()
    {
        // Arrange & Act
        var card = new Card(Rank.Ace, Suit.Hearts);

        // Assert
        Assert.Equal(11, card.Value);
    }

    [Theory]
    [InlineData(Rank.King)]
    [InlineData(Rank.Queen)]
    [InlineData(Rank.Jack)]
    public void Card_FaceCardValue_Returns10(Rank rank)
    {
        // Arrange & Act
        var card = new Card(rank, Suit.Spades);

        // Assert
        Assert.Equal(10, card.Value);
    }

    [Theory]
    [InlineData(Rank.Two, 2)]
    [InlineData(Rank.Three, 3)]
    [InlineData(Rank.Four, 4)]
    [InlineData(Rank.Five, 5)]
    [InlineData(Rank.Six, 6)]
    [InlineData(Rank.Seven, 7)]
    [InlineData(Rank.Eight, 8)]
    [InlineData(Rank.Nine, 9)]
    [InlineData(Rank.Ten, 10)]
    public void Card_NumberCardValue_ReturnsCorrectValue(Rank rank, int expectedValue)
    {
        // Arrange & Act
        var card = new Card(rank, Suit.Diamonds);

        // Assert
        Assert.Equal(expectedValue, card.Value);
    }

    [Fact]
    public void Card_GetImageFileName_AceOfHearts_ReturnsCorrectFilename()
    {
        // Arrange
        var card = new Card(Rank.Ace, Suit.Hearts);

        // Act
        var filename = card.GetImageFileName();

        // Assert
        Assert.Equal("ace_of_hearts.svg", filename);
    }

    [Fact]
    public void Card_GetImageFileName_KingOfSpades_ReturnsCorrectFilename()
    {
        // Arrange
        var card = new Card(Rank.King, Suit.Spades);

        // Act
        var filename = card.GetImageFileName();

        // Assert
        Assert.Equal("king_of_spades.svg", filename);
    }

    [Fact]
    public void Card_GetImageFileName_TwoOfClubs_ReturnsCorrectFilename()
    {
        // Arrange
        var card = new Card(Rank.Two, Suit.Clubs);

        // Act
        var filename = card.GetImageFileName();

        // Assert
        Assert.Equal("2_of_clubs.svg", filename);
    }

    [Fact]
    public void Card_ToString_ReturnsFormattedString()
    {
        // Arrange
        var card = new Card(Rank.Queen, Suit.Diamonds);

        // Act
        var result = card.ToString();

        // Assert
        Assert.Equal("Queen of Diamonds", result);
    }
}
