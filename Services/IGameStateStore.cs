using BlackjackV3.Domain;

namespace BlackjackV3.Services;

/// <summary>
/// Abstraction for storing and retrieving game state by session.
/// </summary>
public interface IGameStateStore
{
    /// <summary>
    /// Attempts to retrieve a game for the specified session.
    /// </summary>
    /// <param name="sessionId">The session identifier.</param>
    /// <param name="game">The retrieved game, or null if not found.</param>
    /// <returns>True if the game was found; otherwise false.</returns>
    bool TryGet(string sessionId, out BlackjackGame? game);

    /// <summary>
    /// Saves a game for the specified session.
    /// </summary>
    /// <param name="sessionId">The session identifier.</param>
    /// <param name="game">The game to save.</param>
    void Save(string sessionId, BlackjackGame game);

/// <summary>
    /// Resets/removes the game for the specified session.
    /// </summary>
    /// <param name="sessionId">The session identifier.</param>
    void Reset(string sessionId);
}
