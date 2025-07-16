using UnityEngine;
using UnityEditor;

namespace ChessBalatro
{
    [System.Serializable]
    public class SpriteSheetSetup : MonoBehaviour
    {
        [Header("Sprite Sheet")]
        public Texture2D spriteSheet;
        
        [Header("Sprite Dimensions")]
        public int spriteWidth = 64;
        public int spriteHeight = 64;
        public int spritesPerRow = 6;
        public int spritesPerColumn = 2;
        
        [Header("Generated Sprites")]
        public Sprite[] whiteSprites = new Sprite[6];
        public Sprite[] blackSprites = new Sprite[6];
        
        [Header("Auto-Assignment")]
        public ChessBoard targetBoard;

        [ContextMenu("Generate Sprites from Sheet")]
        public void GenerateSpritesFromSheet()
        {
            if (spriteSheet == null)
            {
                Debug.LogError("No sprite sheet assigned!");
                return;
            }

            // Assuming the sprite sheet has white pieces on top row, black pieces on bottom row
            // Order: King, Queen, Bishop, Knight, Rook, Pawn (typical chess sprite sheet layout)
            
            for (int i = 0; i < 6; i++)
            {
                // White pieces (top row)
                Rect whiteRect = new Rect(i * spriteWidth, spriteHeight, spriteWidth, spriteHeight);
                whiteSprites[i] = Sprite.Create(spriteSheet, whiteRect, new Vector2(0.5f, 0.5f), 100f);
                whiteSprites[i].name = $"White_{GetPieceName(i)}";
                
                // Black pieces (bottom row)
                Rect blackRect = new Rect(i * spriteWidth, 0, spriteWidth, spriteHeight);
                blackSprites[i] = Sprite.Create(spriteSheet, blackRect, new Vector2(0.5f, 0.5f), 100f);
                blackSprites[i].name = $"Black_{GetPieceName(i)}";
            }
            
            Debug.Log("Sprites generated from sheet!");
            AssignSpritesToBoard();
        }
        
        private string GetPieceName(int index)
        {
            return index switch
            {
                0 => "King",
                1 => "Queen",
                2 => "Bishop",
                3 => "Knight",
                4 => "Rook",
                5 => "Pawn",
                _ => "Unknown"
            };
        }
        
        [ContextMenu("Assign Sprites to Board")]
        public void AssignSpritesToBoard()
        {
            if (targetBoard == null)
            {
                targetBoard = FindFirstObjectByType<ChessBoard>();
            }
            
            if (targetBoard == null)
            {
                Debug.LogError("No ChessBoard found to assign sprites to!");
                return;
            }
            
            // Assign sprites based on typical chess piece order
            if (whiteSprites.Length >= 6 && blackSprites.Length >= 6)
            {
                targetBoard.whiteKing = whiteSprites[0];
                targetBoard.whiteQueen = whiteSprites[1];
                targetBoard.whiteBishop = whiteSprites[2];
                targetBoard.whiteKnight = whiteSprites[3];
                targetBoard.whiteRook = whiteSprites[4];
                targetBoard.whitePawn = whiteSprites[5];
                
                targetBoard.blackKing = blackSprites[0];
                targetBoard.blackQueen = blackSprites[1];
                targetBoard.blackBishop = blackSprites[2];
                targetBoard.blackKnight = blackSprites[3];
                targetBoard.blackRook = blackSprites[4];
                targetBoard.blackPawn = blackSprites[5];
                
                Debug.Log("Sprites assigned to ChessBoard!");
            }
        }
    }
}
