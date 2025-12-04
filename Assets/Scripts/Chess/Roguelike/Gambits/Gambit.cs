using System;
using System.Collections.Generic;
using UnityEngine;

namespace Chess.Roguelike.Gambits
{
    using Chess.Core;

    /// <summary>
    /// Context passed to gambit effects when they trigger
    /// </summary>
    public class GambitContext
    {
        public Board Board { get; set; }
        public Color PlayerColor { get; set; }
        public Color CurrentTurn { get; set; }
        public Move LastMove { get; set; }
        public Piece CapturedPiece { get; set; }
        public Piece CapturingPiece { get; set; }
        public Position? CapturePosition { get; set; }
        
        // Economy hooks
        public Action<int> AddMoney { get; set; }
        public Func<int> GetMoney { get; set; }
        
        // Board modification hooks (for advanced gambits)
        public Action<int> SetBoardSize { get; set; }
        public Func<int> GetBoardSize { get; set; }
    }

    /// <summary>
    /// Base class for all Gambits (passive abilities like Jokers in Balatro).
    /// Gambits provide ongoing effects during trials.
    /// </summary>
    [CreateAssetMenu(fileName = "NewGambit", menuName = "ChessBalatro/Gambit")]
    public class Gambit : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] protected string gambitId;
        [SerializeField] protected string gambitName;
        [SerializeField] [TextArea(2, 4)] protected string description;
        [SerializeField] protected Sprite icon;
        
        [Header("Economy")]
        [SerializeField] protected int shopPrice = 5;
        [SerializeField] protected int sellPrice = 2;
        
        [Header("Rarity")]
        [SerializeField] protected GambitRarity rarity = GambitRarity.Common;

        // Properties
        public string Id => gambitId;
        public string Name => gambitName;
        public string Description => description;
        public Sprite Icon => icon;
        public int ShopPrice => shopPrice;
        public int SellPrice => sellPrice;
        public GambitRarity Rarity => rarity;

        /// <summary>
        /// Called when a piece is captured (either side)
        /// </summary>
        public virtual void OnPieceCaptured(GambitContext context)
        {
        }

        /// <summary>
        /// Called when player captures an enemy piece
        /// </summary>
        public virtual void OnPlayerCapture(GambitContext context)
        {
        }

        /// <summary>
        /// Called when enemy captures a player piece
        /// </summary>
        public virtual void OnEnemyCapture(GambitContext context)
        {
        }

        /// <summary>
        /// Called at the start of each turn
        /// </summary>
        public virtual void OnTurnStart(GambitContext context)
        {
        }

        /// <summary>
        /// Called at the end of each turn
        /// </summary>
        public virtual void OnTurnEnd(GambitContext context)
        {
        }

        /// <summary>
        /// Called when a trial begins
        /// </summary>
        public virtual void OnTrialStart(GambitContext context)
        {
        }

        /// <summary>
        /// Called when a trial ends (win or lose)
        /// </summary>
        public virtual void OnTrialEnd(GambitContext context, bool playerWon)
        {
        }

        /// <summary>
        /// Called when player gives check
        /// </summary>
        public virtual void OnCheckGiven(GambitContext context)
        {
        }

        /// <summary>
        /// Called when player is put in check
        /// </summary>
        public virtual void OnCheckReceived(GambitContext context)
        {
        }

        /// <summary>
        /// Called when a pawn is promoted
        /// </summary>
        public virtual void OnPromotion(GambitContext context, PieceType promotedTo)
        {
        }

        /// <summary>
        /// Modify piece value for scoring (can boost or reduce)
        /// </summary>
        public virtual int ModifyPieceValue(Piece piece, int baseValue)
        {
            return baseValue;
        }

        /// <summary>
        /// Modify shop prices (can give discounts)
        /// </summary>
        public virtual int ModifyShopPrice(int basePrice, string itemType)
        {
            return basePrice;
        }

        /// <summary>
        /// Get formatted description with current values
        /// Override to show dynamic values
        /// </summary>
        public virtual string GetFormattedDescription()
        {
            return description;
        }
    }

    public enum GambitRarity
    {
        Common,
        Uncommon,
        Rare,
        Legendary
    }
}
