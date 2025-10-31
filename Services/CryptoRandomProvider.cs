using System.Security.Cryptography;

namespace BlackjackV3.Services;

/// <summary>
/// Cryptographically secure random number provider using RandomNumberGenerator.
/// </summary>
public class CryptoRandomProvider : IRandomProvider
{
    /// <summary>
    /// Returns a cryptographically secure random integer in the range [0, maxValue).
    /// </summary>
    /// <param name="maxValue">The exclusive upper bound of the random number.</param>
    /// <returns>A random integer between 0 (inclusive) and maxValue (exclusive).</returns>
    public int Next(int maxValue)
    {
        if (maxValue <= 0)
       throw new ArgumentOutOfRangeException(nameof(maxValue), "maxValue must be greater than 0");

        return RandomNumberGenerator.GetInt32(maxValue);
    }
}
