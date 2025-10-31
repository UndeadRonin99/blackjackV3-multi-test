namespace BlackjackV3.Domain;

/// <summary>
/// Represents a single playing card with a rank and suit.
/// </summary>
public class Card
{
    /// <summary>
    /// Gets the rank of this card.
    /// </summary>
    public Rank Rank { get; }

    /// <summary>
    /// Gets the suit of this card.
    /// </summary>
    public Suit Suit { get; }

    /// <summary>
/// Initializes a new instance of the Card class.
    /// </summary>
    /// <param name="rank">The rank of the card.</param>
    /// <param name="suit">The suit of the card.</param>
    public Card(Rank rank, Suit suit)
    {
Rank = rank;
 Suit = suit;
    }

    /// <summary>
    /// Gets the blackjack value of this card (face cards = 10, Ace = 11 initially).
    /// </summary>
    public int Value => Rank switch
    {
        Rank.Ace => 11,
        Rank.King or Rank.Queen or Rank.Jack => 10,
        _ => (int)Rank
    };

    /// <summary>
    /// Gets a string representation of this card for display purposes.
    /// </summary>
    public override string ToString() => $"{Rank} of {Suit}";

    /// <summary>
    /// Gets the filename for the card image asset.
    /// </summary>
    /// <returns>The filename for this card's image (e.g., "ace_of_hearts.svg").</returns>
    public string GetImageFileName()
    {
        var rankName = Rank switch
        {
    Rank.Ace => "ace",
   Rank.King => "king",
       Rank.Queen => "queen",
            Rank.Jack => "jack",
   _ => ((int)Rank).ToString()
        };
        var suitName = Suit.ToString().ToLowerInvariant();
     return $"{rankName}_of_{suitName}.svg";
    }
}
