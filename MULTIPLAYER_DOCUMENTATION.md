# Multiplayer Blackjack Documentation

## Overview

The multiplayer Blackjack feature allows up to 5 players to join the same table and play simultaneously against the dealer. Each player has their own independent game instance with their own balance, bets, and hands, but they all share the same dealer and play synchronized rounds.

## Architecture

### Core Design Principles

1. **Domain Logic Unchanged**: The core `BlackjackGame` class remains unchanged. Each player gets their own instance.
2. **Independent Player Games**: Each player's game is completely independent - their own deck, balance, and decisions.
3. **Shared Dealer**: All players see the same dealer cards (from their respective game instances).
4. **Real-time Communication**: SignalR provides real-time updates to all players at the table.

### Key Components

#### Backend (C#)

**Services**:
- `PlayerState.cs` - Represents a player's state in a multiplayer game
- `GameTable.cs` - Manages a table with multiple players
- `ITableManager.cs` / `InMemoryTableManager.cs` - Manages all active tables

**Models**:
- `MultiplayerGameViewModel.cs` - View model for the multiplayer page
- `PlayerInfo.cs` - Information about a player to be sent to clients
- `SimpleCardInfo.cs` - Serializable card information
- `GameStateUpdate.cs` - DTO for SignalR game state updates

**Hub**:
- `GameHub.cs` - SignalR hub handling all multiplayer communication

**Pages**:
- `MultiplayerGame.cshtml` / `MultiplayerGame.cshtml.cs` - Razor page for multiplayer UI

#### Frontend (JavaScript)

**Scripts**:
- `multiplayer-game.js` - SignalR client and game UI logic

**Styles**:
- `multiplayer-game.css` - Styles for multiplayer game UI

## Game Flow

### 1. Joining a Table

```csharp
// Player connects and joins a table
await connection.invoke("JoinTable", tableId, playerName);
```

- Player provides table ID and their display name
- Server creates `PlayerState` with new `BlackjackGame` instance
- Player is added to the table's SignalR group
- All players receive update showing the new player

### 2. Placing Bets

```csharp
// Player places bet and readies up
await connection.invoke("PlaceBet", tableId, betAmount);
```

- Player chooses bet amount (validated: $10-$200, sufficient balance)
- Their individual `BlackjackGame.StartRound(bet)` is called
- Player marked as "ready"
- When **all** players are ready, round automatically starts

### 3. Playing the Round

```csharp
// Player performs actions
await connection.invoke("PerformAction", tableId, "Hit");
await connection.invoke("PerformAction", tableId, "Stand");
await connection.invoke("PerformAction", tableId, "Double");
```

- Each player plays their own hand independently
- Actions are validated against their individual game state
- All players see real-time updates
- When player stands/busts/doubles, they're marked as "turn complete"
- When **all** players complete their turn, round ends

### 4. Round End & Restart

```csharp
// Player readies for next round
await connection.invoke("ReadyForNextRound", tableId);
```

- Round ends when all players finish
- Outcomes and balance updates shown
- Players click "Ready for Next Round" to reset
- Process repeats

## SignalR Events

### Client ? Server (Invoke Methods)

| Method | Parameters | Description |
|--------|------------|-------------|
| `JoinTable` | `tableId`, `playerName` | Join a game table |
| `PlaceBet` | `tableId`, `betAmount` | Place bet and ready up |
| `PerformAction` | `tableId`, `action` | Perform game action (Hit/Stand/Double) |
| `ReadyForNextRound` | `tableId` | Reset for next round |

### Server ? Client (Events)

| Event | Parameters | Description |
|-------|------------|-------------|
| `GameStateUpdated` | `GameStateUpdate` | Complete game state for player |
| `PlayerJoined` | `connectionId` | New player joined |
| `PlayerLeft` | `playerName` | Player disconnected |
| `PlayerPlacedBet` | `playerName`, `betAmount` | Player placed bet |
| `RoundStarted` | - | All players ready, round begins |
| `RoundEnded` | - | All players finished |
| `Error` | `message` | Error occurred |

## Game State Update Structure

```typescript
interface GameStateUpdate {
    playerSnapshot: GameSnapshot;      // Player's own game state
    allPlayers: PlayerInfo[];       // Info about all players
    dealerCards: SimpleCardInfo[];     // Dealer's cards
    dealerTotal: number;               // Dealer's visible total
    dealerHoleCardHidden: boolean;     // Is hole card hidden?
    isRoundInProgress: boolean;        // Round active?
    message: string;        // Status message
}
```

## Key Differences from Single Player

### What's Different:
1. **Round Synchronization**: Rounds start only when all players are ready
2. **Turn Completion**: Round ends only when all players finish
3. **Multiple Game Instances**: Each player has separate `BlackjackGame` instance
4. **Real-time Updates**: All players see each other's actions in real-time
5. **Shared Table UI**: All players and dealer visible on one screen

