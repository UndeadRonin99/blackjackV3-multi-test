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
    document.getElementById("waitingMessage").style.display = "none";
   });

        this.connection.on("RoundEnded", () => {
          console.log("Round ended!");
      this.showToast("Round complete! ??", 'info');
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
            document.getElementById("betControls").style.display = "none";
         document.getElementById("waitingMessage").style.display = "flex";
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
 document.getElementById("nextRoundControls").style.display = "none";
            document.getElementById("betControls").style.display = "flex";
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
            this.updatePlayerControls(update.playerSnapshot);
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
        card.className = `player-card ${player.isCurrentUser ? 'current-user' : ''} ${player.gameState === 'PlayerTurn' ? 'active-turn' : ''}`;
     
        let statusBadge = '';
        let statusClass = '';
   if (player.isReady) {
  statusBadge = '? Ready';
         statusClass = 'ready';
        } else if (player.gameState === 'RoundOver') {
statusBadge = 'Finished';
            statusClass = 'finished';
   } else if (player.gameState === 'PlayerTurn') {
       statusBadge = '? Playing';
            statusClass = 'playing';
   }

        const handHTML = player.hand.map(c => this.createCardElement(c, false).outerHTML).join('');
        const outcomeHTML = player.outcome !== 'None' ? `<div class="player-outcome">${this.getOutcomeDisplay(player.outcome)}</div>` : '';

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
        const displays = {
        'PlayerBlackjack': '?? Blackjack!',
  'PlayerWin': '? Win!',
            'PlayerBust': '?? Bust',
    'DealerBust': '? Dealer Bust - Win!',
   'DealerWin': '? Lose',
  'Push': '?? Push',
            'DealerBlackjack': '? Dealer Blackjack'
    };
        return displays[outcome] || outcome;
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
    updatePlayerControls(snapshot) {
        const betControls = document.getElementById("betControls");
        const gameControls = document.getElementById("gameControls");
        const nextRoundControls = document.getElementById("nextRoundControls");
    const waitingMessage = document.getElementById("waitingMessage");

        document.getElementById("playerBalance").textContent = `$${snapshot.balance}`;

     if (snapshot.state === "WaitingForBet") {
            betControls.style.display = "flex";
            gameControls.style.display = "none";
  nextRoundControls.style.display = "none";
       waitingMessage.style.display = "none";
     } else if (snapshot.state === "PlayerTurn") {
            betControls.style.display = "none";
            gameControls.style.display = "grid";
            nextRoundControls.style.display = "none";
            waitingMessage.style.display = "none";

            document.getElementById("hitBtn").disabled = !snapshot.canHit;
            document.getElementById("standBtn").disabled = !snapshot.canStand;
       document.getElementById("doubleBtn").disabled = !snapshot.canDouble;
        } else if (snapshot.state === "RoundOver") {
     betControls.style.display = "none";
          gameControls.style.display = "none";
            nextRoundControls.style.display = "block";
            waitingMessage.style.display = "none";
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
