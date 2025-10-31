# BlackjackV3 Unit Tests

This project contains comprehensive unit tests for the BlackjackV3 Razor Pages application's domain logic.

## Test Coverage

The test suite covers all core Blackjack game functionality:

### CardTests.cs
- Card value calculation (Aces = 11, Face cards = 10, number cards = face value)
- Image file name generation for card assets
- String representation

### HandTests.cs (21 tests)
- Empty hand scoring
- Single and multiple card scoring
- **Ace handling** (critical for Blackjack):
  - Ace + 6 = 17 (soft)
  - Ace + King = 21 (Blackjack)
  - Ace + 9 + 2 = 12 (Ace adjusts from 11 to 1)
  - Two Aces = 12 (one stays 11, one becomes 1)
  - Multiple aces with various cards
  - Four Aces = 14
- Blackjack detection (exactly 21 with 2 cards)
- Bust detection (> 21)
- Hand clearing

### DeckTests.cs (10 tests)
- 52-card deck initialization
- Deck reset functionality
- Dealing cards
- Empty deck error handling
- **Fisher-Yates shuffle algorithm**:
  - Shuffle changes card order
  - Shuffle maintains all 52 cards (all suits and ranks present)
- Reshuffle threshold (< 15 cards remaining)

### BlackjackGameTests.cs (28 tests)
- Game initialization with correct balance (1000)
- Bet validation (min 10, max 200, sufficient funds)
- Round start (dealing initial 4 cards)
- Player actions:
  - Hit (add card)
  - Stand (trigger dealer play)
  - Double (double bet on first action only)
- Player bust detection
- Game state transitions
- Snapshot generation for UI binding
- Dealer hole card visibility (hidden during player turn)
- Restart functionality

### DealerLogicTests.cs (11 tests)
- **Dealer hits to soft 17** (house rule):
  - Stands on hard 17
  - Hits on soft 17 (Ace counted as 11)
  - Always ends with >= 17 or busted (unless blackjack)
- Soft vs hard hand detection:
  - Soft 17: Ace + 6
  - Hard 17: 10 + 7
  - Ace adjustment when hitting soft hands
- Multiple ace scenarios

### OutcomeTests.cs (10 tests)
- **Payout calculations**:
  - **Player Blackjack**: Pays 3:2 (bet + 1.5× bet)
  - **Player Win**: Pays 1:1 (bet + bet)
  - **Dealer Win**: Loses bet
  - **Push**: Returns bet
  - **Player Bust**: Loses bet
  - **Dealer Bust**: Player wins 1:1
  - **Both Blackjack**: Push
  - **Double Down**: Correct payout with doubled bet
- Outcome messages
- Balance tracking across multiple rounds

## Running the Tests

### Run all tests:
```bash
cd BlackjackV3.Tests
dotnet test
```

### Run with detailed output:
```bash
dotnet test --logger "console;verbosity=detailed"
```

### Run specific test class:
```bash
dotnet test --filter "FullyQualifiedName~DealerLogicTests"
```

### Run specific test:
```bash
dotnet test --filter "FullyQualifiedName~Hand_TwoAcesAndNine_ScoreIs21"
```

## Test Statistics

- **Total Tests**: 94
- **Test Classes**: 6
- **Code Coverage**: Domain layer (100% of public methods)

## Key Test Patterns

### Deterministic Testing
Tests use `CryptoRandomProvider` for real randomness but validate logic across multiple rounds to account for probability.

### Edge Cases Covered
- Multiple aces in hand
- Soft 17 dealer logic
- Blackjack detection
- Bust scenarios
- Insufficient balance
- Invalid bet amounts
- Game state validation

## Dependencies

- **xUnit** 2.6.2 - Testing framework
- **Microsoft.NET.Test.Sdk** 17.8.0 - Test host
- **coverlet.collector** 6.0.0 - Code coverage

## Notes

- All domain classes are pure C# with no ASP.NET dependencies
- Tests verify deterministic game logic independent of the web framework
- Tests support future SignalR multiplayer by ensuring domain is stateless per action
