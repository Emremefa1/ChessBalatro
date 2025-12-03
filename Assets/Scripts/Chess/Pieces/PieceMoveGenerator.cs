using System.Collections.Generic;

namespace Chess.Pieces
{
    using Core;

    /// <summary>
    /// Generates possible moves for each piece type
    /// </summary>
    public static class PieceMoveGenerator
    {
        /// <summary>
        /// Get all pseudo-legal moves (doesn't check if move leaves king in check)
        /// </summary>
        public static List<Move> GetPseudoLegalMoves(Board board, Position from)
        {
            var piece = board.GetPiece(from);
            if (piece == null)
                return new List<Move>();

            return piece.Type switch
            {
                PieceType.Pawn => GetPawnMoves(board, from, piece),
                PieceType.Knight => GetKnightMoves(board, from, piece),
                PieceType.Bishop => GetBishopMoves(board, from, piece),
                PieceType.Rook => GetRookMoves(board, from, piece),
                PieceType.Queen => GetQueenMoves(board, from, piece),
                PieceType.King => GetKingMoves(board, from, piece),
                _ => new List<Move>()
            };
        }

        /// <summary>
        /// Get all squares that a piece attacks (for check detection).
        /// Different from moves - pawns attack diagonally even if no piece is there.
        /// </summary>
        public static List<Position> GetAttackedSquares(Board board, Position from)
        {
            var piece = board.GetPiece(from);
            if (piece == null)
                return new List<Position>();

            if (piece.Type == PieceType.Pawn)
            {
                return GetPawnAttackSquares(board, from, piece);
            }
            
            // For all other pieces, attacked squares are same as move destinations
            var moves = GetPseudoLegalMoves(board, from);
            var attacked = new List<Position>();
            foreach (var m in moves)
            {
                attacked.Add(m.To);
            }
            return attacked;
        }

        /// <summary>
        /// Get squares a pawn attacks (diagonal squares regardless of occupancy)
        /// </summary>
        private static List<Position> GetPawnAttackSquares(Board board, Position from, Piece pawn)
        {
            var attacks = new List<Position>();
            int direction = pawn.Color == Color.White ? 1 : -1;

            for (int fileOffset = -1; fileOffset <= 1; fileOffset += 2)
            {
                var attackPos = new Position(from.File + fileOffset, from.Rank + direction);
                if (attackPos.IsValid(board.Size))
                {
                    attacks.Add(attackPos);
                }
            }

            return attacks;
        }

        private static List<Move> GetPawnMoves(Board board, Position from, Piece pawn)
        {
            var moves = new List<Move>();
            int direction = pawn.Color == Color.White ? 1 : -1;
            int startRank = pawn.Color == Color.White ? 1 : board.Size - 2;
            int promotionRank = pawn.Color == Color.White ? board.Size - 1 : 0;

            // Forward move
            var forward = new Position(from.File, from.Rank + direction);
            if (forward.IsValid(board.Size) && !board.HasPiece(forward))
            {
                // Check for promotion
                if (forward.Rank == promotionRank)
                {
                    // Add all promotion options
                    foreach (var promoPiece in new[] { PieceType.Queen, PieceType.Rook, PieceType.Bishop, PieceType.Knight })
                    {
                        moves.Add(new Move(from, forward) { PromotionPiece = promoPiece });
                    }
                }
                else
                {
                    moves.Add(new Move(from, forward));

                    // Double move from starting position
                    if (from.Rank == startRank)
                    {
                        var doubleForward = new Position(from.File, from.Rank + 2 * direction);
                        if (!board.HasPiece(doubleForward))
                        {
                            moves.Add(new Move(from, doubleForward));
                        }
                    }
                }
            }

            // Captures (diagonal)
            for (int fileOffset = -1; fileOffset <= 1; fileOffset += 2)
            {
                var capturePos = new Position(from.File + fileOffset, from.Rank + direction);
                if (capturePos.IsValid(board.Size) && board.HasEnemyPiece(capturePos, pawn.Color))
                {
                    var capturedPiece = board.GetPiece(capturePos);
                    
                    // Check for capture with promotion
                    if (capturePos.Rank == promotionRank)
                    {
                        foreach (var promoPiece in new[] { PieceType.Queen, PieceType.Rook, PieceType.Bishop, PieceType.Knight })
                        {
                            moves.Add(new Move(from, capturePos) 
                            { 
                                CapturedPiece = capturedPiece,
                                PromotionPiece = promoPiece 
                            });
                        }
                    }
                    else
                    {
                        moves.Add(new Move(from, capturePos) { CapturedPiece = capturedPiece });
                    }
                }
            }

            // En passant
            if (board.LastMove != null)
            {
                var lastMove = board.LastMove;
                // Check if last move was an enemy pawn double move
                if (lastMove.From.Rank == startRank + direction && 
                    lastMove.To.Rank == from.Rank &&
                    (lastMove.From.File == from.File + 1 || lastMove.From.File == from.File - 1))
                {
                    var enemyPawn = board.GetPiece(lastMove.To);
                    if (enemyPawn != null && enemyPawn.Type == PieceType.Pawn && enemyPawn.Color != pawn.Color)
                    {
                        var enPassantPos = new Position(lastMove.To.File, from.Rank + direction);
                        if (enPassantPos.IsValid(board.Size))
                        {
                            var move = new Move(from, enPassantPos)
                            {
                                IsEnPassant = true,
                                CapturedPiece = enemyPawn
                            };
                            moves.Add(move);
                        }
                    }
                }
            }

            return moves;
        }

