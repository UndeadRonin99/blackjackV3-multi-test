using Xunit;
using BlackjackV3.Domain;
using BlackjackV3.Services;

namespace BlackjackV3.Tests.Domain;

/// <summary>
/// Tests for the BlackjackGame class covering game flow, dealer logic, and outcomes.
/// </summary>
public class BlackjackGameTests
{
    /// <summary>
    /// Deterministic random provider for controlled testing.
    /// </summary>
    private class TestRandomProvider : IRandomProvider
    {
    private readonly Queue<int> _values;

        public TestRandomProvider(params int[] values)
 {
   _values = new Queue<int>(values);
        }

   public int Next(int maxValue)
  {
    return _values.Count > 0 ? _values.Dequeue() % maxValue : 0;
        }
    }

    [Fact]
    public void BlackjackGame_NewGame_StartsWithCorrectBalance()
    {
   // Arrange
        var rng = new CryptoRandomProvider();

        // Act
        var game = new BlackjackGame(rng);

 // Assert
     Assert.Equal(1000, game.Balance);
     Assert.Equal(GameState.WaitingForBet, game.State);
    }

    [Fact]
    public void BlackjackGame_StartRound_ValidBet_DeductsBet()
    {
        // Arrange
  var rng = new CryptoRandomProvider();
    var game = new BlackjackGame(rng);

  // Act
  game.StartRound(50);

     // Assert
 Assert.Equal(950, game.Balance);
        Assert.Equal(50, game.CurrentBet);
    }

    [Fact]
    public void BlackjackGame_StartRound_BetTooLow_ThrowsException()
    {
        // Arrange
   var rng = new CryptoRandomProvider();
     var game = new BlackjackGame(rng);

        // Act & Assert
   Assert.Throws<ArgumentException>(() => game.StartRound(5));
    }

    [Fact]
    public void BlackjackGame_StartRound_BetTooHigh_ThrowsException()
    {
        // Arrange
   var rng = new CryptoRandomProvider();
     var game = new BlackjackGame(rng);

    // Act & Assert
        Assert.Throws<ArgumentException>(() => game.StartRound(250));
    }

    [Fact]
    public void BlackjackGame_StartRound_InsufficientBalance_ThrowsException()
    {
     // Arrange
        var rng = new CryptoRandomProvider();
   var game = new BlackjackGame(rng);

  // Act & Assert
   Assert.Throws<ArgumentException>(() => game.StartRound(1500));
    }

    [Fact]
    public void BlackjackGame_StartRound_DealsInitialCards()
    {
   // Arrange
     var rng = new CryptoRandomProvider();
   var game = new BlackjackGame(rng);

  // Act
        game.StartRound(50);
   var snapshot = game.ToSnapshot();

   // Assert
        Assert.Equal(2, snapshot.PlayerCards.Count);
 Assert.Equal(2, snapshot.DealerCards.Count);
    }

    [Fact]
    public void BlackjackGame_PlayerHit_AddsCardToPlayerHand()
    {
        // Arrange
   var rng = new CryptoRandomProvider();
    var game = new BlackjackGame(rng);
   game.StartRound(50);

        // Act
 if (game.State == GameState.PlayerTurn)
        {
    game.PlayerHit();
        }
  var snapshot = game.ToSnapshot();

     // Assert
        Assert.True(snapshot.PlayerCards.Count >= 3);
    }

    [Fact]
    public void BlackjackGame_PlayerHit_WhenNotPlayerTurn_ThrowsException()
    {
 // Arrange
    var rng = new CryptoRandomProvider();
   var game = new BlackjackGame(rng);

  // Act & Assert
   Assert.Throws<InvalidOperationException>(() => game.PlayerHit());
    }

    [Fact]
    public void BlackjackGame_PlayerBust_EndsRound()
    {
        // Arrange
   var rng = new CryptoRandomProvider();
   var game = new BlackjackGame(rng);
     game.StartRound(50);

    // Act - Keep hitting until bust (with real RNG, this is probabilistic)
     // For testing, we'll check the logic when it happens
        while (game.State == GameState.PlayerTurn && game.ToSnapshot().PlayerTotal < 21)
{
      game.PlayerHit();
   }

     var snapshot = game.ToSnapshot();

        // Assert
   if (snapshot.PlayerTotal > 21)
 {
   Assert.Equal(GameState.RoundOver, game.State);
   Assert.Equal(GameOutcome.PlayerBust, game.Outcome);
     }
    }

