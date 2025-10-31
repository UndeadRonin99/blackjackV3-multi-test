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
    }

  /**
     * Start the SignalR connection and join the table
     */
    async start() {
    // Build SignalR connection
   this.connection = new signalR.HubConnectionBuilder()
    .withUrl("/hub/game")
      .withAutomaticReconnect()
      .configureLogging(signalR.LogLevel.Information)
            .build();

        // Register event handlers
     this.registerEventHandlers();

     try {
   // Start connection
     await this.connection.start();
   console.log("Connected to game server");
   this.updateConnectionStatus("Connected", "success");

  // Join the table
            const joined = await this.connection.invoke("JoinTable", this.tableId, this.playerName);
            
          if (joined) {
    console.log(`Joined table ${this.tableId}`);
   this.connectionId = this.connection.connectionId;
} else {
          this.updateConnectionStatus("Failed to join table", "danger");
    }
   } catch (err) {
     console.error("Connection error:", err);
       this.updateConnectionStatus("Connection failed", "danger");
       }
    }

    /**
 * Register SignalR event handlers
     */
  registerEventHandlers() {
  // Game state updated
        this.connection.on("GameStateUpdated", (update) => {
      console.log("Game state updated:", update);
     this.currentGameState = update;
      this.renderGameState(update);
  });

  // Player joined
   this.connection.on("PlayerJoined", (connectionId) => {
       console.log("Player joined:", connectionId);
   });

        // Player left
     this.connection.on("PlayerLeft", (playerName) => {
            console.log("Player left:", playerName);
     this.showNotification(`${playerName} left the table`);
    });

// Player placed bet
        this.connection.on("PlayerPlacedBet", (playerName, betAmount) => {
     console.log(`${playerName} placed bet: $${betAmount}`);
       this.showNotification(`${playerName} is ready! (Bet: $${betAmount})`);
  });

   // Round started
        this.connection.on("RoundStarted", () => {
     console.log("Round started!");
   this.showNotification("Round started! Good luck!");
            document.getElementById("waitingMessage").style.display = "none";
  });

        // Round ended
  this.connection.on("RoundEnded", () => {
     console.log("Round ended!");
     this.showNotification("Round complete!");
    });

   // Error messages
     this.connection.on("Error", (message) => {
         console.error("Server error:", message);
  this.showError(message);
        });

    // Reconnecting
   this.connection.onreconnecting((error) => {
         console.log("Reconnecting...", error);
    this.updateConnectionStatus("Reconnecting...", "warning");
  });

     // Reconnected
 this.connection.onreconnected((connectionId) => {
   console.log("Reconnected:", connectionId);
      this.updateConnectionStatus("Reconnected", "success");
   });

   // Disconnected
        this.connection.onclose((error) => {
      console.log("Disconnected:", error);
       this.updateConnectionStatus("Disconnected", "danger");
 });
    }

    /**
     * Place a bet and ready up
     */
    async placeBet() {
        const betAmount = parseInt(document.getElementById("betAmount").value);
    
        if (betAmount < 10 || betAmount > 200) {
  this.showError("Bet must be between $10 and $200");
return;
        }

        try {
            await this.connection.invoke("PlaceBet", this.tableId, betAmount);
       document.getElementById("betControls").style.display = "none";
       document.getElementById("waitingMessage").style.display = "block";
  } catch (err) {
       console.error("Error placing bet:", err);
  this.showError("Failed to place bet");
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
        this.showError(`Failed to ${action}`);
        }
 }

    /**
 * Ready for next round
     */
    async readyForNextRound() {
        try {
     await this.connection.invoke("ReadyForNextRound", this.tableId);
       document.getElementById("nextRoundControls").style.display = "none";
       document.getElementById("betControls").style.display = "block";
        } catch (err) {
   console.error("Error readying for next round:", err);
      this.showError("Failed to ready for next round");
        }
    }

    /**
     * Render the complete game state
     */
