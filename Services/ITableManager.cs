using System.Collections.Concurrent;

namespace BlackjackV3.Services;

/// <summary>
/// Service for managing multiplayer game tables.
/// </summary>
public interface ITableManager
{
    /// <summary>
    /// Gets or creates a table with the specified ID.
    /// </summary>
    /// <param name="tableId">The table identifier.</param>
    /// <returns>The game table.</returns>
    GameTable GetOrCreateTable(string tableId);

    /// <summary>
    /// Gets a table by ID if it exists.
    /// </summary>
    /// <param name="tableId">The table identifier.</param>
    /// <param name="table">The table if found.</param>
    /// <returns>True if the table was found; otherwise false.</returns>
    bool TryGetTable(string tableId, out GameTable? table);

    /// <summary>
    /// Removes a table if it's empty.
    /// </summary>
    /// <param name="tableId">The table identifier.</param>
    void RemoveTableIfEmpty(string tableId);

    /// <summary>
    /// Gets all active table IDs.
    /// </summary>
    /// <returns>A collection of active table IDs.</returns>
    IEnumerable<string> GetActiveTableIds();
}

/// <summary>
/// In-memory implementation of table manager.
/// </summary>
public class InMemoryTableManager : ITableManager
{
    private readonly ConcurrentDictionary<string, GameTable> _tables = new();

    public GameTable GetOrCreateTable(string tableId)
    {
        return _tables.GetOrAdd(tableId, id => new GameTable { TableId = id });
    }

    public bool TryGetTable(string tableId, out GameTable? table)
    {
        return _tables.TryGetValue(tableId, out table);
 }

    public void RemoveTableIfEmpty(string tableId)
    {
      if (_tables.TryGetValue(tableId, out var table) && table.IsEmpty)
     {
         _tables.TryRemove(tableId, out _);
        }
    }

    public IEnumerable<string> GetActiveTableIds()
    {
        return _tables.Keys.ToList();
    }
}
