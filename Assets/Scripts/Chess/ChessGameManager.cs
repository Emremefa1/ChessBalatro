using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.Linq;

namespace Chess
{
    using Core;
    using Rules;
    using AI;

    /// <summary>
    /// Core chess game manager - handles game flow, rules, and AI
    /// Use this to manage chess games programmatically
    /// </summary>
    public class ChessGameManager : MonoBehaviour
    {
        [SerializeField] private int boardSize = 8;
        [SerializeField] private int aiDifficulty = 4;
        [SerializeField] private bool hasAI = true;

        private GameState gameState;
        private ChessRules rules;
        private ChessAI ai;
        private bool isAIThinking = false;

        private void Start()
        {
            InitializeGame();
        }

        public void InitializeGame(int customBoardSize = -1)
        {
            if (customBoardSize > 0)
                boardSize = customBoardSize;

            gameState = new GameState(boardSize);
            gameState.SetupStandardChess();
            rules = new ChessRules(gameState.Board);

            if (hasAI)
                ai = new ChessAI(gameState.Board, aiDifficulty);

            Debug.Log($"Chess game initialized: {boardSize}x{boardSize} board");
        }

        /// <summary>
        /// Execute a move on the board and update game state
        /// </summary>
        public bool TryExecuteMove(Move move, Color playerColor)
        {
            if (isAIThinking || gameState.IsGameOver)
                return false;

            if (playerColor != gameState.CurrentPlayer)
                return false;

            var legalMoves = rules.GetLegalMoves(gameState.CurrentPlayer);
            var fullMove = legalMoves.FirstOrDefault(m => m.From == move.From && m.To == move.To);

            if (fullMove == null)
                return false;

            rules.ExecuteMove(fullMove, gameState.CurrentPlayer);
            gameState.AddMove(fullMove);
            gameState.SwitchPlayer();

            if (rules.IsCheckmate(gameState.CurrentPlayer))
            {
                gameState.IsCheckmate = true;
            }
            else if (rules.IsStalemate(gameState.CurrentPlayer))
            {
                gameState.IsStalemate = true;
            }

            return true;
        }

        /// <summary>
        /// Get AI's best move for current player
        /// </summary>
        public Move GetAIMove()
        {
            if (!hasAI || ai == null || gameState.IsGameOver)
                return null;

            return ai.FindBestMove(gameState.CurrentPlayer);
        }

        /// <summary>
        /// Get all legal moves for current player
        /// </summary>
        public List<Move> GetLegalMoves()
        {
            return rules.GetLegalMoves(gameState.CurrentPlayer);
        }

        /// <summary>
        /// Get all legal moves for a specific piece
        /// </summary>
        public List<Move> GetLegalMovesForPiece(Position pos)
        {
            if (!pos.IsValid(boardSize))
                return new List<Move>();

            var moves = rules.GetLegalMoves(gameState.CurrentPlayer);
            return moves.Where(m => m.From == pos).ToList();
        }

        // Accessors
        public GameState GetGameState() => gameState;
        public ChessRules GetRules() => rules;
        public Board GetBoard() => gameState.Board;
        public Color GetCurrentPlayer() => gameState.CurrentPlayer;
        public bool IsGameOver() => gameState.IsGameOver;
        public void SetDifficulty(int difficulty)
        {
            aiDifficulty = Mathf.Clamp(difficulty, 1, 8);
            if (ai != null)
                ai.SetDifficulty(aiDifficulty);
        }
    }
}