renderGameState(update) {
        if (!update) return;

   // Render dealer hand
this.renderDealerHand(update.dealerCards, update.dealerTotal, update.dealerHoleCardHidden);

   // Render all players
        this.renderPlayers(update.allPlayers);

  // Update player's own game controls
        if (update.playerSnapshot) {
   this.updatePlayerControls(update.playerSnapshot);
        }

   // Update message
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

        players.forEach(player => {
  const playerCard = this.createPlayerCard(player);
      container.appendChild(playerCard);
   });
    }

    /**
 * Create a player card element
     */
    createPlayerCard(player) {
        const col = document.createElement("div");
   col.className = "col-md-6 col-lg-4";

const card = document.createElement("div");
     card.className = `card player-card ${player.isCurrentUser ? 'current-user' : ''}`;
        
  let statusBadge = '';
        if (player.isReady) {
       statusBadge = '<span class="badge bg-success">Ready</span>';
        } else if (player.gameState === 'RoundOver') {
     statusBadge = '<span class="badge bg-secondary">Round Over</span>';
 } else if (player.gameState === 'PlayerTurn') {
statusBadge = '<span class="badge bg-warning">Playing</span>';
  }

   card.innerHTML = `
     <div class="card-body">
    <div class="d-flex justify-content-between align-items-center mb-2">
      <h6 class="card-subtitle mb-0">
  ${player.name} ${player.isCurrentUser ? '(You)' : ''}
         </h6>
  ${statusBadge}
     </div>
    <div class="mb-2">
     <small class="text-muted">Balance:</small>
          <strong class="text-success">$${player.balance}</strong>
       <small class="text-muted ms-2">Bet:</small>
<strong>$${player.currentBet}</strong>
     </div>
     <div class="card-container" id="player-hand-${player.connectionId}">
         ${player.hand.map(card => this.createCardElement(card, false).outerHTML).join('')}
       </div>
        <div class="text-center mt-2">
         <span class="badge bg-primary">Total: ${player.handTotal}</span>
               ${player.outcome !== 'None' ? `<span class="badge bg-info ms-2">${player.outcome}</span>` : ''}
         </div>
   </div>
   `;

  col.appendChild(card);
        return col;
    }

    /**
     * Create a card DOM element
     */
    createCardElement(card, isFaceDown) {
        const cardDiv = document.createElement("div");
        cardDiv.className = "playing-card card-deal-animation";

        if (isFaceDown) {
    cardDiv.classList.add("face-down");
  cardDiv.innerHTML = '<div style="font-size: 1rem;">??</div>';
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
       <div style="display: flex; flex-direction: column; align-items: center;">
        <div>${rankDisplay}</div>
          <div>${suitSymbol}</div>
          </div>
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

   // Update balance
 document.getElementById("playerBalance").textContent = `$${snapshot.balance}`;

   // Show/hide controls based on game state
     if (snapshot.state === "WaitingForBet") {
     betControls.style.display = "block";
      gameControls.style.display = "none";
  nextRoundControls.style.display = "none";
       waitingMessage.style.display = "none";
        } else if (snapshot.state === "PlayerTurn") {
       betControls.style.display = "none";
  gameControls.style.display = "flex";
       nextRoundControls.style.display = "none";
    waitingMessage.style.display = "none";

     // Enable/disable buttons based on what's allowed
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
        
  statusDiv.className = `alert alert-${type}`;
   statusText.textContent = message;
    }

    /**
     * Show a notification to the user
     */
    showNotification(message) {
  // You can enhance this with a toast library like Bootstrap Toast
        console.log("Notification:", message);
    }

    /**
     * Show an error message
  */
    showError(message) {
        alert(message);
    }
}

// Event listeners for buttons
document.addEventListener("DOMContentLoaded", () => {
    // Place bet button
    document.getElementById("placeBetBtn")?.addEventListener("click", () => {
        if (window.gameClient) {
   window.gameClient.placeBet();
        }
    });

    // Hit button
    document.getElementById("hitBtn")?.addEventListener("click", () => {
   if (window.gameClient) {
      window.gameClient.performAction("Hit");
  }
    });

    // Stand button
 document.getElementById("standBtn")?.addEventListener("click", () => {
        if (window.gameClient) {
    window.gameClient.performAction("Stand");
   }
    });

    // Double button
  document.getElementById("doubleBtn")?.addEventListener("click", () => {
        if (window.gameClient) {
     window.gameClient.performAction("Double");
      }
    });

    // Next round button
    document.getElementById("nextRoundBtn")?.addEventListener("click", () => {
        if (window.gameClient) {
 window.gameClient.readyForNextRound();
        }
  });

    // Share table button
    document.getElementById("shareTableBtn")?.addEventListener("click", () => {
   const modal = new bootstrap.Modal(document.getElementById("shareModal"));
        modal.show();
    });

  // Copy link button
    document.getElementById("copyLinkBtn")?.addEventListener("click", () => {
        const linkInput = document.getElementById("shareLink");
  linkInput.select();
  document.execCommand("copy");
        
  // Show feedback
   const btn = document.getElementById("copyLinkBtn");
        const originalText = btn.textContent;
        btn.textContent = "Copied!";
  setTimeout(() => {
      btn.textContent = originalText;
 }, 2000);
    });
});
