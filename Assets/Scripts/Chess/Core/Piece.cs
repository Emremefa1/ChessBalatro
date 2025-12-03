namespace Chess.Core
{
    /// <summary>
    /// Represents a chess piece with its type and color
    /// </summary>
    [System.Serializable]
    public class Piece
    {
        public PieceType Type { get; set; }
        public Color Color { get; set; }
        
        // For tracking if piece has moved (used for castling and pawn double-move)
        public bool HasMoved { get; set; }

        public Piece(PieceType type, Color color)
        {
            Type = type;
            Color = color;
            HasMoved = false;
        }

        public Piece Clone()
        {
            return new Piece(Type, Color) { HasMoved = HasMoved };
        }

        public override string ToString()
        {
            return $"{Color} {Type}";
        }
    }
}
