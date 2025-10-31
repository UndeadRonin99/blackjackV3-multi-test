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
    /// Gets the player's individual game instance.
    /// </summary>
    public BlackjackGame Game { get; init; } = null!;

    /// <summary>
    /// Gets or sets whether the player is ready to start the next round.
    /// </summary>
    public bool IsReady { get; set; }

    /// <summary>
    /// Gets or sets whether the player has completed their turn in the current round.
    /// </summary>
    public bool HasCompletedTurn { get; set; }

    /// <summary>
    /// Gets or sets the timestamp of the player's last activity.
    /// </summary>
    public DateTime LastActivity { get; set; } = DateTime.UtcNow;
}