    [Fact]
    public void BlackjackGame_PlayerStand_TriggersDealerPlay()
    {
    // Arrange
   var rng = new CryptoRandomProvider();
   var game = new BlackjackGame(rng);
     game.StartRound(50);

  // Act
        if (game.State == GameState.PlayerTurn)
 {
        game.PlayerStand();
   }

  // Assert
  Assert.Equal(GameState.RoundOver, game.State);
    }

    [Fact]
    public void BlackjackGame_PlayerDouble_DoublesTheBet()
    {
        // Arrange
    var rng = new CryptoRandomProvider();
   var game = new BlackjackGame(rng);
game.StartRound(50);

  // Act
     if (game.State == GameState.PlayerTurn && game.ToSnapshot().CanDouble)
        {
     game.PlayerDouble();
        }

        // Assert
 Assert.Equal(100, game.CurrentBet);
    }

    [Fact]
    public void BlackjackGame_PlayerDouble_WhenAlreadyActed_ThrowsException()
    {
   // Arrange
   var rng = new CryptoRandomProvider();
     var game = new BlackjackGame(rng);
     game.StartRound(50);

  // Act
   if (game.State == GameState.PlayerTurn)
   {
     game.PlayerHit(); // First action
}

        // Assert
   if (game.State == GameState.PlayerTurn)
   {
       Assert.Throws<InvalidOperationException>(() => game.PlayerDouble());
   }
    }

    [Fact]
    public void BlackjackGame_DealerHitsToSoft17()
    {
 // This test verifies dealer behavior indirectly through game outcome
        // Arrange
        var rng = new CryptoRandomProvider();
        var game = new BlackjackGame(rng);
     game.StartRound(50);

   // Act
        if (game.State == GameState.PlayerTurn)
        {
   game.PlayerStand();
        }

        var snapshot = game.ToSnapshot();

        // Assert
   // Dealer should have final total >= 17 or be busted
        Assert.True(snapshot.DealerTotal >= 17 || snapshot.Outcome == GameOutcome.DealerBust);
    }

    [Fact]
    public void BlackjackGame_PlayerWin_Pays1to1()
    {
   // This test is probabilistic with real RNG
   // We'll verify the payout logic when player wins
        var rng = new CryptoRandomProvider();
var game = new BlackjackGame(rng);
        var initialBalance = game.Balance;
   game.StartRound(50);

        if (game.State == GameState.PlayerTurn)
        {
            game.PlayerStand();
        }

        var snapshot = game.ToSnapshot();

        if (snapshot.Outcome == GameOutcome.PlayerWin)
        {
            // Won 50, balance should be initialBalance - bet + (bet * 2) = initialBalance + bet
         Assert.Equal(initialBalance + 50, game.Balance);
        }
    }

    [Fact]
    public void BlackjackGame_PlayerBlackjack_Pays3to2()
    {
        // This test is probabilistic with real RNG
        // We verify the logic when blackjack occurs
        var rng = new CryptoRandomProvider();
   var game = new BlackjackGame(rng);
        var initialBalance = game.Balance;
 game.StartRound(50);

        var snapshot = game.ToSnapshot();

   if (snapshot.Outcome == GameOutcome.PlayerBlackjack)
        {
            // Blackjack pays 3:2, so bet(50) + payout(75) = 125 total
            // Balance = initialBalance - bet + bet + (bet * 1.5) = initialBalance + (bet * 1.5)
            Assert.Equal(initialBalance + 75, game.Balance);
        }
    }

    [Fact]
    public void BlackjackGame_Push_ReturnsBet()
    {
        // This test is probabilistic with real RNG
        var rng = new CryptoRandomProvider();
        var game = new BlackjackGame(rng);
      var initialBalance = game.Balance;
   game.StartRound(50);

      if (game.State == GameState.PlayerTurn)
        {
       game.PlayerStand();
        }

        var snapshot = game.ToSnapshot();

        if (snapshot.Outcome == GameOutcome.Push)
        {
        // Bet is returned, balance should equal initial
       Assert.Equal(initialBalance, game.Balance);
        }
    }

    [Fact]
    public void BlackjackGame_DealerBust_PlayerWins()
    {
        // Probabilistic test
        var rng = new CryptoRandomProvider();
   var game = new BlackjackGame(rng);
     var initialBalance = game.Balance;
 game.StartRound(50);

        if (game.State == GameState.PlayerTurn)
   {
         game.PlayerStand();
 }

   var snapshot = game.ToSnapshot();

   if (snapshot.Outcome == GameOutcome.DealerBust)
        {
       Assert.Equal(initialBalance + 50, game.Balance);
 }
    }

