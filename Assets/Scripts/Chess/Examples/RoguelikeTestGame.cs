using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace Chess.Examples
{
    using Chess.Core;
    using Chess.Rules;
    using Chess.Roguelike;
    using Chess.Roguelike.Core;
    using Chess.Roguelike.Progression;
    using Chess.Roguelike.Economy;
    using Chess.UI;

    /// <summary>
    /// Test script for the Roguelike Chess system.
    /// Attach to a GameObject to test the game loop.
    /// 
    /// CONTROLS:
    /// - N: Start New Run
    /// - S: Enter Shop Phase / Finish Shopping
    /// - P: Print current state
    /// - B: Buy a random piece from shop
    /// - G: Buy a gambit from shop
    /// - R: Reroll shop
    /// - ENTER: Confirm setup and start trial
    /// - Click: Select and move pieces during trial
    /// - SPACE: Print board state
    /// </summary>
    public class RoguelikeTestGame : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private RoguelikeGameManager gameManager;

        [Header("Visual Materials")]
        [SerializeField] private Material whiteMaterial;
        [SerializeField] private Material blackMaterial;
        [SerializeField] private Material highlightMaterial;

        private IsometricBoardRenderer boardRenderer;
        private Position? selectedPiece;
        private List<Position> highlightedSquares = new List<Position>();

        private void Start()
        {
            // Find or create the game manager
            if (gameManager == null)
            {
                gameManager = FindFirstObjectByType<RoguelikeGameManager>();
                
                if (gameManager == null)
                {
                    var go = new GameObject("RoguelikeGameManager");
                    gameManager = go.AddComponent<RoguelikeGameManager>();
                }
            }

            // Subscribe to events
            gameManager.OnPhaseChanged += OnPhaseChanged;
            gameManager.OnTrialEnded += OnTrialEnded;
            gameManager.OnRunStarted += OnRunStarted;
            gameManager.OnRunEnded += OnRunEnded;
            gameManager.OnMoveExecuted += OnMoveExecuted;

            // Setup renderer - DO NOT auto-add, it needs materials from inspector
            boardRenderer = GetComponent<IsometricBoardRenderer>();
            if (boardRenderer == null)
            {
                Debug.LogWarning("[RoguelikeTestGame] No IsometricBoardRenderer found! Please add one in the Inspector and assign materials.");
            }

            PrintControls();
        }

        private void OnDestroy()
        {
            if (gameManager != null)
            {
                gameManager.OnPhaseChanged -= OnPhaseChanged;
                gameManager.OnTrialEnded -= OnTrialEnded;
                gameManager.OnRunStarted -= OnRunStarted;
                gameManager.OnRunEnded -= OnRunEnded;
                gameManager.OnMoveExecuted -= OnMoveExecuted;
            }
        }

        private void PrintControls()
        {
            Debug.Log("╔════════════════════════════════════════════════╗");
            Debug.Log("║  CHESS BALATRO - ROGUELIKE TEST                ║");
            Debug.Log("╠════════════════════════════════════════════════╣");
            Debug.Log("║  N     - Start New Run                         ║");
            Debug.Log("║  S     - Finish Shopping / Go to Setup         ║");
            Debug.Log("║  P     - Print current state                   ║");
            Debug.Log("║  B     - Buy piece from shop                   ║");
            Debug.Log("║  G     - Buy gambit from shop                  ║");
            Debug.Log("║  L     - Buy scroll from shop                  ║");
            Debug.Log("║  R     - Reroll shop                           ║");
            Debug.Log("║  ENTER - Start trial (from setup)              ║");
            Debug.Log("║  Click - Select/move pieces                    ║");
            Debug.Log("║  SPACE - Print board                           ║");
            Debug.Log("║  C     - Continue from reward                  ║");
            Debug.Log("╚════════════════════════════════════════════════╝");
        }

        private void Update()
        {
            // Start new run
            if (Input.GetKeyDown(KeyCode.N))
            {
                Debug.Log("[TEST] Starting new run...");
                gameManager.StartNewRun();
            }

            // Print state
            if (Input.GetKeyDown(KeyCode.P))
            {
                gameManager.DebugPrintState();
                PrintShopContents();
            }

            // Shop controls
            if (gameManager.CurrentPhase == GamePhase.Shop)
            {
                HandleShopInput();
            }

            // Setup controls
            if (gameManager.CurrentPhase == GamePhase.Setup)
            {
                HandleSetupInput();
            }

            // Trial controls
            if (gameManager.CurrentPhase == GamePhase.Trial)
            {
                HandleTrialInput();
            }

            // Reward controls
            if (gameManager.CurrentPhase == GamePhase.Reward)
            {
                if (Input.GetKeyDown(KeyCode.C))
                {
                    Debug.Log("[TEST] Continuing to next trial...");
                    gameManager.ProceedFromReward();
                }
            }

            // Print board anytime
            if (Input.GetKeyDown(KeyCode.Space) && gameManager.TrialBoard != null)
            {
                Debug.Log(gameManager.TrialBoard.ToString());
            }
        }

        private void HandleShopInput()
        {
            var shop = gameManager.Shop;
            var runState = gameManager.RunState;

            // Finish shopping
            if (Input.GetKeyDown(KeyCode.S))
            {
                Debug.Log("[TEST] Finishing shopping, entering setup...");
                gameManager.FinishShopping();
                return;
            }

            // Buy piece (first piece-type item)
            if (Input.GetKeyDown(KeyCode.B))
            {
                var pieceItem = shop.CurrentItems.FirstOrDefault(i => 
                    (i.Type == ShopItem.ItemType.Piece || i.Type == ShopItem.ItemType.PawnPack) && !i.IsSoldOut);
                if (pieceItem != null)
                {
                    if (shop.TryPurchase(pieceItem))
                        Debug.Log($"[TEST] Bought {pieceItem.GetDisplayName()} for ${pieceItem.Price}! Money: ${runState.Money}");
                    else
                        Debug.Log($"[TEST] Cannot afford {pieceItem.GetDisplayName()} (${pieceItem.Price})");
                }
                else
                {
                    Debug.Log("[TEST] No pieces available in shop!");
                }
            }

            // Buy gambit (first gambit-type item)
            if (Input.GetKeyDown(KeyCode.G))
            {
                var gambitItem = shop.CurrentItems.FirstOrDefault(i => 
                    i.Type == ShopItem.ItemType.Gambit && !i.IsSoldOut);
                if (gambitItem != null)
                {
                    if (shop.TryPurchase(gambitItem))
                        Debug.Log($"[TEST] Bought gambit: {gambitItem.GetDisplayName()}! Money: ${runState.Money}");
                    else
                        Debug.Log($"[TEST] Cannot afford or slots full");
                }
                else
                {
                    Debug.Log("[TEST] No gambits available in shop!");
                }
            }

            // Buy scroll (first scroll-type item)
            if (Input.GetKeyDown(KeyCode.L))
            {
                var scrollItem = shop.CurrentItems.FirstOrDefault(i => 
                    i.Type == ShopItem.ItemType.Scroll && !i.IsSoldOut);
                if (scrollItem != null)
                {
                    if (shop.TryPurchase(scrollItem))
                        Debug.Log($"[TEST] Bought scroll: {scrollItem.GetDisplayName()}! Money: ${runState.Money}");
                    else
                        Debug.Log($"[TEST] Cannot afford or slots full");
                }
                else
                {
                    Debug.Log("[TEST] No scrolls available in shop!");
                }
            }

            // Reroll
            if (Input.GetKeyDown(KeyCode.R))
            {
                if (shop.TryReroll())
                    Debug.Log($"[TEST] Rerolled shop! Money: ${runState.Money}");
                else
                    Debug.Log("[TEST] Cannot afford reroll!");
            }
        }

        private void HandleSetupInput()
        {
            // Start trial
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            {
                Debug.Log("[TEST] Starting trial...");
                gameManager.StartTrial();
                
                // Initialize board renderer with the trial board
                if (gameManager.TrialBoard != null)
                {
                    boardRenderer.Initialize(gameManager.TrialBoard);
                }
            }
        }

        private void HandleTrialInput()
        {
            if (!gameManager.IsPlayerTurn)
                return;

            if (Input.GetMouseButtonDown(0))
            {
                HandleTrialClick();
            }
        }

        private void HandleTrialClick()
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (!Physics.Raycast(ray, out RaycastHit hit))
                return;

            var objectName = hit.collider.gameObject.name;
            if (!objectName.StartsWith("Square_"))
                return;

            var parts = objectName.Split('_');
            if (parts.Length < 3)
                return;

            if (!int.TryParse(parts[1], out int file) || !int.TryParse(parts[2], out int rank))
                return;

            var clickedPos = new Position(file, rank);
            ProcessTrialMove(clickedPos);
        }

        private void ProcessTrialMove(Position clickedPos)
        {
            var board = gameManager.TrialBoard;
            var rules = gameManager.TrialRules;
            var playerColor = gameManager.PlayerColor;

            // If nothing selected, try to select
            if (selectedPiece == null)
            {
                var piece = board.GetPiece(clickedPos);
                if (piece != null && piece.Color == playerColor)
                {
                    SelectPiece(clickedPos, rules, playerColor);
                }
            }
            else
            {
                // If clicked on the same piece, deselect (toggle)
                if (selectedPiece.Value == clickedPos)
                {
                    ClearHighlights();
                    selectedPiece = null;
                    Debug.Log($"[TRIAL] Deselected piece");
                    return;
                }
                
                // Clear previous highlights
                ClearHighlights();
                
                // Try to move to clicked position
                // Find the actual legal move (handles promotion, en passant, etc.)
                var legalMoves = rules.GetLegalMoves(playerColor);
                var matchingMoves = legalMoves.FindAll(m => m.From == selectedPiece.Value && m.To == clickedPos);
                
                if (matchingMoves.Count > 0)
                {
                    // If multiple moves (promotion), pick Queen by default
                    var moveToExecute = matchingMoves.Find(m => m.PromotionPiece == PieceType.Queen) 
                                        ?? matchingMoves[0];
                    
                    if (gameManager.ExecuteTrialMove(moveToExecute))
                    {
                        if (moveToExecute.PromotionPiece != PieceType.None)
                        {
                            Debug.Log($"[TRIAL] Promoted pawn to {moveToExecute.PromotionPiece}!");
                        }
                        Debug.Log($"[TRIAL] Moved {selectedPiece.Value} → {clickedPos}");
                        selectedPiece = null;
                    }
                }
                else
                {
                    // Not a valid move destination - maybe clicked a different piece
                    var piece = board.GetPiece(clickedPos);
                    if (piece != null && piece.Color == playerColor)
                    {
                        SelectPiece(clickedPos, rules, playerColor);
                    }
                    else
                    {
                        selectedPiece = null;
                    }
                }
            }
        }

        private void SelectPiece(Position pos, ChessRules rules, Color playerColor)
        {
            selectedPiece = pos;
            var piece = gameManager.TrialBoard.GetPiece(pos);
            Debug.Log($"[TRIAL] Selected {piece.Color} {piece.Type} at {pos}");
            
            // Highlight selected square
            boardRenderer.HighlightSquare(pos, true);
            
            // Show and highlight legal moves (deduplicate for promotion moves)
            var legalMoves = rules.GetLegalMoves(playerColor);
            var pieceMoves = legalMoves.FindAll(m => m.From == pos);
            highlightedSquares.Clear();
            highlightedSquares.Add(pos);
            
            // Use HashSet to avoid highlighting same square multiple times (for promotion)
            var uniqueDestinations = new HashSet<Position>();
            foreach (var m in pieceMoves)
            {
                if (uniqueDestinations.Add(m.To))
                {
                    boardRenderer.HighlightSquare(m.To, false);
                    highlightedSquares.Add(m.To);
                }
            }
            
            Debug.Log($"[TRIAL] Legal moves: {uniqueDestinations.Count}");
        }

        private void ClearHighlights()
        {
            foreach (var pos in highlightedSquares)
            {
                boardRenderer.UnhighlightSquare(pos);
            }
            highlightedSquares.Clear();
        }

        private void PrintShopContents()
        {
            var shop = gameManager.Shop;
            
            Debug.Log("┌─────────────────────────────────────────┐");
            Debug.Log("│             SHOP CONTENTS               │");
            Debug.Log("├─────────────────────────────────────────┤");
            
            Debug.Log("│ PIECES:                                 │");
            foreach (var item in shop.CurrentItems.Where(i => i.Type == ShopItem.ItemType.Piece || i.Type == ShopItem.ItemType.PawnPack))
            {
                var status = item.IsSoldOut ? " (SOLD)" : "";
                Debug.Log($"│   {item.GetDisplayName()} - ${item.Price}{status}");
            }
            
            Debug.Log("│ GAMBITS:                                │");
            foreach (var item in shop.CurrentItems.Where(i => i.Type == ShopItem.ItemType.Gambit))
            {
                var status = item.IsSoldOut ? " (SOLD)" : "";
                Debug.Log($"│   {item.GetDisplayName()} - ${item.Price}{status}");
            }
            
            Debug.Log("│ SCROLLS:                                │");
            foreach (var item in shop.CurrentItems.Where(i => i.Type == ShopItem.ItemType.Scroll))
            {
                var status = item.IsSoldOut ? " (SOLD)" : "";
                Debug.Log($"│   {item.GetDisplayName()} - ${item.Price}{status}");
            }
            
            Debug.Log("└─────────────────────────────────────────┘");
        }

        #region Event Handlers

        private void OnPhaseChanged(GamePhase phase)
        {
            Debug.Log($"[EVENT] Phase changed to: {phase}");

            // Update board renderer when entering trial
            if (phase == GamePhase.Trial && gameManager.TrialBoard != null)
            {
                boardRenderer.Initialize(gameManager.TrialBoard);
            }
        }

        private void OnTrialEnded(WinConditionChecker.TrialResult result)
        {
            Debug.Log($"[EVENT] Trial ended: {result}");
            selectedPiece = null;
            ClearHighlights();
        }

        private void OnRunStarted()
        {
            Debug.Log("[EVENT] New run started!");
        }

        private void OnRunEnded(bool victory)
        {
            Debug.Log($"[EVENT] Run ended - {(victory ? "VICTORY!" : "DEFEAT")}");
            Debug.Log("Press N to start a new run.");
        }

        private void OnMoveExecuted(Move move, Color color)
        {
            // Sync the board visuals after any move (player or AI)
            if (boardRenderer != null)
            {
                boardRenderer.SyncWithBoard();
            }
            
            Debug.Log($"[EVENT] {color} moved: {move.From} → {move.To}");
        }

        #endregion
    }
}
