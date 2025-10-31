using BlackjackV3.Services;

namespace BlackjackV3.Domain;

/// <summary>
/// Core blackjack game logic managing deck, hands, bets, and game flow.
/// </summary>
public class BlackjackGame
{
    private readonly Deck _deck;
    private readonly IRandomProvider _randomProvider;
    private readonly Hand _playerHand;
    private readonly Hand _dealerHand;

    private const int StartingBalance = 1000;
    private const int MinBet = 10;
    private const int MaxBet = 200;

    /// <summary>
    /// Gets the current game state.
    /// </summary>
    public GameState State { get; private set; }

    /// <summary>
    /// Gets the current bet amount.
    /// </summary>
    public int CurrentBet { get; private set; }

    /// <summary>
    /// Gets the player's balance.
    /// </summary>
    public int Balance { get; private set; }

    /// <summary>
    /// Gets the game outcome.
    /// </summary>
    public GameOutcome Outcome { get; private set; }

    /// <summary>
    /// Gets whether the player has taken any action this round.
  /// </summary>
    private bool _hasActed;

    /// <summary>
    /// Initializes a new blackjack game.
    /// </summary>
    /// <param name="randomProvider">The random provider for shuffling.</param>
    public BlackjackGame(IRandomProvider randomProvider)
    {
     _randomProvider = randomProvider;
        _deck = new Deck();
  _playerHand = new Hand();
  _dealerHand = new Hand();
        Balance = StartingBalance;
        State = GameState.WaitingForBet;
    }

    /// <summary>
    /// Starts a new round with the specified bet.
    /// </summary>
    /// <param name="bet">The bet amount.</param>
    /// <exception cref="ArgumentException">Thrown when bet is invalid.</exception>
    /// <exception cref="InvalidOperationException">Thrown when game is not ready for a new round.</exception>
    public void StartRound(int bet)
    {
        if (State != GameState.WaitingForBet)
            throw new InvalidOperationException("Cannot start a new round in the current state");

        if (bet < MinBet || bet > MaxBet)
      throw new ArgumentException($"Bet must be between {MinBet} and {MaxBet}", nameof(bet));

 if (bet > Balance)
  throw new ArgumentException("Insufficient balance for this bet", nameof(bet));

    CurrentBet = bet;
        Balance -= bet;
   _hasActed = false;
        Outcome = GameOutcome.None;

        // Clear hands
      _playerHand.Clear();
        _dealerHand.Clear();

        // Reshuffle if needed
        if (_deck.NeedsReshuffle)
        {
        _deck.Reset();
         _deck.Shuffle(_randomProvider);
     }

        // Deal initial cards
        State = GameState.Dealing;
_playerHand.AddCard(_deck.Deal());
 _dealerHand.AddCard(_deck.Deal());
        _playerHand.AddCard(_deck.Deal());
        _dealerHand.AddCard(_deck.Deal()); // Hole card

        // Check for blackjacks
        if (_playerHand.IsBlackjack && _dealerHand.IsBlackjack)
      {
            Outcome = GameOutcome.Push;
     Balance += CurrentBet; // Return bet
 State = GameState.RoundOver;
        }
        else if (_playerHand.IsBlackjack)
        {
       Outcome = GameOutcome.PlayerBlackjack;
      Balance += CurrentBet + (int)(CurrentBet * 2.5); // Bet + 3:2 payout
            State = GameState.RoundOver;
        }
        else if (_dealerHand.IsBlackjack)
        {
   Outcome = GameOutcome.DealerBlackjack;
   State = GameState.RoundOver;
        }
        else
        {
        State = GameState.PlayerTurn;
        }
    }

    /// <summary>
    /// Player takes another card.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when player cannot hit.</exception>
    public void PlayerHit()
    {
      if (State != GameState.PlayerTurn)
  throw new InvalidOperationException("Cannot hit in the current state");

        _hasActed = true;
  _playerHand.AddCard(_deck.Deal());

        if (_playerHand.IsBusted)
        {
  Outcome = GameOutcome.PlayerBust;
         State = GameState.RoundOver;
        }
    }

    /// <summary>
    /// Player stands, triggering dealer play.
 /// </summary>
  /// <exception cref="InvalidOperationException">Thrown when player cannot stand.</exception>
    public void PlayerStand()
    {
        if (State != GameState.PlayerTurn)
         throw new InvalidOperationException("Cannot stand in the current state");

    State = GameState.DealerTurn;
        DealerPlay();
      ResolveOutcome();
    }

