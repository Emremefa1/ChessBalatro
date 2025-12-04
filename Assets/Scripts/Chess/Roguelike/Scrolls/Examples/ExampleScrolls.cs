using UnityEngine;

namespace Chess.Roguelike.Scrolls.Examples
{
    using Chess.Core;
    using Chess.Roguelike.Core;

    /// <summary>
    /// Promotion Scroll: Instantly promote a pawn to a queen
    /// </summary>
    [CreateAssetMenu(fileName = "Scroll_Promotion", menuName = "ChessBalatro/Scrolls/Promotion")]
    public class PromotionScroll : Scroll
    {
        private void OnEnable()
        {
            scrollId = "promotion";
            scrollName = "Scroll of Promotion";
            description = "Instantly promote one of your pawns to a Queen.";
            shopPrice = 4;
            sellPrice = 2;
            usableIn = ScrollUsagePhase.Setup | ScrollUsagePhase.Shop;
            targetType = ScrollTargetType.OwnedPiece;
            rarity = ScrollRarity.Uncommon;
        }

        public override bool IsValidTarget(ScrollContext context)
        {
            if (context.SelectedPiece == null)
                return false;
            
            return context.SelectedPiece.Type == PieceType.Pawn;
        }

        public override bool Apply(ScrollContext context)
        {
            if (context.SelectedPiece == null || context.SelectedPiece.Type != PieceType.Pawn)
            {
                Debug.LogWarning("[PromotionScroll] Must select a pawn!");
                return false;
            }

            // Remove the pawn and add a queen
            var runState = context.RunState;
            if (runState != null)
            {
                runState.RemovePiece(context.SelectedPiece);
                runState.AddPiece(new PieceInstance(PieceType.Queen));
                Debug.Log("[PromotionScroll] Pawn promoted to Queen!");
                return true;
            }

            return false;
        }
    }

    /// <summary>
    /// Wealth Scroll: Gain $5 immediately
    /// </summary>
    [CreateAssetMenu(fileName = "Scroll_Wealth", menuName = "ChessBalatro/Scrolls/Wealth")]
    public class WealthScroll : Scroll
    {
        [SerializeField] private int goldAmount = 5;

        private void OnEnable()
        {
            scrollId = "wealth";
            scrollName = "Scroll of Wealth";
            description = "Gain $5 immediately.";
            shopPrice = 3;
            sellPrice = 1;
            usableIn = ScrollUsagePhase.Anywhere;
            targetType = ScrollTargetType.None;
            rarity = ScrollRarity.Common;
        }

        public override bool Apply(ScrollContext context)
        {
            if (context.RunState != null)
            {
                context.RunState.AddMoney(goldAmount);
                Debug.Log($"[WealthScroll] Gained ${goldAmount}!");
                return true;
            }
            return false;
        }

        public override string GetFormattedDescription(ScrollContext context = null)
        {
            return $"Gain ${goldAmount} immediately.";
        }
    }

    /// <summary>
    /// Destruction Scroll: Destroy one of your pieces to gain gold equal to its value x2
    /// </summary>
    [CreateAssetMenu(fileName = "Scroll_Destruction", menuName = "ChessBalatro/Scrolls/Destruction")]
    public class DestructionScroll : Scroll
    {
        private void OnEnable()
        {
            scrollId = "destruction";
            scrollName = "Scroll of Destruction";
            description = "Sacrifice a piece to gain gold equal to double its value.";
            shopPrice = 2;
            sellPrice = 1;
            usableIn = ScrollUsagePhase.Setup | ScrollUsagePhase.Shop;
            targetType = ScrollTargetType.OwnedPiece;
            rarity = ScrollRarity.Common;
        }

        public override bool IsValidTarget(ScrollContext context)
        {
            if (context.SelectedPiece == null)
                return false;
            
            // Cannot destroy king
            return !context.SelectedPiece.IsKing;
        }

        public override bool Apply(ScrollContext context)
        {
            if (context.SelectedPiece == null || context.SelectedPiece.IsKing)
            {
                Debug.LogWarning("[DestructionScroll] Invalid target!");
                return false;
            }

            var runState = context.RunState;
            if (runState != null)
            {
                int goldGain = context.SelectedPiece.GetTotalValue() * 2;
                runState.RemovePiece(context.SelectedPiece);
                runState.AddMoney(goldGain);
                Debug.Log($"[DestructionScroll] Destroyed {context.SelectedPiece.Type} for ${goldGain}!");
                return true;
            }

            return false;
        }
    }

    /// <summary>
    /// Clone Scroll: Duplicate one of your pieces (except King)
    /// </summary>
    [CreateAssetMenu(fileName = "Scroll_Clone", menuName = "ChessBalatro/Scrolls/Clone")]
    public class CloneScroll : Scroll
    {
        private void OnEnable()
        {
            scrollId = "clone";
            scrollName = "Scroll of Cloning";
            description = "Create a copy of one of your pieces (except King).";
            shopPrice = 6;
            sellPrice = 3;
            usableIn = ScrollUsagePhase.Setup | ScrollUsagePhase.Shop;
            targetType = ScrollTargetType.OwnedPiece;
            rarity = ScrollRarity.Rare;
        }

        public override bool IsValidTarget(ScrollContext context)
        {
            if (context.SelectedPiece == null)
                return false;
            
            return !context.SelectedPiece.IsKing;
        }

        public override bool Apply(ScrollContext context)
        {
            if (context.SelectedPiece == null || context.SelectedPiece.IsKing)
            {
                Debug.LogWarning("[CloneScroll] Invalid target!");
                return false;
            }

            var runState = context.RunState;
            if (runState != null)
            {
                var clone = new PieceInstance(context.SelectedPiece.Type);
                runState.AddPiece(clone);
                Debug.Log($"[CloneScroll] Cloned {context.SelectedPiece.Type}!");
                return true;
            }

            return false;
        }
    }

    /// <summary>
    /// Smite Scroll: Destroy an enemy piece during battle
    /// </summary>
    [CreateAssetMenu(fileName = "Scroll_Smite", menuName = "ChessBalatro/Scrolls/Smite")]
    public class SmiteScroll : Scroll
    {
        private void OnEnable()
        {
            scrollId = "smite";
            scrollName = "Scroll of Smite";
            description = "Destroy one enemy piece (except King) during battle.";
            shopPrice = 8;
            sellPrice = 4;
            usableIn = ScrollUsagePhase.Trial;
            targetType = ScrollTargetType.EnemyPiece;
            rarity = ScrollRarity.Legendary;
        }

        public override bool IsValidTarget(ScrollContext context)
        {
            if (!context.SelectedPosition.HasValue || context.Board == null)
                return false;
            
            var piece = context.Board.GetPiece(context.SelectedPosition.Value);
            if (piece == null)
                return false;
            
            // Must be enemy, not king
            return piece.Color != context.PlayerColor && piece.Type != PieceType.King;
        }

        public override bool Apply(ScrollContext context)
        {
            if (!context.SelectedPosition.HasValue || context.Board == null)
                return false;

            var piece = context.Board.GetPiece(context.SelectedPosition.Value);
            if (piece == null || piece.Color == context.PlayerColor || piece.Type == PieceType.King)
            {
                Debug.LogWarning("[SmiteScroll] Invalid target!");
                return false;
            }

            context.Board.RemovePiece(context.SelectedPosition.Value);
            Debug.Log($"[SmiteScroll] Destroyed enemy {piece.Type} at {context.SelectedPosition.Value}!");
            return true;
        }
    }
}
