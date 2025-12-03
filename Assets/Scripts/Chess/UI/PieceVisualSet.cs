using UnityEngine;
using System.Collections.Generic;

namespace Chess.UI
{
    using Core;

    /// <summary>
    /// Scriptable asset that defines visual for all pieces
    /// Can be swapped at runtime for different piece themes
    /// </summary>
    [CreateAssetMenu(fileName = "PieceVisualSet", menuName = "Chess/Piece Visual Set")]
    public class PieceVisualSet : ScriptableObject
    {
        [System.Serializable]
        public struct PieceVisual
        {
            public Sprite sprite;
            public Vector3 scale;
        }

        [SerializeField] private PieceVisual whitePawn;
        [SerializeField] private PieceVisual whiteKnight;
        [SerializeField] private PieceVisual whiteBishop;
        [SerializeField] private PieceVisual whiteRook;
        [SerializeField] private PieceVisual whiteQueen;
        [SerializeField] private PieceVisual whiteKing;

        [SerializeField] private PieceVisual blackPawn;
        [SerializeField] private PieceVisual blackKnight;
        [SerializeField] private PieceVisual blackBishop;
        [SerializeField] private PieceVisual blackRook;
        [SerializeField] private PieceVisual blackQueen;
        [SerializeField] private PieceVisual blackKing;

        public PieceVisual GetVisual(Piece piece)
        {
            if (piece.Color == Color.White)
            {
                return piece.Type switch
                {
                    PieceType.Pawn => whitePawn,
                    PieceType.Knight => whiteKnight,
                    PieceType.Bishop => whiteBishop,
                    PieceType.Rook => whiteRook,
                    PieceType.Queen => whiteQueen,
                    PieceType.King => whiteKing,
                    _ => default
                };
            }
            else
            {
                return piece.Type switch
                {
                    PieceType.Pawn => blackPawn,
                    PieceType.Knight => blackKnight,
                    PieceType.Bishop => blackBishop,
                    PieceType.Rook => blackRook,
                    PieceType.Queen => blackQueen,
                    PieceType.King => blackKing,
                    _ => default
                };
            }
        }
    }
}