    /// <summary>
    /// Player doubles down (only allowed on first action).
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when player cannot double.</exception>
    public void PlayerDouble()
    {
        if (State != GameState.PlayerTurn)
            throw new InvalidOperationException("Cannot double in the current state");

        if (_hasActed)
 throw new InvalidOperationException("Can only double on first action");

 if (CurrentBet > Balance)
        throw new InvalidOperationException("Insufficient balance to double");

     Balance -= CurrentBet;
        CurrentBet *= 2;
        _hasActed = true;

        _playerHand.AddCard(_deck.Deal());

    if (_playerHand.IsBusted)
        {
            Outcome = GameOutcome.PlayerBust;
            State = GameState.RoundOver;
      }
        else
        {
            State = GameState.DealerTurn;
    DealerPlay();
ResolveOutcome();
   }
    }

    /// <summary>
    /// Dealer plays according to house rules (hit to soft 17).
    /// </summary>
    private void DealerPlay()
    {
        // Dealer hits until reaching 17, including soft 17
      while (_dealerHand.Score < 17 || (_dealerHand.Score == 17 && _dealerHand.IsSoft))
 {
            _dealerHand.AddCard(_deck.Deal());
      }
    }

    /// <summary>
    /// Resolves the outcome and adjusts balance.
    /// </summary>
    private void ResolveOutcome()
    {
        if (_dealerHand.IsBusted)
        {
            Outcome = GameOutcome.DealerBust;
            Balance += CurrentBet * 2; // Return bet + winnings
        }
        else if (_playerHand.Score > _dealerHand.Score)
        {
     Outcome = GameOutcome.PlayerWin;
    Balance += CurrentBet * 2;
        }
 else if (_playerHand.Score < _dealerHand.Score)
    {
       Outcome = GameOutcome.DealerWin;
     }
      else
        {
            Outcome = GameOutcome.Push;
            Balance += CurrentBet; // Return bet
}

        State = GameState.RoundOver;
    }

    /// <summary>
    /// Resets the game for a new round.
    /// </summary>
    public void Restart()
    {
        if (State != GameState.RoundOver)
            throw new InvalidOperationException("Can only restart after a round is over");

        State = GameState.WaitingForBet;
   CurrentBet = 0;
        Outcome = GameOutcome.None;
    }

    /// <summary>
    /// Creates an immutable snapshot of the current game state.
    /// </summary>
    /// <returns>A GameSnapshot representing the current state.</returns>
    public GameSnapshot ToSnapshot()
    {
        var dealerVisibleTotal = State == GameState.PlayerTurn || State == GameState.Dealing
            ? _dealerHand.Cards.First().Value
: _dealerHand.Score;

        var message = Outcome switch
      {
         GameOutcome.PlayerBlackjack => $"Blackjack! You win ${(int)(CurrentBet * 1.5)}!",
            GameOutcome.DealerBlackjack => "Dealer has Blackjack. You lose.",
       GameOutcome.PlayerBust => "Bust! You lose.",
      GameOutcome.DealerBust => $"Dealer busts! You win ${CurrentBet}!",
      GameOutcome.PlayerWin => $"You win ${CurrentBet}!",
            GameOutcome.DealerWin => "Dealer wins. You lose.",
            GameOutcome.Push => "Push. Bet returned.",
      _ => State == GameState.WaitingForBet ? "Place your bet to start!" : "Your turn!"
        };

 return new GameSnapshot
        {
PlayerCards = _playerHand.Cards,
  DealerCards = _dealerHand.Cards,
            DealerHoleCardHidden = State == GameState.PlayerTurn || State == GameState.Dealing,
            PlayerTotal = _playerHand.Score,
        DealerTotal = dealerVisibleTotal,
            CanHit = State == GameState.PlayerTurn && !_playerHand.IsBusted,
  CanStand = State == GameState.PlayerTurn,
      CanDouble = State == GameState.PlayerTurn && !_hasActed && CurrentBet <= Balance,
    Balance = Balance,
      Bet = CurrentBet,
       State = State,
      Outcome = Outcome,
            Message = message,
          CanRestart = State == GameState.RoundOver
        };
    }

    /// <summary>
    /// Gets the minimum allowed bet.
    /// </summary>
    public static int GetMinBet() => MinBet;

    /// <summary>
    /// Gets the maximum allowed bet.
    /// </summary>
    public static int GetMaxBet() => MaxBet;
}
