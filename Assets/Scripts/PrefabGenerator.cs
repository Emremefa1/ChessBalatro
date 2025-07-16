using UnityEngine;

namespace ChessBalatro
{
    /// <summary>
    /// This script helps create basic prefabs for tiles and pieces if you don't want to manually create them
    /// </summary>
    public class PrefabGenerator : MonoBehaviour
    {
        [Header("Prefab Creation")]
        public bool createTilePrefab = true;
        public bool createPiecePrefab = true;
        
        [Header("Tile Settings")]
        public Material tileMaterial;
        public float tileSize = 1f;
        
        [Header("Piece Settings")]
        public Material pieceMaterial;
        public float pieceScale = 0.8f;

        [ContextMenu("Create Basic Prefabs")]
        public void CreateBasicPrefabs()
        {
            if (createTilePrefab)
            {
                CreateTilePrefab();
            }
            
            if (createPiecePrefab)
            {
                CreatePiecePrefab();
            }
        }

        private void CreateTilePrefab()
        {
            // Create tile prefab
            GameObject tilePrefab = new GameObject("ChessTile");
            
            // Add sprite renderer
            SpriteRenderer tileRenderer = tilePrefab.AddComponent<SpriteRenderer>();
            if (tileMaterial != null)
            {
                tileRenderer.material = tileMaterial;
            }
            
            // Add collider for mouse interaction
            BoxCollider tileCollider = tilePrefab.AddComponent<BoxCollider>();
            tileCollider.size = new Vector3(tileSize, 0.1f, tileSize);
            
            // Add BoardTile component
            tilePrefab.AddComponent<BoardTile>();
            
            // Create the prefab folder if it doesn't exist
            string prefabPath = "Assets/Prefabs/";
            if (!System.IO.Directory.Exists(prefabPath))
            {
                System.IO.Directory.CreateDirectory(prefabPath);
            }
            
            Debug.Log("Tile prefab created! You can now save it as a prefab in the Assets/Prefabs folder.");
        }

        private void CreatePiecePrefab()
        {
            // Create piece prefab
            GameObject piecePrefab = new GameObject("ChessPiece");
            
            // Add sprite renderer
            SpriteRenderer pieceRenderer = piecePrefab.AddComponent<SpriteRenderer>();
            pieceRenderer.sortingOrder = 1; // Render above tiles
            if (pieceMaterial != null)
            {
                pieceRenderer.material = pieceMaterial;
            }
            
            // Add collider for interaction
            BoxCollider pieceCollider = piecePrefab.AddComponent<BoxCollider>();
            pieceCollider.size = new Vector3(pieceScale, 0.2f, pieceScale);
            
            // Add ChessPiece component
            piecePrefab.AddComponent<ChessPiece>();
            
            // Scale the piece
            piecePrefab.transform.localScale = Vector3.one * pieceScale;
            
            Debug.Log("Piece prefab created! You can now save it as a prefab in the Assets/Prefabs folder.");
        }
    }
}
