using System;
using UnityEngine;

namespace Chess.Roguelike.Scrolls
{
    using Chess.Core;
    using Chess.Roguelike.Core;

    /// <summary>
    /// Context for scroll usage
    /// </summary>
    public class ScrollContext
    {
        public Board Board { get; set; }
        public RunState RunState { get; set; }
        public Color PlayerColor { get; set; }
        public Position? SelectedPosition { get; set; }
        public PieceInstance SelectedPiece { get; set; }
        
        // Phase info
        public bool IsInTrial { get; set; }
        public bool IsInShop { get; set; }
        public bool IsInSetup { get; set; }
    }

    /// <summary>
    /// Defines when a scroll can be used
    /// </summary>
    [Flags]
    public enum ScrollUsagePhase
    {
        None = 0,
        Shop = 1,
        Setup = 2,
        Trial = 4,
        Anywhere = Shop | Setup | Trial
    }

    /// <summary>
    /// Defines what target a scroll requires
    /// </summary>
    public enum ScrollTargetType
    {
        None,           // No target needed (immediate effect)
        OwnPiece,       // Must target player's piece
        EnemyPiece,     // Must target enemy piece
        AnyPiece,       // Any piece on board
        EmptySquare,    // Empty square on board
        OwnedPiece      // Piece in player inventory (setup phase)
    }

    /// <summary>
    /// Base class for all Scrolls (consumable items like Tarots in Balatro).
    /// Scrolls provide one-time effects when used.
    /// </summary>
    [CreateAssetMenu(fileName = "NewScroll", menuName = "ChessBalatro/Scroll")]
    public class Scroll : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] protected string scrollId;
        [SerializeField] protected string scrollName;
        [SerializeField] [TextArea(2, 4)] protected string description;
        [SerializeField] protected Sprite icon;
        
        [Header("Economy")]
        [SerializeField] protected int shopPrice = 3;
        [SerializeField] protected int sellPrice = 1;
        
        [Header("Usage")]
        [SerializeField] protected ScrollUsagePhase usableIn = ScrollUsagePhase.Anywhere;
        [SerializeField] protected ScrollTargetType targetType = ScrollTargetType.None;
        
        [Header("Rarity")]
        [SerializeField] protected ScrollRarity rarity = ScrollRarity.Common;

        // Properties
        public string Id => scrollId;
        public string Name => scrollName;
        public string Description => description;
        public Sprite Icon => icon;
        public int ShopPrice => shopPrice;
        public int SellPrice => sellPrice;
        public ScrollUsagePhase UsableIn => usableIn;
        public ScrollTargetType TargetType => targetType;
        public ScrollRarity Rarity => rarity;

        /// <summary>
        /// Check if scroll can be used in current phase
        /// </summary>
        public bool CanUseInPhase(ScrollContext context)
        {
            if (context.IsInShop && usableIn.HasFlag(ScrollUsagePhase.Shop))
                return true;
            if (context.IsInSetup && usableIn.HasFlag(ScrollUsagePhase.Setup))
                return true;
            if (context.IsInTrial && usableIn.HasFlag(ScrollUsagePhase.Trial))
                return true;
            return false;
        }

        /// <summary>
        /// Check if the provided target is valid for this scroll
        /// </summary>
        public virtual bool IsValidTarget(ScrollContext context)
        {
            switch (targetType)
            {
                case ScrollTargetType.None:
                    return true;
                    
                case ScrollTargetType.OwnPiece:
                    if (!context.SelectedPosition.HasValue) return false;
                    var ownPiece = context.Board?.GetPiece(context.SelectedPosition.Value);
                    return ownPiece != null && ownPiece.Color == context.PlayerColor;
                    
                case ScrollTargetType.EnemyPiece:
                    if (!context.SelectedPosition.HasValue) return false;
                    var enemyPiece = context.Board?.GetPiece(context.SelectedPosition.Value);
                    return enemyPiece != null && enemyPiece.Color != context.PlayerColor;
                    
                case ScrollTargetType.AnyPiece:
                    if (!context.SelectedPosition.HasValue) return false;
                    return context.Board?.GetPiece(context.SelectedPosition.Value) != null;
                    
                case ScrollTargetType.EmptySquare:
                    if (!context.SelectedPosition.HasValue) return false;
                    return context.Board?.GetPiece(context.SelectedPosition.Value) == null;
                    
                case ScrollTargetType.OwnedPiece:
                    return context.SelectedPiece != null;
                    
                default:
                    return false;
            }
        }

        /// <summary>
        /// Apply the scroll effect. Override in derived classes.
        /// </summary>
        /// <returns>True if effect was applied successfully</returns>
        public virtual bool Apply(ScrollContext context)
        {
            Debug.Log($"[Scroll] {scrollName} applied (base implementation - no effect)");
            return true;
        }

        /// <summary>
        /// Get description with any dynamic values filled in
        /// </summary>
        public virtual string GetFormattedDescription(ScrollContext context = null)
        {
            return description;
        }
    }

    public enum ScrollRarity
    {
        Common,
        Uncommon,
        Rare,
        Legendary
    }
}
