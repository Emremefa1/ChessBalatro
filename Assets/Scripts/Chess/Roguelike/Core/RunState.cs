using System;
using System.Collections.Generic;
using UnityEngine;

namespace Chess.Roguelike.Core
{
    using Chess.Core;

    /// <summary>
    /// Persistent state for a roguelike run.
    /// Tracks progression, economy, owned pieces, gambits, and scrolls.
    /// </summary>
    [Serializable]
    public class RunState
    {
        public event Action OnMoneyChanged;
        public event Action OnCycleChanged;
        public event Action OnTrialChanged;
        public event Action OnArmyChanged;

        [Header("Progression")]
        [SerializeField] private int currentCycle = 1;      // Ante equivalent
        [SerializeField] private int currentTrial = 1;      // Blind equivalent (1-3, 3 is boss)
        [SerializeField] private int trialsPerCycle = 3;
        
        [Header("Economy")]
        [SerializeField] private int money = 10;            // Starting gold
        [SerializeField] private int rerollCost = 2;
        [SerializeField] private int rerollsThisShop = 0;
        
        [Header("Board Settings")]
        [SerializeField] private int boardSize = 8;         // Can be modified by gambits
        [SerializeField] private int playerSetupRows = 2;   // How many rows player can place pieces
        
        [Header("Win Condition Tracking")]
        [SerializeField] private int valueDominanceRounds = 0;  // Track consecutive rounds with >13 value lead
        [SerializeField] private int valueDominanceRequired = 2;
        [SerializeField] private int valueDominanceThreshold = 13;

        [Header("Army")]
        [SerializeField] private List<PieceInstance> ownedPieces = new();
        
        [Header("Gambits & Scrolls")]
        [SerializeField] private List<string> activeGambitIds = new();
        [SerializeField] private List<string> heldScrollIds = new();
        [SerializeField] private int maxGambits = 4;
        [SerializeField] private int maxScrolls = 4;

        // Properties
        public int CurrentCycle => currentCycle;
        public int CurrentTrial => currentTrial;
        public int TrialsPerCycle => trialsPerCycle;
        public bool IsBossTrial => currentTrial == trialsPerCycle;
        
        public int Money => money;
        public int RerollCost => rerollCost;
        
        public int BoardSize => boardSize;
        public int PlayerSetupRows => playerSetupRows;
        
        public int ValueDominanceRounds => valueDominanceRounds;
        public int ValueDominanceRequired => valueDominanceRequired;
        public int ValueDominanceThreshold => valueDominanceThreshold;
        
        public IReadOnlyList<PieceInstance> OwnedPieces => ownedPieces;
        public IReadOnlyList<string> ActiveGambitIds => activeGambitIds;
        public IReadOnlyList<string> HeldScrollIds => heldScrollIds;
        
        public int MaxGambits => maxGambits;
        public int MaxScrolls => maxScrolls;
        public int GambitSlotsRemaining => maxGambits - activeGambitIds.Count;
        public int ScrollSlotsRemaining => maxScrolls - heldScrollIds.Count;

        /// <summary>
        /// Initialize a new run with starting pieces
        /// </summary>
        public void InitializeNewRun()
        {
            currentCycle = 1;
            currentTrial = 1;
            money = 10;
            rerollCost = 2;
            rerollsThisShop = 0;
            boardSize = 8;
            valueDominanceRounds = 0;
            
            ownedPieces.Clear();
            activeGambitIds.Clear();
            heldScrollIds.Clear();

            // Starting army: King + 8 Pawns + 2 Rooks + 2 Knights + 2 Bishops + 1 Queen
            AddPiece(new PieceInstance(PieceType.King, isPlayerKing: true));
            AddPiece(new PieceInstance(PieceType.Queen));
            AddPiece(new PieceInstance(PieceType.Rook));
            AddPiece(new PieceInstance(PieceType.Rook));
            AddPiece(new PieceInstance(PieceType.Bishop));
            AddPiece(new PieceInstance(PieceType.Bishop));
            AddPiece(new PieceInstance(PieceType.Knight));
            AddPiece(new PieceInstance(PieceType.Knight));
            for (int i = 0; i < 8; i++)
            {
                AddPiece(new PieceInstance(PieceType.Pawn));
            }

            Debug.Log($"[RunState] New run initialized with {ownedPieces.Count} pieces, ${money}");
        }

        #region Money Management

        public bool CanAfford(int cost) => money >= cost;

        public bool TrySpendMoney(int amount)
        {
            if (amount <= 0) return true;
            if (money < amount) return false;
            
            money -= amount;
            OnMoneyChanged?.Invoke();
            return true;
        }

        public void AddMoney(int amount)
        {
            if (amount <= 0) return;
            money += amount;
            OnMoneyChanged?.Invoke();
        }

