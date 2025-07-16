# Chess Balatro - Setup Guide

## Overview
This is a 2.5D roguelike chess game setup for Unity. The system includes:
- **ChessBoard**: Main board generator and piece manager
- **ChessPiece**: Individual piece components with full chess movement logic
- **BoardTile**: Individual tile components with interaction
- **GameManager**: Overall game controller and input handler
- **ChessAI**: Intelligent AI opponent with minimax algorithm
- **SpriteSheetSetup**: Helper to extract sprites from your sprite sheet
- **PrefabGenerator**: Helper to create basic prefabs

## Quick Setup Instructions

### 1. Setup the Scene
1. Create an empty GameObject and name it "ChessBoard"
2. Attach the `ChessBoard` script to it
3. Create another empty GameObject and name it "GameManager" 
4. Attach the `GameManager` script to it
5. Create another empty GameObject and name it "ChessAI"
6. Attach the `ChessAI` script to it

### 2. Setup Your Sprites
1. Import your chess piece sprite sheet into the Assets/Pieces folder
2. In the Inspector, set the sprite sheet to "Multiple" mode and slice it
3. OR use the `SpriteSheetSetup` script:
   - Create an empty GameObject and attach `SpriteSheetSetup`
   - Assign your sprite sheet texture
   - Configure sprite dimensions (width, height, sprites per row/column)
   - Right-click the script and select "Generate Sprites from Sheet"

### 3. Create Prefabs (Optional)
1. Use the `PrefabGenerator` script to create basic prefabs:
   - Create an empty GameObject and attach `PrefabGenerator`
   - Right-click and select "Create Basic Prefabs"
   - Save the created objects as prefabs in Assets/Prefabs/

### 4. Configure the ChessBoard
1. Select the ChessBoard GameObject
2. In the ChessBoard script component:
   - Assign tile and piece prefabs (if using)
   - Assign all piece sprites (white and black for each piece type)
   - Assign tile sprites for light and dark tiles
   - Configure camera settings

### 5. Configure the GameManager
1. Select the GameManager GameObject
2. Assign the ChessBoard and ChessAI references
3. Set whether you want to play vs AI (vsAI checkbox)
4. Choose your color (humanPlayer dropdown)
5. Optionally create a selection highlight GameObject

### 6. Configure the AI
1. Select the ChessAI GameObject
2. Adjust AI settings:
   - **AI Color**: Which color the AI plays (auto-set by GameManager)
   - **Thinking Time**: How long AI appears to "think" (visual delay)
   - **Search Depth**: How many moves ahead AI calculates (3-5 recommended)
   - **Aggressiveness**: How much AI prioritizes attacks (0-1)
   - **Defensiveness**: How much AI prioritizes defense (0-1)

### 6. Setup Camera
The system will automatically configure the camera for 2.5D view, but you can:
- Adjust the camera offset and rotation in the ChessBoard component
- Or manually position your camera to look down at the board at an angle
- **Note**: Camera coordinates are preserved and won't be modified during gameplay

## Controls
- **Left Click**: Select pieces and tiles (only on your turn)
- **R Key**: Restart game
- **Escape**: Deselect current selection
- **Tab Key**: Toggle AI mode on/off

## Key Features

### Board Generation
- Automatically generates an 8x8 chess board
- Places pieces in standard starting positions
- Supports custom board sizes
- 2.5D positioning with proper depth

### Piece Management
- Full piece type support (King, Queen, Rook, Bishop, Knight, Pawn)
- Color-based team management
- Basic movement validation
- Visual feedback for selection

### 2.5D Rendering
- Tiles rendered at ground level (Y = 0)
- Pieces rendered slightly above tiles (Y = 0.1)
- Camera positioned at an angle for 2.5D effect
- Proper sorting order for sprites

### Roguelike Features (Ready for Expansion)
- `GenerateRandomBoard()` method for random board layouts
- `GenerateNewLevel()` method for level progression
- Extensible piece and tile system for special abilities

### Chess AI
- **Minimax Algorithm**: AI uses minimax with alpha-beta pruning
- **Piece Evaluation**: Standard chess piece values with positional bonuses
- **Safety Analysis**: Considers king safety and piece attacks
- **Configurable Difficulty**: Adjust search depth and personality traits
- **Smart Move Selection**: Prioritizes captures, threats, and strategic positions

### Game Flow
- Turn-based gameplay with automatic player switching
- AI moves automatically when it's the AI's turn
- Input is disabled during AI thinking time
- Visual feedback for piece selection and valid moves

## Customization

### Adding Special Piece Types
1. Add new values to the `PieceType` enum
2. Add sprites for the new pieces
3. Implement movement logic in `ChessPiece.IsValidMove()`
4. Add sprite assignment in `ChessBoard.GetPieceSprite()`

### Adding Special Tiles
1. Extend `BoardTile` with new properties
2. Add new tile types or effects
3. Modify board generation to include special tiles

### Roguelike Elements
- Modify `ChessBoard.GenerateRandomBoard()` for procedural generation
- Add special abilities, power-ups, or obstacles
- Implement different board layouts and sizes
- Add progression systems

## Troubleshooting

### No Pieces Visible
- Check that sprites are assigned in the ChessBoard component
- Ensure the camera is positioned correctly
- Verify that piece prefabs have SpriteRenderer components

### Can't Select Pieces
- Make sure pieces have BoxCollider components
- Check that the GameManager is properly configured
- Ensure the camera is tagged as "MainCamera"

### Sprites Not Loading
- Verify sprite import settings (set to "Multiple" and slice)
- Check that sprite assignments match your actual sprites
- Use the SpriteSheetSetup script for automated assignment

## Next Steps for Roguelike Features
1. Implement special piece abilities
2. Add different board layouts and obstacles
3. Create a progression system with levels
4. Add special tiles with effects
5. Implement deck-building mechanics (like Balatro)
6. Add visual effects and animations
