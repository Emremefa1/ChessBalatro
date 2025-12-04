using UnityEngine;
using System.Collections;
using System.Linq;

namespace Chess.Examples
{
    using Core;
    using Rules;
    using AI;
    using UI;

    /// <summary>
    /// Simplified chess game with visual feedback
    /// Attach this to an empty GameObject and play!
    /// </summary>
    public class SimpleChessGame : MonoBehaviour
    {
        [Header("Game Settings")]
        [SerializeField] private int boardSize = 8;
        [SerializeField] private int aiDifficulty = 4;
        [SerializeField] private float squareSize = 1f;
        [SerializeField] private bool playAsWhite = true;

        [Header("Visual Materials")]
        [SerializeField] private Material whiteMaterial;
        [SerializeField] private Material blackMaterial;
        [SerializeField] private Material highlightMaterial;
        [SerializeField] private Material selectedMaterial;

        private GameState gameState;
        private ChessRules rules;
        private ChessAI ai;
        private IsometricBoardRenderer boardRenderer;
        private Position? selectedPiece;
        private Color currentPlayer = Color.White;

        private void Start()
        {
            InitializeGame();
        }

        private void InitializeGame()
        {
            // Setup game state
            gameState = new GameState(boardSize);
            gameState.SetupStandardChess();
            currentPlayer = Color.White;

            // Setup rules
            rules = new ChessRules(gameState.Board);
            
            // Enable detailed logging for debugging - set to true to trace move generation
            ChessRules.SetDetailedLogging(false);

            // Setup AI
            ai = new ChessAI(gameState.Board, aiDifficulty);

            // Setup renderer
            boardRenderer = GetComponent<IsometricBoardRenderer>();
            if (boardRenderer == null)
                boardRenderer = gameObject.AddComponent<IsometricBoardRenderer>();

            boardRenderer.Initialize(gameState.Board);

            Debug.Log("╔════════════════════════════════════╗");
            Debug.Log("║  Chess Game Initialized!           ║");
            Debug.Log($"║  Board: {boardSize}x{boardSize}                            ║");
            Debug.Log($"║  AI Difficulty: {aiDifficulty}/8                   ║");
            Debug.Log($"║  You play as: {(playAsWhite ? "WHITE" : "BLACK")}              ║");
            Debug.Log("║  Click pieces to move!             ║");
            Debug.Log("╚════════════════════════════════════╝");
            Debug.Log(gameState.Board.ToString());
        }

        private void Update()
        {
            if (gameState.IsGameOver)
                return;

            if (Input.GetMouseButtonDown(0))
            {
                HandleClick();
            }

            if (Input.GetKeyDown(KeyCode.Space))
            {
                Debug.Log(gameState.Board.ToString());
            }

            if (Input.GetKeyDown(KeyCode.R))
            {
                InitializeGame();
            }
        }

        private void HandleClick()
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (!Physics.Raycast(ray, out RaycastHit hit))
                return;

            // Parse which square was clicked
            var objectName = hit.collider.gameObject.name;
            if (!objectName.StartsWith("Square_"))
                return;

            var parts = objectName.Split('_');
            if (parts.Length < 3)
                return;

            if (!int.TryParse(parts[1], out int file) || !int.TryParse(parts[2], out int rank))
                return;

            var clickedPos = new Position(file, rank);
            ProcessMove(clickedPos);
        }

