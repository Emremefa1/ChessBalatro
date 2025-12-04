using System.Collections.Generic;
using UnityEngine;

namespace Chess.Roguelike.Gambits
{
    using Chess.Core;

    /// <summary>
    /// Manages the player's active gambits and dispatches events to them
    /// </summary>
    public class GambitManager
    {
        private List<Gambit> activeGambits = new();
        private GambitContext context;

        public IReadOnlyList<Gambit> ActiveGambits => activeGambits;
        public int Count => activeGambits.Count;

        public GambitManager()
        {
            context = new GambitContext();
        }

        /// <summary>
        /// Update the context with current game state
        /// </summary>
        public void UpdateContext(Board board, Color playerColor, Color currentTurn)
        {
            context.Board = board;
            context.PlayerColor = playerColor;
            context.CurrentTurn = currentTurn;
        }

        /// <summary>
        /// Set economy hooks
        /// </summary>
        public void SetEconomyHooks(System.Action<int> addMoney, System.Func<int> getMoney)
        {
            context.AddMoney = addMoney;
            context.GetMoney = getMoney;
        }

        /// <summary>
        /// Set board modification hooks
        /// </summary>
        public void SetBoardHooks(System.Action<int> setBoardSize, System.Func<int> getBoardSize)
        {
            context.SetBoardSize = setBoardSize;
            context.GetBoardSize = getBoardSize;
        }

        public void AddGambit(Gambit gambit)
        {
            if (gambit != null && !activeGambits.Contains(gambit))
            {
                activeGambits.Add(gambit);
                Debug.Log($"[GambitManager] Added gambit: {gambit.Name}");
            }
        }

        public bool RemoveGambit(Gambit gambit)
        {
            bool removed = activeGambits.Remove(gambit);
            if (removed)
            {
                Debug.Log($"[GambitManager] Removed gambit: {gambit.Name}");
            }
            return removed;
        }

        public void ClearAllGambits()
        {
            activeGambits.Clear();
        }

        #region Event Dispatchers

        public void TriggerOnPieceCaptured(Move move, Piece captured, Piece capturer, Position capturePos)
        {
            context.LastMove = move;
            context.CapturedPiece = captured;
            context.CapturingPiece = capturer;
            context.CapturePosition = capturePos;

            foreach (var gambit in activeGambits)
            {
                gambit.OnPieceCaptured(context);

                // Determine if player or enemy captured
                if (capturer.Color == context.PlayerColor)
                {
                    gambit.OnPlayerCapture(context);
                }
                else
                {
                    gambit.OnEnemyCapture(context);
                }
            }
        }

        public void TriggerOnTurnStart()
        {
            foreach (var gambit in activeGambits)
            {
                gambit.OnTurnStart(context);
            }
        }

        public void TriggerOnTurnEnd()
        {
            foreach (var gambit in activeGambits)
            {
                gambit.OnTurnEnd(context);
            }
        }

        public void TriggerOnTrialStart()
        {
            foreach (var gambit in activeGambits)
            {
                gambit.OnTrialStart(context);
            }
        }

        public void TriggerOnTrialEnd(bool playerWon)
        {
            foreach (var gambit in activeGambits)
            {
                gambit.OnTrialEnd(context, playerWon);
            }
        }

        public void TriggerOnCheckGiven()
        {
            foreach (var gambit in activeGambits)
            {
                gambit.OnCheckGiven(context);
            }
        }

        public void TriggerOnCheckReceived()
        {
            foreach (var gambit in activeGambits)
            {
                gambit.OnCheckReceived(context);
            }
        }

        public void TriggerOnPromotion(PieceType promotedTo)
        {
            foreach (var gambit in activeGambits)
            {
                gambit.OnPromotion(context, promotedTo);
            }
        }

        #endregion

        #region Value Modifiers

        /// <summary>
        /// Get modified piece value after all gambit effects
        /// </summary>
        public int GetModifiedPieceValue(Piece piece, int baseValue)
        {
            int value = baseValue;
            foreach (var gambit in activeGambits)
            {
                value = gambit.ModifyPieceValue(piece, value);
            }
            return value;
        }

        /// <summary>
        /// Get modified shop price after all gambit effects
        /// </summary>
        public int GetModifiedShopPrice(int basePrice, string itemType)
        {
            int price = basePrice;
            foreach (var gambit in activeGambits)
            {
                price = gambit.ModifyShopPrice(price, itemType);
            }
            return Mathf.Max(0, price);
        }

        #endregion
    }
}
