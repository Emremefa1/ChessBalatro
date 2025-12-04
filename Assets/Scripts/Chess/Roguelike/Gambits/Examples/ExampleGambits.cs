using UnityEngine;

namespace Chess.Roguelike.Gambits.Examples
{
    using Chess.Core;

    /// <summary>
    /// Greedy Gambit: Earn +$1 for each piece captured
    /// </summary>
    [CreateAssetMenu(fileName = "Gambit_Greedy", menuName = "ChessBalatro/Gambits/Greedy")]
    public class GreedyGambit : Gambit
    {
        [SerializeField] private int goldPerCapture = 1;

        private void OnEnable()
        {
            gambitId = "greedy";
            gambitName = "Greedy";
            description = "Earn +$1 for each enemy piece you capture.";
            shopPrice = 4;
            sellPrice = 2;
            rarity = GambitRarity.Common;
        }

        public override void OnPlayerCapture(GambitContext context)
        {
            context.AddMoney?.Invoke(goldPerCapture);
            Debug.Log($"[Greedy] +${goldPerCapture} for capturing {context.CapturedPiece?.Type}");
        }

        public override string GetFormattedDescription()
        {
            return $"Earn +${goldPerCapture} for each enemy piece you capture.";
        }
    }

    /// <summary>
    /// Queen's Favor: Queens are worth +3 value for win conditions
    /// </summary>
    [CreateAssetMenu(fileName = "Gambit_QueensFavor", menuName = "ChessBalatro/Gambits/QueensFavor")]
    public class QueensFavorGambit : Gambit
    {
        [SerializeField] private int bonusValue = 3;

        private void OnEnable()
        {
            gambitId = "queens_favor";
            gambitName = "Queen's Favor";
            description = "Queens are worth +3 value for dominance calculations.";
            shopPrice = 5;
            sellPrice = 2;
            rarity = GambitRarity.Uncommon;
        }

        public override int ModifyPieceValue(Piece piece, int baseValue)
        {
            if (piece.Type == PieceType.Queen)
            {
                return baseValue + bonusValue;
            }
            return baseValue;
        }
    }

    /// <summary>
    /// Bigger Board: Increase board size by 1 at trial start
    /// </summary>
    [CreateAssetMenu(fileName = "Gambit_BiggerBoard", menuName = "ChessBalatro/Gambits/BiggerBoard")]
    public class BiggerBoardGambit : Gambit
    {
        [SerializeField] private int sizeIncrease = 1;

        private void OnEnable()
        {
            gambitId = "bigger_board";
            gambitName = "Bigger Board";
            description = "Board size increased by 1 (10x10 instead of 8x8).";
            shopPrice = 6;
            sellPrice = 3;
            rarity = GambitRarity.Rare;
        }

        public override void OnTrialStart(GambitContext context)
        {
            int currentSize = context.GetBoardSize?.Invoke() ?? 8;
            context.SetBoardSize?.Invoke(currentSize + sizeIncrease);
            Debug.Log($"[BiggerBoard] Board size increased to {currentSize + sizeIncrease}");
        }
    }

    /// <summary>
    /// Check Bonus: Earn +$2 every time you put the enemy in check
    /// </summary>
    [CreateAssetMenu(fileName = "Gambit_CheckBonus", menuName = "ChessBalatro/Gambits/CheckBonus")]
    public class CheckBonusGambit : Gambit
    {
        [SerializeField] private int goldPerCheck = 2;

        private void OnEnable()
        {
            gambitId = "check_bonus";
            gambitName = "Check Bonus";
            description = "Earn +$2 every time you put the enemy King in check.";
            shopPrice = 5;
            sellPrice = 2;
            rarity = GambitRarity.Uncommon;
        }

        public override void OnCheckGiven(GambitContext context)
        {
            context.AddMoney?.Invoke(goldPerCheck);
            Debug.Log($"[CheckBonus] +${goldPerCheck} for giving check!");
        }
    }

    /// <summary>
    /// Pawn Pusher: Pawns that reach rank 5+ are worth double
    /// </summary>
    [CreateAssetMenu(fileName = "Gambit_PawnPusher", menuName = "ChessBalatro/Gambits/PawnPusher")]
    public class PawnPusherGambit : Gambit
    {
        private void OnEnable()
        {
            gambitId = "pawn_pusher";
            gambitName = "Pawn Pusher";
            description = "Your pawns that advance past the center are worth double value.";
            shopPrice = 4;
            sellPrice = 2;
            rarity = GambitRarity.Common;
        }

        // Note: This would need to check pawn position during value calculation
        // Implementation would be in the value calculation system
    }

    /// <summary>
    /// Discount Shopper: All shop prices reduced by $1 (minimum $1)
    /// </summary>
    [CreateAssetMenu(fileName = "Gambit_DiscountShopper", menuName = "ChessBalatro/Gambits/DiscountShopper")]
    public class DiscountShopperGambit : Gambit
    {
        [SerializeField] private int discount = 1;

        private void OnEnable()
        {
            gambitId = "discount_shopper";
            gambitName = "Discount Shopper";
            description = "All shop prices reduced by $1.";
            shopPrice = 6;
            sellPrice = 3;
            rarity = GambitRarity.Rare;
        }

        public override int ModifyShopPrice(int basePrice, string itemType)
        {
            return Mathf.Max(1, basePrice - discount);
        }
    }

    /// <summary>
    /// Victory Lap: Earn +$5 when you win by checkmate
    /// </summary>
    [CreateAssetMenu(fileName = "Gambit_VictoryLap", menuName = "ChessBalatro/Gambits/VictoryLap")]
    public class VictoryLapGambit : Gambit
    {
        [SerializeField] private int checkmateBonus = 5;

        private void OnEnable()
        {
            gambitId = "victory_lap";
            gambitName = "Victory Lap";
            description = "Earn +$5 bonus when you win by checkmate.";
            shopPrice = 4;
            sellPrice = 2;
            rarity = GambitRarity.Common;
        }

        public override void OnTrialEnd(GambitContext context, bool playerWon)
        {
            // Note: Would need to check if win was by checkmate specifically
            // For now, just give bonus on any win
            if (playerWon)
            {
                context.AddMoney?.Invoke(checkmateBonus);
                Debug.Log($"[VictoryLap] +${checkmateBonus} for winning!");
            }
        }
    }
}
