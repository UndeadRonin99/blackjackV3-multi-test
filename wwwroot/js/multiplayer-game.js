/**
 * Multiplayer Blackjack Game Client using SignalR
 */
class MultiplayerGameClient {
 constructor(tableId, playerName) {
this.tableId = tableId;
        this.playerName = playerName;
this.connection = null;
        this.currentGameState = null;
 this.connectionId = null;
        this.hasReceivedFirstUpdate = false;
    this.initializeEventListeners();
    }

    /**
     * Initialize UI event listeners
     */
    initializeEventListeners() {
    // Bet adjustment buttons
    document.getElementById('increaseBet')?.addEventListener('click', () => {
     const input = document.getElementById('betAmount');
      const current = parseInt(input.value);
    if (current < 200) {
    input.value = Math.min(200, current + 10);
      }
        });

        document.getElementById('decreaseBet')?.addEventListener('click', () => {
        const input = document.getElementById('betAmount');
        const current = parseInt(input.value);
    if (current > 10) {
   input.value = Math.max(10, current - 10);
     }
    });

      // Bet preset buttons
        document.querySelectorAll('.btn-preset').forEach(btn => {
  btn.addEventListener('click', () => {
      const amount = btn.getAttribute('data-amount');
      document.getElementById('betAmount').value = amount;
        });
   });

        // Share modal
        document.getElementById('shareTableBtn')?.addEventListener('click', () => {
      document.getElementById('shareModalOverlay').style.display = 'flex';
        });

  document.getElementById('closeShareModal')?.addEventListener('click', () => {
            document.getElementById('shareModalOverlay').style.display = 'none';
      });

        document.getElementById('shareModalOverlay')?.addEventListener('click', (e) => {
    if (e.target.id === 'shareModalOverlay') {
    e.target.style.display = 'none';
     }
    });

   // Copy link button
        document.getElementById('copyLinkBtn')?.addEventListener('click', () => {
        const input = document.getElementById('shareLink');
      input.select();
document.execCommand('copy');
  
     const btn = document.getElementById('copyLinkBtn');
    const originalHTML = btn.innerHTML;
   btn.innerHTML = '<svg width="16" height="16" fill="currentColor" viewBox="0 0 16 16"><path d="M13.854 3.646a.5.5 0 0 1 0 .708l-7 7a.5.5 0 0 1-.708 0l-3.5-3.5a.5.5 0 1 1 .708-.708L6.5 10.293l6.646-6.647a.5.5 0 0 1 .708 0z"/></svg> Copied!';
  setTimeout(() => {
   btn.innerHTML = originalHTML;
}, 2000);
     });

        // Game action buttons
   document.getElementById('placeBetBtn')?.addEventListener('click', () => {
   if (this) this.placeBet();
 });

    document.getElementById('hitBtn')?.addEventListener('click', () => {
       if (this) this.performAction('Hit');
        });

     document.getElementById('standBtn')?.addEventListener('click', () => {
  if (this) this.performAction('Stand');
 });

        document.getElementById('doubleBtn')?.addEventListener('click', () => {
  if (this) this.performAction('Double');
    });

      document.getElementById('nextRoundBtn')?.addEventListener('click', () => {
  if (this) this.readyForNextRound();
    });
    }

    /**
     * Start the SignalR connection and join the table
     */
    async start() {
        this.connection = new signalR.HubConnectionBuilder()
      .withUrl("/hub/game")
    .withAutomaticReconnect()
   .configureLogging(signalR.LogLevel.Information)
      .build();

        this.registerEventHandlers();

        try {
     await this.connection.start();
      console.log("Connected to game server");
    this.updateConnectionStatus("Connected to server", "success");

    const joined = await this.connection.invoke("JoinTable", this.tableId, this.playerName);
    
 if (joined) {
  console.log(`Joined table ${this.tableId}`);
   this.connectionId = this.connection.connectionId;
     } else {
     this.updateConnectionStatus("Failed to join table", "danger");
  }
        } catch (err) {
      console.error("Connection error:", err);
  this.updateConnectionStatus("Connection failed - Please refresh", "danger");
}
    }

