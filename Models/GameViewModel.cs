using BlackjackV3.Domain;

namespace BlackjackV3.Models;

/// <summary>
/// View model for the game page, containing the game snapshot and UI state.
/// </summary>
public class GameViewModel
{
    /// <summary>
    /// Gets or sets the game snapshot.
    /// </summary>
    public GameSnapshot Snapshot { get; set; } = new();

    /// <summary>
    /// Gets or sets the bet amount input by the user.
    /// </summary>
    public int BetAmount { get; set; } = 50;

    /// <summary>
    /// Gets or sets whether to trigger deal animation.
    /// </summary>
    public bool TriggerDealAnimation { get; set; }

 /// <summary>
    /// Gets or sets whether to trigger hit animation.
    /// </summary>
    public bool TriggerHitAnimation { get; set; }

    /// <summary>
    /// Gets or sets whether to trigger dealer reveal animation.
    /// </summary>
    public bool TriggerRevealAnimation { get; set; }

    /// <summary>
    /// Gets or sets an error message to display.
    /// </summary>
    public string? ErrorMessage { get; set; }
}