        private static List<Move> GetKnightMoves(Board board, Position from, Piece knight)
        {
            var moves = new List<Move>();
            var knightOffsets = new[]
            {
                (-2, -1), (-2, 1), (-1, -2), (-1, 2),
                (1, -2), (1, 2), (2, -1), (2, 1)
            };

            foreach (var (fileOffset, rankOffset) in knightOffsets)
            {
                var to = new Position(from.File + fileOffset, from.Rank + rankOffset);
                if (to.IsValid(board.Size))
                {
                    if (!board.HasFriendlyPiece(to, knight.Color))
                    {
                        var move = new Move(from, to);
                        if (board.HasEnemyPiece(to, knight.Color))
                            move.CapturedPiece = board.GetPiece(to);
                        moves.Add(move);
                    }
                }
            }

            return moves;
        }

        private static List<Move> GetBishopMoves(Board board, Position from, Piece bishop)
        {
            var moves = new List<Move>();
            var diagonals = new[] { (1, 1), (1, -1), (-1, 1), (-1, -1) };
            AddSlidingMoves(board, from, bishop, diagonals, moves);
            return moves;
        }

        private static List<Move> GetRookMoves(Board board, Position from, Piece rook)
        {
            var moves = new List<Move>();
            var straights = new[] { (1, 0), (-1, 0), (0, 1), (0, -1) };
            AddSlidingMoves(board, from, rook, straights, moves);
            return moves;
        }

        private static List<Move> GetQueenMoves(Board board, Position from, Piece queen)
        {
            var moves = new List<Move>();
            var allDirections = new[] 
            { 
                (1, 0), (-1, 0), (0, 1), (0, -1),
                (1, 1), (1, -1), (-1, 1), (-1, -1)
            };
            AddSlidingMoves(board, from, queen, allDirections, moves);
            return moves;
        }

        private static List<Move> GetKingMoves(Board board, Position from, Piece king)
        {
            var moves = new List<Move>();
            var kingOffsets = new[]
            {
                (-1, -1), (-1, 0), (-1, 1),
                (0, -1),           (0, 1),
                (1, -1),  (1, 0),  (1, 1)
            };

            foreach (var (fileOffset, rankOffset) in kingOffsets)
            {
                var to = new Position(from.File + fileOffset, from.Rank + rankOffset);
                if (to.IsValid(board.Size) && !board.HasFriendlyPiece(to, king.Color))
                {
                    var move = new Move(from, to);
                    if (board.HasEnemyPiece(to, king.Color))
                        move.CapturedPiece = board.GetPiece(to);
                    moves.Add(move);
                }
            }

            return moves;
        }

        private static void AddSlidingMoves(Board board, Position from, Piece piece,
            (int, int)[] directions, List<Move> moves)
        {
            foreach (var (fileDir, rankDir) in directions)
            {
                int file = from.File + fileDir;
                int rank = from.Rank + rankDir;

                while (new Position(file, rank).IsValid(board.Size))
                {
                    var to = new Position(file, rank);

                    if (board.HasFriendlyPiece(to, piece.Color))
                        break;

                    var move = new Move(from, to);
                    if (board.HasEnemyPiece(to, piece.Color))
                    {
                        move.CapturedPiece = board.GetPiece(to);
                        moves.Add(move);
                        break;
                    }

                    moves.Add(move);
                    file += fileDir;
                    rank += rankDir;
                }
            }
        }
    }
}