  /**
   * Register SignalR event handlers
   */
    registerEventHandlers() {
      this.connection.on("GameStateUpdated", (update) => {
   console.log("Game state updated:", update);
      this.currentGameState = update;
   this.hasReceivedFirstUpdate = true;
       this.renderGameState(update);
     });

     this.connection.on("PlayerJoined", (connectionId) => {
  console.log("Player joined:", connectionId);
     });

        this.connection.on("PlayerLeft", (playerName) => {
 console.log("Player left:", playerName);
 this.showToast(`${playerName} left the table`, 'info');
 });

    this.connection.on("PlayerPlacedBet", (playerName, betAmount) => {
       console.log(`${playerName} placed bet: $${betAmount}`);
    this.showToast(`${playerName} is ready! (Bet: $${betAmount})`, 'success');
 });

   this.connection.on("RoundStarted", () => {
console.log("Round started!");
   this.showToast("Round started! Good luck! ??", 'success');
   });

  this.connection.on("RoundEnded", () => {
    console.log("Round ended!");
      this.showToast("Round complete! ??", 'info');
 });

        this.connection.on("YourTurn", () => {
     console.log("It's your turn!");
       this.showToast("It's your turn! ??", 'success');
   // Optional: Play a sound or show a visual notification
       if (document.getElementById("gameMessage")) {
        const messageEl = document.getElementById("gameMessage");
    messageEl.style.background = '#fef3c7';
   messageEl.style.color = '#92400e';
messageEl.style.border = '2px solid #f59e0b';
    messageEl.style.animation = 'pulse 1s ease-in-out 3';
 }
        });

    this.connection.on("WaitingForPlayers", (message) => {
   console.log("Waiting for players:", message);
       const messageEl = document.getElementById("gameMessage");
     if (messageEl) {
         messageEl.textContent = message;
      messageEl.style.background = '#dbeafe';
   messageEl.style.color = '#1e40af';
   messageEl.style.border = '2px solid #3b82f6';
            }
  });

        this.connection.on("AllPlayersReadyForBetting", () => {
       console.log("All players ready for betting!");
       this.showToast("All players ready! Place your bets! ??", 'success');
   const messageEl = document.getElementById("gameMessage");
   if (messageEl) {
           messageEl.textContent = "All players ready! Place your bet to start!";
    messageEl.style.background = '#d1fae5';
         messageEl.style.color = '#065f46';
   messageEl.style.border = '2px solid #10b981';
  }
        });

   this.connection.on("Error", (message) => {
        console.error("Server error:", message);
this.showToast(message, 'danger');
   });

  this.connection.onreconnecting((error) => {
   console.log("Reconnecting...", error);
    this.updateConnectionStatus("Reconnecting...", "warning");
      });

  this.connection.onreconnected((connectionId) => {
console.log("Reconnected:", connectionId);
   this.updateConnectionStatus("Reconnected to server", "success");
  });

      this.connection.onclose((error) => {
console.log("Disconnected:", error);
    this.updateConnectionStatus("Disconnected - Please refresh", "danger");
});
    }

    /**
     * Place a bet and ready up
   */
    async placeBet() {
    const betAmount = parseInt(document.getElementById("betAmount").value);
    
     if (betAmount < 10 || betAmount > 200) {
          this.showToast("Bet must be between $10 and $200", 'danger');
 return;
  }

     try {
  await this.connection.invoke("PlaceBet", this.tableId, betAmount);
       // Don't hide controls here - let the state update handle it
        } catch (err) {
  console.error("Error placing bet:", err);
         this.showToast("Failed to place bet", 'danger');
  }
    }

    /**
   * Perform a game action (Hit, Stand, Double)
   */
async performAction(action) {
     try {
       await this.connection.invoke("PerformAction", this.tableId, action);
     } catch (err) {
      console.error(`Error performing ${action}:`, err);
      this.showToast(`Failed to ${action}`, 'danger');
 }
    }

    /**
     * Ready for next round
     */
async readyForNextRound() {
     try {
   await this.connection.invoke("ReadyForNextRound", this.tableId);
        } catch (err) {
       console.error("Error readying for next round:", err);
        this.showToast("Failed to ready for next round", 'danger');
  }
    }

    /**
     * Render the complete game state
     */
  renderGameState(update) {
  if (!update) return;

        this.renderDealerHand(update.dealerCards, update.dealerTotal, update.dealerHoleCardHidden);
        this.renderPlayers(update.allPlayers);

  if (update.playerSnapshot) {
  this.updatePlayerControls(update.playerSnapshot, update.isRoundInProgress);
        }

    if (update.message) {
   document.getElementById("gameMessage").textContent = update.message;
        }
    }

    /**
  * Render dealer's hand
     */
    renderDealerHand(cards, total, holeCardHidden) {
      const dealerHand = document.getElementById("dealerHand");
     dealerHand.innerHTML = "";

  cards.forEach((card, index) => {
 const isFaceDown = holeCardHidden && index === 1;
        const cardElement = this.createCardElement(card, isFaceDown);
    dealerHand.appendChild(cardElement);
        });

   document.getElementById("dealerTotal").textContent = total;
  }

