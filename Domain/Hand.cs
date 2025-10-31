namespace BlackjackV3.Domain;

/// <summary>
/// Represents a hand of cards in blackjack with automatic score calculation.
/// </summary>
public class Hand
{
    private readonly List<Card> _cards = new();

    /// <summary>
    /// Gets the cards in this hand.
    /// </summary>
    public IReadOnlyList<Card> Cards => _cards.AsReadOnly();

    /// <summary>
    /// Adds a card to this hand.
    /// </summary>
  /// <param name="card">The card to add.</param>
    public void AddCard(Card card)
    {
        _cards.Add(card);
    }

    /// <summary>
    /// Gets the best score for this hand, treating aces as 1 or 11 to avoid busting when possible.
    /// </summary>
    public int Score
    {
        get
        {
   int total = 0;
         int aceCount = 0;

            foreach (var card in _cards)
 {
          total += card.Value;
    if (card.Rank == Rank.Ace)
     {
        aceCount++;
       }
     }

            // Adjust aces from 11 to 1 if needed to avoid busting
  while (total > 21 && aceCount > 0)
            {
      total -= 10; // Convert one ace from 11 to 1
      aceCount--;
            }

      return total;
   }
    }

    /// <summary>
    /// Determines if this hand is a blackjack (21 with exactly two cards).
    /// </summary>
    public bool IsBlackjack => _cards.Count == 2 && Score == 21;

    /// <summary>
    /// Determines if this hand is busted (score over 21).
    /// </summary>
    public bool IsBusted => Score > 21;

    /// <summary>
    /// Determines if this hand is a soft hand (contains an ace counted as 11).
    /// </summary>
    public bool IsSoft
    {
get
   {
          if (!_cards.Any(c => c.Rank == Rank.Ace))
      return false;

   int total = 0;
     foreach (var card in _cards)
            {
        total += card.Value;
      }

            // If total would bust but current score doesn't, an ace is being counted as 1
            return total > 21 && Score <= 21 && Score != total - (_cards.Count(c => c.Rank == Rank.Ace) * 10);
        }
    }

    /// <summary>
    /// Clears all cards from this hand.
    /// </summary>
    public void Clear()
    {
        _cards.Clear();
    }

    /// <summary>
    /// Gets a string representation of this hand.
  /// </summary>
    public override string ToString() => $"Hand: {string.Join(", ", _cards)} (Score: {Score})";
}
