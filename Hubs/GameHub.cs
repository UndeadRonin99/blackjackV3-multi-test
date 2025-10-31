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

  private const int StartingBalance = 1000;
    private const int MinBet = 10;
    private const int MaxBet = 200;

  public GameHub(ITableManager tableManager, IRandomProvider randomProvider, ILogger<GameHub> logger)
    {
        _tableManager = tableManager;
_randomProvider = randomProvider;
      _logger = logger;
    }

    /// <summary>
    /// Allows a player to join a game table.
    /// </summary>
    public async Task<bool> JoinTable(string tableId, string playerName)
    {
        try
   {
  var table = _tableManager.GetOrCreateTable(tableId);

 if (table.PlayerCount >= table.MaxPlayers)
   {
 await Clients.Caller.SendAsync("Error", "Table is full");
        return false;
            }

    var player = new PlayerState
  {
    ConnectionId = Context.ConnectionId,
 PlayerName = playerName,
        Hand = new Hand(),
 Balance = StartingBalance,
State = GameState.WaitingForBet, // ? Start in WaitingForBet so they can bet immediately
         IsReady = false,
    HasConfirmedNextRound = false
       };

      if (!table.TryAddPlayer(player))
    {
        await Clients.Caller.SendAsync("Error", "Could not join table");
       return false;
  }

            await Groups.AddToGroupAsync(Context.ConnectionId, tableId);
        _logger.LogInformation("Player {PlayerName} joined table {TableId} (position {Position})", 
    playerName, tableId, table.PlayerOrder.Count);

     await BroadcastTableState(tableId);
     return true;
        }
 catch (Exception ex)
   {
            _logger.LogError(ex, "Error joining table");
   await Clients.Caller.SendAsync("Error", "Failed to join table");
     return false;
        }
    }

    /// <summary>
    /// Allows a player to place a bet and mark themselves as ready.
    /// </summary>
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

  // Check if player is in correct state to place bet
      if (player.State != GameState.WaitingForBet)
     {
     await Clients.Caller.SendAsync("Error", "Cannot place bet right now");
 return;
  }

    // Determine if this is the first round OR if this player just joined
  // First round: No one at the table has confirmed yet
            // Just joined: This player hasn't confirmed yet AND round is not in progress
    bool isFirstRound = !table.Players.Values.Any(p => p.HasConfirmedNextRound);
  bool justJoined = !player.HasConfirmedNextRound && !table.IsRoundInProgress;
        
      // Only enforce confirmation check if:
      // - NOT first round AND
            // - Player didn't just join (they should have clicked Ready after a previous round)
    if (!isFirstRound && !justJoined && !player.HasConfirmedNextRound)
    {
  _logger.LogWarning("Player {PlayerName} tried to bet without confirming next round", player.PlayerName);
    await Clients.Caller.SendAsync("Error", "Please click 'Ready for Next Round' first");
   return;
   }

      if (betAmount < MinBet || betAmount > MaxBet)
   {
    await Clients.Caller.SendAsync("Error", $"Bet must be between ${MinBet} and ${MaxBet}");
return;
      }

       if (betAmount > player.Balance)
 {
     await Clients.Caller.SendAsync("Error", "Insufficient balance");
    return;
  }

  player.CurrentBet = betAmount;
  player.Balance -= betAmount;
 player.IsReady = true;
  player.State = GameState.Dealing;

 // Auto-confirm if first round or just joined
            if (isFirstRound || justJoined)
     {
          player.HasConfirmedNextRound = true;
  _logger.LogInformation("Player {PlayerName} auto-confirmed (first round: {First}, just joined: {Joined})", 
        player.PlayerName, isFirstRound, justJoined);
  }

    _logger.LogInformation("Player {PlayerName} placed bet ${Bet}. Ready: {Ready}", 
        player.PlayerName, betAmount, player.IsReady);

       await Clients.Group(tableId).SendAsync("PlayerPlacedBet", player.PlayerName, betAmount);

   // Start round when all players have placed bets (all IsReady)
 if (table.AreAllPlayersReady() && !table.IsRoundInProgress)
      {
        _logger.LogInformation("All players have placed bets at table {TableId}, starting round", tableId);
  await StartRound(tableId);
}
    else
    {
      _logger.LogInformation("Waiting for more players to bet. Bet: {BetCount}/{TotalCount}", 
       table.Players.Values.Count(p => p.IsReady), table.PlayerCount);
await BroadcastTableState(tableId);
        }
        }
   catch (Exception ex)
  {
      _logger.LogError(ex, "Error placing bet");
await Clients.Caller.SendAsync("Error", ex.Message);
   }
    }

    /// <summary>
    /// Starts a new round for all players at the table.
    /// </summary>
    private async Task StartRound(string tableId)
    {
        if (!_tableManager.TryGetTable(tableId, out var table) || table == null)
       return;

    table.IsRoundInProgress = true;
        table.CurrentPlayerIndex = 0; // Start with first player

        // Initialize shared deck and shuffle
        table.SharedDeck = new Deck();
 table.SharedDeck.Shuffle(_randomProvider);

        // Initialize dealer hand
        table.DealerHand = new Hand();

  // Deal initial cards to all players
      foreach (var connectionId in table.PlayerOrder)
        {
            if (table.TryGetPlayer(connectionId, out var player) && player != null)
      {
                player.Hand.Clear();
       player.Hand.AddCard(table.SharedDeck.Deal());
    player.HasActed = false;
      player.HasCompletedTurn = false;
          }
        }

     // Dealer gets face-up card only (hole card dealt after all players finish)
    table.DealerHand.AddCard(table.SharedDeck.Deal());

     // Deal second card to all players
  foreach (var connectionId in table.PlayerOrder)
   {
   if (table.TryGetPlayer(connectionId, out var player) && player != null)
    {
     player.Hand.AddCard(table.SharedDeck.Deal());
 }
        }

        // Check for blackjacks and set initial states
        foreach (var connectionId in table.PlayerOrder)
        {
   if (table.TryGetPlayer(connectionId, out var player) && player != null)
     {
           if (player.Hand.IsBlackjack)
                {
          player.Outcome = GameOutcome.PlayerBlackjack; // Will resolve later
           player.State = GameState.RoundOver;
 player.HasCompletedTurn = true;
     _logger.LogInformation("Player {PlayerName} has Blackjack!", player.PlayerName);
   }
         else
      {
       player.State = GameState.Dealing; // Waiting for turn
                }
     }
     }

  await Clients.Group(tableId).SendAsync("RoundStarted");
        
      // Broadcast initial state showing all dealt cards
   await BroadcastTableState(tableId);

      // Set first non-blackjack player to PlayerTurn (this will broadcast again with updated state)
        await SetNextPlayerTurn(tableId);
    }

    /// <summary>
    /// Sets the next player's turn.
    /// </summary>
    private async Task SetNextPlayerTurn(string tableId)
    {
    if (!_tableManager.TryGetTable(tableId, out var table) || table == null)
 return;

        var currentConnectionId = table.GetCurrentPlayerConnectionId();
        
        if (currentConnectionId != null && table.TryGetPlayer(currentConnectionId, out var currentPlayer) && currentPlayer != null)
        {
        // Skip if already done (blackjack, bust, or already completed)
            if (!currentPlayer.HasCompletedTurn && !currentPlayer.Hand.IsBlackjack && !currentPlayer.Hand.IsBusted)
    {
           currentPlayer.State = GameState.PlayerTurn;
       
            // Check if player can actually do anything
bool canHit = currentPlayer.Hand.Score < 21 && !currentPlayer.Hand.IsBusted;
          bool canStand = true; // Can always stand
       bool canDouble = !currentPlayer.HasActed && currentPlayer.CurrentBet <= currentPlayer.Balance;
        
   // If player has 21 (but not blackjack), they should automatically stand
   if (currentPlayer.Hand.Score == 21)
                {
   _logger.LogInformation("Player {PlayerName} has 21, automatically standing.", currentPlayer.PlayerName);
        currentPlayer.State = GameState.RoundOver;
      currentPlayer.HasCompletedTurn = true;
       await BroadcastTableState(tableId);
   await SetNextPlayerTurn(tableId); // Recursively move to next player
         return;
        }
         
     _logger.LogInformation("It's now {PlayerName}'s turn (State set to PlayerTurn)", currentPlayer.PlayerName);
        await Clients.Client(currentConnectionId).SendAsync("YourTurn");
 await BroadcastTableState(tableId); // ? Broadcast AFTER setting PlayerTurn
    return;
   }
 else
         {
   // Current player is done but index hasn't advanced yet
        _logger.LogInformation("Player {PlayerName} is already done (Blackjack/Bust/Completed), advancing...", currentPlayer.PlayerName);
   }
        }

        // Current player is done or doesn't exist, advance to next
    var nextConnectionId = table.AdvanceToNextPlayer();
        
 if (nextConnectionId != null && table.TryGetPlayer(nextConnectionId, out var nextPlayer) && nextPlayer != null)
        {
  nextPlayer.State = GameState.PlayerTurn;
 
 // Check if next player can actually do anything
            if (nextPlayer.Hand.Score == 21)
    {
         _logger.LogInformation("Player {PlayerName} has 21, automatically standing.", nextPlayer.PlayerName);
           nextPlayer.State = GameState.RoundOver;
  nextPlayer.HasCompletedTurn = true;
      await BroadcastTableState(tableId);
     await SetNextPlayerTurn(tableId); // Recursively move to next player
         return;
 }
  
            _logger.LogInformation("It's now {PlayerName}'s turn (State set to PlayerTurn)", nextPlayer.PlayerName);
   await Clients.Client(nextConnectionId).SendAsync("YourTurn");
 await BroadcastTableState(tableId); // ? Broadcast AFTER setting PlayerTurn
        }
        else
        {
            // All players done, dealer plays
 _logger.LogInformation("All players have completed their turns, dealer playing...");
       await DealerPlay(tableId);
      }
    }

    /// <summary>
    /// Allows a player to perform a game action.
    /// </summary>
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
       await Clients.Caller.SendAsync("Error", "Player not found");
     return;
   }

    // Check if it's this player's turn
      var currentPlayerConnectionId = table.GetCurrentPlayerConnectionId();
 
        _logger.LogInformation("PerformAction: Current player index={Index}, ConnectionId={CurrentId}, Action requested by={RequestedBy}", 
    table.CurrentPlayerIndex, currentPlayerConnectionId, Context.ConnectionId);
        
    if (currentPlayerConnectionId != Context.ConnectionId)
         {
   _logger.LogWarning("Player {PlayerName} tried to act but it's not their turn!", player.PlayerName);
   await Clients.Caller.SendAsync("Error", "Not your turn!");
       return;
   }

            if (player.State != GameState.PlayerTurn)
 {
        _logger.LogWarning("Player {PlayerName} state is {State}, not PlayerTurn!", player.PlayerName, player.State);
  await Clients.Caller.SendAsync("Error", "Not your turn");
          return;
   }

         switch (action.ToLower())
         {
  case "hit":
          player.Hand.AddCard(table.SharedDeck!.Deal());
     player.HasActed = true;
      
    _logger.LogInformation("Player {PlayerName} hit. Hand: {Hand}, Score: {Score}", 
  player.PlayerName, string.Join(", ", player.Hand.Cards), player.Hand.Score);

      if (player.Hand.IsBusted)
        {
       player.Outcome = GameOutcome.PlayerBust;
  player.State = GameState.RoundOver;
   player.HasCompletedTurn = true;
         
  _logger.LogInformation("Player {PlayerName} busted! Advancing to next player.", player.PlayerName);
            
  await BroadcastTableState(tableId);
   await SetNextPlayerTurn(tableId);
      }
          else
            {
          await BroadcastTableState(tableId);
        }
    break;

             case "stand":
    player.State = GameState.RoundOver;
  player.HasCompletedTurn = true;
            
           _logger.LogInformation("Player {PlayerName} stands with {Score}. Advancing to next player.", 
           player.PlayerName, player.Hand.Score);

       await BroadcastTableState(tableId);
         await SetNextPlayerTurn(tableId);
  break;

    case "double":
       if (player.HasActed)
   {
       await Clients.Caller.SendAsync("Error", "Can only double on first action");
        return;
            }

   if (player.CurrentBet > player.Balance)
   {
     await Clients.Caller.SendAsync("Error", "Insufficient balance to double");
        return;
       }

           player.Balance -= player.CurrentBet;
           player.CurrentBet *= 2;
  player.Hand.AddCard(table.SharedDeck!.Deal());
 player.HasActed = true;
   player.State = GameState.RoundOver;
       player.HasCompletedTurn = true;

      if (player.Hand.IsBusted)
{
          player.Outcome = GameOutcome.PlayerBust;
  _logger.LogInformation("Player {PlayerName} doubled and busted with {Score}! Advancing to next player.", 
           player.PlayerName, player.Hand.Score);
    }
            else
    {
     _logger.LogInformation("Player {PlayerName} doubled to {Score}. Advancing to next player.", 
 player.PlayerName, player.Hand.Score);
      }
      
    await BroadcastTableState(tableId);
       await SetNextPlayerTurn(tableId);
         break;

        default:
  await Clients.Caller.SendAsync("Error", "Invalid action");
     return;
  }
     }
        catch (Exception ex)
        {
       _logger.LogError(ex, "Error performing action");
      await Clients.Caller.SendAsync("Error", "Failed to perform action");
        }
    }

    /// <summary>
    /// Dealer plays their hand.
    /// </summary>
  private async Task DealerPlay(string tableId)
    {
        if (!_tableManager.TryGetTable(tableId, out var table) || table == null || table.DealerHand == null)
         return;

        _logger.LogInformation("Dealer is playing at table {TableId}", tableId);

        // Deal dealer's hole card now
  table.DealerHand.AddCard(table.SharedDeck!.Deal());
        
        // Set all players who haven't finished to DealerTurn state (for UI)
        foreach (var connectionId in table.PlayerOrder)
        {
      if (table.TryGetPlayer(connectionId, out var player) && player != null)
            {
         // Only change state if player hasn't busted/finished already
    if (player.Outcome == GameOutcome.None || player.Outcome == GameOutcome.PlayerBlackjack)
         {
       player.State = GameState.DealerTurn;
        }
        // Players who already busted stay in RoundOver state
      }
        }

        await BroadcastTableState(tableId);
     await Task.Delay(1000); // Brief pause for dramatic effect

        // Check for dealer blackjack
        bool dealerBlackjack = table.DealerHand.IsBlackjack;

        if (dealerBlackjack)
        {
        _logger.LogInformation("Dealer has Blackjack!");
   }

        // Dealer hits to soft 17 (only if no blackjack or players still in play)
        while (!dealerBlackjack && (table.DealerHand.Score < 17 || (table.DealerHand.Score == 17 && table.DealerHand.IsSoft)))
        {
   table.DealerHand.AddCard(table.SharedDeck!.Deal());
            await BroadcastTableState(tableId);
     await Task.Delay(800); // Show each card being dealt
}

        // Resolve outcomes for all players
        foreach (var connectionId in table.PlayerOrder)
        {
   if (table.TryGetPlayer(connectionId, out var player) && player != null)
         {
  player.State = GameState.RoundOver;

                // Player already busted
        if (player.Outcome == GameOutcome.PlayerBust)
         {
         // Already lost, keep outcome
   continue;
                }

    // Player has blackjack
              if (player.Hand.IsBlackjack)
        {
    if (dealerBlackjack)
    {
    player.Outcome = GameOutcome.Push;
        player.Balance += player.CurrentBet;
         }
    else
       {
            player.Outcome = GameOutcome.PlayerBlackjack;
            player.Balance += player.CurrentBet + (int)(player.CurrentBet * 2.5); // 3:2 payout
    }
       continue;
    }

     // Dealer blackjack
        if (dealerBlackjack)
    {
           player.Outcome = GameOutcome.DealerBlackjack;
               continue;
     }

  // Normal resolution
      if (table.DealerHand.IsBusted)
        {
player.Outcome = GameOutcome.DealerBust;
     player.Balance += player.CurrentBet * 2;
          }
     else if (player.Hand.Score > table.DealerHand.Score)
                {
     player.Outcome = GameOutcome.PlayerWin;
           player.Balance += player.CurrentBet * 2;
       }
                else if (player.Hand.Score < table.DealerHand.Score)
     {
      player.Outcome = GameOutcome.DealerWin;
         }
  else
     {
        player.Outcome = GameOutcome.Push;
            player.Balance += player.CurrentBet;
       }
            }
        }

        await EndRound(tableId);
    }

    /// <summary>
    /// Ends the current round.
    /// </summary>
    private async Task EndRound(string tableId)
    {
        if (!_tableManager.TryGetTable(tableId, out var table) || table == null)
   return;

        table.IsRoundInProgress = false;

   // Reset all players' ready states for next round
        foreach (var player in table.Players.Values)
 {
      player.IsReady = false; // Reset bet readiness
   player.HasConfirmedNextRound = false; // Reset next round confirmation
    _logger.LogInformation("Player {PlayerName} reset for next round (IsReady=false, HasConfirmedNextRound=false)", 
      player.PlayerName);
        }

_logger.LogInformation("Round ended at table {TableId}. All players must confirm and bet again.", tableId);

     await Clients.Group(tableId).SendAsync("RoundEnded");
 await BroadcastTableState(tableId);
    }

    /// <summary>
    /// Allows a player to ready up for the next round.
    /// </summary>
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
       await Clients.Caller.SendAsync("Error", "Player not found");
     return;
    }

            // Check if player can ready up (must be in RoundOver state)
    if (player.State != GameState.RoundOver)
  {
       await Clients.Caller.SendAsync("Error", "Round is not over yet");
  return;
   }

    // Reset player for next round
   player.Hand.Clear();
