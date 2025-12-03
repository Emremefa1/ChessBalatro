## Visual Setup Guide for Unity

### Scene Hierarchy
```
Scene
├── ChessGameManager (GameObject)
│   └── SimpleChessGame (Script Component)
│       └── IsometricBoardRenderer (Auto-added)
│
└── Main Camera
    └── (Position: 0, 8, -8 | Rotation: 45, 45, 0)
```

### Inspector for SimpleChessGame
```
┌─────────────────────────────────────┐
│ Simple Chess Game (Script)          │
├─────────────────────────────────────┤
│ Game Settings                       │
│  ├─ Board Size: 8                   │
│  ├─ AI Difficulty: 4                │
│  ├─ Square Size: 1                  │
│  └─ Play As White: ☑               │
├─────────────────────────────────────┤
│ Visual Materials (optional)         │
│  ├─ White Material: None            │
│  ├─ Black Material: None            │
│  ├─ Highlight Material: None        │
│  └─ Selected Material: None         │
└─────────────────────────────────────┘
```

### Camera Setup
```
┌─────────────────────────────────────┐
│ Main Camera Transform               │
├─────────────────────────────────────┤
│ Position:                           │
│   X: 0                              │
│   Y: 8                              │
│   Z: -8                             │
│                                     │
│ Rotation:                           │
│   X: 45                             │
│   Y: 45                             │
│   Z: 0                              │
│                                     │
│ Field of View: 60                   │
└─────────────────────────────────────┘
```

### Board Layout (Isometric View from Camera)
```
         Black Side (Rank 7-0)
                  ↓
    ♜ ♞ ♝ ♛ ♚ ♝ ♞ ♜  (Back)
    ♟ ♟ ♟ ♟ ♟ ♟ ♟ ♟
    
    . . . . . . . .
    . . . . . . . .
    . . . . . . . .
    . . . . . . . .
    
    ♙ ♙ ♙ ♙ ♙ ♙ ♙ ♙
    ♖ ♘ ♗ ♕ ♔ ♗ ♘ ♖  (Front)
         White Side →
         (File 0-7)
```

### Game Flow Diagram
```
┌──────────────────────────────────────────┐
│ SimpleChessGame Starts                   │
├──────────────────────────────────────────┤
│ 1. Initialize GameState                  │
│ 2. Create ChessRules                     │
│ 3. Create ChessAI                        │
│ 4. Create IsometricBoardRenderer         │
│ 5. Render board with pieces              │
└──────────────────────────────────────────┘
           │
           ↓
┌──────────────────────────────────────────┐
│ Waiting for Player Input                 │
├──────────────────────────────────────────┤
│ • Mouse Click → Raycast to board         │
│ • Parse square coordinates               │
│ • ProcessMove(clickedPosition)           │
└──────────────────────────────────────────┘
           │
           ↓
        ┌──────────────────┐
        │ Selected Piece?  │
        └──────────────────┘
         /                 \
       No                   Yes
        │                   │
        ↓                   ↓
    ┌────────────┐    ┌──────────────┐
    │ Highlight  │    │ Check if     │
    │ piece      │    │ legal move   │
    │ & moves    │    │ to destination
    └────────────┘    └──────────────┘
                       /            \
                      ✓              ✗
                      │              │
                      ↓              ↓
                  ┌────────┐    ┌─────────┐
                  │ Execute│    │Illegal  │
                  │ Move   │    │Move     │
                  └────────┘    │Feedback │
                      │         └─────────┘
                      ↓
            ┌──────────────────────┐
            │ Update Board Visual  │
            │ Update Game State    │
            │ Switch Current Player│
            └──────────────────────┘
                      │
                      ↓
            ┌──────────────────────┐
            │ Check Game Status    │
            │ • Checkmate?         │
            │ • Stalemate?         │
            │ • Check?             │
            └──────────────────────┘
                      │
                      ↓
        ┌─────────────────────────┐
        │ Current Player = Black? │
        └─────────────────────────┘
         /                        \
        Yes                        No
        │                          │
        ↓                          ↓
    ┌──────────┐         ┌──────────────┐
    │ AI Think │         │ Wait for     │
    │ & Move   │         │ next click   │
    └──────────┘         └──────────────┘
        │
        └─────────────┬──────────────┘
                      ↓
            ┌──────────────────┐
            │ Back to Input    │
            │ Loop             │
            └──────────────────┘
```

### Keyboard Controls
```
┌──────────────────────────────┐
│ Keyboard Controls            │
├──────────────────────────────┤
│ Click Mouse  → Select/Move   │
│ Space        → Print Board   │
│ R            → Restart Game  │
└──────────────────────────────┘
```

### Console Output Example
```
╔════════════════════════════════════╗
║  Chess Game Initialized!           ║
║  Board: 8x8                        ║
║  AI Difficulty: 4/8                ║
║  Click pieces to move!             ║
╚════════════════════════════════════╝

r n b q k b n r
p p p p p p p p
. . . . . . . .
. . . . . . . .
. . . . . . . .
. . . . . . . .
P P P P P P P P
R N B Q K B N R

Selected White Pawn at (0, 1)
✓ White moved: (0, 1) -> (0, 3)

⚠️  Black is in CHECK!

✓ Black moved: (4, 6) -> (4, 4)

╔═══════════════════════════════╗
║  CHECKMATE! White Wins!       ║
╚═══════════════════════════════╝
```
