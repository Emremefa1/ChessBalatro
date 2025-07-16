using UnityEngine;

namespace ChessBalatro
{
    public enum PieceType
    {
        None,
        King,
        Queen,
        Rook,
        Bishop,
        Knight,
        Pawn
    }

    public enum PieceColor
    {
        White,
        Black
    }

    [System.Serializable]
    public class PieceData
    {
        public PieceType type;
        public PieceColor color;
        public Sprite sprite;

        public PieceData(PieceType type, PieceColor color, Sprite sprite = null)
        {
            this.type = type;
            this.color = color;
            this.sprite = sprite;
        }
    }
}