player.CurrentBet = 0;
player.Outcome = GameOutcome.None;
      player.State = GameState.WaitingForBet;
  player.IsReady = false; // Not ready until they bet
   player.HasConfirmedNextRound = true; // ? Confirmed they want to play next round
  player.HasCompletedTurn = false;
   player.HasActed = false;

       var confirmedCount = table.GetPlayersConfirmedNextRoundCount();
       var totalCount = table.PlayerCount;

    _logger.LogInformation("Player {PlayerName} confirmed next round. Confirmed: {Confirmed}/{Total}", 
   player.PlayerName, confirmedCount, totalCount);

            // Check if all players have confirmed
      if (!table.HaveAllPlayersConfirmedNextRound())
  {
   // Not all players have confirmed yet
    await Clients.Caller.SendAsync("WaitingForPlayers", 
    $"Waiting for other players to ready up ({confirmedCount}/{totalCount} ready)");
       }
      else
   {
      // All players have confirmed - notify everyone they can bet now
          _logger.LogInformation("All players confirmed next round at table {TableId}, ready for betting", tableId);
  await Clients.Group(tableId).SendAsync("AllPlayersReadyForBetting");
  }

       await BroadcastTableState(tableId);
        }
        catch (Exception ex)
        {
 _logger.LogError(ex, "Error readying for next round");
   }
    }

    /// <summary>
    /// Called when a client disconnects.
    /// </summary>
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
try
        {
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
      
         // If it was this player's turn, advance to next
 if (table.IsRoundInProgress)
   {
             await SetNextPlayerTurn(tableId);
          }
     
          await BroadcastTableState(tableId);

   _tableManager.RemoveTableIfEmpty(tableId);
      }
        }
         }
        }
    catch (Exception ex)
        {
    _logger.LogError(ex, "Error handling disconnect");
        }

        await base.OnDisconnectedAsync(exception);
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
    var update = CreateGameStateUpdate(table, player);
            await Clients.Client(player.ConnectionId).SendAsync("GameStateUpdated", update);
        }
    }

    /// <summary>
 /// Creates a game state update for a player.
    /// </summary>
    private GameStateUpdate CreateGameStateUpdate(GameTable table, PlayerState currentPlayer)
    {
    // Dealer cards visibility:
      // - During player turns: Only show face-up card (hole card not dealt yet)
        // - During/after dealer turn: Show all cards
bool dealerIsPlaying = currentPlayer.State == GameState.DealerTurn || currentPlayer.State == GameState.RoundOver;
        bool showAllDealerCards = dealerIsPlaying && table.DealerHand != null && table.DealerHand.Cards.Count > 1;

      var dealerCards = table.DealerHand?.Cards
         .Take(showAllDealerCards ? int.MaxValue : 1) // Only show first card until dealer plays
            .Select(c => new SimpleCardInfo
            {
     Rank = c.Rank.ToString(),
    Suit = c.Suit.ToString(),
           ImageFileName = c.GetImageFileName()
            }).ToList() ?? new List<SimpleCardInfo>();

      int dealerVisibleTotal = showAllDealerCards && table.DealerHand != null
      ? table.DealerHand.Score
          : table.DealerHand?.Cards.FirstOrDefault()?.Value ?? 0;

        // Create player info for all players with turn order
        var allPlayers = table.PlayerOrder
            .Select((connectionId, index) =>
     {
                if (table.TryGetPlayer(connectionId, out var p) && p != null)
        {
         return new PlayerInfo
    {
              ConnectionId = p.ConnectionId,
     Name = p.PlayerName,
                Balance = p.Balance,
            CurrentBet = p.CurrentBet,
           IsReady = p.IsReady,
       Hand = p.Hand.Cards.Select(c => new SimpleCardInfo
 {
      Rank = c.Rank.ToString(),
   Suit = c.Suit.ToString(),
 ImageFileName = c.GetImageFileName()
    }).ToList(),
    HandTotal = p.Hand.Score,
    GameState = ((int)p.State).ToString(),
             Outcome = ((int)p.Outcome).ToString(),
      IsCurrentUser = p.ConnectionId == currentPlayer.ConnectionId
          };
          }
       return null;
            })
 .Where(p => p != null)
            .Cast<PlayerInfo>()
            .ToList();

        // Create player snapshot
  string message = GetMessageForPlayer(currentPlayer, table);

        var playerSnapshot = new GameSnapshot
        {
            PlayerCards = currentPlayer.Hand.Cards,
            DealerCards = table.DealerHand?.Cards ?? new List<Card>(),
         DealerHoleCardHidden = !showAllDealerCards,
     PlayerTotal = currentPlayer.Hand.Score,
            DealerTotal = dealerVisibleTotal,
     CanHit = currentPlayer.State == GameState.PlayerTurn && !currentPlayer.Hand.IsBusted,
   CanStand = currentPlayer.State == GameState.PlayerTurn,
    CanDouble = currentPlayer.State == GameState.PlayerTurn && !currentPlayer.HasActed && currentPlayer.CurrentBet <= currentPlayer.Balance,
        Balance = currentPlayer.Balance,
            Bet = currentPlayer.CurrentBet,
            State = currentPlayer.State,
            Outcome = currentPlayer.Outcome,
        Message = message,
            CanRestart = currentPlayer.State == GameState.RoundOver
        };

        return new GameStateUpdate
    {
            PlayerSnapshot = playerSnapshot,
          AllPlayers = allPlayers,
            DealerCards = dealerCards,
         DealerTotal = dealerVisibleTotal,
            DealerHoleCardHidden = !showAllDealerCards,
IsRoundInProgress = table.IsRoundInProgress,
            Message = message
        };
    }

    private string GetMessageForPlayer(PlayerState player, GameTable table)
    {
        // Show different message based on turn
        if (player.State == GameState.PlayerTurn)
        {
            return "Your turn! Choose your action.";
        }

        if (player.State == GameState.Dealing)
        {
     var currentConnectionId = table.GetCurrentPlayerConnectionId();
            if (currentConnectionId != null && table.TryGetPlayer(currentConnectionId, out var currentTurnPlayer) && currentTurnPlayer != null)
     {
  return $"Waiting for {currentTurnPlayer.PlayerName}'s turn...";
        }
   return "Waiting for your turn...";
        }

        if (player.State == GameState.DealerTurn && player.Outcome == GameOutcome.None)
    {
            return "Dealer is playing...";
        }

        return player.Outcome switch
        {
            GameOutcome.PlayerBlackjack => $"Blackjack! You win ${(int)(player.CurrentBet * 1.5)}!",
  GameOutcome.DealerBlackjack => "Dealer has Blackjack. You lose.",
    GameOutcome.PlayerBust => "Bust! You lose.",
      GameOutcome.DealerBust => $"Dealer busts! You win ${player.CurrentBet}!",
       GameOutcome.PlayerWin => $"You win ${player.CurrentBet}!",
 GameOutcome.DealerWin => "Dealer wins. You lose.",
 GameOutcome.Push => "Push. Bet returned.",
            _ => player.State == GameState.WaitingForBet ? "Place your bet to start!" : "Waiting..."
        };
    }
}
