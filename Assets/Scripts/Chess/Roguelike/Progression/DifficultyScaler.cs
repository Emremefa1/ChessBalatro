using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Chess.Roguelike.Progression
{
    using Chess.Core;
    using Chess.Roguelike.Core;

    /// <summary>
    /// Generates enemy armies based on cycle/trial difficulty.
    /// Scales piece count and composition as runs progress.
    /// </summary>
    public static class DifficultyScaler
    {
        /// <summary>
        /// Configuration for enemy army generation
        /// </summary>
        [Serializable]
        public class EnemyArmyConfig
        {
            public int totalValue;
            public int minPieces;
            public int maxPieces;
            public bool allowQueen;
            public bool allowMultipleQueens;
            public float queenChance;
            public float rookChance;
            public float bishopChance;
            public float knightChance;
        }

        /// <summary>
        /// Get the target army value for an enemy based on cycle and trial
        /// </summary>
        public static int GetEnemyArmyValue(int cycle, int trial)
        {
            // Base value: 20 (roughly a standard pawn-heavy setup)
            // Cycle scaling: +8 value per cycle
            // Trial scaling: +2 value per trial within cycle
            // Boss trial (3) gets +5 bonus
            
            int baseValue = 20;
            int cycleBonus = (cycle - 1) * 8;
            int trialBonus = (trial - 1) * 2;
            int bossBonus = trial == 3 ? 5 : 0;
            
            return baseValue + cycleBonus + trialBonus + bossBonus;
        }

        /// <summary>
        /// Get configuration for enemy army generation
        /// </summary>
        public static EnemyArmyConfig GetEnemyConfig(int cycle, int trial)
        {
            int targetValue = GetEnemyArmyValue(cycle, trial);
            
            return new EnemyArmyConfig
            {
                totalValue = targetValue,
                minPieces = 5 + cycle,          // Minimum pieces increases with cycle
                maxPieces = 12 + cycle * 2,     // Max pieces also scales
                allowQueen = cycle >= 1,        // Queens from cycle 1
                allowMultipleQueens = cycle >= 3, // Multiple queens from cycle 3
                queenChance = 0.1f + cycle * 0.05f,
                rookChance = 0.2f + cycle * 0.03f,
                bishopChance = 0.25f,
                knightChance = 0.25f
            };
        }

        /// <summary>
        /// Generate a random enemy army for a trial
        /// </summary>
        public static List<PieceInstance> GenerateEnemyArmy(int cycle, int trial)
        {
            var config = GetEnemyConfig(cycle, trial);
            var army = new List<PieceInstance>();
            
            // Always add king
            army.Add(new PieceInstance(PieceType.King));
            
            int currentValue = 0;
            int targetValue = config.totalValue;
            int pieceCount = 1; // King counts
            
            // Try to add higher-value pieces first
            
            // Queens
            if (config.allowQueen && currentValue < targetValue)
            {
                int maxQueens = config.allowMultipleQueens ? 2 : 1;
                for (int i = 0; i < maxQueens && currentValue + 9 <= targetValue; i++)
                {
                    if (Random.value < config.queenChance)
                    {
                        army.Add(new PieceInstance(PieceType.Queen));
                        currentValue += 9;
                        pieceCount++;
                    }
                }
            }
            
            // Rooks
            for (int i = 0; i < 2 && currentValue + 5 <= targetValue; i++)
            {
                if (Random.value < config.rookChance)
                {
                    army.Add(new PieceInstance(PieceType.Rook));
                    currentValue += 5;
                    pieceCount++;
                }
            }
            
            // Bishops
            for (int i = 0; i < 2 && currentValue + 3 <= targetValue; i++)
            {
                if (Random.value < config.bishopChance)
                {
                    army.Add(new PieceInstance(PieceType.Bishop));
                    currentValue += 3;
                    pieceCount++;
                }
            }
            
            // Knights
            for (int i = 0; i < 2 && currentValue + 3 <= targetValue; i++)
            {
                if (Random.value < config.knightChance)
                {
                    army.Add(new PieceInstance(PieceType.Knight));
                    currentValue += 3;
                    pieceCount++;
                }
            }
            
            // Fill remaining value with pawns
            while (currentValue < targetValue && pieceCount < config.maxPieces)
            {
                army.Add(new PieceInstance(PieceType.Pawn));
                currentValue += 1;
                pieceCount++;
            }
            
            // Ensure minimum pieces (add pawns if needed)
            while (pieceCount < config.minPieces)
            {
                army.Add(new PieceInstance(PieceType.Pawn));
                currentValue += 1;
                pieceCount++;
            }
            
            Debug.Log($"[DifficultyScaler] Generated enemy army: {pieceCount} pieces, {currentValue} value " +
                     $"(target: {targetValue}) for Cycle {cycle}, Trial {trial}");
            
            return army;
        }

        /// <summary>
        /// Calculate total value of a piece list
        /// </summary>
        public static int CalculateArmyValue(List<PieceInstance> army)
        {
            int total = 0;
            foreach (var piece in army)
            {
                total += piece.GetTotalValue();
            }
            return total;
        }

        /// <summary>
        /// Calculate value from board pieces of a specific color
        /// </summary>
        public static int CalculateBoardValue(Board board, Color color)
        {
            int total = 0;
            var pieces = board.GetPiecesOfColor(color);
            foreach (var (_, piece) in pieces)
            {
                if (piece.Type != PieceType.King)
                {
                    total += PieceInstance.GetBaseValue(piece.Type);
                }
            }
            return total;
        }

        /// <summary>
        /// Get reward money for completing a trial
        /// </summary>
        public static int GetTrialReward(int cycle, int trial, bool flawless)
        {
            int baseReward = 3;
            int cycleBonus = cycle;
            int bossBonus = trial == 3 ? 5 : 0;
            int flawlessBonus = flawless ? 3 : 0; // Bonus for not losing any pieces
            
            return baseReward + cycleBonus + bossBonus + flawlessBonus;
        }
    }
}
