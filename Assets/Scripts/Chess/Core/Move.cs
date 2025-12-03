namespace Chess.Core
{
    /// <summary>
    /// Represents a chess move with all necessary information
    /// </summary>
    [System.Serializable]
    public class Move
    {
        public Position From { get; set; }
        public Position To { get; set; }
        
        // For pawn promotion
        public PieceType PromotionPiece { get; set; }
        
        // For special moves
        public bool IsCastle { get; set; }
        public bool IsEnPassant { get; set; }
        
        // The piece that was captured (null if no capture)
        public Piece CapturedPiece { get; set; }

        public Move(Position from, Position to)
        {
            From = from;
            To = to;
            PromotionPiece = PieceType.None;
            IsCastle = false;
            IsEnPassant = false;
            CapturedPiece = null;
        }

        public override string ToString()
        {
            return $"{From} -> {To}";
        }
    }
}