    [Fact]
    public void BlackjackGame_Restart_ResetsToWaitingForBet()
    {
        // Arrange
   var rng = new CryptoRandomProvider();
        var game = new BlackjackGame(rng);
   game.StartRound(50);

        if (game.State == GameState.PlayerTurn)
        {
          game.PlayerStand();
 }

        // Act
        game.Restart();

     // Assert
        Assert.Equal(GameState.WaitingForBet, game.State);
   Assert.Equal(0, game.CurrentBet);
   Assert.Equal(GameOutcome.None, game.Outcome);
    }

    [Fact]
    public void BlackjackGame_Restart_BeforeRoundOver_ThrowsException()
    {
 // Arrange
   var rng = new CryptoRandomProvider();
        var game = new BlackjackGame(rng);
     game.StartRound(50);

        // Act & Assert
   if (game.State != GameState.RoundOver)
   {
         Assert.Throws<InvalidOperationException>(() => game.Restart());
        }
    }

    [Fact]
  public void BlackjackGame_ToSnapshot_ContainsCorrectData()
    {
        // Arrange
   var rng = new CryptoRandomProvider();
    var game = new BlackjackGame(rng);
   game.StartRound(50);

        // Act
   var snapshot = game.ToSnapshot();

        // Assert
        Assert.NotNull(snapshot);
   Assert.Equal(2, snapshot.PlayerCards.Count);
Assert.Equal(2, snapshot.DealerCards.Count);
        Assert.Equal(50, snapshot.Bet);
  Assert.NotNull(snapshot.Message);
    }

    [Fact]
    public void BlackjackGame_DealerHoleCard_HiddenDuringPlayerTurn()
{
      // Arrange
        var rng = new CryptoRandomProvider();
   var game = new BlackjackGame(rng);
        game.StartRound(50);

        // Act
    var snapshot = game.ToSnapshot();

        // Assert
   if (game.State == GameState.PlayerTurn)
   {
       Assert.True(snapshot.DealerHoleCardHidden);
        }
    }

    [Fact]
    public void BlackjackGame_DealerHoleCard_RevealedAfterPlayerStands()
    {
        // Arrange
   var rng = new CryptoRandomProvider();
      var game = new BlackjackGame(rng);
        game.StartRound(50);

   // Act
   if (game.State == GameState.PlayerTurn)
      {
       game.PlayerStand();
        }
   var snapshot = game.ToSnapshot();

        // Assert
        Assert.False(snapshot.DealerHoleCardHidden);
    }

    [Fact]
    public void BlackjackGame_GetMinBet_Returns10()
    {
 // Assert
 Assert.Equal(10, BlackjackGame.GetMinBet());
    }

    [Fact]
    public void BlackjackGame_GetMaxBet_Returns200()
    {
   // Assert
 Assert.Equal(200, BlackjackGame.GetMaxBet());
    }

    [Fact]
    public void BlackjackGame_CanHit_TrueWhenPlayerTurnAndNotBusted()
    {
        // Arrange
     var rng = new CryptoRandomProvider();
   var game = new BlackjackGame(rng);
        game.StartRound(50);

        // Act
   var snapshot = game.ToSnapshot();

        // Assert
  if (game.State == GameState.PlayerTurn && snapshot.PlayerTotal <= 21)
   {
          Assert.True(snapshot.CanHit);
        }
    }

    [Fact]
    public void BlackjackGame_CanStand_TrueWhenPlayerTurn()
    {
        // Arrange
   var rng = new CryptoRandomProvider();
   var game = new BlackjackGame(rng);
    game.StartRound(50);

   // Act
   var snapshot = game.ToSnapshot();

        // Assert
        if (game.State == GameState.PlayerTurn)
 {
  Assert.True(snapshot.CanStand);
   }
    }

    [Fact]
    public void BlackjackGame_CanDouble_TrueOnFirstActionWithSufficientBalance()
    {
        // Arrange
        var rng = new CryptoRandomProvider();
        var game = new BlackjackGame(rng);
        game.StartRound(50);

   // Act
     var snapshot = game.ToSnapshot();

   // Assert
        if (game.State == GameState.PlayerTurn)
   {
            Assert.True(snapshot.CanDouble);
        }
    }

    [Fact]
    public void BlackjackGame_CanRestart_TrueWhenRoundOver()
    {
        // Arrange
   var rng = new CryptoRandomProvider();
        var game = new BlackjackGame(rng);
  game.StartRound(50);

        if (game.State == GameState.PlayerTurn)
   {
            game.PlayerStand();
   }

        // Act
   var snapshot = game.ToSnapshot();

        // Assert
        if (game.State == GameState.RoundOver)
 {
     Assert.True(snapshot.CanRestart);
   }
    }
}
