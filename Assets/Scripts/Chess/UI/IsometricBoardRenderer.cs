using UnityEngine;
using System.Collections.Generic;
using System.Reflection;

namespace Chess.UI
{
    using Core;

    /// <summary>
    /// Renders the chess board in isometric 2.5D perspective
    /// </summary>
    public class IsometricBoardRenderer : MonoBehaviour
    {
        [SerializeField] private float squareSize = 1f;
        [SerializeField] private float tileHeight = 0.15f;  // Depth/thickness of the tile
        [SerializeField] private float heightOffset = 0.1f;
        [SerializeField] private Material whiteMaterial;
        [SerializeField] private Material blackMaterial;
        [SerializeField] private Material highlightMaterial;
        [SerializeField] private Material selectedMaterial;
        [SerializeField] private Object pieceVisualSetObject; // Use Object to avoid compilation issues

        private GameObject[,] squareObjects;
        private Dictionary<Position, Transform> pieceTransforms;
        private Board board;
        private int boardSize;

        public void Initialize(Board chessBoard)
        {
            // Clean up existing visuals first
            CleanupVisuals();
            
            board = chessBoard;
            boardSize = board.Size;
            squareObjects = new GameObject[boardSize, boardSize];
            pieceTransforms = new Dictionary<Position, Transform>();

            CreateBoardVisualsAndPieces();
        }

        /// <summary>
        /// Destroy all existing visual objects
        /// </summary>
        private void CleanupVisuals()
        {
            // Destroy all piece objects
            if (pieceTransforms != null)
            {
                foreach (var kvp in pieceTransforms)
                {
                    if (kvp.Value != null)
                        Destroy(kvp.Value.gameObject);
                }
                pieceTransforms.Clear();
            }

            // Destroy all square objects
            if (squareObjects != null)
            {
                for (int file = 0; file < squareObjects.GetLength(0); file++)
                {
                    for (int rank = 0; rank < squareObjects.GetLength(1); rank++)
                    {
                        if (squareObjects[file, rank] != null)
                            Destroy(squareObjects[file, rank]);
                    }
                }
            }
        }

        private void CreateBoardVisualsAndPieces()
        {
            for (int file = 0; file < boardSize; file++)
            {
                for (int rank = 0; rank < boardSize; rank++)
                {
                    var pos = new Position(file, rank);

                    // Create square visual
                    var squareGO = CreateSquare(file, rank);
                    squareObjects[file, rank] = squareGO;

                    // Create piece if exists
                    var piece = board.GetPiece(pos);
                    if (piece != null)
                    {
                        var pieceGO = CreatePiece(piece, pos);
                        pieceTransforms[pos] = pieceGO.transform;
                    }
                }
            }
        }

        private GameObject CreateSquare(int file, int rank)
        {
            var squareGO = new GameObject($"Square_{file}_{rank}");
            squareGO.transform.SetParent(transform);

            // Simple grid layout (not isometric projection)
            // Position tile so the top surface is at y=0
            var isoPos = new Vector3(file * squareSize, -tileHeight / 2f, rank * squareSize);
            squareGO.transform.position = isoPos;

            // Add visual component
            var meshFilter = squareGO.AddComponent<MeshFilter>();
            var meshRenderer = squareGO.AddComponent<MeshRenderer>();
            
            meshFilter.mesh = CreateTileMesh();
            meshRenderer.material = ((file + rank) % 2 == 0) ? whiteMaterial : blackMaterial;

            // Add collider for click detection - sized to match the cube
            var collider = squareGO.AddComponent<BoxCollider>();
            collider.size = new Vector3(squareSize, tileHeight, squareSize);
            collider.isTrigger = true;

            return squareGO;
        }

