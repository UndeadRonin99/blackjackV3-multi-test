using BlackjackV3.Domain;

namespace BlackjackV3.Models;

/// <summary>
/// Model for displaying a single card.
/// </summary>
public class CardDisplayModel
{
    /// <summary>
    /// Gets or sets the card to display.
    /// </summary>
    public Card? Card { get; set; }

    /// <summary>
    /// Gets or sets whether the card should be shown face down.
    /// </summary>
    public bool FaceDown { get; set; }

    /// <summary>
    /// Gets or sets the index of this card in the hand (for animation delays).
    /// </summary>
    public int CardIndex { get; set; }
}
