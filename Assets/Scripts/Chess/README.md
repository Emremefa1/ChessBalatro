# Chess Balatro - Chess Engine Architecture

A modular, scalable chess game engine built for Unity with support for full chess rules, AI opponent, and customizable board configurations.

## Features

- **Full Chess Rules**: Castling, en passant, pawn promotion, check/checkmate/stalemate detection
- **Scalable Board**: Support for arbitrary board sizes (5x5, 8x8, 10x10, etc.)
- **Customizable Setup**: Configure piece placement before game starts
- **AI Opponent**: Minimax algorithm with alpha-beta pruning
- **Isometric 2.5D View**: Rendered for 3D chess perspective
- **Modular Architecture**: Easy to extend with roguelike elements

## Project Structure

```
Assets/Scripts/Chess/
├── Core/                 # Core chess engine
│   ├── Position.cs       # Board position representation
│   ├── PieceType.cs      # Piece type enum
│   ├── Color.cs          # White/Black color enum
│   ├── Piece.cs          # Piece class
│   ├── Move.cs           # Move representation
│   ├── Board.cs          # Board state management
│   ├── GameState.cs      # Overall game state
│   └── BoardConfigurator.cs  # Custom board setup
├── Pieces/               # Piece movement logic
│   └── PieceMoveGenerator.cs # Generates legal moves
├── Rules/                # Chess rules enforcement
│   └── ChessRules.cs     # Validates moves, checks, etc.
├── AI/                   # AI opponent
│   └── ChessAI.cs        # Minimax chess AI
├── UI/                   # User interface
│   ├── ChessBoardInput.cs      # Input handling
│   └── IsometricBoardRenderer.cs # Visual rendering
├── ChessGameManager.cs   # Main game controller
└── Examples/
    └── SimpleChessGame.cs # Complete game implementation
```

## Quick Start

### 1. Basic Setup

```csharp
var gameManager = gameObject.AddComponent<ChessGameManager>();
gameManager.InitializeGame(); // Creates standard 8x8 board
```

### 2. Custom Board Size

```csharp
var gameManager = gameObject.AddComponent<ChessGameManager>();
gameManager.InitializeGame(customBoardSize: 10); // 10x10 board
```

### 3. Custom Board Configuration

```csharp
// Create and configure board before game
var config = new BoardConfigurator(8);
config.Clear();
config.AddPiece(new Position(4, 4), PieceType.King, Color.White);
config.AddPiece(new Position(3, 3), PieceType.Queen, Color.Black);

// Use in game manager
var gameManager = gameObject.AddComponent<ChessGameManager>();
gameManager.InitializeGame();
gameManager.GetBoard().Clear();
// Copy your configured pieces to the board
```

### 4. Get AI Move

```csharp
var gameManager = GetComponent<ChessGameManager>();
Move aiMove = gameManager.GetAIMove();
if (aiMove != null)
{
    gameManager.TryExecuteMove(aiMove, gameManager.GetCurrentPlayer());
}
```

### 5. Execute Player Move

```csharp
var move = new Move(fromPosition, toPosition);
bool success = gameManager.TryExecuteMove(move, Color.White);
if (!success)
    Debug.Log("Illegal move!");
```

## Key Classes

### Board
Manages the chess board state and piece positions.

```csharp
var piece = board.GetPiece(new Position(0, 0));
board.SetPiece(new Position(0, 0), new Piece(PieceType.Rook, Color.White));
var allWhitePieces = board.GetPiecesOfColor(Color.White);
```

### ChessRules
Enforces all chess rules and validates moves.

```csharp
var rules = new ChessRules(board);
var legalMoves = rules.GetLegalMoves(Color.White);
bool isLegal = rules.IsLegalMove(move, Color.White);
bool isCheckmate = rules.IsCheckmate(Color.Black);
rules.ExecuteMove(move, Color.White);
```

### PieceMoveGenerator
Generates pseudo-legal moves for each piece type.

```csharp
var moves = PieceMoveGenerator.GetPseudoLegalMoves(board, position);
```

### ChessAI
AI opponent using minimax with alpha-beta pruning.

```csharp
var ai = new ChessAI(board, searchDepth: 4);
var bestMove = ai.FindBestMove(Color.Black);
ai.SetDifficulty(6);
```

### BoardConfigurator
Flexible board setup before game starts.

```csharp
var config = new BoardConfigurator(8);
config.SetupStandardChess();
config.AddPiece(new Position(4, 4), PieceType.King, Color.White);
```

## Game Flow

1. **Initialization**: Create GameManager and configure the board
2. **Player Input**: Click square to select piece, click destination to move
3. **Move Validation**: ChessRules validates move legality
4. **Move Execution**: Piece moves, captures, promotions handled
5. **Game State Check**: Detect check, checkmate, stalemate
6. **AI Move**: If applicable, AI calculates and executes move
7. **Repeat**: Loop until game over

## Extending with Roguelike Elements

The architecture supports easy extension:

### Add Enchantments
```csharp
// Extend Piece class
public class EnchantedPiece : Piece
{
    public List<Enchantment> Enchantments { get; set; }
    // Custom behavior based on enchantments
}
```

### Add Shop System
```csharp
// Create separate Shop system
public class ChessShop
{
    public List<Enchantment> AvailableEnchantments { get; set; }
    public void PurchaseEnchantment(Piece piece, Enchantment enchantment) { }
}
```

### Modify Rules
```csharp
// Extend ChessRules for special roguelike rules
public class RogueChessRules : ChessRules
{
    public override List<Move> GetLegalMoves(Color color)
    {
        var moves = base.GetLegalMoves(color);
        // Add custom logic for enchanted pieces
        return moves;
    }
}
```

## Performance Notes

- Minimax AI at depth 4 is suitable for real-time gameplay
- Increase difficulty (depth) for stronger AI, but impacts response time
- Board cloning for move validation is optimized with shallow cloning
- Alpha-beta pruning significantly reduces search space

## Future Enhancements

- Move animations and visual feedback
- Piece selection UI improvements
- Network multiplayer support
- Save/load game states
- Opening book for AI
- Position evaluation refinement
