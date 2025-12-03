# Chess Engine Documentation

## Core Files

- **[README.md](README.md)** - Architecture, features, and usage examples
- **[VISUAL_SETUP.md](VISUAL_SETUP.md)** - Diagrams and visual reference

## Code Structure

### Core Engine
```
Core/
  ├── Position.cs          - Board coordinates
  ├── Piece.cs             - Piece definition
  ├── PieceType.cs         - Piece type enum
  ├── Color.cs             - Color enum
  ├── Move.cs              - Move representation
  ├── Board.cs             - Board state
  ├── GameState.cs         - Game tracking
  └── BoardConfigurator.cs - Custom board setup

Pieces/
  └── PieceMoveGenerator.cs - Movement rules for all pieces

Rules/
  └── ChessRules.cs        - Rule enforcement, check/checkmate detection

AI/
  └── ChessAI.cs           - Minimax AI with alpha-beta pruning

UI/
  ├── ChessBoardInput.cs       - Input handling
  └── IsometricBoardRenderer.cs - 2.5D rendering
```

### Game Management
- `ChessGameManager.cs` - Core game controller (attach to GameObject)
- `Examples/SimpleChessGame.cs` - Complete working implementation

## Quick Start

### 1. Add to GameObject
```csharp
var gameManager = gameObject.AddComponent<ChessGameManager>();
gameManager.InitializeGame(); // 8x8 board with standard pieces
```

### 2. Custom Board Size
```csharp
gameManager.InitializeGame(customBoardSize: 10); // 10x10 board
```

### 3. Get Legal Moves
```csharp
var moves = gameManager.GetLegalMovesForPiece(position);
```

### 4. Execute Move
```csharp
bool success = gameManager.TryExecuteMove(move, Color.White);
```

### 5. Get AI Move
```csharp
Move aiMove = gameManager.GetAIMove();
gameManager.TryExecuteMove(aiMove, gameManager.GetCurrentPlayer());
```

## Key Concepts

### Board Coordinates
- File: 0-7 (columns, left to right)
- Rank: 0-7 (rows, bottom to top)
- Position(file, rank)

### Move Validation
All moves are validated against:
- Piece movement rules
- Not leaving king in check
- Special moves (castling, en passant, promotion)

### AI Difficulty
- 1-2: Weak, fast moves
- 3-4: Balanced
- 5-8: Strong, slower responses

## Extending the System

The architecture is modular for easy extension:

### Add Custom Rules
```csharp
public class RogueChessRules : ChessRules
{
    public override List<Move> GetLegalMoves(Color color)
    {
        var moves = base.GetLegalMoves(color);
        // Add custom logic
        return moves;
    }
}
```

### Add Enchantments
```csharp
public class EnchantedPiece : Piece
{
    public List<Enchantment> Enchantments { get; set; }
    // Custom behavior
}
```

### Add Game Features
- Shop system for upgrades
- Run-based gameplay
- Character progression
- Custom board generation

## Architecture Overview

```
Input Handler
    ↓
Game Manager (ChessGameManager)
    ├→ Board State (GameState + Board)
    ├→ Rules Engine (ChessRules)
    ├→ AI Engine (ChessAI)
    └→ Renderer (IsometricBoardRenderer)
```

All systems are independent and can be modified without affecting others.

---

For usage examples, see README.md
For visual diagrams, see VISUAL_SETUP.md
