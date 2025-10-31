namespace BlackjackV3.Domain;

/// <summary>
/// Immutable snapshot of the current game state for rendering.
/// </summary>
public class GameSnapshot
{
    /// <summary>
    /// Gets the player's cards.
 /// </summary>
    public IReadOnlyList<Card> PlayerCards { get; init; } = Array.Empty<Card>();

    /// <summary>
    /// Gets the dealer's cards.
    /// </summary>
    public IReadOnlyList<Card> DealerCards { get; init; } = Array.Empty<Card>();

    /// <summary>
    /// Gets whether the dealer's second card should be hidden.
    /// </summary>
 public bool DealerHoleCardHidden { get; init; }

    /// <summary>
    /// Gets the player's hand total.
    /// </summary>
    public int PlayerTotal { get; init; }

    /// <summary>
    /// Gets the dealer's visible total.
    /// </summary>
    public int DealerTotal { get; init; }

    /// <summary>
/// Gets whether the player can hit.
    /// </summary>
    public bool CanHit { get; init; }

    /// <summary>
    /// Gets whether the player can stand.
    /// </summary>
    public bool CanStand { get; init; }

    /// <summary>
    /// Gets whether the player can double down.
    /// </summary>
    public bool CanDouble { get; init; }

    /// <summary>
  /// Gets the player's current balance.
    /// </summary>
    public int Balance { get; init; }

    /// <summary>
    /// Gets the current bet amount.
    /// </summary>
    public int Bet { get; init; }

    /// <summary>
    /// Gets the current game state.
    /// </summary>
    public GameState State { get; init; }

    /// <summary>
    /// Gets the game outcome.
    /// </summary>
    public GameOutcome Outcome { get; init; }

    /// <summary>
 /// Gets a message describing the current game state or outcome.
    /// </summary>
    public string Message { get; init; } = string.Empty;

    /// <summary>
    /// Gets whether a new round can be started.
    /// </summary>
    public bool CanRestart { get; init; }
}
