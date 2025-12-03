using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Chess.Rules
{
    using Core;
    using Pieces;

    /// <summary>
    /// Enforces all chess rules and validates moves
    /// </summary>
    public class ChessRules
    {
        public Board Board { get; set; }
        private static bool enableDetailedLogging = false;  // Set to true for deep debugging

        public ChessRules(Board board)
        {
            Board = board;
        }

        /// <summary>
        /// Enable/disable detailed move logging
        /// </summary>
        public static void SetDetailedLogging(bool enable)
        {
            enableDetailedLogging = enable;
        }

        /// <summary>
        /// Get all legal moves for a player (considering check)
        /// </summary>
        public List<Move> GetLegalMoves(Color color)
        {
            var legalMoves = new List<Move>();
            var pieces = Board.GetPiecesOfColor(color);

            if (enableDetailedLogging)
                Debug.Log($"🔍 GetLegalMoves for {color}: Found {pieces.Count} pieces");

            foreach (var (pos, piece) in pieces)
            {
                var pseudoLegalMoves = PieceMoveGenerator.GetPseudoLegalMoves(Board, pos);
                
                if (enableDetailedLogging && piece.Type == PieceType.Pawn)
                {
                    Debug.Log($"  🔍 {color} {piece.Type} at {pos}: {pseudoLegalMoves.Count} pseudo-legal moves");
                    foreach (var pm in pseudoLegalMoves)
                    {
                        var target = Board.GetPiece(pm.To);
                        string targetInfo = target != null ? $" (target: {target.Color} {target.Type})" : "";
                        Debug.Log($"    → {pm.From} to {pm.To}{targetInfo}");
                    }
                }

                foreach (var move in pseudoLegalMoves)
                {
                    // Basic validation: can't capture your own piece
                    var targetPiece = Board.GetPiece(move.To);
                    if (targetPiece != null && targetPiece.Color == color)
                    {
                        if (enableDetailedLogging)
                            Debug.Log($"    ❌ Rejected {move.From}→{move.To}: Friendly fire ({targetPiece.Type})");
                        continue;
                    }

                    // Verify move doesn't leave king in check
                    var testBoard = Board.Clone();
                    ExecuteMoveOnBoard(testBoard, move, color);

                    // Check if king is in check after the move
                    if (!IsInCheck(testBoard, color))
                    {
                        legalMoves.Add(move);
                    }
                    else if (enableDetailedLogging)
                    {
                        Debug.Log($"    ❌ Rejected {move.From}→{move.To}: Leaves king in check");
                    }
                }
            }

            // Add castling moves
            legalMoves.AddRange(GetCastlingMoves(color));

            return legalMoves;
        }

        /// <summary>
        /// Check if a color's king is in check
        /// </summary>
        public bool IsInCheck(Board board, Color color)
        {
            var kingPos = board.FindKing(color);
            if (kingPos == null)
                return false;

            return IsPositionAttacked(board, kingPos.Value, color);
        }

        /// <summary>
        /// Check if a position is attacked by opponent
        /// </summary>
        public bool IsPositionAttacked(Board board, Position pos, Color defendingColor)
        {
            var attackerColor = defendingColor.Opposite();
            var attackingPieces = board.GetPiecesOfColor(attackerColor);

            foreach (var (attackerPos, attacker) in attackingPieces)
            {
                // Use GetAttackedSquares for proper pawn attack detection
                var attackedSquares = PieceMoveGenerator.GetAttackedSquares(board, attackerPos);
                if (attackedSquares.Any(sq => sq == pos))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Check if a player is in checkmate
        /// </summary>
        public bool IsCheckmate(Color color)
        {
            if (!IsInCheck(Board, color))
                return false;

            return GetLegalMoves(color).Count == 0;
        }

        /// <summary>
        /// Check if a player is in stalemate
        /// </summary>
        public bool IsStalemate(Color color)
        {
            if (IsInCheck(Board, color))
                return false;

            return GetLegalMoves(color).Count == 0;
        }

        /// <summary>
        /// Execute a move on the board
        /// </summary>
        public void ExecuteMove(Move move, Color color)
        {
            ExecuteMoveOnBoard(Board, move, color);
        }

        private void ExecuteMoveOnBoard(Board board, Move move, Color color)
        {
            var piece = board.RemovePiece(move.From);
            
            if (piece == null)
                return; // Invalid move, piece doesn't exist
            
            // Handle en passant capture
            if (move.IsEnPassant)
            {
                int captureRank = move.From.Rank;
                board.RemovePiece(new Position(move.To.File, captureRank));
            }
            else
            {
                // Check if destination has a piece (capture)
                var targetPiece = board.GetPiece(move.To);
                if (targetPiece != null)
                {
                    board.RemovePiece(move.To);
                }
            }

            // Handle castling
            if (move.IsCastle)
            {
                ExecuteCastle(board, move.From, move.To, color);
            }

            // Mark piece as moved
            piece.HasMoved = true;
            board.SetPiece(move.To, piece);

            // Handle pawn promotion
            if (move.PromotionPiece != PieceType.None)
            {
                var promotedPiece = new Piece(move.PromotionPiece, color) { HasMoved = true };
                board.SetPiece(move.To, promotedPiece);
            }

            board.LastMove = move;
        }

        private List<Move> GetCastlingMoves(Color color)
        {
            var moves = new List<Move>();
            var kingPos = Board.FindKing(color);
            if (kingPos == null)
                return moves;

            var king = Board.GetPiece(kingPos.Value);
            if (king.HasMoved)
                return moves;

            int rank = color == Color.White ? 0 : Board.Size - 1;

            // Kingside castle
            var kingsideRookPos = new Position(Board.Size - 1, rank);
            if (CanCastle(Board, kingPos.Value, kingsideRookPos, color))
            {
                moves.Add(new Move(kingPos.Value, new Position(kingPos.Value.File + 2, rank))
                {
                    IsCastle = true
                });
            }

            // Queenside castle
            var queensideRookPos = new Position(0, rank);
            if (CanCastle(Board, kingPos.Value, queensideRookPos, color))
            {
                moves.Add(new Move(kingPos.Value, new Position(kingPos.Value.File - 2, rank))
                {
                    IsCastle = true
                });
            }

            return moves;
        }

        private bool CanCastle(Board board, Position kingPos, Position rookPos, Color color)
        {
            // Check if rook exists and hasn't moved
            var rook = board.GetPiece(rookPos);
            if (rook == null || rook.Type != PieceType.Rook || rook.HasMoved)
                return false;

            // Check all squares between king and rook
            int minFile = System.Math.Min(kingPos.File, rookPos.File);
            int maxFile = System.Math.Max(kingPos.File, rookPos.File);

            for (int f = minFile + 1; f < maxFile; f++)
            {
                if (board.HasPiece(new Position(f, kingPos.Rank)))
                    return false;
            }

            // Check if king is in check or passes through check
            if (IsInCheck(board, color))
                return false;

            for (int f = minFile + 1; f < maxFile; f++)
            {
                if (IsPositionAttacked(board, new Position(f, kingPos.Rank), color))
                    return false;
            }

            return true;
        }

        private void ExecuteCastle(Board board, Position kingFrom, Position kingTo, Color color)
        {
            int rank = color == Color.White ? 0 : board.Size - 1;
            Position rookFrom, rookTo;

            if (kingTo.File > kingFrom.File)
            {
                // Kingside
                rookFrom = new Position(board.Size - 1, rank);
                rookTo = new Position(kingFrom.File + 1, rank);
            }
            else
            {
                // Queenside
                rookFrom = new Position(0, rank);
                rookTo = new Position(kingFrom.File - 1, rank);
            }

            var rook = board.RemovePiece(rookFrom);
            rook.HasMoved = true;
            board.SetPiece(rookTo, rook);
        }
    }
}
