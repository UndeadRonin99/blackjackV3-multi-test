using System.Collections.Concurrent;
using BlackjackV3.Domain;

namespace BlackjackV3.Services;

/// <summary>
/// In-memory implementation of game state storage using a concurrent dictionary.
/// TODO: Replace with Redis or EF Core for persistent, distributed storage.
/// </summary>
public class InMemoryGameStateStore : IGameStateStore
{
    private readonly ConcurrentDictionary<string, BlackjackGame> _games = new();

    /// <summary>
    /// Attempts to retrieve a game for the specified session.
    /// </summary>
    /// <param name="sessionId">The session identifier.</param>
    /// <param name="game">The retrieved game, or null if not found.</param>
    /// <returns>True if the game was found; otherwise false.</returns>
    public bool TryGet(string sessionId, out BlackjackGame? game)
    {
        return _games.TryGetValue(sessionId, out game);
  }

    /// <summary>
    /// Saves a game for the specified session.
    /// </summary>
    /// <param name="sessionId">The session identifier.</param>
    /// <param name="game">The game to save.</param>
    public void Save(string sessionId, BlackjackGame game)
    {
      _games[sessionId] = game;
    }

    /// <summary>
    /// Resets/removes the game for the specified session.
    /// </summary>
    /// <param name="sessionId">The session identifier.</param>
    public void Reset(string sessionId)
    {
     _games.TryRemove(sessionId, out _);
    }
}
