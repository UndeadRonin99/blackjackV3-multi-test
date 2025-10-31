using BlackjackV3.Domain;

namespace BlackjackV3.Models;

/// <summary>
/// Model for displaying a hand of cards.
/// </summary>
public class HandDisplayModel
{
    /// <summary>
    /// Gets or sets the cards to display.
    /// </summary>
    public IReadOnlyList<Card> Cards { get; set; } = Array.Empty<Card>();

    /// <summary>
    /// Gets or sets whether the second card should be hidden (dealer hole card).
    /// </summary>
    public bool HideSecondCard { get; set; }

    /// <summary>
    /// Gets or sets the HTML ID for this hand container.
    /// </summary>
    public string HandId { get; set; } = string.Empty;
}
