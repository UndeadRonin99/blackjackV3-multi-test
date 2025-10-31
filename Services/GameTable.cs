using System.Collections.Concurrent;

namespace BlackjackV3.Services;

/// <summary>
/// Represents a multiplayer blackjack table with multiple players.
/// </summary>
public class GameTable
{
    private readonly ConcurrentDictionary<string, PlayerState> _players = new();
    private readonly object _lock = new();

    /// <summary>
    /// Gets the table identifier.
    /// </summary>
  public string TableId { get; init; } = string.Empty;

    /// <summary>
    /// Gets the maximum number of players allowed at this table.
    /// </summary>
    public int MaxPlayers { get; init; } = 5;

    /// <summary>
    /// Gets the table's creation timestamp.
    /// </summary>
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;

    /// <summary>
  /// Gets or sets whether the table is currently in a round.
    /// </summary>
 public bool IsRoundInProgress { get; set; }

    /// <summary>
    /// Gets all players at the table.
    /// </summary>
    public IReadOnlyDictionary<string, PlayerState> Players => _players;

    /// <summary>
    /// Attempts to add a player to the table.
    /// </summary>
    /// <param name="player">The player to add.</param>
    /// <returns>True if the player was added; false if the table is full.</returns>
    public bool TryAddPlayer(PlayerState player)
    {
        lock (_lock)
        {
  if (_players.Count >= MaxPlayers)
     return false;

    return _players.TryAdd(player.ConnectionId, player);
        }
    }

    /// <summary>
    /// Removes a player from the table.
    /// </summary>
    /// <param name="connectionId">The connection ID of the player to remove.</param>
    /// <returns>True if the player was removed; otherwise false.</returns>
    public bool RemovePlayer(string connectionId)
    {
        return _players.TryRemove(connectionId, out _);
    }

    /// <summary>
    /// Gets a player by connection ID.
    /// </summary>
    /// <param name="connectionId">The connection ID.</param>
    /// <param name="player">The player state if found.</param>
    /// <returns>True if the player was found; otherwise false.</returns>
    public bool TryGetPlayer(string connectionId, out PlayerState? player)
    {
        return _players.TryGetValue(connectionId, out player);
    }

    /// <summary>
    /// Checks if all players are ready to start a new round.
    /// </summary>
    /// <returns>True if all players are ready; otherwise false.</returns>
    public bool AreAllPlayersReady()
    {
        if (_players.IsEmpty)
          return false;

   return _players.Values.All(p => p.IsReady);
    }

    /// <summary>
    /// Checks if all players have completed their turn.
  /// </summary>
    /// <returns>True if all players have completed their turn; otherwise false.</returns>
    public bool HaveAllPlayersCompletedTurn()
    {
        if (_players.IsEmpty)
     return false;

        return _players.Values.All(p => p.HasCompletedTurn || p.Game.State == Domain.GameState.RoundOver);
    }

    /// <summary>
    /// Resets all players' ready state for a new round.
    /// </summary>
    public void ResetReadyStates()
    {
   foreach (var player in _players.Values)
        {
            player.IsReady = false;
         player.HasCompletedTurn = false;
      }
    }

  /// <summary>
    /// Gets the count of active players.
    /// </summary>
    public int PlayerCount => _players.Count;

    /// <summary>
    /// Determines if the table is empty.
 /// </summary>
    public bool IsEmpty => _players.IsEmpty;
}
