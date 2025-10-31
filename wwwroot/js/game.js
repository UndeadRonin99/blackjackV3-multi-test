// Blackjack Game Animations

(function() {
  'use strict';

    // Initialize on page load
    document.addEventListener('DOMContentLoaded', function() {
        const container = document.querySelector('.game-container');
  if (!container) return;

        const shouldDeal = container.getAttribute('data-deal') === 'True';
        const shouldHit = container.getAttribute('data-hit') === 'True';
        const shouldReveal = container.getAttribute('data-reveal') === 'True';

      if (shouldDeal) {
            triggerDealAnimation();
 } else if (shouldHit) {
            triggerHitAnimation();
        }

        if (shouldReveal) {
            setTimeout(flipDealerHoleCard, shouldHit ? 500 : 100);
        }
    });

    /**
     * Triggers the deal animation for initial cards.
     */
    function triggerDealAnimation() {
        const playerHand = document.getElementById('player-hand');
const dealerHand = document.getElementById('dealer-hand');

   if (playerHand) {
            animateCardsInHand(playerHand, 'deal');
        }

   if (dealerHand) {
          animateCardsInHand(dealerHand, 'deal');
}
    }

    /**
     * Triggers the hit animation for the last card in player's hand.
     */
    function triggerHitAnimation() {
        const playerHand = document.getElementById('player-hand');
  if (!playerHand) return;

        const cards = playerHand.querySelectorAll('.card');
        if (cards.length > 0) {
   const lastCard = cards[cards.length - 1];
            lastCard.classList.add('animating-hit');
        }
    }

    /**
     * Flips the dealer's hole card (second card).
  */
    function flipDealerHoleCard() {
        const dealerHand = document.getElementById('dealer-hand');
        if (!dealerHand) return;

      const cards = dealerHand.querySelectorAll('.card');
 if (cards.length >= 2) {
    const holeCard = cards[1];
     
   // Remove face-down class and add flip animation
      holeCard.classList.add('animating-flip');
   
    // After flip completes, reveal the card
   setTimeout(() => {
         holeCard.classList.remove('card--facedown');
      holeCard.classList.remove('animating-flip');
   }, 300);
 }

    // Animate any additional dealer cards that were dealt
  setTimeout(() => {
        for (let i = 2; i < cards.length; i++) {
       cards[i].classList.add('animating-hit');
         cards[i].style.animationDelay = `${(i - 2) * 0.3}s`;
    }
   }, 400);
    }

    /**
     * Animates all cards in a hand.
     * @param {HTMLElement} hand - The hand container element.
     * @param {string} animationType - The animation type ('deal' or 'hit').
     */
function animateCardsInHand(hand, animationType) {
  const cards = hand.querySelectorAll('.card');
  cards.forEach((card, index) => {
       const animationClass = `animating-${animationType}`;
            card.classList.add(animationClass);
   
  // Stagger the animations
            card.style.animationDelay = `${index * 0.2}s`;
       
     // Clean up after animation
   setTimeout(() => {
       card.classList.remove(animationClass);
card.style.animationDelay = '';
   }, 1000 + (index * 200));
   });
    }

    // Expose functions globally for potential future use
    window.blackjackGame = {
        triggerDealAnimation,
        triggerHitAnimation,
  flipDealerHoleCard
    };
})();
