using UnityEngine;
using System.Collections.Generic;

namespace ChessBalatro
{
    public class ChessBoard : MonoBehaviour
    {
        [Header("Board Settings")]
        public int boardSize = 8;
        public float tileSpacing = 1f;
        
        [Header("Prefabs")]
        public GameObject tilePrefab;
        public GameObject piecePrefab;
        
        [Header("Sprites")]
        public Sprite lightTileSprite;
        public Sprite darkTileSprite;
        
        [Header("Piece Sprites - White")]
        public Sprite whiteKing;
        public Sprite whiteQueen;
        public Sprite whiteRook;
        public Sprite whiteBishop;
        public Sprite whiteKnight;
        public Sprite whitePawn;
        
        [Header("Piece Sprites - Black")]
        public Sprite blackKing;
        public Sprite blackQueen;
        public Sprite blackRook;
        public Sprite blackBishop;
        public Sprite blackKnight;
        public Sprite blackPawn;
        
        [Header("2.5D Camera Settings")]
        public Camera gameCamera;
        public Vector3 cameraOffset = new Vector3(3.5f, 10f, -3.5f);
        public Vector3 cameraRotation = new Vector3(60f, 0f, 0f);

        // Board state
        private BoardTile[,] tiles;
        private ChessPiece[,] pieces;
        private Transform boardParent;
        private Transform piecesParent;

        private void Start()
        {
            InitializeBoard();
            SetupCamera();
            PlacePiecesInStartingPositions();
        }

        private void InitializeBoard()
        {
            // Create parent objects for organization
            boardParent = new GameObject("Board Tiles").transform;
            boardParent.SetParent(transform);
            
            piecesParent = new GameObject("Chess Pieces").transform;
            piecesParent.SetParent(transform);

            // Initialize arrays
            tiles = new BoardTile[boardSize, boardSize];
            pieces = new ChessPiece[boardSize, boardSize];

            // Generate tiles
            for (int x = 0; x < boardSize; x++)
            {
                for (int y = 0; y < boardSize; y++)
                {
                    CreateTile(x, y);
                }
            }

            Debug.Log($"Chess board initialized with {boardSize}x{boardSize} tiles, spacing: {tileSpacing}");
        }        private void CreateTile(int x, int y)
        {
            Vector3 position = new Vector3(x * tileSpacing, 0, y * tileSpacing);
            bool isLightTile = (x + y) % 2 == 0;

            GameObject tileObject;
            if (tilePrefab != null)
            {
                tileObject = Instantiate(tilePrefab, position, Quaternion.Euler(90f, 0f, 0f), boardParent);
                tileObject.name = $"Tile_{x}_{y}";
            }
            else
            {
                // Create a basic tile if no prefab is assigned
                tileObject = new GameObject($"Tile_{x}_{y}");
                tileObject.transform.SetParent(boardParent);
                tileObject.transform.position = position;
                tileObject.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
                
                // Add sprite renderer
                SpriteRenderer sr = tileObject.AddComponent<SpriteRenderer>();
                sr.sprite = isLightTile ? lightTileSprite : darkTileSprite;
                
                // Scale the sprite to match tile spacing
                if (sr.sprite != null)
                {
                    tileObject.transform.localScale = Vector3.one * tileSpacing;
                }
                
                // Add collider for mouse interaction - SIZE BASED ON TILE SPACING
                BoxCollider collider = tileObject.AddComponent<BoxCollider>();
                collider.size = new Vector3(tileSpacing, 0.1f, tileSpacing);
            }

            // Add or get BoardTile component
            BoardTile tile = tileObject.GetComponent<BoardTile>();
            if (tile == null)
                tile = tileObject.AddComponent<BoardTile>();

            tile.Initialize(new Vector2Int(x, y), isLightTile, isLightTile ? lightTileSprite : darkTileSprite);
            tiles[x, y] = tile;
        }

