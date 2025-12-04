using System;
using UnityEngine;

namespace Chess.Roguelike.Core
{
    using Chess.Core;

    /// <summary>
    /// Represents an individual piece owned by the player with persistence across trials.
    /// Tracks value, enchantments, and visual customization.
    /// </summary>
    [Serializable]
    public class PieceInstance
    {
        [SerializeField] private string id;
        [SerializeField] private PieceType pieceType;
        [SerializeField] private int baseValue;
        [SerializeField] private int sellValue;
        [SerializeField] private bool isKing; // King cannot be sold or moved in setup

        // Future: Enchantments
        [SerializeField] private string enchantmentId;
        
        // Board position (null if not placed)
        [SerializeField] private int? placedFile;
        [SerializeField] private int? placedRank;

        public string Id => id;
        public PieceType Type => pieceType;
        public int BaseValue => baseValue;
        public int SellValue => sellValue;
        public bool IsKing => isKing;
        public string EnchantmentId => enchantmentId;
        public Position? PlacedPosition => placedFile.HasValue && placedRank.HasValue 
            ? new Position(placedFile.Value, placedRank.Value) 
            : null;

        // Piece values matching standard chess but scaled for economy
        public static readonly int[] StandardValues = new int[]
        {
            1,  // Pawn
            3,  // Knight
            3,  // Bishop
            5,  // Rook
            9,  // Queen
            0   // King (priceless, not counted in value calculations)
        };

        public PieceInstance(PieceType type, bool isPlayerKing = false)
        {
            id = Guid.NewGuid().ToString();
            pieceType = type;
            isKing = isPlayerKing || type == PieceType.King;
            baseValue = GetBaseValue(type);
            sellValue = Mathf.Max(1, baseValue / 2); // Sell for half value, minimum 1
            enchantmentId = null;
            placedFile = null;
            placedRank = null;
        }

        public static int GetBaseValue(PieceType type)
        {
            return type switch
            {
                PieceType.Pawn => 1,
                PieceType.Knight => 3,
                PieceType.Bishop => 3,
                PieceType.Rook => 5,
                PieceType.Queen => 9,
                PieceType.King => 0,
                _ => 0
            };
        }

        public int GetTotalValue()
        {
            int value = baseValue;
            // Future: Add enchantment value modifiers
            return value;
        }

        public void SetPosition(Position pos)
        {
            placedFile = pos.File;
            placedRank = pos.Rank;
        }

        public void ClearPosition()
        {
            placedFile = null;
            placedRank = null;
        }

        public void SetEnchantment(string enchantId)
        {
            enchantmentId = enchantId;
        }

        public void ClearEnchantment()
        {
            enchantmentId = null;
        }

        /// <summary>
        /// Create a Chess.Core.Piece from this instance for use in gameplay
        /// </summary>
        public Piece ToPiece(Color color)
        {
            return new Piece(pieceType, color);
        }

        public override string ToString()
        {
            string enchant = string.IsNullOrEmpty(enchantmentId) ? "" : $" [{enchantmentId}]";
            string pos = PlacedPosition.HasValue ? $" @{PlacedPosition.Value}" : " (unplaced)";
            return $"{pieceType}{enchant} (val:{GetTotalValue()}){pos}";
        }
    }
}