        private void ProcessMove(Position clickedPos)
        {
            // Check if it's the player's turn
            Color playerColor = playAsWhite ? Color.White : Color.Black;
            if (currentPlayer != playerColor)
            {
                Debug.Log($"⏳ It's {currentPlayer}'s turn, not yours!");
                return;
            }

            // If nothing selected, select the piece
            if (selectedPiece == null)
            {
                var piece = rules.Board.GetPiece(clickedPos);
                if (piece != null && piece.Color == currentPlayer)
                {
                    selectedPiece = clickedPos;
                    Debug.Log($"Selected {piece.Color} {piece.Type} at {clickedPos}");
                    
                    // Debug: show all legal moves for this piece
                    var allLegalMoves = rules.GetLegalMoves(currentPlayer);
                    var pieceLegalMoves = allLegalMoves.FindAll(m => m.From == clickedPos);
                    Debug.Log($"  Legal moves for this piece: {pieceLegalMoves.Count}");
                    foreach (var m in pieceLegalMoves)
                    {
                        var targetPiece = rules.Board.GetPiece(m.To);
                        if (targetPiece != null)
                            Debug.Log($"    {m.From} → {m.To} (capture {targetPiece.Color} {targetPiece.Type})");
                        else
                            Debug.Log($"    {m.From} → {m.To}");
                    }
                    
                    HighlightAvailableMoves(clickedPos);
                }
                return;
            }

            // If same square, deselect
            if (clickedPos == selectedPiece)
            {
                ClearHighlights();
                selectedPiece = null;
                Debug.Log("Deselected piece");
                return;
            }

            // Try to move
            var legalMoves = rules.GetLegalMoves(currentPlayer);
            Move move = null;
            foreach (var m in legalMoves)
            {
                if (m.From == selectedPiece && m.To == clickedPos)
                {
                    // If this is a promotion move, prefer Queen (or take first if no Queen)
                    if (m.PromotionPiece != PieceType.None)
                    {
                        if (m.PromotionPiece == PieceType.Queen)
                        {
                            move = m;
                            break; // Queen is best, stop looking
                        }
                        else if (move == null)
                        {
                            move = m; // Take first promotion as fallback
                        }
                    }
                    else
                    {
                        move = m;
                        break;
                    }
                }
            }

            if (move != null)
            {
                ExecuteMove(move);
            }
            else
            {
                Debug.LogWarning($"❌ Move rejected: {selectedPiece} → {clickedPos}");
                var piece = rules.Board.GetPiece(clickedPos);
                if (piece != null)
                {
                    Debug.LogWarning($"   Target square has {piece.Color} {piece.Type}");
                }
                // Reselect if different piece
                piece = rules.Board.GetPiece(clickedPos);
                if (piece != null && piece.Color == currentPlayer)
                {
                    ClearHighlights();
                    selectedPiece = clickedPos;
                    HighlightAvailableMoves(clickedPos);
                }
                else
                {
                    ClearHighlights();
                    selectedPiece = null;
                }
            }
        }

        private void ExecuteMove(Move move)
        {
            Color aiColor = playAsWhite ? Color.Black : Color.White;
            var capturedPiece = rules.Board.GetPiece(move.To);
            
            // Debug: validate move doesn't capture friendly piece
            if (capturedPiece != null && capturedPiece.Color == currentPlayer)
            {
                Debug.LogError($"❌ INVALID MOVE: {currentPlayer} tried to capture own {capturedPiece.Type}!");
                Debug.LogError($"   Move was: {move.From} → {move.To}");
                
                // Still switch turns to prevent game from getting stuck
                currentPlayer = currentPlayer.Opposite();
                
                // If it was AI's turn and move failed, let player continue
                if (currentPlayer != aiColor)
                {
                    Debug.Log($"👤 It's {currentPlayer}'s turn (Player's turn)");
                }
                else
                {
                    // AI failed, try again
                    Debug.Log($"🤖 AI move failed, trying again...");
                    StartCoroutine(ExecuteAIMove());
                }
                return;
            }

            rules.ExecuteMove(move, currentPlayer);

            // Log capture
            if (capturedPiece != null)
            {
                Debug.Log($"⚔️ {currentPlayer} captured {capturedPiece.Color} {capturedPiece.Type}");
            }

            // Update renderer - IMPORTANT: Remove captured piece visuals BEFORE moving the attacker
            if (move.IsCastle)
            {
                // Handle castle moves separately
                boardRenderer.UpdatePiecePosition(move.From, move.To);
                int rank = currentPlayer == Color.White ? 0 : boardSize - 1;
                if (move.To.File > move.From.File)
                {
                    // Kingside
                    boardRenderer.UpdatePiecePosition(
                        new Position(boardSize - 1, rank),
                        new Position(move.From.File + 1, rank)
                    );
                }
                else
                {
                    // Queenside
                    boardRenderer.UpdatePiecePosition(
                        new Position(0, rank),
                        new Position(move.From.File - 1, rank)
                    );
                }
            }
            else if (move.IsEnPassant)
            {
                // En passant: Remove captured pawn first, then move attacking pawn
                boardRenderer.RemovePiece(new Position(move.To.File, move.From.Rank));
                boardRenderer.UpdatePiecePosition(move.From, move.To);
            }
            else
            {
                // Regular move/capture: Remove captured piece first if any
                if (capturedPiece != null)
                {
                    boardRenderer.RemovePiece(move.To);
                }
                boardRenderer.UpdatePiecePosition(move.From, move.To);
            }

            // Handle promotion - remove pawn visual and create promoted piece visual
            if (move.PromotionPiece != PieceType.None)
            {
                // Remove the pawn visual that was just moved to the promotion square
                boardRenderer.RemovePiece(move.To);
                // Create the new promoted piece visual
                boardRenderer.CreatePieceVisual(move.To, new Piece(move.PromotionPiece, currentPlayer));
                Debug.Log($"♟️ Pawn promoted to {move.PromotionPiece}!");
            }

            ClearHighlights();
            selectedPiece = null;

            Debug.Log($"✓ {currentPlayer} moved: {move}");

            gameState.AddMove(move);
            currentPlayer = currentPlayer.Opposite();

            // Check game state
            CheckGameState();

            // AI move - only if it's the AI's turn
            if (!gameState.IsGameOver && currentPlayer == aiColor)
            {
                Debug.Log($"🤖 It's {currentPlayer}'s turn (AI's turn). AI is thinking...");
                StartCoroutine(ExecuteAIMove());
            }
            else
            {
                Debug.Log($"👤 It's {currentPlayer}'s turn (Player's turn)");
            }
        }

