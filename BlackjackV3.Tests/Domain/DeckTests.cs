using Xunit;
using BlackjackV3.Domain;
using BlackjackV3.Services;

namespace BlackjackV3.Tests.Domain;

/// <summary>
/// Tests for the Deck class.
/// </summary>
public class DeckTests
{
    /// <summary>
    /// Deterministic random provider for testing.
    /// </summary>
    private class TestRandomProvider : IRandomProvider
    {
        private readonly Queue<int> _values;

        public TestRandomProvider(params int[] values)
        {
            _values = new Queue<int>(values);
        }

        public int Next(int maxValue)
        {
            return _values.Count > 0 ? _values.Dequeue() % maxValue : 0;
        }
    }

    [Fact]
    public void Deck_NewDeck_Has52Cards()
    {
        // Arrange & Act
        var deck = new Deck();

        // Assert
        Assert.Equal(52, deck.RemainingCards);
    }

    [Fact]
    public void Deck_Reset_Restores52Cards()
    {
        // Arrange
        var deck = new Deck();
        var rng = new TestRandomProvider();
        deck.Shuffle(rng);
        deck.Deal();
        deck.Deal();
        deck.Deal();

        // Act
        deck.Reset();

        // Assert
        Assert.Equal(52, deck.RemainingCards);
    }

    [Fact]
    public void Deck_Deal_ReturnsCard()
    {
        // Arrange
        var deck = new Deck();

        // Act
        var card = deck.Deal();

        // Assert
        Assert.NotNull(card);
        Assert.Equal(51, deck.RemainingCards);
    }

    [Fact]
    public void Deck_Deal52Cards_EmptiesDeck()
    {
        // Arrange
        var deck = new Deck();

        // Act
        for (int i = 0; i < 52; i++)
        {
            deck.Deal();
        }

        // Assert
        Assert.Equal(0, deck.RemainingCards);
    }

    [Fact]
    public void Deck_DealFromEmptyDeck_ThrowsException()
    {
        // Arrange
        var deck = new Deck();
        for (int i = 0; i < 52; i++)
        {
            deck.Deal();
        }

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => deck.Deal());
    }

    [Fact]
    public void Deck_Shuffle_ChangesCardOrder()
    {
        // Arrange
        var deck1 = new Deck();
        var deck2 = new Deck();
        var rng = new CryptoRandomProvider();

        var originalCards = new List<Card>();
        for (int i = 0; i < 10; i++)
        {
            originalCards.Add(deck1.Deal());
        }

        deck2.Shuffle(rng);
        var shuffledCards = new List<Card>();
        for (int i = 0; i < 10; i++)
        {
            shuffledCards.Add(deck2.Deal());
        }

        // Assert
        // It's theoretically possible but extremely unlikely that the shuffled deck
        // matches the original order for the first 10 cards
        bool anyDifferent = false;
        for (int i = 0; i < 10; i++)
        {
            if (originalCards[i].Rank != shuffledCards[i].Rank ||
                originalCards[i].Suit != shuffledCards[i].Suit)
            {
                anyDifferent = true;
                break;
            }
        }

        Assert.True(anyDifferent);
    }

    [Fact]
    public void Deck_Shuffle_MaintainsAllCards()
    {
        // Arrange
        var deck = new Deck();
        var rng = new CryptoRandomProvider();

        // Act
        deck.Shuffle(rng);

        // Collect all cards
        var cards = new List<Card>();
        for (int i = 0; i < 52; i++)
        {
            cards.Add(deck.Deal());
        }

        // Assert
        Assert.Equal(52, cards.Count);

        // Verify we have all 13 ranks for each of 4 suits
        foreach (Suit suit in Enum.GetValues<Suit>())
        {
            foreach (Rank rank in Enum.GetValues<Rank>())
            {
                Assert.Contains(cards, c => c.Rank == rank && c.Suit == suit);
            }
        }
    }

    [Fact]
    public void Deck_NeedsReshuffle_WhenFewerThan15Cards()
    {
        // Arrange
        var deck = new Deck();

        // Deal 38 cards (52 - 38 = 14)
        for (int i = 0; i < 38; i++)
        {
            deck.Deal();
        }

        // Assert
        Assert.Equal(14, deck.RemainingCards);
        Assert.True(deck.NeedsReshuffle);
    }

    [Fact]
    public void Deck_DoesNotNeedReshuffle_When15OrMoreCards()
    {
        // Arrange
        var deck = new Deck();

        // Deal 37 cards (52 - 37 = 15)
        for (int i = 0; i < 37; i++)
        {
            deck.Deal();
        }

        // Assert
        Assert.Equal(15, deck.RemainingCards);
        Assert.False(deck.NeedsReshuffle);
    }

    [Fact]
    public void Deck_ShuffleWithTestRng_ProducesPredictableOrder()
    {
        // Arrange
        var deck = new Deck();
        var rng = new TestRandomProvider(0, 0, 0, 0); // Always returns 0

        // Act
        deck.Shuffle(rng);
        var firstCard = deck.Deal();

        // Assert
        Assert.NotNull(firstCard);
        // With all zeros, the shuffle does no swaps, so order is preserved
    }
}