        private GameObject CreatePiece(Piece piece, Position pos)
        {
            var pieceName = $"{piece.Color}_{piece.Type}";
            var pieceGO = new GameObject(pieceName);
            pieceGO.transform.SetParent(transform);

            // Position piece on square
            var isoPos = new Vector3(pos.File * squareSize, heightOffset, pos.Rank * squareSize);
            pieceGO.transform.position = isoPos;

            // Add SpriteRenderer component
            var spriteRenderer = pieceGO.AddComponent<SpriteRenderer>();
            
            if (pieceVisualSetObject != null)
            {
                ApplyVisualToPiece(spriteRenderer, pieceGO.transform, piece);
            }
            
            spriteRenderer.sortingOrder = 10; // Draw pieces above squares

            return pieceGO;
        }

        private void ApplyVisualToPiece(SpriteRenderer spriteRenderer, Transform pieceTransform, Piece piece)
        {
            if (pieceVisualSetObject == null)
                return;

            var visualSetType = pieceVisualSetObject.GetType();
            
            // Get the field name based on piece color and type
            string fieldName = piece.Color == Core.Color.White ? "white" : "black";
            fieldName += piece.Type switch
            {
                PieceType.Pawn => "Pawn",
                PieceType.Knight => "Knight",
                PieceType.Bishop => "Bishop",
                PieceType.Rook => "Rook",
                PieceType.Queen => "Queen",
                PieceType.King => "King",
                _ => null
            };

            if (fieldName == null)
                return;

            var field = visualSetType.GetField(fieldName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field == null)
                return;

            var visual = field.GetValue(pieceVisualSetObject);
            if (visual == null)
                return;

            // Extract sprite and scale from struct
            var spriteField = visual.GetType().GetField("sprite", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            var scaleField = visual.GetType().GetField("scale", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);

            if (spriteField != null)
            {
                var sprite = (Sprite)spriteField.GetValue(visual);
                if (sprite != null)
                    spriteRenderer.sprite = sprite;
            }

            if (scaleField != null)
            {
                pieceTransform.localScale = (Vector3)scaleField.GetValue(visual);
            }
        }

        private Mesh CreateTileMesh()
        {
            var mesh = new Mesh();
            
            float s = squareSize / 2f;  // Half size for centering
            float h = tileHeight / 2f;  // Half height for centering

            // 8 vertices for a cube
            mesh.vertices = new Vector3[]
            {
                // Bottom face
                new Vector3(-s, -h, -s),  // 0
                new Vector3( s, -h, -s),  // 1
                new Vector3( s, -h,  s),  // 2
                new Vector3(-s, -h,  s),  // 3
                // Top face
                new Vector3(-s,  h, -s),  // 4
                new Vector3( s,  h, -s),  // 5
                new Vector3( s,  h,  s),  // 6
                new Vector3(-s,  h,  s),  // 7
            };

            mesh.triangles = new int[]
            {
                // Bottom face
                0, 2, 1,
                0, 3, 2,
                // Top face
                4, 5, 6,
                4, 6, 7,
                // Front face (z = -s)
                0, 1, 5,
                0, 5, 4,
                // Back face (z = +s)
                2, 3, 7,
                2, 7, 6,
                // Left face (x = -s)
                3, 0, 4,
                3, 4, 7,
                // Right face (x = +s)
                1, 2, 6,
                1, 6, 5,
            };

            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }

        private Vector3 FileRankToIsometric(int file, int rank)
        {
            // Simple grid layout
            return new Vector3(file * squareSize, 0, rank * squareSize);
        }

        public Vector3 GetSquareWorldPosition(Position pos)
        {
            return new Vector3(pos.File * squareSize, 0, pos.Rank * squareSize) + transform.position;
        }

        public void HighlightSquare(Position pos, bool isSelected = false)
        {
            if (!pos.IsValid(boardSize))
                return;

            var squareGO = squareObjects[pos.File, pos.Rank];
            var renderer = squareGO.GetComponent<MeshRenderer>();
            renderer.material = isSelected ? selectedMaterial : highlightMaterial;
        }

        public void UnhighlightSquare(Position pos)
        {
            if (!pos.IsValid(boardSize))
                return;

            var squareGO = squareObjects[pos.File, pos.Rank];
            var renderer = squareGO.GetComponent<MeshRenderer>();
            renderer.material = ((pos.File + pos.Rank) % 2 == 0) ? whiteMaterial : blackMaterial;
        }

        public void UpdatePiecePosition(Position from, Position to)
        {
            if (pieceTransforms.TryGetValue(from, out var pieceTransform))
            {
                var isoPos = new Vector3(to.File * squareSize, heightOffset, to.Rank * squareSize);
                pieceTransform.position = isoPos;

                // Update dictionary
                pieceTransforms.Remove(from);
                pieceTransforms[to] = pieceTransform;
            }
        }

        public void RemovePiece(Position pos)
        {
            if (pieceTransforms.TryGetValue(pos, out var pieceTransform))
            {
                Destroy(pieceTransform.gameObject);
                pieceTransforms.Remove(pos);
            }
        }

        public void CreatePieceVisual(Position pos, Piece piece)
        {
            var pieceGO = CreatePiece(piece, pos);
            pieceTransforms[pos] = pieceGO.transform;
        }

        /// <summary>
        /// Sync visual state with the current board state.
        /// More efficient than Initialize() for updating after moves.
        /// </summary>
        public void SyncWithBoard()
        {
            if (board == null) return;

            // First pass: Find pieces to remove or update
            var toRemove = new List<Position>();
            var toReplace = new List<Position>();
            
            foreach (var kvp in pieceTransforms)
            {
                var pos = kvp.Key;
                var visualTransform = kvp.Value;
                var boardPiece = board.GetPiece(pos);

                if (boardPiece == null)
                {
                    // No piece on board at this position - remove visual
                    toRemove.Add(pos);
                }
                else
                {
                    // Check if the visual matches the actual piece
                    // Visual name format: "Color_Type" e.g. "White_Pawn"
                    string expectedName = $"{boardPiece.Color}_{boardPiece.Type}";
                    if (visualTransform.gameObject.name != expectedName)
                    {
                        // Wrong piece visual - needs replacement (capture happened)
                        toReplace.Add(pos);
                    }
                }
            }

            // Remove visuals for empty squares
            foreach (var pos in toRemove)
            {
                RemovePiece(pos);
            }

            // Replace visuals where a different piece now occupies the square
            foreach (var pos in toReplace)
            {
                RemovePiece(pos);
                var piece = board.GetPiece(pos);
                if (piece != null)
                {
                    CreatePieceVisual(pos, piece);
                }
            }

            // Find new pieces that need visuals
            for (int file = 0; file < boardSize; file++)
            {
                for (int rank = 0; rank < boardSize; rank++)
                {
                    var pos = new Position(file, rank);
                    var piece = board.GetPiece(pos);

                    if (piece != null && !pieceTransforms.ContainsKey(pos))
                    {
                        // New piece at this position - create visual
                        CreatePieceVisual(pos, piece);
                    }
                }
            }
        }

        public void SetPieceVisualSet(Object visualSetObject)
        {
            pieceVisualSetObject = visualSetObject;
            
            if (visualSetObject == null)
                return;

            // Refresh all piece visuals
            var piecesToUpdate = new List<(Position pos, Transform transform)>();
            foreach (var pos in pieceTransforms.Keys)
            {
                piecesToUpdate.Add((pos, pieceTransforms[pos]));
            }

            foreach (var (pos, pieceTransform) in piecesToUpdate)
            {
                var piece = board.GetPiece(pos);
                if (piece != null)
                {
                    var spriteRenderer = pieceTransform.GetComponent<SpriteRenderer>();
                    if (spriteRenderer != null)
                    {
                        ApplyVisualToPiece(spriteRenderer, pieceTransform, piece);
                    }
                }
            }
        }
    }
}
