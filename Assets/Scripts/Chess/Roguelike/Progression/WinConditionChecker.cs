using System;
using System.Collections.Generic;
using UnityEngine;

namespace Chess.Roguelike.Progression
{
    using Chess.Core;
    using Chess.Rules;
    using Chess.Roguelike.Core;

    /// <summary>
    /// Checks win/loss conditions during a trial.
    /// Win conditions:
    /// 1. Checkmate the enemy
    /// 2. Capture all enemy pieces except the king
    /// 3. Have 13+ more piece value than enemy for 2 consecutive rounds
    /// </summary>
    public class WinConditionChecker
    {
        public enum TrialResult
        {
            Ongoing,
            PlayerWin_Checkmate,
            PlayerWin_Elimination,
            PlayerWin_ValueDominance,
            PlayerLoss_Checkmate,
            PlayerLoss_Elimination,
            PlayerLoss_ValueDominance,
            Draw_Stalemate
        }

        private readonly int valueDominanceThreshold;
        private readonly int valueDominanceRoundsRequired;
        private readonly bool checkmateEnabled;
        private readonly bool eliminationEnabled;
        private readonly bool valueDominanceEnabled;
        
        private int playerDominanceRounds = 0;
        private int enemyDominanceRounds = 0;

        public int PlayerDominanceRounds => playerDominanceRounds;
        public int EnemyDominanceRounds => enemyDominanceRounds;
        public int ValueDominanceThreshold => valueDominanceThreshold;
        public int RoundsRequired => valueDominanceRoundsRequired;
        public bool CheckmateEnabled => checkmateEnabled;
        public bool EliminationEnabled => eliminationEnabled;
        public bool ValueDominanceEnabled => valueDominanceEnabled;

        public WinConditionChecker(
            int valueThreshold = 13, 
            int roundsRequired = 2,
            bool enableCheckmate = true,
            bool enableElimination = true,
            bool enableValueDominance = true)
        {
            valueDominanceThreshold = valueThreshold;
            valueDominanceRoundsRequired = roundsRequired;
            checkmateEnabled = enableCheckmate;
            eliminationEnabled = enableElimination;
            valueDominanceEnabled = enableValueDominance;
        }

        /// <summary>
        /// Reset state for a new trial
        /// </summary>
        public void Reset()
        {
            playerDominanceRounds = 0;
            enemyDominanceRounds = 0;
        }

        /// <summary>
        /// Check all win conditions after a move
        /// </summary>
        public TrialResult CheckWinConditions(Board board, ChessRules rules, Color playerColor, bool afterPlayerMove)
        {
            Color enemyColor = playerColor.Opposite();

            // 1. Check for checkmate (if enabled)
            if (checkmateEnabled)
            {
                if (rules.IsCheckmate(enemyColor))
                {
                    Debug.Log("[WinCondition] Player wins by CHECKMATE!");
                    return TrialResult.PlayerWin_Checkmate;
                }
                
                if (rules.IsCheckmate(playerColor))
                {
                    Debug.Log("[WinCondition] Player loses by CHECKMATE!");
                    return TrialResult.PlayerLoss_Checkmate;
                }
            }

            // 2. Check for stalemate (always active if checkmate is enabled)
            if (checkmateEnabled)
            {
                if (rules.IsStalemate(enemyColor) || rules.IsStalemate(playerColor))
                {
                    Debug.Log("[WinCondition] STALEMATE - Draw");
                    return TrialResult.Draw_Stalemate;
                }
            }

            // 3. Check for elimination (only king remaining) - if enabled
            if (eliminationEnabled)
            {
                var enemyPieces = board.GetPiecesOfColor(enemyColor);
                var playerPieces = board.GetPiecesOfColor(playerColor);

                bool enemyOnlyKing = IsOnlyKingRemaining(enemyPieces);
                bool playerOnlyKing = IsOnlyKingRemaining(playerPieces);

                if (enemyOnlyKing)
                {
                    Debug.Log("[WinCondition] Player wins by ELIMINATION!");
                    return TrialResult.PlayerWin_Elimination;
                }
                
                if (playerOnlyKing)
                {
                    Debug.Log("[WinCondition] Player loses by ELIMINATION!");
                    return TrialResult.PlayerLoss_Elimination;
                }
            }

            // 4. Check for value dominance (only update after full round - after enemy move) - if enabled
            if (valueDominanceEnabled && !afterPlayerMove)
            {
                var dominanceResult = CheckValueDominance(board, playerColor, enemyColor);
                if (dominanceResult != TrialResult.Ongoing)
                {
                    return dominanceResult;
                }
            }

            return TrialResult.Ongoing;
        }

        private bool IsOnlyKingRemaining(List<(Position pos, Piece piece)> pieces)
        {
            if (pieces.Count != 1) return false;
            return pieces[0].piece.Type == PieceType.King;
        }

        private TrialResult CheckValueDominance(Board board, Color playerColor, Color enemyColor)
        {
            int playerValue = DifficultyScaler.CalculateBoardValue(board, playerColor);
            int enemyValue = DifficultyScaler.CalculateBoardValue(board, enemyColor);
            int difference = playerValue - enemyValue;

            Debug.Log($"[WinCondition] Value check - Player: {playerValue}, Enemy: {enemyValue}, Diff: {difference}");

            // Check player dominance
            if (difference >= valueDominanceThreshold)
            {
                playerDominanceRounds++;
                enemyDominanceRounds = 0; // Reset enemy streak
                
                Debug.Log($"[WinCondition] Player dominance round {playerDominanceRounds}/{valueDominanceRoundsRequired}");
                
                if (playerDominanceRounds >= valueDominanceRoundsRequired)
                {
                    Debug.Log("[WinCondition] Player wins by VALUE DOMINANCE!");
                    return TrialResult.PlayerWin_ValueDominance;
                }
            }
            // Check enemy dominance
            else if (difference <= -valueDominanceThreshold)
            {
                enemyDominanceRounds++;
                playerDominanceRounds = 0; // Reset player streak
                
                Debug.Log($"[WinCondition] Enemy dominance round {enemyDominanceRounds}/{valueDominanceRoundsRequired}");
                
                if (enemyDominanceRounds >= valueDominanceRoundsRequired)
                {
                    Debug.Log("[WinCondition] Player loses by VALUE DOMINANCE!");
                    return TrialResult.PlayerLoss_ValueDominance;
                }
            }
            else
            {
                // No dominance, reset both streaks
                playerDominanceRounds = 0;
                enemyDominanceRounds = 0;
            }

            return TrialResult.Ongoing;
        }

        /// <summary>
        /// Get a human-readable description of the current game state
        /// </summary>
        public string GetStatusDescription(Board board, Color playerColor)
        {
            int playerValue = DifficultyScaler.CalculateBoardValue(board, playerColor);
            int enemyValue = DifficultyScaler.CalculateBoardValue(board, playerColor.Opposite());
            int diff = playerValue - enemyValue;

            string status = $"Value: {playerValue} vs {enemyValue}";
            
            if (playerDominanceRounds > 0)
            {
                status += $" | Dominance: {playerDominanceRounds}/{valueDominanceRoundsRequired}";
            }
            else if (enemyDominanceRounds > 0)
            {
                status += $" | Enemy Dominance: {enemyDominanceRounds}/{valueDominanceRoundsRequired}";
            }
            
            if (Mathf.Abs(diff) >= valueDominanceThreshold)
            {
                status += diff > 0 ? " ★" : " ⚠";
            }

            return status;
        }
    }
}