        private void SetupCamera()
        {
            if (gameCamera == null)
                gameCamera = Camera.main;

            if (gameCamera != null)
            {
                // UPDATED: Calculate board center using tileSpacing
                Vector3 boardCenter = new Vector3(
                    (boardSize - 1) * tileSpacing * 0.5f, 
                    0, 
                    (boardSize - 1) * tileSpacing * 0.5f
                );
                
                // UPDATED: Scale camera offset based on tile spacing and board size
                Vector3 scaledOffset = new Vector3(
                    cameraOffset.x * tileSpacing,
                    cameraOffset.y * tileSpacing,
                    cameraOffset.z * tileSpacing
                );
                
                gameCamera.transform.position = boardCenter + scaledOffset;
                gameCamera.transform.rotation = Quaternion.Euler(cameraRotation);
                
                Debug.Log($"Camera positioned at: {gameCamera.transform.position}, looking at board center: {boardCenter}");
            }
        }

        public void PlacePiecesInStartingPositions()
        {
            // Clear existing pieces
            ClearAllPieces();

            // Place white pieces (bottom of board)
            PlacePiece(PieceType.Rook, PieceColor.White, 0, 0);
            PlacePiece(PieceType.Knight, PieceColor.White, 1, 0);
            PlacePiece(PieceType.Bishop, PieceColor.White, 2, 0);
            PlacePiece(PieceType.Queen, PieceColor.White, 3, 0);
            PlacePiece(PieceType.King, PieceColor.White, 4, 0);
            PlacePiece(PieceType.Bishop, PieceColor.White, 5, 0);
            PlacePiece(PieceType.Knight, PieceColor.White, 6, 0);
            PlacePiece(PieceType.Rook, PieceColor.White, 7, 0);

            // Place white pawns
            for (int x = 0; x < boardSize; x++)
            {
                PlacePiece(PieceType.Pawn, PieceColor.White, x, 1);
            }

            // Place black pieces (top of board)
            PlacePiece(PieceType.Rook, PieceColor.Black, 0, 7);
            PlacePiece(PieceType.Knight, PieceColor.Black, 1, 7);
            PlacePiece(PieceType.Bishop, PieceColor.Black, 2, 7);
            PlacePiece(PieceType.Queen, PieceColor.Black, 3, 7);
            PlacePiece(PieceType.King, PieceColor.Black, 4, 7);
            PlacePiece(PieceType.Bishop, PieceColor.Black, 5, 7);
            PlacePiece(PieceType.Knight, PieceColor.Black, 6, 7);
            PlacePiece(PieceType.Rook, PieceColor.Black, 7, 7);

            // Place black pawns
            for (int x = 0; x < boardSize; x++)
            {
                PlacePiece(PieceType.Pawn, PieceColor.Black, x, 6);
            }

            Debug.Log("All pieces placed in starting positions");
        }

        public void PlacePiece(PieceType type, PieceColor color, int x, int y)
        {
            if (!IsValidPosition(x, y)) return;

            // UPDATED: Use tileSpacing for piece positioning and height
            Vector3 position = new Vector3(x * tileSpacing, 0.1f * tileSpacing, y * tileSpacing);
            
            GameObject pieceObject;
            if (piecePrefab != null)
            {
                pieceObject = Instantiate(piecePrefab, position, Quaternion.identity, piecesParent);
                pieceObject.name = $"{color}_{type}_{x}_{y}";
            }
            else
            {
                // Create a basic piece if no prefab is assigned
                pieceObject = new GameObject($"{color}_{type}_{x}_{y}");
                pieceObject.transform.SetParent(piecesParent);
                pieceObject.transform.position = position;
                
                // Add sprite renderer
                SpriteRenderer sr = pieceObject.AddComponent<SpriteRenderer>();
                sr.sortingOrder = 1; // Render above tiles
                
                // UPDATED: Scale piece to match tile spacing
                if (sr.sprite != null)
                {
                    pieceObject.transform.localScale = Vector3.one * tileSpacing * 0.8f; // Slightly smaller than tile
                }
                
                // UPDATED: Add collider for interaction - size based on tile spacing
                BoxCollider collider = pieceObject.AddComponent<BoxCollider>();
                collider.size = new Vector3(tileSpacing * 0.8f, tileSpacing * 0.2f, tileSpacing * 0.8f);
            }

            // Add or get ChessPiece component
            ChessPiece piece = pieceObject.GetComponent<ChessPiece>();
            if (piece == null)
                piece = pieceObject.AddComponent<ChessPiece>();

            // Get the appropriate sprite for this piece
            Sprite pieceSprite = GetPieceSprite(type, color);
            PieceData pieceData = new PieceData(type, color, pieceSprite);
            
            piece.Initialize(pieceData, new Vector2Int(x, y));
            pieces[x, y] = piece;
        }

