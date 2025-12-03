using System.Collections.Generic;
using System.Linq;

namespace Chess.AI
{
    using Core;
    using Rules;
    using Pieces;

    /// <summary>
    /// Chess AI using minimax with alpha-beta pruning
    /// </summary>
    public class ChessAI
    {
        private ChessRules rules;
        private int searchDepth = 4; // Adjustable difficulty
        private const int MaxEvaluationDifference = 1000000;

        // Piece values
        private readonly Dictionary<PieceType, int> pieceValues = new()
        {
            { PieceType.Pawn, 100 },
            { PieceType.Knight, 300 },
            { PieceType.Bishop, 300 },
            { PieceType.Rook, 500 },
            { PieceType.Queen, 900 },
            { PieceType.King, 0 } // King is priceless
        };

        public ChessAI(Board board, int depth = 4)
        {
            rules = new ChessRules(board);
            searchDepth = depth;
        }

        /// <summary>
        /// Find the best move for a color
        /// </summary>
        public Move FindBestMove(Color color)
        {
            UnityEngine.Debug.Log($"🤖 ChessAI.FindBestMove() called for {color} at depth {searchDepth}");
            
            var legalMoves = rules.GetLegalMoves(color);

            if (legalMoves.Count == 0)
            {
                UnityEngine.Debug.Log($"❌ No legal moves found for {color}");
                return null;
            }

            UnityEngine.Debug.Log($"✓ Found {legalMoves.Count} legal moves");

            if (legalMoves.Count == 1)
            {
                UnityEngine.Debug.Log($"✓ Only 1 legal move, returning it");
                return legalMoves[0];
            }

            // Save original board state
            var originalBoard = rules.Board;
            
            Move bestMove = null;
            int bestEval = color == Color.White ? int.MinValue : int.MaxValue;

            foreach (var move in legalMoves)
            {
                // Create a test board for this move
                var testBoard = originalBoard.Clone();
                var testRules = new ChessRules(testBoard);

                testRules.ExecuteMove(move, color);

                int eval = Minimax(testRules, searchDepth - 1, color.Opposite(), int.MinValue, int.MaxValue, color);

                if (color == Color.White && eval > bestEval || color == Color.Black && eval < bestEval)
                {
                    bestEval = eval;
                    bestMove = move;
                }
            }

            if (bestMove != null)
            {
                UnityEngine.Debug.Log($"✓ Best move selected: {bestMove.From} → {bestMove.To} (eval: {bestEval})");
            }
            else
            {
                UnityEngine.Debug.LogError("❌ No best move found!");
            }

            return bestMove;
        }

        private int Minimax(ChessRules testRules, int depth, Color color, int alpha, int beta, Color originalColor)
        {
            if (depth == 0)
            {
                var eval = EvaluateBoard(testRules.Board, originalColor);
                UnityEngine.Debug.Log($"  🤖 Leaf node evaluation (depth 0): {eval}");
                return eval;
            }

            var legalMoves = testRules.GetLegalMoves(color);

            // Checkmate
            if (legalMoves.Count == 0)
            {
                if (testRules.IsInCheck(testRules.Board, color))
                    return color == originalColor ? -MaxEvaluationDifference : MaxEvaluationDifference;
                else
                    return 0; // Stalemate
            }

            if (color == originalColor)
            {
                // Maximizing
                int maxEval = int.MinValue;
                foreach (var move in legalMoves)
                {
                    var newTestBoard = testRules.Board.Clone();
                    var newTestRules = new ChessRules(newTestBoard);

                    newTestRules.ExecuteMove(move, color);
                    int eval = Minimax(newTestRules, depth - 1, color.Opposite(), alpha, beta, originalColor);

                    maxEval = System.Math.Max(maxEval, eval);
                    alpha = System.Math.Max(alpha, eval);
                    if (beta <= alpha)
                        break; // Beta cutoff
                }
                return maxEval;
            }
            else
            {
                // Minimizing
                int minEval = int.MaxValue;
                foreach (var move in legalMoves)
                {
                    var newTestBoard = testRules.Board.Clone();
                    var newTestRules = new ChessRules(newTestBoard);

                    newTestRules.ExecuteMove(move, color);
                    int eval = Minimax(newTestRules, depth - 1, color.Opposite(), alpha, beta, originalColor);

                    minEval = System.Math.Min(minEval, eval);
                    beta = System.Math.Min(beta, eval);
                    if (beta <= alpha)
                        break; // Alpha cutoff
                }
                return minEval;
            }
        }

        private int EvaluateBoard(Board board, Color playerColor)
        {
            int eval = 0;

            // Material evaluation
            var whitePieces = board.GetPiecesOfColor(Color.White);
            var blackPieces = board.GetPiecesOfColor(Color.Black);

            foreach (var (_, piece) in whitePieces)
            {
                eval += pieceValues[piece.Type];
            }

            foreach (var (_, piece) in blackPieces)
            {
                eval -= pieceValues[piece.Type];
            }

            // Positional evaluation
            eval += EvaluatePosition(board, Color.White);
            eval -= EvaluatePosition(board, Color.Black);

            // Check bonus/penalty
            if (rules.IsInCheck(board, Color.White))
                eval -= 50;
            if (rules.IsInCheck(board, Color.Black))
                eval += 50;

            // Return eval from the perspective of the player color
            return playerColor == Color.White ? eval : -eval;
        }

        private int EvaluatePosition(Board board, Color color)
        {
            int eval = 0;
            var pieces = board.GetPiecesOfColor(color);

            foreach (var (pos, piece) in pieces)
            {
                // Center control (simplified)
                int centerControl = 0;
                if (pos.File >= 2 && pos.File <= 5 && pos.Rank >= 2 && pos.Rank <= 5)
                    centerControl = 10;

                // Pawn advancement
                int advancement = 0;
                if (piece.Type == PieceType.Pawn)
                {
                    if (color == Color.White)
                        advancement = pos.Rank * 5;
                    else
                        advancement = (rules.Board.Size - 1 - pos.Rank) * 5;
                }

                eval += centerControl + advancement;
            }

            return eval;
        }

        /// <summary>
        /// Set search depth for difficulty
        /// </summary>
        public void SetDifficulty(int depth)
        {
            searchDepth = System.Math.Clamp(depth, 1, 8);
        }
    }
}