  /**
     * Render all players at the table
     */
    renderPlayers(players) {
const container = document.getElementById("playersContainer");
        container.innerHTML = "";

        // Update player count
  document.getElementById("playerCountDisplay").textContent = `${players.length}/5`;

      players.forEach(player => {
 const playerCard = this.createPlayerCard(player);
     container.appendChild(playerCard);
});
    }

    /**
     * Create a player card element
     */
    createPlayerCard(player) {
        const card = document.createElement("div");
        
// GameState enum: WaitingForBet=0, Dealing=1, PlayerTurn=2, DealerTurn=3, RoundOver=4
        const state = player.gameState;
   const isPlayerTurn = (state === 2 || state === "PlayerTurn");
        card.className = `player-card ${player.isCurrentUser ? 'current-user' : ''} ${isPlayerTurn ? 'active-turn' : ''}`;
     
   let statusBadge = '';
        let statusClass = '';
 
    if (player.isReady && (state === 0 || state === "WaitingForBet" || state === 1 || state === "Dealing")) {
   statusBadge = '? Ready';
         statusClass = 'ready';
     } else if (state === 4 || state === "RoundOver") {
    statusBadge = 'Finished';
          statusClass = 'finished';
     } else if (state === 2 || state === "PlayerTurn") {
      statusBadge = '? Playing';
      statusClass = 'playing';
        } else if (state === 3 || state === "DealerTurn") {
   statusBadge = '? Dealer Turn';
       statusClass = 'finished';
     }

     const handHTML = player.hand.map(c => this.createCardElement(c, false).outerHTML).join('');
  
   // GameOutcome enum: None=0, so check if outcome is not 0 or "None"
        const hasOutcome = player.outcome && player.outcome !== 0 && player.outcome !== "None";
  const outcomeHTML = hasOutcome ? `<div class="player-outcome">${this.getOutcomeDisplay(player.outcome)}</div>` : '';

        card.innerHTML = `
  <div class="player-card-header">
         <div class="player-name">${player.name}${player.isCurrentUser ? ' ??' : ''}</div>
      ${statusBadge ? `<div class="player-status ${statusClass}">${statusBadge}</div>` : ''}
  </div>
        <div class="player-stats">
  <div class="stat">
          <span class="stat-label">Balance</span>
            <span class="stat-value balance">$${player.balance}</span>
        </div>
    <div class="stat">
      <span class="stat-label">Bet</span>
   <span class="stat-value bet">$${player.currentBet}</span>
         </div>
 </div>
    <div class="player-hand">
    ${handHTML}
        </div>
  <div class="player-total">Total: ${player.handTotal}</div>
       ${outcomeHTML}
      `;

   return card;
    }

    /**
     * Get friendly outcome display text
     */
    getOutcomeDisplay(outcome) {
        // GameOutcome enum: None=0, PlayerBlackjack=1, DealerBlackjack=2, PlayerBust=3,
        // DealerBust=4, Push=5, PlayerWin=6, DealerWin=7
   const displays = {
      // Numeric values
       0: '',
1: '?? Blackjack!',
   2: '? Dealer Blackjack',
     3: '?? Bust',
     4: '? Dealer Bust - Win!',
       5: '?? Push',
     6: '? Win!',
 7: '? Lose',
    // String values (fallback)
    'None': '',
      'PlayerBlackjack': '?? Blackjack!',
        'DealerBlackjack': '? Dealer Blackjack',
  'PlayerBust': '?? Bust',
       'DealerBust': '? Dealer Bust - Win!',
   'Push': '?? Push',
   'PlayerWin': '? Win!',
  'DealerWin': '? Lose'
    };
   return displays[outcome] || '';
    }

    /**
     * Create a card DOM element
 */
    createCardElement(card, isFaceDown) {
     const cardDiv = document.createElement("div");
      cardDiv.className = "playing-card card-deal-animation";

    if (isFaceDown) {
  cardDiv.classList.add("face-down");
          cardDiv.innerHTML = '??';
        } else {
     const isRed = card.suit === 'Hearts' || card.suit === 'Diamonds';
    cardDiv.classList.add(isRed ? 'red' : 'black');
     
  let rankDisplay = card.rank;
if (card.rank === 'Jack') rankDisplay = 'J';
    else if (card.rank === 'Queen') rankDisplay = 'Q';
       else if (card.rank === 'King') rankDisplay = 'K';
       else if (card.rank === 'Ace') rankDisplay = 'A';

 let suitSymbol = '';
    if (card.suit === 'Hearts') suitSymbol = '?';
     else if (card.suit === 'Diamonds') suitSymbol = '?';
  else if (card.suit === 'Clubs') suitSymbol = '?';
        else if (card.suit === 'Spades') suitSymbol = '?';

     cardDiv.innerHTML = `
 <div class="card-rank">${rankDisplay}</div>
        <div class="card-suit">${suitSymbol}</div>
       `;
    }

return cardDiv;
  }

