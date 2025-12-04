using System.Collections.Generic;
using UnityEngine;

namespace Chess.Roguelike.Scrolls
{
    /// <summary>
    /// Manages player's held scrolls (consumables)
    /// </summary>
    public class ScrollManager
    {
        private List<Scroll> heldScrolls = new();
        private int maxScrolls;
        private ScrollContext context;

        public IReadOnlyList<Scroll> HeldScrolls => heldScrolls;
        public int Count => heldScrolls.Count;
        public int MaxScrolls => maxScrolls;
        public int SlotsRemaining => maxScrolls - heldScrolls.Count;

        public ScrollManager(int maxSlots = 4)
        {
            maxScrolls = maxSlots;
            context = new ScrollContext();
        }

        /// <summary>
        /// Update context for scroll usage checks
        /// </summary>
        public void UpdateContext(Chess.Core.Board board, Chess.Roguelike.Core.RunState runState, 
                                   Chess.Core.Color playerColor, bool inTrial, bool inShop, bool inSetup)
        {
            context.Board = board;
            context.RunState = runState;
            context.PlayerColor = playerColor;
            context.IsInTrial = inTrial;
            context.IsInShop = inShop;
            context.IsInSetup = inSetup;
        }

        public void SetMaxScrolls(int max)
        {
            maxScrolls = Mathf.Max(1, max);
        }

        public bool CanAddScroll() => heldScrolls.Count < maxScrolls;

        public bool AddScroll(Scroll scroll)
        {
            if (!CanAddScroll())
            {
                Debug.LogWarning("[ScrollManager] No scroll slots available!");
                return false;
            }

            if (scroll == null)
            {
                Debug.LogWarning("[ScrollManager] Cannot add null scroll!");
                return false;
            }

            heldScrolls.Add(scroll);
            Debug.Log($"[ScrollManager] Added scroll: {scroll.Name}");
            return true;
        }

        public bool RemoveScroll(Scroll scroll)
        {
            return heldScrolls.Remove(scroll);
        }

        /// <summary>
        /// Check if a scroll can be used right now
        /// </summary>
        public bool CanUseScroll(Scroll scroll)
        {
            if (!heldScrolls.Contains(scroll))
                return false;

            return scroll.CanUseInPhase(context);
        }

        /// <summary>
        /// Use a scroll, consuming it if successful
        /// </summary>
        public bool UseScroll(Scroll scroll, Chess.Core.Position? targetPos = null, 
                              Chess.Roguelike.Core.PieceInstance targetPiece = null)
        {
            if (!CanUseScroll(scroll))
            {
                Debug.LogWarning($"[ScrollManager] Cannot use {scroll.Name} in current phase!");
                return false;
            }

            // Set target in context
            context.SelectedPosition = targetPos;
            context.SelectedPiece = targetPiece;

            // Validate target
            if (!scroll.IsValidTarget(context))
            {
                Debug.LogWarning($"[ScrollManager] Invalid target for {scroll.Name}!");
                return false;
            }

            // Apply effect
            bool success = scroll.Apply(context);
            
            if (success)
            {
                // Consume the scroll
                heldScrolls.Remove(scroll);
                Debug.Log($"[ScrollManager] Used scroll: {scroll.Name}");
            }

            return success;
        }

        /// <summary>
        /// Get scrolls that can be used in the current phase
        /// </summary>
        public List<Scroll> GetUsableScrolls()
        {
            var usable = new List<Scroll>();
            foreach (var scroll in heldScrolls)
            {
                if (scroll.CanUseInPhase(context))
                {
                    usable.Add(scroll);
                }
            }
            return usable;
        }
    }
}