        private Sprite GetPieceSprite(PieceType type, PieceColor color)
        {
            if (color == PieceColor.White)
            {
                return type switch
                {
                    PieceType.King => whiteKing,
                    PieceType.Queen => whiteQueen,
                    PieceType.Rook => whiteRook,
                    PieceType.Bishop => whiteBishop,
                    PieceType.Knight => whiteKnight,
                    PieceType.Pawn => whitePawn,
                    _ => null
                };
            }
            else
            {
                return type switch
                {
                    PieceType.King => blackKing,
                    PieceType.Queen => blackQueen,
                    PieceType.Rook => blackRook,
                    PieceType.Bishop => blackBishop,
                    PieceType.Knight => blackKnight,
                    PieceType.Pawn => blackPawn,
                    _ => null
                };
            }
        }

        public ChessPiece GetPieceAt(Vector2Int position)
        {
            if (!IsValidPosition(position.x, position.y))
                return null;
            
            return pieces[position.x, position.y];
        }

        public BoardTile GetTileAt(Vector2Int position)
        {
            if (!IsValidPosition(position.x, position.y))
                return null;
            
            return tiles[position.x, position.y];
        }

        public bool IsValidPosition(int x, int y)
        {
            return x >= 0 && x < boardSize && y >= 0 && y < boardSize;
        }

        public void ClearAllPieces()
        {
            for (int x = 0; x < boardSize; x++)
            {
                for (int y = 0; y < boardSize; y++)
                {
                    if (pieces[x, y] != null)
                    {
                        DestroyImmediate(pieces[x, y].gameObject);
                        pieces[x, y] = null;
                    }
                }
            }
        }        public bool MovePiece(Vector2Int from, Vector2Int to)
        {
            if (!IsValidPosition(from.x, from.y) || !IsValidPosition(to.x, to.y))
                return false;

            ChessPiece piece = pieces[from.x, from.y];
            if (piece == null)
                return false;

            // Check if the move is valid for this piece type
            if (!piece.IsValidMove(to, this))
                return false;

            // CRITICAL: Check if this move would put own king in check
            if (WouldMovePutKingInCheck(from, to, piece.pieceData.color))
            {
                Debug.Log($"Move rejected: Would put {piece.pieceData.color} king in check!");
                return false;
            }

            // Capture piece if there's one at the destination
            ChessPiece capturedPiece = pieces[to.x, to.y];
            if (capturedPiece != null)
            {
                DestroyImmediate(capturedPiece.gameObject);
            }

            // Move the piece
            pieces[from.x, from.y] = null;
            pieces[to.x, to.y] = piece;
            piece.SetBoardPosition(to);

            Debug.Log($"Move completed: {piece.pieceData.color} {piece.pieceData.type} from {from} to {to}");
            return true;
        }

        // Method for roguelike features - randomize board layout
        public void GenerateRandomBoard()
        {
            // TODO: Implement random board generation for roguelike elements
            // This could include different board sizes, obstacles, special tiles, etc.
            Debug.Log("Random board generation not yet implemented");
        }