        private System.Collections.IEnumerator ExecuteAIMove()
        {
            yield return new WaitForSeconds(0.5f); // Delay for better UX

            Color aiColor = playAsWhite ? Color.Black : Color.White;
            Debug.Log($"🤖 AI move calculation started for {aiColor}...");
            var move = ai.FindBestMove(aiColor);
            
            if (move != null)
            {
                Debug.Log($"🤖 AI found move: {move.From} → {move.To}");
                ExecuteMove(move);
            }
            else
            {
                Debug.LogError("❌ AI returned null move!");
            }
        }

        private void CheckGameState()
        {
            if (rules.IsCheckmate(currentPlayer))
            {
                gameState.IsCheckmate = true;
                var winner = currentPlayer.Opposite();
                Debug.Log($"\n╔═══════════════════════════════╗");
                Debug.Log($"║  CHECKMATE! {winner} Wins!        ║");
                Debug.Log($"╚═══════════════════════════════╝\n");
            }
            else if (rules.IsStalemate(currentPlayer))
            {
                gameState.IsStalemate = true;
                Debug.Log($"\n╔═══════════════════════════════╗");
                Debug.Log($"║  STALEMATE!                   ║");
                Debug.Log($"╚═══════════════════════════════╝\n");
            }
            else if (rules.IsInCheck(gameState.Board, currentPlayer))
            {
                Debug.Log($"⚠️  {currentPlayer} is in CHECK!");
            }
        }

        private void HighlightAvailableMoves(Position from)
        {
            var legalMoves = rules.GetLegalMoves(currentPlayer);
            foreach (var move in legalMoves)
            {
                if (move.From == from)
                {
                    boardRenderer.HighlightSquare(move.To);
                }
            }
            boardRenderer.HighlightSquare(from, isSelected: true);
        }

        private void ClearHighlights()
        {
            for (int f = 0; f < boardSize; f++)
            {
                for (int r = 0; r < boardSize; r++)
                {
                    boardRenderer.UnhighlightSquare(new Position(f, r));
                }
            }
        }

        public void RestartGame()
        {
            InitializeGame();
        }

        public void SetDifficulty(int difficulty)
        {
            aiDifficulty = Mathf.Clamp(difficulty, 1, 8);
            if (ai != null)
                ai.SetDifficulty(aiDifficulty);
        }
    }
}