### What's the Same:
1. **Core Game Rules**: Identical Blackjack rules (hit to soft 17, 3:2 blackjack, etc.)
2. **Domain Logic**: Uses the exact same `BlackjackGame` class
3. **Bet Limits**: Same $10-$200 range
4. **Starting Balance**: Each player starts with $1,000
5. **Animations**: Same card dealing animations

## Usage Examples

### Creating/Joining a Table

```html
<!-- Create a new table (auto-generated ID) -->
<a href="/MultiplayerGame">Create New Table</a>

<!-- Join specific table -->
<a href="/MultiplayerGame?tableId=abc123&playerName=Alice">Join Table</a>
```

### Sharing a Table

Players can share their table URL:
```
https://yoursite.com/MultiplayerGame?tableId=abc123
```

Friends clicking this link will join the same table.

## Implementation Details

### Table Management

```csharp
// Tables are stored in-memory (singleton)
services.AddSingleton<ITableManager, InMemoryTableManager>();

// Each table can have up to 5 players
public class GameTable
{
    public int MaxPlayers { get; init; } = 5;
    private ConcurrentDictionary<string, PlayerState> _players;
    // ...
}
```

### Player State

```csharp
public class PlayerState
{
    public string ConnectionId { get; init; }
    public string PlayerName { get; init; }
    public BlackjackGame Game { get; init; }    // Individual game instance!
    public bool IsReady { get; set; }
    public bool HasCompletedTurn { get; set; }
}
```

### Synchronization Logic

```csharp
// Start round when all ready
if (table.AreAllPlayersReady() && !table.IsRoundInProgress)
{
    table.IsRoundInProgress = true;
    await Clients.Group(tableId).SendAsync("RoundStarted");
}

// End round when all complete
if (table.HaveAllPlayersCompletedTurn())
{
    await EndRound(tableId);
}
```

## Testing the Multiplayer Feature

### Local Testing

1. **Open Multiple Browser Tabs/Windows**:
   ```
   Tab 1: http://localhost:5000/MultiplayerGame?playerName=Alice
   Tab 2: http://localhost:5000/MultiplayerGame?playerName=Bob
   ```
   (Use the same table ID from Tab 1's URL)

2. **Use Different Browsers**:
   - Chrome, Edge, Firefox simultaneously
   - Better simulation of separate users

3. **Browser Private/Incognito Windows**:
   - Separate sessions for realistic testing

### Test Scenarios

1. **Basic Flow**:
   - Player 1 joins ? Player 2 joins
   - Both place bets ? Round starts
   - Both play hands ? Round ends
   - Both ready up ? New round

2. **Edge Cases**:
   - Player disconnects mid-round
   - Player joins during active round
- One player very slow, others waiting
   - Table reaches max capacity (6th player)

3. **Bet Validation**:
   - Try invalid bet amounts
   - Try bet > balance
   - Try betting when already ready

## Future Enhancements

### Potential Additions

1. **Persistent Storage**:
   - Replace `InMemoryTableManager` with Redis/SQL
   - Store player balances across sessions
   - Game history and statistics

2. **Chat System**:
   - Table chat via SignalR
   - Predefined message buttons

3. **Lobby System**:
   - Browse active tables
   - Table creation with custom settings
   - Private vs public tables

4. **Advanced Features**:
   - Side bets (insurance, pair bets)
   - Multiple simultaneous hands per player
 - Tournament mode
   - Leaderboards

5. **Security**:
   - Authentication/authorization
   - Rate limiting
   - Anti-cheat measures

6. **Scalability**:
   - Redis backplane for multi-server SignalR
   - Horizontal scaling with sticky sessions
   - Table sharding by region

## Troubleshooting

### Common Issues

**Players not seeing updates**:
- Check SignalR connection status
- Verify players in same table ID
- Check browser console for errors

**Round not starting**:
- Ensure all players clicked "Place Bet & Ready Up"
- Check all players have sufficient balance

**Disconnection handling**:
- Players automatically removed on disconnect
- Reconnection requires rejoining table

### Debug Mode

Enable detailed SignalR logging:
```javascript
const connection = new signalR.HubConnectionBuilder()
    .withUrl("/hub/game")
    .configureLogging(signalR.LogLevel.Trace)  // Maximum logging
    .build();
```

## Performance Considerations

- **In-Memory Tables**: Lost on server restart (acceptable for demo)
- **SignalR Overhead**: Broadcasts to all table members on each action
- **Concurrent Players**: Tested up to 5 players per table
- **Table Cleanup**: Empty tables removed automatically

## Security Notes

- Table IDs are GUIDs (hard to guess)
- No authentication in v1 (add for production)
- Validate all inputs server-side
- Rate limiting recommended for production
- Consider adding table passwords

---

## Quick Start

```bash
# Run the application
dotnet run

# Open browser to
http://localhost:5000

# Click "Play Multiplayer"
# Share the table link with a friend
# Both place bets and play!
```
