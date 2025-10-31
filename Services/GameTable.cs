using System.Collections.Concurrent;
using BlackjackV3.Domain;

namespace BlackjackV3.Services;

/// <summary>
/// Represents a multiplayer blackjack table with multiple players.
/// </summary>
public class GameTable
{
    private readonly ConcurrentDictionary<string, PlayerState> _players = new();
    private readonly List<string> _playerOrder = new(); // Track join order
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
    /// Gets or sets the shared dealer hand for all players.
    /// </summary>
    public Hand? DealerHand { get; set; }

    /// <summary>
    /// Gets or sets the shared deck for all players at the table.
    /// </summary>
    public Deck? SharedDeck { get; set; }

    /// <summary>
    /// Gets or sets the index of the current player whose turn it is.
    /// </summary>
    public int CurrentPlayerIndex { get; set; } = 0;

    /// <summary>
    /// Gets all players at the table.
    /// </summary>
    public IReadOnlyDictionary<string, PlayerState> Players => _players;

    /// <summary>
    /// Gets the ordered list of player connection IDs (join order).
    /// </summary>
    public IReadOnlyList<string> PlayerOrder => _playerOrder.AsReadOnly();

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

            if (_players.TryAdd(player.ConnectionId, player))
            {
                _playerOrder.Add(player.ConnectionId);
                return true;
            }

            return false;
        }
    }

    /// <summary>
    /// Removes a player from the table.
    /// </summary>
    /// <param name="connectionId">The connection ID of the player to remove.</param>
    /// <returns>True if the player was removed; otherwise false.</returns>
    public bool RemovePlayer(string connectionId)
    {
        lock (_lock)
        {
            _playerOrder.Remove(connectionId);
            return _players.TryRemove(connectionId, out _);
        }
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
    /// Checks if all players have confirmed they're ready for the next round (clicked "Ready for Next Round").
    /// </summary>
    /// <returns>True if all players have confirmed; otherwise false.</returns>
    public bool HaveAllPlayersConfirmedNextRound()
    {
        if (_players.IsEmpty)
            return false;

        return _players.Values.All(p => p.HasConfirmedNextRound);
    }

    /// <summary>
    /// Gets the count of players who have confirmed they're ready for the next round.
    /// </summary>
    /// <returns>The number of players who have confirmed.</returns>
    public int GetPlayersConfirmedNextRoundCount()
    {
        return _players.Values.Count(p => p.HasConfirmedNextRound);
    }

    /// <summary>
    /// Checks if all players have readied up for the next round (in WaitingForBet state).
    /// </summary>
    /// <returns>True if all players are in WaitingForBet state; otherwise false.</returns>
    public bool AreAllPlayersReadyForBetting()
    {
        if (_players.IsEmpty)
            return false;

        return _players.Values.All(p => p.State == GameState.WaitingForBet);
    }

    /// <summary>
    /// Gets the count of players who are ready for betting (in WaitingForBet state).
    /// </summary>
    /// <returns>The number of players ready for betting.</returns>
    public int GetPlayersReadyForBettingCount()
    {
        return _players.Values.Count(p => p.State == GameState.WaitingForBet);
    }

    /// <summary>
    /// Checks if all players have completed their turn.
    /// </summary>
    /// <returns>True if all players have completed their turn; otherwise false.</returns>
    public bool HaveAllPlayersCompletedTurn()
    {
        if (_players.IsEmpty)
            return false;

        return _players.Values.All(p => p.HasCompletedTurn || p.Hand.IsBusted || p.Hand.IsBlackjack);
    }

    /// <summary>
    /// Gets the connection ID of the current player whose turn it is.
    /// </summary>
    /// <returns>The connection ID, or null if no valid player.</returns>
    public string? GetCurrentPlayerConnectionId()
    {
        lock (_lock)
        {
            if (CurrentPlayerIndex >= 0 && CurrentPlayerIndex < _playerOrder.Count)
            {
                return _playerOrder[CurrentPlayerIndex];
            }
            return null;
        }
    }

    /// <summary>
    /// Advances to the next player's turn.
    /// </summary>
    /// <returns>The connection ID of the next player, or null if all players are done.</returns>
    public string? AdvanceToNextPlayer()
    {
        lock (_lock)
        {
            CurrentPlayerIndex++;

            // Skip players who are already done (blackjack or bust)
            while (CurrentPlayerIndex < _playerOrder.Count)
            {
                var connectionId = _playerOrder[CurrentPlayerIndex];
                if (_players.TryGetValue(connectionId, out var player))
                {
                    if (!player.HasCompletedTurn && !player.Hand.IsBlackjack && !player.Hand.IsBusted)
                    {
                        return connectionId;
                    }
                }
                CurrentPlayerIndex++;
            }

            return null; // All players done
        }
    }

    /// <summary>
    /// Resets all players' ready state for a new round.
    /// </summary>
    public void ResetReadyStates()
    {
        lock (_lock)
        {
            foreach (var player in _players.Values)
            {
                player.IsReady = false;
                player.HasCompletedTurn = false;
            }
            CurrentPlayerIndex = 0;
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
