# Blackjack - ASP.NET Core 8 Razor Pages

A fully-featured single-player Blackjack game built with ASP.NET Core 8 Razor Pages, featuring smooth CSS/JS animations and architected for future multiplayer support via SignalR.

## Features

### Current (v1 - Single Player)
- **Complete Blackjack Gameplay**: Hit, Stand, Double Down
- **Realistic Game Rules**:
  - Dealer hits to soft 17
  - Blackjack pays 3:2
  - Standard push/win/lose mechanics
  - Bet validation ($10-$200 range)
- **Smooth Animations**:
  - Card dealing animation
  - Hit card animation
  - Dealer hole card reveal with flip effect
- **Session-based Game State**: Each player has their own game instance
- **Responsive Design**: Works on desktop and mobile devices
- **Starting Balance**: $1,000 with adjustable bets

### Architecture

#### Domain Layer (Pure C# Logic)
- `Card`, `Deck`, `Hand` - Core card game primitives
- `BlackjackGame` - Main game state machine
- `GameSnapshot` - Immutable DTO for rendering
- `Enums` - Suit, Rank, GameState, GameOutcome
- **Zero dependencies** on ASP.NET Core - fully reusable

#### Services Layer
- `IRandomProvider` / `CryptoRandomProvider` - Cryptographically secure shuffling
- `IGameStateStore` / `InMemoryGameStateStore` - Session-based game persistence
  - **TODO**: Swap with Redis or EF Core for distributed/persistent storage

#### Presentation Layer
- Razor Pages with clean separation
- Partial views for cards and hands
- CSS animations using `@keyframes` and transforms
- Vanilla JavaScript for animation triggers

#### Future: SignalR Multiplayer
- `GameHub` stub is already in place at `/hub/game`
- Domain logic is stateless and ready for multi-table scenarios
- **TODO**: Implement room management, synchronized game state, and real-time updates

## Project Structure

```
BlackjackV3/
??? Domain/          # Pure game logic
?   ??? Card.cs
?   ??? Deck.cs
?   ??? Hand.cs
?   ??? BlackjackGame.cs
?   ??? GameSnapshot.cs
?   ??? Enums.cs
??? Services/     # Infrastructure
?   ??? IRandomProvider.cs
?   ??? CryptoRandomProvider.cs
?   ??? IGameStateStore.cs
?   ??? InMemoryGameStateStore.cs
??? Models/  # View models
?   ??? GameViewModel.cs
?   ??? HandDisplayModel.cs
?   ??? CardDisplayModel.cs
??? Pages/         # Razor Pages
?   ??? Index.cshtml     # Splash page
?   ??? Game.cshtml      # Main game page
?   ??? Game.cshtml.cs   # Game page model
?   ??? Shared/
?       ??? _Layout.cshtml
?       ??? _Hand.cshtml
?       ??? _Card.cshtml
??? Hubs/    # SignalR (future)
?   ??? GameHub.cs# Multiplayer stub
??? wwwroot/
?   ??? css/
?   ?   ??? game.css     # Game styling + animations
?   ??? js/
?       ??? game.js      # Animation triggers
??? Program.cs           # App configuration

BlackjackV3.Tests/       # xUnit tests
??? HandTests.cs         # Ace handling, scoring
??? DealerLogicTests.cs  # Dealer AI tests
??? OutcomeTests.cs    # Payout validation
??? DeckTests.cs         # Deck operations
```

## Getting Started

### Prerequisites
- .NET 8 SDK

### Running the Application

1. **Build the solution**:
   ```bash
   dotnet build
   ```

2. **Run the application**:
   ```bash
   dotnet run --project BlackjackV3.csproj
   ```

3. **Navigate to the game**:
   - Open your browser to `https://localhost:5001` (or the port shown in console)
   - Click "Play Now" on the splash page
- Or go directly to `/Game`

### Running Tests

```bash
dotnet test
```