        // UTILITY: Method to refresh board spacing at runtime (useful for testing)        [ContextMenu("Refresh Board Spacing")]
        public void RefreshBoardSpacing()
        {
            if (tiles != null)
            {
                for (int x = 0; x < boardSize; x++)
                {
                    for (int y = 0; y < boardSize; y++)
                    {
                        if (tiles[x, y] != null)
                        {
                            Vector3 newPosition = new Vector3(x * tileSpacing, 0, y * tileSpacing);
                            tiles[x, y].transform.position = newPosition;
                            tiles[x, y].transform.rotation = Quaternion.Euler(90f, 0f, 0f);
                            tiles[x, y].transform.localScale = Vector3.one * tileSpacing;
                            
                            // Update collider size
                            BoxCollider collider = tiles[x, y].GetComponent<BoxCollider>();
                            if (collider != null)
                            {
                                collider.size = new Vector3(tileSpacing, 0.1f, tileSpacing);
                            }
                        }
                    }
                }
            }

            if (pieces != null)
            {
                for (int x = 0; x < boardSize; x++)
                {
                    for (int y = 0; y < boardSize; y++)
                    {
                        if (pieces[x, y] != null)
                        {
                            Vector3 newPosition = new Vector3(x * tileSpacing, 0.1f * tileSpacing, y * tileSpacing);
                            pieces[x, y].transform.position = newPosition;
                            pieces[x, y].transform.localScale = Vector3.one * tileSpacing * 0.8f;
                        }
                    }
                }
            }

            SetupCamera();
            Debug.Log($"Board spacing refreshed to: {tileSpacing}");
        }

        // CHESS RULES VALIDATION METHODS

        /// <summary>
        /// Finds the king of the specified color on the board
        /// </summary>
        public Vector2Int FindKing(PieceColor color)
        {
            for (int x = 0; x < boardSize; x++)
            {
                for (int y = 0; y < boardSize; y++)
                {
                    ChessPiece piece = pieces[x, y];
                    if (piece != null && piece.pieceData.type == PieceType.King && piece.pieceData.color == color)
                    {
                        return new Vector2Int(x, y);
                    }
                }
            }
            return new Vector2Int(-1, -1); // King not found (shouldn't happen in valid game)
        }

