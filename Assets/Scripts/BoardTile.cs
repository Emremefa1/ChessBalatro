using UnityEngine;

namespace ChessBalatro
{
    public class BoardTile : MonoBehaviour
    {
        [Header("Tile Settings")]
        public Vector2Int boardPosition;
        public bool isLightTile;
        
        [Header("Visual Components")]
        public SpriteRenderer spriteRenderer;
        public Material lightTileMaterial;
        public Material darkTileMaterial;
        
        [Header("2.5D Settings")]
        public float tileHeight = 0f;

        private void Awake()
        {
            if (spriteRenderer == null)
                spriteRenderer = GetComponent<SpriteRenderer>();
        }

        public void Initialize(Vector2Int position, bool isLight, Sprite tileSprite = null)
        {
            boardPosition = position;
            isLightTile = isLight;
            
            // Set position
            transform.position = new Vector3(position.x, tileHeight, position.y);
            
            // Set visual appearance
            if (spriteRenderer != null)
            {
                if (tileSprite != null)
                {
                    spriteRenderer.sprite = tileSprite;
                }
                
                // Apply material based on tile color
                if (isLight && lightTileMaterial != null)
                {
                    spriteRenderer.material = lightTileMaterial;
                }
                else if (!isLight && darkTileMaterial != null)
                {
                    spriteRenderer.material = darkTileMaterial;
                }
                
                // Set tile color if no materials are assigned
                if (lightTileMaterial == null && darkTileMaterial == null)
                {
                    spriteRenderer.color = isLight ? Color.white : new Color(0.3f, 0.3f, 0.3f);
                }
            }
            
            gameObject.name = $"Tile_{position.x}_{position.y}_{(isLight ? "Light" : "Dark")}";
        }

        private void OnMouseDown()
        {
            // Notify the game manager about tile selection
            GameManager gameManager = FindFirstObjectByType<GameManager>();
            if (gameManager != null)
            {
                gameManager.OnTileClicked(this);
            }
        }
    }
}
