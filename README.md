# Blackjack - ASP.NET Core 8 Razor Pages

A fully-featured **single-player and multiplayer** Blackjack game built with ASP.NET Core 8 Razor Pages, featuring a modern, polished UI with smooth CSS animations and real-time multiplayer via SignalR.

## ?? Features

### Multiplayer Mode ? NEW!
- **Up to 5 players** at one table simultaneously
- **Real-time synchronization** using SignalR
- **Independent games** - each player has their own balance and hands
- **Share table links** - invite friends with one click
- **Modern, beautiful UI** with card animations
- **Live player status** - see who's ready, playing, or finished

### Single Player Mode
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
- **Responsive Design**: Works beautifully on desktop and mobile
- **Starting Balance**: $1,000 with adjustable bets

## ?? Quick Start

### Prerequisites
- .NET 8 SDK

### Running the Application

1. **Clone and build**:
   ```bash
   git clone <your-repo>
   cd BlackjackV3
   dotnet build
   ```

2. **Run the application**:
   ```bash
   dotnet run
   ```

3. **Play the game**:
   - Open your browser to `https://localhost:5xxx`
   - Choose **Single Player** or **Multiplayer**
   - For multiplayer: Share the table link with friends!

### Running Tests

```bash
dotnet test
```

**94 tests** covering:
- Hand scoring with ace soft/hard logic
- Dealer hit-to-soft-17 rules
- Blackjack detection
- Payout calculations (3:2 blackjack, 1:1 wins)
- Deck operations

## ?? How to Play

### Single Player
1. Navigate to `/Game`
2. Place your bet ($10-$200)
3. Click **Deal**
4. Choose **Hit**, **Stand**, or **Double**
5. Dealer plays automatically
6. See your outcome and updated balance

### Multiplayer
1. Navigate to `/MultiplayerGame`
2. Share your table link with friends
3. All players place bets
4. Round starts when everyone is ready
5. Each player plays their hand independently
6. Round ends when all players finish
7. Click "Ready for Next Round" to continue

## ??? Architecture

### Clean Domain Layer
```
Domain/
??? Card.cs     # Playing card with rank, suit, value
??? Deck.cs           # 52-card deck with Fisher-Yates shuffle
??? Hand.cs    # Card collection with ace soft/hard scoring
??? BlackjackGame.cs  # Core game state machine
??? GameSnapshot.cs   # Immutable DTO for UI
??? Enums.cs   # Suit, Rank, GameState, GameOutcome
```

**Zero dependencies** on ASP.NET Core - fully reusable and testable!

### Services Layer
```
Services/
??? IRandomProvider / CryptoRandomProvider    # Secure RNG
??? IGameStateStore / InMemoryGameStateStore  # Session storage
??? PlayerState.cs   # Multiplayer player tracking
??? GameTable.cs         # Table with multiple players
??? ITableManager.cs     # Manages all active tables
```

### Presentation Layer
```
Pages/
??? Index.cshtml              # Modern landing page
??? Game.cshtml        # Single-player UI
??? MultiplayerGame.cshtml    # Multiplayer UI (polished design!)
??? Shared/
    ??? _Card.cshtml          # Card partial
    ??? _Hand.cshtml          # Hand partial
```

### Real-time Multiplayer
```
Hubs/
??? GameHub.cs    # SignalR hub
  ??? JoinTable(tableId, playerName)
    ??? PlaceBet(tableId, betAmount)
    ??? PerformAction(tableId, action)
    ??? ReadyForNextRound(tableId)
```

## ?? UI Design

### Multiplayer Interface
- **Felt green dealer section** with radial gradient lighting
- **Player cards grid** showing all players in real-time
- **Sticky control panel** for your game actions
- **Status badges** - Ready, Playing, Finished
- **Live balance updates** with color-coded stats
- **Modern card designs** with smooth animations
- **Share modal** with one-click link copying
- **Connection status indicator** with color states

### Color Palette
- **Primary Green**: `#10b981` - Success, ready states
- **Info Blue**: `#3b82f6` - Player info, tables
- **Warning Orange**: `#f59e0b` - Active turns, bets
- **Danger Red**: `#ef4444` - Errors, losses
- **Felt Green**: `#0d5f3a` - Dealer background
- **Gold**: `#fbbf24` - Accents, hero text

## ?? Game Rules

- **Dealer Strategy**: Hits to soft 17, stands on hard 17+
- **Blackjack Payout**: 3:2 (bet $100, win $150)
- **Normal Win**: 1:1 (bet $100, win $100)
- **Push**: Bet returned
- **Double Down**: Only on first action
- **Deck Reshuffle**: When < 15 cards remain

## ?? Testing

Comprehensive test suite with **94 passing tests**:

