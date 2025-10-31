namespace BlackjackV3.Domain;

/// <summary>
/// Represents the suit of a playing card.
/// </summary>
public enum Suit
{
    Hearts,
    Diamonds,
    Clubs,
    Spades
}

/// <summary>
/// Represents the rank of a playing card.
/// </summary>
public enum Rank
{
    Two = 2,
    Three = 3,
    Four = 4,
    Five = 5,
    Six = 6,
    Seven = 7,
    Eight = 8,
    Nine = 9,
    Ten = 10,
    Jack = 11,
    Queen = 12,
    King = 13,
    Ace = 14
}

/// <summary>
/// Represents the current state of a blackjack game.
/// </summary>
public enum GameState
{
    WaitingForBet,
    Dealing,
    PlayerTurn,
    DealerTurn,
    RoundOver
}

/// <summary>
/// Represents possible game outcomes.
/// </summary>
public enum GameOutcome
{
    None,
    PlayerBlackjack,
    DealerBlackjack,
    PlayerBust,
    DealerBust,
    Push,
    PlayerWin,
    DealerWin
}