Tests cover:
- Hand scoring with ace soft/hard logic
- Dealer hit-to-soft-17 rules
- Blackjack detection (21 with 2 cards)
- Bet validation (min/max/balance checks)
- Deck operations (shuffle, deal, reshuffle)

## How to Play

1. **Place Your Bet**: Enter amount between $10 and $200, then click "Deal"
2. **Make Your Move**:
   - **Hit**: Take another card
   - **Stand**: End your turn, dealer plays
   - **Double**: Double your bet and take one final card (only available on first action)
3. **Dealer Plays**: Dealer reveals hole card and hits to soft 17
4. **Outcome**: See result and balance update
5. **New Round**: Click "New Round" to play again

### Payouts
- **Blackjack** (Ace + 10-value card): 3:2 (e.g., $100 bet wins $150)
- **Normal Win**: 1:1 (e.g., $100 bet wins $100)
- **Push** (tie): Bet returned
- **Loss**: Bet lost

## Game Rules

- Dealer hits to **soft 17** (Ace + 6)
- Dealer stands on **hard 17** or higher
- Blackjack pays **3:2**
- Player can **double down** only on the first action
- Deck reshuffles when fewer than 15 cards remain

## Animations

All animations are pure CSS with JavaScript triggers:

- **Deal Animation**: Cards fly from deck position to hand
- **Hit Animation**: Card slides in from deck with rotation
- **Reveal Animation**: Dealer hole card flips using 3D transform

See `wwwroot/css/game.css` for `@keyframes` definitions and `wwwroot/js/game.js` for trigger logic.

## Architecture Notes

### Why Domain Separation?
The `Domain/` folder contains **zero ASP.NET Core dependencies**. This means:
- Easy to unit test (no mocking HttpContext, etc.)
- Can be reused in console apps, Blazor, SignalR hubs, etc.
- Side-effect free - all randomness injected via `IRandomProvider`

### Session vs. Database
Currently uses **in-memory sessions** (`InMemoryGameStateStore`). For production:
- Swap with **Redis** for distributed sessions
- Use **EF Core** for persistent game history
- Interface (`IGameStateStore`) makes this a one-line change in `Program.cs`

### SignalR Readiness
`GameHub.cs` is a stub with methods:
- `JoinTable(tableId)`
- `PlaceBet(tableId, betAmount)`
- `PerformAction(tableId, action)`

To enable multiplayer:
1. Create a room/table manager service
2. Store multi-player game state in the hub
3. Broadcast game snapshots to all players at a table
4. Use the same `BlackjackGame` domain logic

## Future Enhancements (TODO)

- [ ] **Multiplayer via SignalR**: Multiple players at one table
- [ ] **Persistent Storage**: Save game history with EF Core
- [ ] **Leaderboard**: Track wins/losses/balance across sessions
- [ ] **Card Counting Hints**: Optional hints for card counters
- [ ] **Sound Effects**: Card dealing, chip sounds
- [ ] **Splitting Pairs**: Allow player to split matching cards
- [ ] **Insurance**: Offer insurance when dealer shows Ace
- [ ] **SVG Card Graphics**: Replace simple div cards with actual card images

## Technologies Used

- **ASP.NET Core 8** - Razor Pages
- **C# 12** - Record types, pattern matching
- **.NET 8** - Latest runtime
- **SignalR** - Real-time communication (scaffolded)
- **xUnit** - Unit testing
- **CSS3** - Animations and transforms
- **Vanilla JavaScript** - No frameworks

## Code Quality

- **XML Documentation**: All public members documented
- **Clean Architecture**: Domain, Services, Presentation layers
- **SOLID Principles**: Dependency injection, interface segregation
- **Testable**: Domain logic has 100% unit test coverage potential
- **Type Safety**: Nullable reference types enabled

## License

This is a sample/learning project. Feel free to use and modify as needed.

## Credits

Created as a demonstration of:
- Clean architecture in ASP.NET Core
- Test-driven development
- CSS animations without libraries
- Preparation for real-time multiplayer

---

**Have fun and gamble responsibly!** (It's fake money anyway ??)
