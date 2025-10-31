using Microsoft.AspNetCore.SignalR;
using BlackjackV3.Services;
using BlackjackV3.Domain;
using BlackjackV3.Models;

namespace BlackjackV3.Hubs;

/// <summary>
/// SignalR hub for multiplayer blackjack games.
/// </summary>
public class GameHub : Hub
{
private readonly ITableManager _tableManager;
    private readonly IRandomProvider _randomProvider;
    private readonly ILogger<GameHub> _logger;

    public GameHub(ITableManager tableManager, IRandomProvider randomProvider, ILogger<GameHub> logger)
    {
  _tableManager = tableManager;
        _randomProvider = randomProvider;
        _logger = logger;
    }

    /// <summary>
    /// Allows a player to join a game table.
    /// </summary>
    /// <param name="tableId">The table identifier.</param>
    /// <param name="playerName">The player's display name.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task<bool> JoinTable(string tableId, string playerName)
    {
     try
        {
         var table = _tableManager.GetOrCreateTable(tableId);

            // Check if table is full
      if (table.PlayerCount >= table.MaxPlayers)
       {
              await Clients.Caller.SendAsync("Error", "Table is full");
                return false;
    }

    // Create player state
 var player = new PlayerState
 {
      ConnectionId = Context.ConnectionId,
      PlayerName = playerName,
    Game = new BlackjackGame(_randomProvider),
        IsReady = false,
                HasCompletedTurn = false
            };

    if (!table.TryAddPlayer(player))
            {
         await Clients.Caller.SendAsync("Error", "Could not join table");
    return false;
            }

            await Groups.AddToGroupAsync(Context.ConnectionId, tableId);

            _logger.LogInformation("Player {PlayerName} ({ConnectionId}) joined table {TableId}",
  playerName, Context.ConnectionId, tableId);

 // Notify all players
     await BroadcastTableState(tableId);

            return true;
        }
  catch (Exception ex)
        {
            _logger.LogError(ex, "Error joining table {TableId}", tableId);
    await Clients.Caller.SendAsync("Error", "Failed to join table");
            return false;
        }
    }

  /// <summary>
/// Allows a player to place a bet and mark themselves as ready.
    /// </summary>
    /// <param name="tableId">The table identifier.</param>
    /// <param name="betAmount">The bet amount.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task PlaceBet(string tableId, int betAmount)
    {
        try
        {
            if (!_tableManager.TryGetTable(tableId, out var table) || table == null)
            {
  await Clients.Caller.SendAsync("Error", "Table not found");
          return;
            }

        if (!table.TryGetPlayer(Context.ConnectionId, out var player) || player == null)
     {
     await Clients.Caller.SendAsync("Error", "Player not found at table");
     return;
   }

        // Validate and start round for this player
     try
            {
        player.Game.StartRound(betAmount);
                player.IsReady = true;
        player.LastActivity = DateTime.UtcNow;

    _logger.LogInformation("Player {PlayerName} placed bet {Bet} at table {TableId}",
       player.PlayerName, betAmount, tableId);

                // Send update to this player
                await SendPlayerUpdate(tableId, player);

          // Notify all players about the bet
                await Clients.Group(tableId).SendAsync("PlayerPlacedBet",
   player.PlayerName, betAmount);

     // Check if all players are ready to start
            if (table.AreAllPlayersReady() && !table.IsRoundInProgress)
      {
   table.IsRoundInProgress = true;
       await Clients.Group(tableId).SendAsync("RoundStarted");
                    await BroadcastTableState(tableId);
      }
         else
                {
        await BroadcastTableState(tableId);
  }
          }
       catch (Exception ex)
       {
                await Clients.Caller.SendAsync("Error", ex.Message);
            }
  }
        catch (Exception ex)
     {
      _logger.LogError(ex, "Error placing bet at table {TableId}", tableId);
       await Clients.Caller.SendAsync("Error", "Failed to place bet");
     }
    }

    /// <summary>
    /// Allows a player to perform a game action (hit, stand, double).
    /// </summary>
    /// <param name="tableId">The table identifier.</param>
    /// <param name="action">The action to perform (Hit, Stand, Double).</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task PerformAction(string tableId, string action)
    {
        try
      {
            if (!_tableManager.TryGetTable(tableId, out var table) || table == null)
       {
                await Clients.Caller.SendAsync("Error", "Table not found");
       return;
        }

            if (!table.TryGetPlayer(Context.ConnectionId, out var player) || player == null)
            {
   await Clients.Caller.SendAsync("Error", "Player not found at table");
       return;
      }

      try
            {
       switch (action.ToLower())
    {
 case "hit":
              player.Game.PlayerHit();
       break;
       case "stand":
player.Game.PlayerStand();
        player.HasCompletedTurn = true;
    break;
            case "double":
           player.Game.PlayerDouble();
       player.HasCompletedTurn = true;
  break;
    default:
        await Clients.Caller.SendAsync("Error", "Invalid action");
      return;
      }

                player.LastActivity = DateTime.UtcNow;

          _logger.LogInformation("Player {PlayerName} performed action {Action} at table {TableId}",
            player.PlayerName, action, tableId);

// If player busted or stood, mark turn as complete
   if (player.Game.State == GameState.RoundOver)
         {
    player.HasCompletedTurn = true;
           }

         // Send update to all players
      await BroadcastTableState(tableId);

              // Check if all players have completed their turn
     if (table.HaveAllPlayersCompletedTurn())
            {
   await EndRound(tableId);
     }
         }
            catch (Exception ex)
      {
         await Clients.Caller.SendAsync("Error", ex.Message);
      }
        }
        catch (Exception ex)
 {
       _logger.LogError(ex, "Error performing action at table {TableId}", tableId);
   await Clients.Caller.SendAsync("Error", "Failed to perform action");
  }
    }

