using UnityEngine;

namespace ChessBalatro
{    public class GameManager : MonoBehaviour
    {
        [Header("Game References")]
        public ChessBoard chessBoard;
        public ChessAI chessAI;
        
        [Header("Game State")]
        public PieceColor currentPlayer = PieceColor.White;
        public PieceColor humanPlayer = PieceColor.White;
        public bool gameActive = true;
        public bool vsAI = true;
        
        [Header("Selection")]
        private ChessPiece selectedPiece;
        private BoardTile selectedTile;
        private BoardTile hoveredTile; // Track currently hovered tile
        
        [Header("UI")]
        public GameObject selectionHighlight;
        public GameObject hoverHighlight; // New highlight for cursor hover
        
        [Header("Hover Settings")]
        public Color hoverColor = new Color(1f, 1f, 0f, 0.3f); // Yellow with transparency
        public Color validMoveHoverColor = new Color(0f, 1f, 0f, 0.5f); // Green for valid moves
        public Color invalidMoveHoverColor = new Color(1f, 0f, 0f, 0.3f); // Red for invalid moves
        public float hoverHeightOffset = 0.005f;
        
        private void Start()
        {            if (chessBoard == null)
                chessBoard = FindFirstObjectByType<ChessBoard>();
            
            if (chessAI == null)
                chessAI = FindFirstObjectByType<ChessAI>();
            
            if (chessBoard == null)
            {
                Debug.LogError("No ChessBoard found in the scene!");
                return;
            }
            
            // Auto-create hover highlight if not assigned
            CreateHoverHighlightIfMissing();
            
            Debug.Log($"Game started. Current player: {currentPlayer}");
            Debug.Log($"Playing vs AI: {vsAI}, Human plays as: {humanPlayer}");
            
            // If AI goes first, make AI move
            if (vsAI && currentPlayer != humanPlayer)
            {
                Invoke(nameof(TriggerAIMove), 0.5f);
            }
        }

        private void Update()
        {
            HandleInput();
        }        private void HandleInput()
        {
            if (!gameActive) return;
            
            // Don't handle input if it's AI's turn
            if (vsAI && currentPlayer != humanPlayer) return;

            // Handle continuous mouse hover detection for tile highlighting
            HandleTileHover();

            // Handle mouse input for piece and tile selection
            if (Input.GetMouseButtonDown(0))
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;

                if (Physics.Raycast(ray, out hit))
                {
                    // Check if we hit a piece
                    ChessPiece piece = hit.collider.GetComponent<ChessPiece>();
                    if (piece != null)
                    {
                        OnPieceClicked(piece);
                        return;
                    }

                    // Check if we hit a tile
                    BoardTile tile = hit.collider.GetComponent<BoardTile>();
                    if (tile != null)
                    {
                        OnTileClicked(tile);
                        return;
                    }
                }
            }

            // Handle keyboard input for game controls
            if (Input.GetKeyDown(KeyCode.R))
            {
                RestartGame();
            }
            
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                DeselectAll();
            }
            
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                ToggleAI();
            }
        }public void OnPieceClicked(ChessPiece piece)
        {
            if (!gameActive) return;
            
            // Don't allow input if it's AI's turn
            if (vsAI && currentPlayer != humanPlayer) return;

            // If no piece is selected, select this piece (if it belongs to current player)
            if (selectedPiece == null)
            {
                if (piece.pieceData.color == currentPlayer)
                {
                    SelectPiece(piece);
                }
                else
                {
                    Debug.Log($"It's {currentPlayer}'s turn!");
                }
            }
            else
            {
                // If a piece is already selected, try to capture or deselect
                if (piece.pieceData.color != currentPlayer)
                {
                    // Try to capture the piece
                    TryMovePiece(selectedPiece.boardPosition, piece.boardPosition);
                }
                else
                {
                    // Select the new piece
                    SelectPiece(piece);
                }
            }
        }        public void OnTileClicked(BoardTile tile)
        {
            if (!gameActive) return;
            
            // Don't allow input if it's AI's turn
            if (vsAI && currentPlayer != humanPlayer) return;

            // If a piece is selected, try to move it to this tile
            if (selectedPiece != null)
            {
                TryMovePiece(selectedPiece.boardPosition, tile.boardPosition);
            }
            
            selectedTile = tile;
        }

        private void SelectPiece(ChessPiece piece)
        {
            DeselectAll();
            selectedPiece = piece;
            
            // Visual feedback for selection
            if (selectionHighlight != null)
            {
                selectionHighlight.SetActive(true);
                selectionHighlight.transform.position = piece.transform.position + Vector3.up * 0.01f;
            }
            
            Debug.Log($"Selected {piece.pieceData.color} {piece.pieceData.type} at {piece.boardPosition}");
        }        private void TryMovePiece(Vector2Int from, Vector2Int to)
        {
            if (chessBoard.MovePiece(from, to))
            {
                Debug.Log($"✅ Moved {selectedPiece.pieceData.color} {selectedPiece.pieceData.type} from {from} to {to}");
                
                // Switch turns
                SwitchPlayer();
                DeselectAll();
                
                // Check for game ending conditions
                CheckGameState();
            }
            else
            {
                // Provide more detailed feedback about why the move failed
                ChessPiece piece = chessBoard.GetPieceAt(from);
                if (piece != null)
                {
                    if (!piece.IsValidMove(to, chessBoard))
                    {
                        Debug.Log($"❌ Invalid move for {piece.pieceData.type}: {from} → {to}");
                    }
                    else if (chessBoard.WouldMovePutKingInCheck(from, to, piece.pieceData.color))
                    {
                        Debug.Log($"❌ Move rejected: Would put {piece.pieceData.color} king in check!");
                    }
                    else
                    {
                        Debug.Log($"❌ Move failed: {from} → {to}");
                    }
                }
                else
                {
                    Debug.Log($"❌ No piece at {from}");
                }
            }
        }private void SwitchPlayer()
        {
            currentPlayer = (currentPlayer == PieceColor.White) ? PieceColor.Black : PieceColor.White;
            Debug.Log($"Current player: {currentPlayer}");
            
            // Trigger AI move if it's AI's turn
            if (vsAI && currentPlayer != humanPlayer)
            {
                Invoke(nameof(TriggerAIMove), 0.5f);
            }
        }        private void DeselectAll()
        {
            selectedPiece = null;
            selectedTile = null;
            
            if (selectionHighlight != null)
            {
                selectionHighlight.SetActive(false);
            }
            
            // Don't clear hover highlight here - it should stay independent
            // hoveredTile and hoverHighlight are managed separately
        }private void CheckGameState()
        {
            if (chessBoard == null) return;
            
            // Check current player's game state
            bool isInCheck = chessBoard.IsKingInCheck(currentPlayer);
            bool isCheckmate = chessBoard.IsCheckmate(currentPlayer);
            bool isStalemate = chessBoard.IsStalemate(currentPlayer);
            
            // Enhanced debugging with game state string
            Debug.Log($"Game State: {chessBoard.GetGameStateString(currentPlayer)}");
              if (isCheckmate)
            {
                PieceColor winner = currentPlayer == PieceColor.White ? PieceColor.Black : PieceColor.White;
                Debug.Log($"🏆 CHECKMATE! {winner} wins! 🏆");
                gameActive = false;
                ClearHoverHighlight(); // Clear hover when game ends
                
                // TODO: Show checkmate UI notification
                // For now, just log the result
            }
            else if (isStalemate)
            {
                Debug.Log("⚡ STALEMATE! The game is a draw. ⚡");
                gameActive = false;
                ClearHoverHighlight(); // Clear hover when game ends
                
                // TODO: Show stalemate UI notification
                // For now, just log the result
            }
            else if (isInCheck)
            {
                Debug.Log($"⚠️ {currentPlayer} is in CHECK! ⚠️");
                
                // TODO: Highlight the king in check or show visual warning
                // For now, just log the warning
            }
            
            // Additional game state checks could be added here:
            // - Fifty-move rule
            // - Threefold repetition
            // - Insufficient material
        }        public void RestartGame()
        {
            DeselectAll();
            ClearHoverHighlight();
            currentPlayer = PieceColor.White;
            gameActive = true;
            
            if (chessBoard != null)
            {
                chessBoard.PlacePiecesInStartingPositions();
            }
            
            Debug.Log("Game restarted!");
            
            // If AI goes first, make AI move
            if (vsAI && currentPlayer != humanPlayer)
            {
                Invoke(nameof(TriggerAIMove), 0.5f);
            }
        }

        public void PauseGame()
        {
            gameActive = false;
            Debug.Log("Game paused");
        }

        public void ResumeGame()
        {
            gameActive = true;
            Debug.Log("Game resumed");
        }        // Roguelike features
        public void GenerateNewLevel()
        {
            // TODO: Implement roguelike level generation
            if (chessBoard != null)
            {
                chessBoard.GenerateRandomBoard();
            }
            
            RestartGame();
        }
        
        // AI Integration Methods
        private void TriggerAIMove()
        {
            if (chessAI != null && gameActive && vsAI && currentPlayer != humanPlayer)
            {
                chessAI.MakeMove();
            }
        }
        
        public void OnAIMoveComplete()
        {
            // Called by AI when it completes a move
            SwitchPlayer();
            DeselectAll();
            CheckGameState();
        }
        
        public void ToggleAI()
        {
            vsAI = !vsAI;
            Debug.Log($"AI mode: {(vsAI ? "ON" : "OFF")}");
            
            if (vsAI && currentPlayer != humanPlayer)
            {
                Invoke(nameof(TriggerAIMove), 0.5f);
            }
        }
        
        public void SetHumanPlayerColor(PieceColor color)
        {
            humanPlayer = color;
            
            if (chessAI != null)
            {
                chessAI.aiColor = (color == PieceColor.White) ? PieceColor.Black : PieceColor.White;
            }
            
            Debug.Log($"Human plays as: {humanPlayer}, AI plays as: {chessAI?.aiColor}");
        }
        
        /// <summary>
        /// Handles continuous mouse hover detection for tile highlighting
        /// </summary>
        private void HandleTileHover()
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            BoardTile newHoveredTile = null;

            // Check what we're hovering over
            if (Physics.Raycast(ray, out hit))
            {
                // Check if we hit a tile
                BoardTile tile = hit.collider.GetComponent<BoardTile>();
                if (tile != null)
                {
                    newHoveredTile = tile;
                }
            }

            // Update hover highlight if hovered tile changed
            if (newHoveredTile != hoveredTile)
            {
                SetHoveredTile(newHoveredTile);
            }
        }

        /// <summary>
        /// Sets the currently hovered tile and updates highlight
        /// </summary>
        private void SetHoveredTile(BoardTile tile)
        {
            hoveredTile = tile;
            UpdateHoverHighlight();
        }        /// <summary>
        /// Updates the hover highlight position and visibility
        /// </summary>
        private void UpdateHoverHighlight()
        {
            if (hoverHighlight == null) return;

            if (hoveredTile != null)
            {
                // Position hover highlight over the hovered tile
                Vector3 hoverPosition = hoveredTile.transform.position + Vector3.up * hoverHeightOffset;
                hoverHighlight.transform.position = hoverPosition;
                hoverHighlight.SetActive(true);
                
                // Determine hover color based on game context
                Color targetColor = GetContextualHoverColor();
                
                // Apply hover highlight color/material
                Renderer hoverRenderer = hoverHighlight.GetComponent<Renderer>();
                if (hoverRenderer != null)
                {
                    hoverRenderer.material.color = targetColor;
                }
                
                // Debug info (can be removed later)
                // Debug.Log($"Hovering over tile: {hoveredTile.boardPosition}");
            }
            else
            {
                // No tile hovered, hide highlight
                hoverHighlight.SetActive(false);
            }
        }        /// <summary>
        /// Gets the appropriate hover color based on current game context
        /// </summary>
        private Color GetContextualHoverColor()
        {
            // If no piece is selected, show default hover color
            if (selectedPiece == null)
            {
                return hoverColor;
            }

            // If a piece is selected, show whether this tile is a valid move destination
            if (chessBoard != null && hoveredTile != null)
            {
                bool isValidMove = selectedPiece.IsValidMove(hoveredTile.boardPosition, chessBoard) &&
                                   !chessBoard.WouldMovePutKingInCheck(selectedPiece.boardPosition, hoveredTile.boardPosition, selectedPiece.pieceData.color);
                
                return isValidMove ? validMoveHoverColor : invalidMoveHoverColor;
            }

            return hoverColor;
        }
        
        /// <summary>
        /// Clears hover highlight (useful when game becomes inactive)
        /// </summary>
        public void ClearHoverHighlight()
        {
            SetHoveredTile(null);
        }

        /// <summary>
        /// Creates the hover highlight object if it doesn't exist
        /// </summary>
        private void CreateHoverHighlightIfMissing()
        {
            if (hoverHighlight == null)
            {
                // Create hover highlight GameObject
                hoverHighlight = GameObject.CreatePrimitive(PrimitiveType.Quad);
                hoverHighlight.name = "HoverHighlight";
                hoverHighlight.transform.rotation = Quaternion.Euler(90, 0, 0);
                hoverHighlight.transform.localScale = new Vector3(0.9f, 0.9f, 1f);
                
                // Remove the collider (we don't want it to interfere with raycasting)
                Collider hoverCollider = hoverHighlight.GetComponent<Collider>();
                if (hoverCollider != null)
                {
                    DestroyImmediate(hoverCollider);
                }
                
                // Create and assign material for transparency
                Material hoverMaterial = new Material(Shader.Find("Standard"));
                hoverMaterial.SetFloat("_Mode", 3); // Transparent mode
                hoverMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                hoverMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                hoverMaterial.SetInt("_ZWrite", 0);
                hoverMaterial.DisableKeyword("_ALPHATEST_ON");
                hoverMaterial.EnableKeyword("_ALPHABLEND_ON");
                hoverMaterial.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                hoverMaterial.renderQueue = 3000;
                hoverMaterial.color = hoverColor;
                
                hoverHighlight.GetComponent<Renderer>().material = hoverMaterial;
                hoverHighlight.SetActive(false);
                
                Debug.Log("✅ Hover highlight created programmatically!");
            }
        }
    }
}
