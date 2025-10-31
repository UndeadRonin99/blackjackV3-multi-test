using BlackjackV3.Domain;

namespace BlackjackV3.Services;

/// <summary>
/// Represents a player's state in a multiplayer game.
/// </summary>
public class PlayerState
{
    /// <summary>
    /// Gets the player's connection ID.
    /// </summary>
    public string ConnectionId { get; init; } = string.Empty;

    /// <summary>
    /// Gets the player's display name.
    /// </summary>
    public string PlayerName { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the player's hand.
    /// </summary>
    public Hand Hand { get; set; } = new Hand();

    /// <summary>
    /// Gets or sets the player's balance.
    /// </summary>
    public int Balance { get; set; } = 1000;

    /// <summary>
    /// Gets or sets the player's current bet.
    /// </summary>
    public int CurrentBet { get; set; }

    /// <summary>
    /// Gets or sets the player's game outcome.
    /// </summary>
    public GameOutcome Outcome { get; set; } = GameOutcome.None;

    /// <summary>
    /// Gets or sets the player's game state.
    /// </summary>
    public GameState State { get; set; } = GameState.WaitingForBet;

    /// <summary>
    /// Gets or sets whether the player has placed their bet and is ready to start the round.
    /// </summary>
    public bool IsReady { get; set; }

    /// <summary>
    /// Gets or sets whether the player has confirmed they're ready for the next round (clicked "Ready for Next Round" button).
    /// This is separate from IsReady which indicates they've placed a bet.
    /// </summary>
    public bool HasConfirmedNextRound { get; set; }

    /// <summary>
    /// Gets or sets whether the player has completed their turn in the current round.
    /// </summary>
    public bool HasCompletedTurn { get; set; }

    /// <summary>
    /// Gets or sets whether the player has taken any action this round.
    /// </summary>
    public bool HasActed { get; set; }

    /// <summary>
    /// Gets or sets the timestamp of the player's last activity.
    /// </summary>
    public DateTime LastActivity { get; set; } = DateTime.UtcNow;
}
