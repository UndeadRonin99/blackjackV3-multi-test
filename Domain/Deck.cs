using BlackjackV3.Services;

namespace BlackjackV3.Domain;

/// <summary>
/// Represents a deck of 52 playing cards with shuffle capability.
/// </summary>
public class Deck
{
    private readonly List<Card> _cards = new();
    private int _currentIndex;

    /// <summary>
    /// Initializes a new standard 52-card deck.
    /// </summary>
    public Deck()
  {
        Reset();
    }

    /// <summary>
    /// Gets the number of cards remaining in the deck.
    /// </summary>
    public int RemainingCards => _cards.Count - _currentIndex;

    /// <summary>
    /// Resets the deck to a full 52-card deck.
    /// </summary>
    public void Reset()
    {
      _cards.Clear();
 _currentIndex = 0;

        foreach (Suit suit in Enum.GetValues<Suit>())
      {
  foreach (Rank rank in Enum.GetValues<Rank>())
     {
   _cards.Add(new Card(rank, suit));
 }
        }
    }

    /// <summary>
    /// Shuffles the deck using the Fisher-Yates algorithm.
    /// </summary>
    /// <param name="rng">The random number provider to use for shuffling.</param>
    public void Shuffle(IRandomProvider rng)
    {
        _currentIndex = 0;
    int n = _cards.Count;

        // Fisher-Yates shuffle
      for (int i = n - 1; i > 0; i--)
        {
   int j = rng.Next(i + 1);
       (_cards[i], _cards[j]) = (_cards[j], _cards[i]);
        }
    }

    /// <summary>
    /// Deals a card from the top of the deck.
    /// </summary>
    /// <returns>The next card in the deck.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the deck is empty.</exception>
    public Card Deal()
    {
        if (_currentIndex >= _cards.Count)
        {
            throw new InvalidOperationException("No cards remaining in deck");
        }

   return _cards[_currentIndex++];
    }

    /// <summary>
    /// Determines if the deck needs to be reshuffled (fewer than 15 cards remaining).
    /// </summary>
    public bool NeedsReshuffle => RemainingCards < 15;
}