    /**
     * Update player's own game controls based on game state
     */
    updatePlayerControls(snapshot, isRoundInProgress) {
        const betControls = document.getElementById("betControls");
        const gameControls = document.getElementById("gameControls");
     const nextRoundControls = document.getElementById("nextRoundControls");
        const waitingMessage = document.getElementById("waitingMessage");

   // Update balance
        document.getElementById("playerBalance").textContent = `$${snapshot.balance}`;

        console.log("Updating controls - State:", snapshot.state, "Round in progress:", isRoundInProgress);
  console.log("Full snapshot:", JSON.stringify(snapshot, null, 2));

        // Hide all controls first
        betControls.style.display = "none";
        gameControls.style.display = "none";
        nextRoundControls.style.display = "none";
        waitingMessage.style.display = "none";

        // GameState enum values from C#:
    // WaitingForBet = 0, Dealing = 1, PlayerTurn = 2, DealerTurn = 3, RoundOver = 4
        const state = snapshot.state;
        
        if (state === 0 || state === "WaitingForBet") {
            console.log("Showing bet controls");
   betControls.style.display = "flex";
        } else if (state === 2 || state === "PlayerTurn") {
     console.log("Showing game controls");
   // Player's turn - show game controls
    gameControls.style.display = "grid";
       
    // Enable/disable buttons based on what's allowed
            document.getElementById("hitBtn").disabled = !snapshot.canHit;
            document.getElementById("standBtn").disabled = !snapshot.canStand;
  document.getElementById("doubleBtn").disabled = !snapshot.canDouble;
        } else if (state === 1 || state === "Dealing") {
       console.log("Showing waiting (dealing)");
     // Cards dealt but waiting for other players
      waitingMessage.style.display = "flex";
   waitingMessage.innerHTML = '<div class="spinner"></div><span>Waiting for other players...</span>';
      } else if (state === 3 || state === "DealerTurn") {
  console.log("Showing waiting (dealer turn)");
       // Dealer is playing
          waitingMessage.style.display = "flex";
          waitingMessage.innerHTML = '<div class="spinner"></div><span>Dealer is playing...</span>';
        } else if (state === 4 || state === "RoundOver") {
          console.log("Showing next round button or waiting");
    
          // Check if the round is still in progress (other players still playing)
            if (isRoundInProgress) {
        // Player is done but round continues - show waiting
     waitingMessage.style.display = "flex";
            waitingMessage.innerHTML = '<div class="spinner"></div><span>Waiting for other players to finish...</span>';
      } else {
// Round is completely over - show next round button
     nextRoundControls.style.display = "block";
   }
        } else {
          // Unknown state - log it and default to bet controls
      console.warn("Unknown game state:", state, "Full snapshot:", snapshot);

            // Default to bet controls if no cards
          if (snapshot.playerCards && snapshot.playerCards.length === 0) {
             console.log("No cards - defaulting to bet controls");
     betControls.style.display = "flex";
            } else {
                waitingMessage.style.display = "flex";
        waitingMessage.innerHTML = '<div class="spinner"></div><span>Loading game state...</span>';
      }
     }
    }

  /**
     * Update connection status display
 */
    updateConnectionStatus(message, type) {
  const statusDiv = document.getElementById("connectionStatus");
  const statusText = document.getElementById("statusText");
     
    statusDiv.className = `connection-status ${type}`;
  statusText.textContent = message;
    }

    /**
     * Show a toast notification
     */
  showToast(message, type = 'info') {
        // Simple implementation - you can enhance with a toast library
      console.log(`[${type.toUpperCase()}] ${message}`);
     
        // Visual feedback in game message if appropriate
        if (type === 'danger' || type === 'warning') {
  const messageEl = document.getElementById("gameMessage");
 if (messageEl) {
     messageEl.textContent = message;
        messageEl.style.background = type === 'danger' ? '#fee2e2' : '#fef3c7';
   messageEl.style.color = type === 'danger' ? '#991b1b' : '#92400e';
      }
   }
    }
}

// Initialize when DOM is ready
document.addEventListener("DOMContentLoaded", () => {
    console.log("Multiplayer game UI initialized");
});
