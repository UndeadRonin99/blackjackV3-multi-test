using BlackjackV3.Domain;

namespace BlackjackV3.Models;

/// <summary>
/// View model for multiplayer game state.
/// </summary>
public class MultiplayerGameViewModel
{
    /// <summary>
    /// Gets or sets the table ID.
    /// </summary>
    public string TableId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the player's name.
    /// </summary>
    public string PlayerName { get; set; } = string.Empty;

    /// <summary>
/// Gets or sets the list of players at the table.
/// </summary>
public List<PlayerInfo> Players { get; set; } = new();

    /// <summary>
    /// Gets or sets whether the round is in progress.
    /// </summary>
    public bool IsRoundInProgress { get; set; }

    /// <summary>
    /// Gets or sets the maximum number of players.
    /// </summary>
    public int MaxPlayers { get; set; } = 5;
}

/// <summary>
/// Information about a player at the table.
/// </summary>
public class PlayerInfo
{
    /// <summary>
    /// Gets or sets the player's connection ID.
    /// </summary>
    public string ConnectionId { get; set; } = string.Empty;

  /// <summary>
    /// Gets or sets the player's name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the player's balance.
    /// </summary>
  public int Balance { get; set; }

    /// <summary>
    /// Gets or sets the player's current bet.
    /// </summary>
    public int CurrentBet { get; set; }

    /// <summary>
    /// Gets or sets whether the player is ready.
    /// </summary>
    public bool IsReady { get; set; }

    /// <summary>
    /// Gets or sets the player's hand cards.
    /// </summary>
public List<SimpleCardInfo> Hand { get; set; } = new();

    /// <summary>
    /// Gets or sets the player's hand total.
    /// </summary>
    public int HandTotal { get; set; }

    /// <summary>
    /// Gets or sets the player's game state.
    /// </summary>
    public string GameState { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the player's outcome.
    /// </summary>
    public string Outcome { get; set; } = string.Empty;

/// <summary>
    /// Gets or sets whether this is the current user.
    /// </summary>
    public bool IsCurrentUser { get; set; }
}

/// <summary>
/// Simple card information for serialization.
/// </summary>
public class SimpleCardInfo
{
    public string Rank { get; set; } = string.Empty;
    public string Suit { get; set; } = string.Empty;
 public string ImageFileName { get; set; } = string.Empty;
}

/// <summary>
/// DTO for game state updates sent via SignalR.
/// </summary>
public class GameStateUpdate
{
    /// <summary>
  /// Gets or sets the player's game snapshot.
    /// </summary>
    public GameSnapshot? PlayerSnapshot { get; set; }

    /// <summary>
    /// Gets or sets information about all players at the table.
    /// </summary>
    public List<PlayerInfo> AllPlayers { get; set; } = new();

    /// <summary>
  /// Gets or sets the dealer's cards.
    /// </summary>
    public List<SimpleCardInfo> DealerCards { get; set; } = new();

    /// <summary>
    /// Gets or sets the dealer's visible total.
    /// </summary>
 public int DealerTotal { get; set; }

    /// <summary>
    /// Gets or sets whether the dealer's hole card is hidden.
    /// </summary>
  public bool DealerHoleCardHidden { get; set; }

    /// <summary>
    /// Gets or sets whether the round is in progress.
    /// </summary>
  public bool IsRoundInProgress { get; set; }

    /// <summary>
    /// Gets or sets a message for the player.
    /// </summary>
    public string Message { get; set; } = string.Empty;
}
