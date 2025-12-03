using System.Collections.Generic;

namespace Chess.Core
{
    /// <summary>
    /// Allows flexible board setup before game starts
    /// </summary>
    public class BoardConfigurator
    {
        public Board Board { get; set; }

        public BoardConfigurator(int boardSize = 8)
        {
            Board = new Board(boardSize);
        }

        /// <summary>
        /// Clear the board
        /// </summary>
        public void Clear()
        {
            Board.Clear();
        }

        /// <summary>
        /// Add a piece at position
        /// </summary>
        public void AddPiece(Position pos, PieceType type, Color color)
        {
            if (!pos.IsValid(Board.Size))
            {
                UnityEngine.Debug.LogError($"Position {pos} is outside board bounds (0-{Board.Size - 1})");
                return;
            }

            Board.SetPiece(pos, new Piece(type, color));
        }

        /// <summary>
        /// Remove piece at position
        /// </summary>
        public void RemovePiece(Position pos)
        {
            Board.RemovePiece(pos);
        }

        /// <summary>
        /// Setup standard chess
        /// </summary>
        public void SetupStandardChess()
        {
            Clear();

            // White pieces
            AddPiece(new Position(0, 0), PieceType.Rook, Color.White);
            AddPiece(new Position(1, 0), PieceType.Knight, Color.White);
            AddPiece(new Position(2, 0), PieceType.Bishop, Color.White);
            AddPiece(new Position(3, 0), PieceType.Queen, Color.White);
            AddPiece(new Position(4, 0), PieceType.King, Color.White);
            AddPiece(new Position(5, 0), PieceType.Bishop, Color.White);
            AddPiece(new Position(6, 0), PieceType.Knight, Color.White);
            AddPiece(new Position(7, 0), PieceType.Rook, Color.White);

            for (int f = 0; f < 8; f++)
                AddPiece(new Position(f, 1), PieceType.Pawn, Color.White);

            // Black pieces
            AddPiece(new Position(0, 7), PieceType.Rook, Color.Black);
            AddPiece(new Position(1, 7), PieceType.Knight, Color.Black);
            AddPiece(new Position(2, 7), PieceType.Bishop, Color.Black);
            AddPiece(new Position(3, 7), PieceType.Queen, Color.Black);
            AddPiece(new Position(4, 7), PieceType.King, Color.Black);
            AddPiece(new Position(5, 7), PieceType.Bishop, Color.Black);
            AddPiece(new Position(6, 7), PieceType.Knight, Color.Black);
            AddPiece(new Position(7, 7), PieceType.Rook, Color.Black);

            for (int f = 0; f < 8; f++)
                AddPiece(new Position(f, 6), PieceType.Pawn, Color.Black);
        }

        /// <summary>
        /// Get all pieces on the board
        /// </summary>
        public List<(Position pos, Piece piece)> GetAllPieces()
        {
            var pieces = new List<(Position, Piece)>();
            for (int f = 0; f < Board.Size; f++)
            {
                for (int r = 0; r < Board.Size; r++)
                {
                    var pos = new Position(f, r);
                    var piece = Board.GetPiece(pos);
                    if (piece != null)
                        pieces.Add((pos, piece));
                }
            }
            return pieces;
        }
    }
}