        /// <summary>
        /// Checks if a given position is under attack by the specified color
        /// </summary>
        public bool IsPositionUnderAttack(Vector2Int position, PieceColor attackingColor)
        {
            for (int x = 0; x < boardSize; x++)
            {
                for (int y = 0; y < boardSize; y++)
                {
                    ChessPiece piece = pieces[x, y];
                    if (piece != null && piece.pieceData.color == attackingColor)
                    {
                        // Use the piece's IsValidMove method, but we need to handle pawns specially
                        // since pawn attacks are different from pawn moves
                        if (piece.pieceData.type == PieceType.Pawn)
                        {
                            if (IsPawnAttackingPosition(piece, position))
                                return true;
                        }
                        else
                        {
                            if (piece.IsValidMove(position, this))
                                return true;
                        }
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Checks if a pawn is attacking a specific position (pawns attack diagonally)
        /// </summary>
        private bool IsPawnAttackingPosition(ChessPiece pawn, Vector2Int targetPosition)
        {
            Vector2Int pawnPos = pawn.boardPosition;
            int deltaX = targetPosition.x - pawnPos.x;
            int deltaY = targetPosition.y - pawnPos.y;
            int direction = pawn.pieceData.color == PieceColor.White ? 1 : -1;
            
            // Pawn attacks diagonally forward
            return Mathf.Abs(deltaX) == 1 && deltaY == direction;
        }

        /// <summary>
        /// Checks if the specified color's king is currently in check
        /// </summary>
        public bool IsKingInCheck(PieceColor kingColor)
        {
            Vector2Int kingPosition = FindKing(kingColor);
            if (kingPosition.x == -1) return false; // King not found
            
            PieceColor enemyColor = kingColor == PieceColor.White ? PieceColor.Black : PieceColor.White;
            return IsPositionUnderAttack(kingPosition, enemyColor);
        }

        /// <summary>
        /// Simulates a move and checks if it would leave the king in check
        /// </summary>
        public bool WouldMovePutKingInCheck(Vector2Int from, Vector2Int to, PieceColor playerColor)
        {
            // Store the original state
            ChessPiece movingPiece = pieces[from.x, from.y];
            ChessPiece capturedPiece = pieces[to.x, to.y];
            
            if (movingPiece == null) return true; // Invalid move
            
            // Temporarily make the move
            pieces[from.x, from.y] = null;
            pieces[to.x, to.y] = movingPiece;
            Vector2Int originalPosition = movingPiece.boardPosition;
            movingPiece.SetBoardPosition(to);
            
            // Check if king is in check after this move
            bool kingInCheck = IsKingInCheck(playerColor);
            
            // Restore the original state
            pieces[from.x, from.y] = movingPiece;
            pieces[to.x, to.y] = capturedPiece;
            movingPiece.SetBoardPosition(originalPosition);
            
            return kingInCheck;
        }

        /// <summary>
        /// Gets all legal moves for a specific piece (moves that don't put own king in check)
        /// </summary>
        public List<Vector2Int> GetLegalMovesForPiece(Vector2Int piecePosition)
        {
            List<Vector2Int> legalMoves = new List<Vector2Int>();
            ChessPiece piece = GetPieceAt(piecePosition);
            
            if (piece == null) return legalMoves;
            
            // Check all possible positions on the board
            for (int x = 0; x < boardSize; x++)
            {
                for (int y = 0; y < boardSize; y++)
                {
                    Vector2Int targetPosition = new Vector2Int(x, y);
                    
                    // Skip if it's the same position
                    if (targetPosition == piecePosition) continue;
                    
                    // Check if the move is valid for this piece type
                    if (piece.IsValidMove(targetPosition, this))
                    {
                        // Check if this move would put own king in check
                        if (!WouldMovePutKingInCheck(piecePosition, targetPosition, piece.pieceData.color))
                        {
                            legalMoves.Add(targetPosition);
                        }
                    }
                }
            }
            
            return legalMoves;
        }

        /// <summary>
        /// Gets all legal moves for all pieces of the specified color
        /// </summary>
        public List<Vector2Int> GetAllLegalMoves(PieceColor color)
        {
            List<Vector2Int> allMoves = new List<Vector2Int>();
            
            for (int x = 0; x < boardSize; x++)
            {
                for (int y = 0; y < boardSize; y++)
                {
                    ChessPiece piece = pieces[x, y];
                    if (piece != null && piece.pieceData.color == color)
                    {
                        List<Vector2Int> pieceMoves = GetLegalMovesForPiece(new Vector2Int(x, y));
                        allMoves.AddRange(pieceMoves);
                    }
                }
            }
            
            return allMoves;
        }

        /// <summary>
        /// Checks if the specified color is in checkmate
        /// </summary>
        public bool IsCheckmate(PieceColor color)
        {
            // Must be in check AND have no legal moves
            return IsKingInCheck(color) && GetAllLegalMoves(color).Count == 0;
        }

        /// <summary>
        /// Checks if the specified color is in stalemate
        /// </summary>
        public bool IsStalemate(PieceColor color)
        {
            // Must NOT be in check AND have no legal moves
            return !IsKingInCheck(color) && GetAllLegalMoves(color).Count == 0;
        }

        /// <summary>
        /// Get the current game state as a string for debugging
        /// </summary>
        public string GetGameStateString(PieceColor color)
        {
            bool inCheck = IsKingInCheck(color);
            bool inCheckmate = IsCheckmate(color);
            bool inStalemate = IsStalemate(color);
            int legalMoves = GetAllLegalMoves(color).Count;
            
            if (inCheckmate)
                return $"{color} is in CHECKMATE!";
            else if (inStalemate)
                return $"{color} is in STALEMATE!";
            else if (inCheck)
                return $"{color} is in CHECK! ({legalMoves} legal moves available)";
            else
                return $"{color} has {legalMoves} legal moves available";
        }
    }
}