```bash
# Run all tests
dotnet test

# Run with detailed output
dotnet test --logger "console;verbosity=detailed"

# Run specific test class
dotnet test --filter "FullyQualifiedName~HandTests"
```

**Test Coverage**:
- ? Card value calculations (Aces, face cards, numbers)
- ? Hand scoring with multiple aces
- ? Dealer logic (hit to soft 17)
- ? Blackjack detection (21 with exactly 2 cards)
- ? Outcome resolution and payouts
- ? Deck operations (shuffle, deal, reshuffle)

## ?? Multiplayer Features

### Synchronization
- **Round starts** when all players are ready
- **Round ends** when all players finish
- **Real-time updates** via SignalR
- **Automatic cleanup** when players disconnect

### Table Management
- **5-player maximum** per table
- **Unique table IDs** (shareable links)
- **In-memory storage** (easily swappable with Redis/SQL)
- **Automatic table cleanup** when empty

### Player Independence
- Each player has their own `BlackjackGame` instance
- Separate decks (no card counting across players)
- Individual balances and bets
- Independent decision-making

## ?? Technologies

- **ASP.NET Core 8** - Razor Pages
- **SignalR** - Real-time communication
- **C# 12** - Latest language features
- **.NET 8** - Latest runtime
- **xUnit** - Unit testing framework
- **CSS3** - Animations and modern styling
- **Vanilla JavaScript** - No frameworks needed

## ?? Project Structure

```
BlackjackV3/
??? Domain/         # Pure game logic
??? Services/            # Infrastructure services
??? Models/# View models and DTOs
??? Pages/    # Razor Pages
??? Hubs/                # SignalR hubs
??? wwwroot/
?   ??? css/
?   ?   ??? game.css # Single-player styles
?   ?   ??? multiplayer-game.css  # Multiplayer styles ?
?   ?   ??? index.css         # Landing page styles ?
?   ??? js/
?       ??? game.js   # Single-player JS
?       ??? multiplayer-game.js   # Multiplayer client ?
??? Program.cs

BlackjackV3.Tests/       # 94 unit tests
??? Domain/
?   ??? CardTests.cs
?   ??? HandTests.cs
?   ??? DeckTests.cs
?   ??? BlackjackGameTests.cs
?   ??? DealerLogicTests.cs
?   ??? OutcomeTests.cs
??? README.md
```

## ?? Design Principles

### SOLID Architecture
- **S**ingle Responsibility - Each class has one job
- **O**pen/Closed - Extensible via interfaces
- **L**iskov Substitution - Interfaces over concrete types
- **I**nterface Segregation - Focused interfaces
- **D**ependency Inversion - Inject dependencies

### Clean Code
- ? XML documentation on all public members
- ? Descriptive method and variable names
- ? Consistent naming conventions
- ? Nullable reference types enabled
- ? Async/await best practices

## ?? Deployment Considerations

### For Production

1. **Replace InMemoryTableManager** with Redis:
   ```csharp
   services.AddSingleton<ITableManager, RedisTableManager>();
   ```

2. **Add Authentication**:
   ```csharp
   services.AddAuthentication(...)
   [Authorize] on GameHub
   ```

3. **Enable HTTPS**:
   - Already configured in `Program.cs`
   - Add production SSL certificate

4. **Configure SignalR for scale**:
   ```csharp
   services.AddSignalR().AddAzureSignalR(...);
   ```

5. **Add Rate Limiting**:
   ```csharp
   services.AddRateLimiter(...)
   ```

## ?? Documentation

- **README.md** (this file) - Overview and quick start
- **MULTIPLAYER_DOCUMENTATION.md** - Detailed multiplayer architecture
- **BlackjackV3.Tests/README.md** - Test suite documentation

## ?? Screenshots

### Landing Page
Modern hero section with gradient backgrounds and game mode selection

### Single Player
Classic green felt table with smooth card animations

### Multiplayer
Beautiful multi-player interface with:
- Live player cards grid
- Dealer section with radial lighting
- Sticky control panel
- Real-time status updates

## ?? Future Enhancements

- [ ] **Persistent player profiles** with EF Core
- [ ] **Leaderboards** and statistics
- [ ] **Card counting hints** (optional feature)
- [ ] **Sound effects** for card dealing and wins
- [ ] **Splitting pairs** gameplay
- [ ] **Insurance** side bet
- [ ] **Tournament mode** with brackets
- [ ] **Chat system** in multiplayer
- [ ] **SVG card graphics** library
- [ ] **Mobile app** using MAUI

## ?? License

This is a sample/learning project. Feel free to use and modify!

## ?? Credits

Built to demonstrate:
- Clean architecture in ASP.NET Core
- Real-time communication with SignalR
- Modern UI design with CSS
- Test-driven development
- Multiplayer game state management

---

**Have fun and gamble responsibly!** ?? (It's fake money anyway ??)