        public void SetMoney(int amount)
        {
            money = Mathf.Max(0, amount);
            OnMoneyChanged?.Invoke();
        }

        #endregion

        #region Progression

        public void AdvanceToNextTrial()
        {
            currentTrial++;
            if (currentTrial > trialsPerCycle)
            {
                currentTrial = 1;
                currentCycle++;
                OnCycleChanged?.Invoke();
            }
            
            // Reset per-trial state
            valueDominanceRounds = 0;
            rerollsThisShop = 0;
            
            OnTrialChanged?.Invoke();
            Debug.Log($"[RunState] Advanced to Cycle {currentCycle}, Trial {currentTrial}");
        }

        public void ResetValueDominance()
        {
            valueDominanceRounds = 0;
        }

        public bool IncrementValueDominance()
        {
            valueDominanceRounds++;
            return valueDominanceRounds >= valueDominanceRequired;
        }

        #endregion

        #region Army Management

        public void AddPiece(PieceInstance piece)
        {
            ownedPieces.Add(piece);
            OnArmyChanged?.Invoke();
        }

        public bool RemovePiece(PieceInstance piece)
        {
            if (piece.IsKing)
            {
                Debug.LogWarning("[RunState] Cannot remove King!");
                return false;
            }
            
            bool removed = ownedPieces.Remove(piece);
            if (removed)
            {
                OnArmyChanged?.Invoke();
            }
            return removed;
        }

        public PieceInstance FindPieceById(string id)
        {
            return ownedPieces.Find(p => p.Id == id);
        }

        public int GetTotalArmyValue()
        {
            int total = 0;
            foreach (var piece in ownedPieces)
            {
                total += piece.GetTotalValue();
            }
            return total;
        }

        public List<PieceInstance> GetPlacedPieces()
        {
            return ownedPieces.FindAll(p => p.PlacedPosition.HasValue);
        }

        public List<PieceInstance> GetUnplacedPieces()
        {
            return ownedPieces.FindAll(p => !p.PlacedPosition.HasValue);
        }

        /// <summary>
        /// Sell a piece for gold
        /// </summary>
        public bool SellPiece(PieceInstance piece)
        {
            if (piece.IsKing)
            {
                Debug.LogWarning("[RunState] Cannot sell King!");
                return false;
            }

            int sellValue = piece.SellValue;
            if (RemovePiece(piece))
            {
                AddMoney(sellValue);
                Debug.Log($"[RunState] Sold {piece.Type} for ${sellValue}");
                return true;
            }
            return false;
        }

        #endregion

        #region Gambits

        public bool CanAddGambit() => activeGambitIds.Count < maxGambits;

        public bool AddGambit(string gambitId)
        {
            if (!CanAddGambit())
            {
                Debug.LogWarning("[RunState] No gambit slots available!");
                return false;
            }
            
            activeGambitIds.Add(gambitId);
            Debug.Log($"[RunState] Added gambit: {gambitId}");
            return true;
        }

        public bool RemoveGambit(string gambitId)
        {
            return activeGambitIds.Remove(gambitId);
        }

        public bool HasGambit(string gambitId)
        {
            return activeGambitIds.Contains(gambitId);
        }

        #endregion

        #region Scrolls

        public bool CanAddScroll() => heldScrollIds.Count < maxScrolls;

        public bool AddScroll(string scrollId)
        {
            if (!CanAddScroll())
            {
                Debug.LogWarning("[RunState] No scroll slots available!");
                return false;
            }
            
            heldScrollIds.Add(scrollId);
            Debug.Log($"[RunState] Added scroll: {scrollId}");
            return true;
        }

        public bool UseScroll(string scrollId)
        {
            return heldScrollIds.Remove(scrollId);
        }

        public bool HasScroll(string scrollId)
        {
            return heldScrollIds.Contains(scrollId);
        }

        #endregion

        #region Board Modifiers (for Gambits)

        public void SetBoardSize(int size)
        {
            boardSize = Mathf.Clamp(size, 6, 12); // Reasonable limits
            Debug.Log($"[RunState] Board size changed to {boardSize}");
        }

        public void ModifyMaxGambits(int delta)
        {
            maxGambits = Mathf.Max(1, maxGambits + delta);
        }

        public void ModifyMaxScrolls(int delta)
        {
            maxScrolls = Mathf.Max(1, maxScrolls + delta);
        }

        #endregion

        #region Shop

        public int GetCurrentRerollCost()
        {
            // Reroll cost can increase with each reroll
            return rerollCost + rerollsThisShop;
        }

        public bool TryReroll()
        {
            int cost = GetCurrentRerollCost();
            if (TrySpendMoney(cost))
            {
                rerollsThisShop++;
                return true;
            }
            return false;
        }

        public void ResetShopRerolls()
        {
            rerollsThisShop = 0;
        }

        #endregion
    }
}
