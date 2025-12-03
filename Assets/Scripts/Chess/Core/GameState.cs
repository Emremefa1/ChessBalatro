using System.Collections.Generic;

namespace Chess.Core
{
    /// <summary>
    /// Tracks the overall game state
    /// </summary>
    public class GameState
    {
        public Board Board { get; set; }
        public Color CurrentPlayer { get; set; }
        public List<Move> MoveHistory { get; set; }
        
        // Game ending states
        public bool IsCheckmate { get; set; }
        public bool IsStalemate { get; set; }
        public bool IsGameOver => IsCheckmate || IsStalemate;

        public GameState(int boardSize = 8)
        {
            Board = new Board(boardSize);
            CurrentPlayer = Color.White;
            MoveHistory = new List<Move>();
            IsCheckmate = false;
            IsStalemate = false;
        }

        public void SwitchPlayer()
        {
            CurrentPlayer = CurrentPlayer.Opposite();
        }

        public void AddMove(Move move)
        {
            MoveHistory.Add(move);
        }

        public void SetupStandardChess()
        {
            Board.Clear();
            
            // White pieces
            Board.SetPiece(new Position(0, 0), new Piece(PieceType.Rook, Color.White));
            Board.SetPiece(new Position(1, 0), new Piece(PieceType.Knight, Color.White));
            Board.SetPiece(new Position(2, 0), new Piece(PieceType.Bishop, Color.White));
            Board.SetPiece(new Position(3, 0), new Piece(PieceType.Queen, Color.White));
            Board.SetPiece(new Position(4, 0), new Piece(PieceType.King, Color.White));
            Board.SetPiece(new Position(5, 0), new Piece(PieceType.Bishop, Color.White));
            Board.SetPiece(new Position(6, 0), new Piece(PieceType.Knight, Color.White));
            Board.SetPiece(new Position(7, 0), new Piece(PieceType.Rook, Color.White));
            
            for (int f = 0; f < 8; f++)
                Board.SetPiece(new Position(f, 1), new Piece(PieceType.Pawn, Color.White));

            // Black pieces
            Board.SetPiece(new Position(0, 7), new Piece(PieceType.Rook, Color.Black));
            Board.SetPiece(new Position(1, 7), new Piece(PieceType.Knight, Color.Black));
            Board.SetPiece(new Position(2, 7), new Piece(PieceType.Bishop, Color.Black));
            Board.SetPiece(new Position(3, 7), new Piece(PieceType.Queen, Color.Black));
            Board.SetPiece(new Position(4, 7), new Piece(PieceType.King, Color.Black));
            Board.SetPiece(new Position(5, 7), new Piece(PieceType.Bishop, Color.Black));
            Board.SetPiece(new Position(6, 7), new Piece(PieceType.Knight, Color.Black));
            Board.SetPiece(new Position(7, 7), new Piece(PieceType.Rook, Color.Black));
            
            for (int f = 0; f < 8; f++)
                Board.SetPiece(new Position(f, 6), new Piece(PieceType.Pawn, Color.Black));
        }
    }
}
