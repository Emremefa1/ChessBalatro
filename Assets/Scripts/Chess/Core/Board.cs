using System.Collections.Generic;
using UnityEngine;

namespace Chess.Core
{
    /// <summary>
    /// Represents the chess board with pieces
    /// </summary>
    public class Board
    {
        private Piece[,] squares;
        public int Size { get; private set; }
        
        // Track last move for en passant
        public Move LastMove { get; set; }

        public Board(int size = 8)
        {
            Size = size;
            squares = new Piece[size, size];
        }

        /// <summary>
        /// Get piece at position
        /// </summary>
        public Piece GetPiece(Position pos)
        {
            if (!pos.IsValid(Size))
                return null;
            return squares[pos.File, pos.Rank];
        }

        /// <summary>
        /// Set piece at position
        /// </summary>
        public void SetPiece(Position pos, Piece piece)
        {
            if (!pos.IsValid(Size))
                return;
            squares[pos.File, pos.Rank] = piece;
        }

        /// <summary>
        /// Remove piece from position
        /// </summary>
        public Piece RemovePiece(Position pos)
        {
            if (!pos.IsValid(Size))
                return null;
            var piece = squares[pos.File, pos.Rank];
            squares[pos.File, pos.Rank] = null;
            return piece;
        }

        /// <summary>
        /// Check if position has a piece
        /// </summary>
        public bool HasPiece(Position pos)
        {
            return GetPiece(pos) != null;
        }

        /// <summary>
        /// Check if position has an enemy piece
        /// </summary>
        public bool HasEnemyPiece(Position pos, Color color)
        {
            var piece = GetPiece(pos);
            return piece != null && piece.Color != color;
        }

        /// <summary>
        /// Check if position has a friendly piece
        /// </summary>
        public bool HasFriendlyPiece(Position pos, Color color)
        {
            var piece = GetPiece(pos);
            return piece != null && piece.Color == color;
        }

        /// <summary>
        /// Get all pieces of a given color
        /// </summary>
        public List<(Position pos, Piece piece)> GetPiecesOfColor(Color color)
        {
            var pieces = new List<(Position, Piece)>();
            for (int f = 0; f < Size; f++)
            {
                for (int r = 0; r < Size; r++)
                {
                    var piece = squares[f, r];
                    if (piece != null && piece.Color == color)
                    {
                        pieces.Add((new Position(f, r), piece));
                    }
                }
            }
            return pieces;
        }

        /// <summary>
        /// Find king position for a color
        /// </summary>
        public Position? FindKing(Color color)
        {
            for (int f = 0; f < Size; f++)
            {
                for (int r = 0; r < Size; r++)
                {
                    var piece = squares[f, r];
                    if (piece != null && piece.Type == PieceType.King && piece.Color == color)
                    {
                        return new Position(f, r);
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Clear the board
        /// </summary>
        public void Clear()
        {
            for (int f = 0; f < Size; f++)
            {
                for (int r = 0; r < Size; r++)
                {
                    squares[f, r] = null;
                }
            }
        }

        /// <summary>
        /// Create a deep copy of the board
        /// </summary>
        public Board Clone()
        {
            var cloned = new Board(Size);
            for (int f = 0; f < Size; f++)
            {
                for (int r = 0; r < Size; r++)
                {
                    if (squares[f, r] != null)
                    {
                        cloned.squares[f, r] = squares[f, r].Clone();
                    }
                }
            }
            cloned.LastMove = LastMove;
            return cloned;
        }

        public override string ToString()
        {
            var sb = new System.Text.StringBuilder();
            for (int r = Size - 1; r >= 0; r--)
            {
                for (int f = 0; f < Size; f++)
                {
                    var piece = squares[f, r];
                    if (piece == null)
                        sb.Append(". ");
                    else
                        sb.Append($"{GetPieceSymbol(piece)} ");
                }
                sb.AppendLine();
            }
            return sb.ToString();
        }

        private string GetPieceSymbol(Piece piece)
        {
            string symbol = piece.Type switch
            {
                PieceType.Pawn => "P",
                PieceType.Knight => "N",
                PieceType.Bishop => "B",
                PieceType.Rook => "R",
                PieceType.Queen => "Q",
                PieceType.King => "K",
                _ => "?"
            };
            return piece.Color == Color.White ? symbol : symbol.ToLower();
        }
    }
}
