using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Chess.AI
{
    using Core;
    using Rules;
    using Pieces;

    /// <summary>
    /// Chess AI using heuristic-based move scoring.
    /// Designed for roguelike chess variants - fast, tactical, and extensible.
    /// </summary>
    public class ChessAI
    {
        private ChessRules rules;
        private int difficultyLevel = 3;
        private System.Random rng = new System.Random();

        // Piece values for evaluation
        private readonly Dictionary<PieceType, int> pieceValues = new()
        {
            { PieceType.Pawn, 100 },
            { PieceType.Knight, 320 },
            { PieceType.Bishop, 330 },
            { PieceType.Rook, 500 },
            { PieceType.Queen, 900 },
            { PieceType.King, 20000 }
        };

        // Weights for different tactical considerations
        private readonly Dictionary<string, float> tacticalWeights = new()
        {
            { "capture", 1.0f },
            { "checkThreat", 0.8f },
            { "centerControl", 0.3f },
            { "pieceDevelopment", 0.4f },
            { "kingSafety", 0.6f },
            { "pawnAdvancement", 0.2f },
            { "hangingPiece", 0.9f },
            { "randomness", 0.1f }
        };

        public ChessAI(Board board, int difficulty = 3)
        {
            rules = new ChessRules(board);
            difficultyLevel = Mathf.Clamp(difficulty, 1, 8);
        }

        /// <summary>
        /// Update the board reference (call this if board is recreated)
        /// </summary>
        public void SetBoard(Board board)
        {
            rules = new ChessRules(board);
        }

        /// <summary>
        /// Find the best move using heuristic scoring (fast, no recursion)
        /// </summary>
        public Move FindBestMove(Color color)
        {
            var legalMoves = rules.GetLegalMoves(color);

            if (legalMoves.Count == 0)
            {
                Debug.Log($"❌ AI ({color}): No legal moves");
                return null;
            }

            Debug.Log($"🤖 AI ({color}): Evaluating {legalMoves.Count} legal moves");

            if (legalMoves.Count == 1)
            {
                Debug.Log($"🤖 AI ({color}): Only one legal move");
                return legalMoves[0];
            }

            // Score all moves
            var scoredMoves = new List<(Move move, float score)>();
            
            foreach (var move in legalMoves)
            {
                // Double-check move is still valid (board state may have visual desync)
                var targetPiece = rules.Board.GetPiece(move.To);
                if (targetPiece != null && targetPiece.Color == color)
                {
                    Debug.LogWarning($"🤖 AI: Skipping invalid move {move.From}→{move.To} (friendly piece)");
                    continue;
                }
                
                float score = ScoreMove(move, color);
                scoredMoves.Add((move, score));
            }

            if (scoredMoves.Count == 0)
            {
                Debug.LogError($"❌ AI ({color}): All moves were invalid!");
                return legalMoves[0]; // Fallback to first legal move
            }

            // Sort by score descending
            scoredMoves.Sort((a, b) => b.score.CompareTo(a.score));

            // Select move based on difficulty
            Move selectedMove = SelectMoveByDifficulty(scoredMoves);

            Debug.Log($"🤖 AI ({color}): Selected {selectedMove.From}→{selectedMove.To} " +
                     $"(score: {scoredMoves.Find(m => m.move == selectedMove).score:F1})");

            return selectedMove;
        }

        /// <summary>
        /// Score a move based on tactical heuristics
        /// </summary>
        private float ScoreMove(Move move, Color color)
        {
            float score = 0f;
            var board = rules.Board;

            // 1. Capture value - most important!
            if (move.CapturedPiece != null)
            {
                int captureValue = pieceValues[move.CapturedPiece.Type];
                var movingPiece = board.GetPiece(move.From);
                int movingValue = movingPiece != null ? pieceValues[movingPiece.Type] : 0;
                
                // MVV-LVA: Most Valuable Victim - Least Valuable Attacker
                score += (captureValue - movingValue * 0.1f) * tacticalWeights["capture"];
                
                // Bonus for winning trades
                if (captureValue > movingValue)
                    score += 50;
            }

            // 2. Check threat
            var testBoard = board.Clone();
            var testRules = new ChessRules(testBoard);
            testRules.ExecuteMove(move, color);
            
            if (testRules.IsInCheck(testBoard, color.Opposite()))
            {
                score += 80 * tacticalWeights["checkThreat"];
                
                // Checkmate detection (quick check)
                if (testRules.GetLegalMoves(color.Opposite()).Count == 0)
                {
                    score += 10000; // Checkmate!
                }
            }

            // 3. Center control
            score += GetCenterControlScore(move.To) * tacticalWeights["centerControl"];

            // 4. Piece development (moving pieces off back rank early)
            var piece = board.GetPiece(move.From);
            if (piece != null)
            {
                score += GetDevelopmentScore(move, piece, color) * tacticalWeights["pieceDevelopment"];
            }

            // 5. Avoid moving to attacked squares (hanging piece)
            if (IsSquareAttacked(testBoard, move.To, color))
            {
                var movingPiece = board.GetPiece(move.From);
                if (movingPiece != null)
                {
                    // Penalty proportional to piece value
                    score -= pieceValues[movingPiece.Type] * 0.5f * tacticalWeights["hangingPiece"];
                }
            }

            // 6. Pawn advancement
            if (piece != null && piece.Type == PieceType.Pawn)
            {
                int advancement = color == Color.White ? move.To.Rank : (board.Size - 1 - move.To.Rank);
                score += advancement * 10 * tacticalWeights["pawnAdvancement"];
                
                // Promotion bonus
                if (move.PromotionPiece != PieceType.None)
                {
                    score += pieceValues[move.PromotionPiece] * 0.8f;
                }
            }

            // 7. King safety - penalize early king moves
            if (piece != null && piece.Type == PieceType.King && !move.IsCastle)
            {
                score -= 30 * tacticalWeights["kingSafety"];
            }
            
            // Castling bonus
            if (move.IsCastle)
            {
                score += 60 * tacticalWeights["kingSafety"];
            }

            // 8. Add controlled randomness for variety
            score += (float)(rng.NextDouble() * 20 - 10) * tacticalWeights["randomness"];

            return score;
        }

        private float GetCenterControlScore(Position pos)
        {
            // Center squares are more valuable
            int centerDist = Mathf.Abs(pos.File - 3) + Mathf.Abs(pos.Rank - 3);
            return Mathf.Max(0, (6 - centerDist) * 5);
        }

        private float GetDevelopmentScore(Move move, Piece piece, Color color)
        {
            // Encourage moving knights and bishops off back rank
            if (piece.Type == PieceType.Knight || piece.Type == PieceType.Bishop)
            {
                int backRank = color == Color.White ? 0 : rules.Board.Size - 1;
                if (move.From.Rank == backRank && move.To.Rank != backRank)
                {
                    return 25;
                }
            }
            return 0;
        }

        private bool IsSquareAttacked(Board board, Position pos, Color byColor)
        {
            // Check if any enemy piece attacks this square
            var enemyColor = byColor.Opposite();
            var enemyPieces = board.GetPiecesOfColor(enemyColor);

            foreach (var (enemyPos, _) in enemyPieces)
            {
                var attacks = PieceMoveGenerator.GetAttackedSquares(board, enemyPos);
                if (attacks.Any(p => p == pos))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Select a move based on difficulty level
        /// Lower difficulty = more randomness, higher = pick best move
        /// </summary>
        private Move SelectMoveByDifficulty(List<(Move move, float score)> scoredMoves)
        {
            if (scoredMoves.Count == 0)
                return null;

            // Difficulty 1-2: Pick randomly from top 50%
            // Difficulty 3-4: Pick randomly from top 30%
            // Difficulty 5-6: Pick randomly from top 3 moves
            // Difficulty 7-8: Always pick best move

            int poolSize;
            if (difficultyLevel <= 2)
            {
                poolSize = Mathf.Max(1, scoredMoves.Count / 2);
            }
            else if (difficultyLevel <= 4)
            {
                poolSize = Mathf.Max(1, scoredMoves.Count / 3);
            }
            else if (difficultyLevel <= 6)
            {
                poolSize = Mathf.Min(3, scoredMoves.Count);
            }
            else
            {
                poolSize = 1; // Always best
            }

            // Weight selection towards better moves
            int selectedIndex = 0;
            if (poolSize > 1)
            {
                // Weighted random: prefer earlier (better) moves
                float totalWeight = 0;
                for (int i = 0; i < poolSize; i++)
                {
                    totalWeight += (poolSize - i); // Higher weight for better moves
                }

                float randomValue = (float)rng.NextDouble() * totalWeight;
                float cumulative = 0;
                for (int i = 0; i < poolSize; i++)
                {
                    cumulative += (poolSize - i);
                    if (randomValue <= cumulative)
                    {
                        selectedIndex = i;
                        break;
                    }
                }
            }

            return scoredMoves[selectedIndex].move;
        }

        /// <summary>
        /// Set difficulty level (1-8)
        /// </summary>
        public void SetDifficulty(int difficulty)
        {
            difficultyLevel = Mathf.Clamp(difficulty, 1, 8);
        }

        /// <summary>
        /// Adjust tactical weight for roguelike modifiers
        /// Call this when special cards/abilities change AI behavior
        /// </summary>
        public void SetTacticalWeight(string tactic, float weight)
        {
            if (tacticalWeights.ContainsKey(tactic))
            {
                tacticalWeights[tactic] = Mathf.Clamp(weight, 0f, 2f);
            }
        }

        /// <summary>
        /// Get current tactical weights (for UI/debugging)
        /// </summary>
        public Dictionary<string, float> GetTacticalWeights()
        {
            return new Dictionary<string, float>(tacticalWeights);
        }
    }
}
