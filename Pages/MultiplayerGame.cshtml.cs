using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using BlackjackV3.Models;

namespace BlackjackV3.Pages;

/// <summary>
/// Page model for multiplayer blackjack game.
/// </summary>
public class MultiplayerGameModel : PageModel
{
 [BindProperty(SupportsGet = true)]
    public string TableId { get; set; } = "default";

    [BindProperty(SupportsGet = true)]
    public string PlayerName { get; set; } = string.Empty;

    public MultiplayerGameViewModel ViewModel { get; set; } = new();

    public void OnGet()
    {
 // Generate random table ID if not provided
    if (string.IsNullOrEmpty(TableId) || TableId == "default")
        {
     TableId = Guid.NewGuid().ToString("N").Substring(0, 8);
        }

        // Use session-based player name if not provided
      if (string.IsNullOrEmpty(PlayerName))
 {
     PlayerName = HttpContext.Session.GetString("PlayerName") ?? $"Player{Random.Shared.Next(1000, 9999)}";
            HttpContext.Session.SetString("PlayerName", PlayerName);
        }

        ViewModel.TableId = TableId;
  ViewModel.PlayerName = PlayerName;
    }
}
