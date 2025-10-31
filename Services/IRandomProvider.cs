namespace BlackjackV3.Services;

/// <summary>
/// Provides cryptographically secure random numbers for game operations.
/// </summary>
public interface IRandomProvider
{
    /// <summary>
    /// Returns a random integer in the range [0, maxValue).
    /// </summary>
    /// <param name="maxValue">The exclusive upper bound of the random number.</param>
    /// <returns>A random integer between 0 (inclusive) and maxValue (exclusive).</returns>
    int Next(int maxValue);
}
