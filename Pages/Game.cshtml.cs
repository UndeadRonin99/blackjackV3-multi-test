using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using BlackjackV3.Domain;
using BlackjackV3.Models;
using BlackjackV3.Services;

namespace BlackjackV3.Pages;

/// <summary>
/// Page model for the blackjack game.
/// </summary>
public class GameModel : PageModel
{
    private readonly IGameStateStore _gameStateStore;
    private readonly IRandomProvider _randomProvider;
    private readonly ILogger<GameModel> _logger;

    [BindProperty]
    public GameViewModel ViewModel { get; set; } = new();

    public GameModel(
        IGameStateStore gameStateStore,
        IRandomProvider randomProvider,
      ILogger<GameModel> logger)
    {
      _gameStateStore = gameStateStore;
        _randomProvider = randomProvider;
     _logger = logger;
    }

    /// <summary>
    /// Gets the session ID for the current user.
    /// </summary>
    private string GetSessionId()
    {
        var sessionId = HttpContext.Session.GetString("SessionId");
        if (string.IsNullOrEmpty(sessionId))
    {
   sessionId = Guid.NewGuid().ToString();
            HttpContext.Session.SetString("SessionId", sessionId);
      }
        return sessionId;
    }

  /// <summary>
    /// Gets or creates the game for the current session.
    /// </summary>
    private BlackjackGame GetOrCreateGame()
    {
        var sessionId = GetSessionId();
        if (!_gameStateStore.TryGet(sessionId, out var game) || game == null)
        {
  game = new BlackjackGame(_randomProvider);
            _gameStateStore.Save(sessionId, game);
        }
        return game;
    }

    /// <summary>
    /// Handles GET requests to load the game state.
    /// </summary>
    public void OnGet()
    {
        var game = GetOrCreateGame();
        ViewModel.Snapshot = game.ToSnapshot();
    }

    /// <summary>
    /// Handles starting a new round with a bet.
  /// </summary>
    public IActionResult OnPostStart(int betAmount)
    {
        try
   {
        var game = GetOrCreateGame();
            
  if (betAmount < BlackjackGame.GetMinBet() || betAmount > BlackjackGame.GetMaxBet())
       {
                ViewModel.ErrorMessage = $"Bet must be between ${BlackjackGame.GetMinBet()} and ${BlackjackGame.GetMaxBet()}";
    ViewModel.Snapshot = game.ToSnapshot();
           return Page();
            }

            game.StartRound(betAmount);
       _gameStateStore.Save(GetSessionId(), game);
      
     ViewModel.Snapshot = game.ToSnapshot();
        ViewModel.TriggerDealAnimation = true;
            ViewModel.BetAmount = betAmount;

 return Page();
        }
        catch (Exception ex)
  {
     _logger.LogError(ex, "Error starting round");
     ViewModel.ErrorMessage = ex.Message;
     var game = GetOrCreateGame();
    ViewModel.Snapshot = game.ToSnapshot();
     return Page();
        }
    }

    /// <summary>
    /// Handles player hitting.
    /// </summary>
    public IActionResult OnPostHit()
    {
        try
        {
            var game = GetOrCreateGame();
            game.PlayerHit();
            _gameStateStore.Save(GetSessionId(), game);
            
      ViewModel.Snapshot = game.ToSnapshot();
         ViewModel.TriggerHitAnimation = true;

      return Page();
        }
     catch (Exception ex)
        {
      _logger.LogError(ex, "Error on hit");
    ViewModel.ErrorMessage = ex.Message;
     var game = GetOrCreateGame();
         ViewModel.Snapshot = game.ToSnapshot();
            return Page();
        }
    }

    /// <summary>
    /// Handles player standing.
    /// </summary>
    public IActionResult OnPostStand()
    {
        try
        {
            var game = GetOrCreateGame();
      game.PlayerStand();
      _gameStateStore.Save(GetSessionId(), game);
            
      ViewModel.Snapshot = game.ToSnapshot();
            ViewModel.TriggerRevealAnimation = true;

        return Page();
        }
        catch (Exception ex)
        {
    _logger.LogError(ex, "Error on stand");
            ViewModel.ErrorMessage = ex.Message;
        var game = GetOrCreateGame();
            ViewModel.Snapshot = game.ToSnapshot();
          return Page();
        }
    }

    /// <summary>
    /// Handles player doubling down.
    /// </summary>
    public IActionResult OnPostDouble()
 {
     try
   {
          var game = GetOrCreateGame();
   game.PlayerDouble();
  _gameStateStore.Save(GetSessionId(), game);
       
     ViewModel.Snapshot = game.ToSnapshot();
            ViewModel.TriggerHitAnimation = true;
            ViewModel.TriggerRevealAnimation = game.ToSnapshot().State == GameState.RoundOver;

            return Page();
        }
catch (Exception ex)
        {
     _logger.LogError(ex, "Error on double");
   ViewModel.ErrorMessage = ex.Message;
   var game = GetOrCreateGame();
            ViewModel.Snapshot = game.ToSnapshot();
            return Page();
        }
    }

    /// <summary>
    /// Handles restarting for a new round.
    /// </summary>
    public IActionResult OnPostRestart()
    {
        try
        {
       var game = GetOrCreateGame();
      game.Restart();
        _gameStateStore.Save(GetSessionId(), game);
      
       ViewModel.Snapshot = game.ToSnapshot();

    return Page();
    }
        catch (Exception ex)
        {
  _logger.LogError(ex, "Error on restart");
      ViewModel.ErrorMessage = ex.Message;
     var game = GetOrCreateGame();
      ViewModel.Snapshot = game.ToSnapshot();
   return Page();
        }
    }
}