    /// <summary>
    /// Allows a player to ready up for the next round.
    /// </summary>
    /// <param name="tableId">The table identifier.</param>
  /// <returns>A task representing the asynchronous operation.</returns>
    public async Task ReadyForNextRound(string tableId)
    {
   try
        {
       if (!_tableManager.TryGetTable(tableId, out var table) || table == null)
      {
  await Clients.Caller.SendAsync("Error", "Table not found");
   return;
            }

      if (!table.TryGetPlayer(Context.ConnectionId, out var player) || player == null)
            {
     await Clients.Caller.SendAsync("Error", "Player not found at table");
       return;
            }

     if (player.Game.State == GameState.RoundOver)
   {
     player.Game.Restart();
     player.IsReady = false;
          player.HasCompletedTurn = false;
       }

 await BroadcastTableState(tableId);
 }
        catch (Exception ex)
     {
    _logger.LogError(ex, "Error readying for next round at table {TableId}", tableId);
  }
    }

    /// <summary>
    /// Called when a client disconnects.
    /// </summary>
    /// <param name="exception">The exception that caused the disconnect, if any.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        try
        {
        // Find all tables this player is in and remove them
       foreach (var tableId in _tableManager.GetActiveTableIds())
            {
              if (_tableManager.TryGetTable(tableId, out var table) && table != null)
        {
        if (table.TryGetPlayer(Context.ConnectionId, out var player) && player != null)
              {
     table.RemovePlayer(Context.ConnectionId);

    _logger.LogInformation("Player {PlayerName} disconnected from table {TableId}",
      player.PlayerName, tableId);

        await Clients.Group(tableId).SendAsync("PlayerLeft", player.PlayerName);
                  await BroadcastTableState(tableId);

        _tableManager.RemoveTableIfEmpty(tableId);
  }
                }
       }
        }
        catch (Exception ex)
        {
    _logger.LogError(ex, "Error handling disconnect for connection {ConnectionId}", Context.ConnectionId);
        }

        await base.OnDisconnectedAsync(exception);
    }

    /// <summary>
    /// Ends the current round and broadcasts final results.
  /// </summary>
  private async Task EndRound(string tableId)
    {
        if (!_tableManager.TryGetTable(tableId, out var table) || table == null)
    return;

        table.IsRoundInProgress = false;

        _logger.LogInformation("Round ended at table {TableId}", tableId);

  await Clients.Group(tableId).SendAsync("RoundEnded");
        await BroadcastTableState(tableId);
  }

    /// <summary>
    /// Sends a game state update to a specific player.
    /// </summary>
    private async Task SendPlayerUpdate(string tableId, PlayerState player)
  {
    if (!_tableManager.TryGetTable(tableId, out var table) || table == null)
      return;

      var snapshot = player.Game.ToSnapshot();
        var update = CreateGameStateUpdate(table, player, snapshot);

     await Clients.Client(player.ConnectionId).SendAsync("GameStateUpdated", update);
    }

    /// <summary>
    /// Broadcasts the complete table state to all players.
    /// </summary>
    private async Task BroadcastTableState(string tableId)
    {
        if (!_tableManager.TryGetTable(tableId, out var table) || table == null)
        return;

        foreach (var player in table.Players.Values)
     {
      var snapshot = player.Game.ToSnapshot();
            var update = CreateGameStateUpdate(table, player, snapshot);

            await Clients.Client(player.ConnectionId).SendAsync("GameStateUpdated", update);
        }
    }

    /// <summary>
    /// Creates a game state update for a player.
    /// </summary>
    private GameStateUpdate CreateGameStateUpdate(GameTable table, PlayerState currentPlayer, GameSnapshot snapshot)
    {
     // Get dealer cards from any player's game (they all share the same dealer)
    var dealerCards = snapshot.DealerCards
  .Select(c => new SimpleCardInfo
       {
  Rank = c.Rank.ToString(),
   Suit = c.Suit.ToString(),
         ImageFileName = c.GetImageFileName()
    })
        .ToList();

    // Create player info for all players
    var allPlayers = table.Players.Values.Select(p =>
{
     var pSnapshot = p.Game.ToSnapshot();
        return new PlayerInfo
    {
    ConnectionId = p.ConnectionId,
  Name = p.PlayerName,
         Balance = pSnapshot.Balance,
     CurrentBet = pSnapshot.Bet,
  IsReady = p.IsReady,
     Hand = pSnapshot.PlayerCards
    .Select(c => new SimpleCardInfo
       {
         Rank = c.Rank.ToString(),
  Suit = c.Suit.ToString(),
      ImageFileName = c.GetImageFileName()
     })
   .ToList(),
   HandTotal = pSnapshot.PlayerTotal,
    GameState = pSnapshot.State.ToString(),
       Outcome = pSnapshot.Outcome.ToString(),
    IsCurrentUser = p.ConnectionId == currentPlayer.ConnectionId
      };
        }).ToList();

   return new GameStateUpdate
    {
    PlayerSnapshot = snapshot,
      AllPlayers = allPlayers,
 DealerCards = dealerCards,
      DealerTotal = snapshot.DealerTotal,
        DealerHoleCardHidden = snapshot.DealerHoleCardHidden,
      IsRoundInProgress = table.IsRoundInProgress,
 Message = snapshot.Message
   };
    }
}
