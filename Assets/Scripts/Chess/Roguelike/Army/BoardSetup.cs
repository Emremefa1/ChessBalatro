using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Chess.Roguelike.Army
{
    using Chess.Core;
    using Chess.Roguelike.Core;
    using Chess.Roguelike.Progression;

    /// <summary>
    /// Manages the board setup phase where player arranges their army before a trial.
    /// Also handles enemy army placement.
    /// </summary>
    public class BoardSetup
    {
        public event Action OnPiecePlaced;
        public event Action OnPieceRemoved;
        public event Action OnSetupComplete;

        private RunState runState;
        private Board board;
        private int boardSize;
        private int playerSetupRows;  // How many rows from player's side they can use

        public Board Board => board;
        public int BoardSize => boardSize;
        public int PlayerSetupRows => playerSetupRows;

        public BoardSetup(RunState state)
        {
            runState = state;
        }

        /// <summary>
        /// Initialize the board for setup phase
        /// </summary>
        public void InitializeSetup()
        {
            boardSize = runState.BoardSize;
            playerSetupRows = runState.PlayerSetupRows;
            board = new Board(boardSize);

            // Clear all piece placements from previous trial
            foreach (var piece in runState.OwnedPieces)
            {
                piece.ClearPosition();
            }

            Debug.Log($"[BoardSetup] Initialized {boardSize}x{boardSize} board with {playerSetupRows} player rows");
        }

        /// <summary>
        /// Check if a position is valid for player piece placement
        /// </summary>
        public bool IsValidPlayerPosition(Position pos)
        {
            if (!pos.IsValid(boardSize))
                return false;

            // Player places on lower ranks (0 to playerSetupRows-1)
            return pos.Rank < playerSetupRows;
        }

        /// <summary>
        /// Check if a position is valid for enemy piece placement
        /// </summary>
        public bool IsValidEnemyPosition(Position pos)
        {
            if (!pos.IsValid(boardSize))
                return false;

            // Enemy places on upper ranks
            return pos.Rank >= boardSize - playerSetupRows;
        }

        /// <summary>
        /// Try to place a piece at a position
        /// </summary>
        public bool TryPlacePiece(PieceInstance piece, Position pos)
        {
            if (piece == null)
                return false;

            // King cannot be moved by player in setup
            // (King will be auto-placed)
            // Actually, let's allow moving king within valid area
            
            if (!IsValidPlayerPosition(pos))
            {
                Debug.LogWarning($"[BoardSetup] Invalid position {pos} for player piece");
                return false;
            }

            // Check if position is already occupied
            if (board.HasPiece(pos))
            {
                Debug.LogWarning($"[BoardSetup] Position {pos} is already occupied");
                return false;
            }

            // Remove from previous position if placed
            if (piece.PlacedPosition.HasValue)
            {
                board.RemovePiece(piece.PlacedPosition.Value);
            }

            // Place the piece
            piece.SetPosition(pos);
            board.SetPiece(pos, piece.ToPiece(Color.White));

            Debug.Log($"[BoardSetup] Placed {piece.Type} at {pos}");
            OnPiecePlaced?.Invoke();
            return true;
        }

        /// <summary>
        /// Remove a piece from the board (return to inventory)
        /// </summary>
        public bool RemovePieceFromBoard(PieceInstance piece)
        {
            if (piece == null || !piece.PlacedPosition.HasValue)
                return false;

            // King must stay on board (but can be repositioned)
            if (piece.IsKing)
            {
                Debug.LogWarning("[BoardSetup] Cannot remove King from board");
                return false;
            }

            board.RemovePiece(piece.PlacedPosition.Value);
            piece.ClearPosition();

            Debug.Log($"[BoardSetup] Removed {piece.Type} from board");
            OnPieceRemoved?.Invoke();
            return true;
        }

        /// <summary>
        /// Auto-place all unplaced pieces in valid positions
        /// </summary>
        public void AutoPlaceRemainingPieces()
        {
            var unplaced = runState.GetUnplacedPieces();
            
            // Sort: King first, then by value (high to low)
            unplaced.Sort((a, b) =>
            {
                if (a.IsKing) return -1;
                if (b.IsKing) return 1;
                return b.GetTotalValue().CompareTo(a.GetTotalValue());
            });

            foreach (var piece in unplaced)
            {
                var pos = FindEmptyPlayerPosition();
                if (pos.HasValue)
                {
                    TryPlacePiece(piece, pos.Value);
                }
                else
                {
                    Debug.LogWarning($"[BoardSetup] No space for {piece.Type}!");
                }
            }
        }

        /// <summary>
        /// Place default starting formation
        /// </summary>
        public void PlaceDefaultFormation()
        {
            InitializeSetup();

            var pieces = new List<PieceInstance>(runState.OwnedPieces);
            
            // Sort pieces for standard chess formation
            var king = pieces.Find(p => p.IsKing);
            var queens = pieces.FindAll(p => p.Type == PieceType.Queen);
            var rooks = pieces.FindAll(p => p.Type == PieceType.Rook);
            var bishops = pieces.FindAll(p => p.Type == PieceType.Bishop);
            var knights = pieces.FindAll(p => p.Type == PieceType.Knight);
            var pawns = pieces.FindAll(p => p.Type == PieceType.Pawn);

            // Back row (rank 0)
            int centerFile = boardSize / 2;
            
            // Place king in center-right (standard king position is file 4)
            if (king != null)
            {
                TryPlacePiece(king, new Position(Mathf.Min(centerFile, boardSize - 1), 0));
            }

            // Place other back-row pieces around king
            int[] backRowOrder = { -1, 1, -2, 2, -3, 3, -4, 4 };
            var backRowPieces = new List<PieceInstance>();
            backRowPieces.AddRange(queens);
            backRowPieces.AddRange(bishops);
            backRowPieces.AddRange(knights);
            backRowPieces.AddRange(rooks);

            int orderIndex = 0;
            foreach (var piece in backRowPieces)
            {
                if (orderIndex >= backRowOrder.Length) break;
                
                int file = centerFile + backRowOrder[orderIndex];
                if (file >= 0 && file < boardSize)
                {
                    var pos = new Position(file, 0);
                    if (!board.HasPiece(pos))
                    {
                        TryPlacePiece(piece, pos);
                    }
                }
                orderIndex++;
            }

            // Place pawns on rank 1
            int pawnIndex = 0;
            for (int f = 0; f < boardSize && pawnIndex < pawns.Count; f++)
            {
                var pos = new Position(f, 1);
                if (!board.HasPiece(pos))
                {
                    TryPlacePiece(pawns[pawnIndex], pos);
                    pawnIndex++;
                }
            }

            // Place any remaining pieces
            AutoPlaceRemainingPieces();
        }

        /// <summary>
        /// Generate and place enemy army
        /// </summary>
        public List<PieceInstance> PlaceEnemyArmy(int cycle, int trial)
        {
            var enemyArmy = DifficultyScaler.GenerateEnemyArmy(cycle, trial);

            // Sort enemy pieces
            var king = enemyArmy.Find(p => p.Type == PieceType.King);
            var queens = enemyArmy.FindAll(p => p.Type == PieceType.Queen);
            var rooks = enemyArmy.FindAll(p => p.Type == PieceType.Rook);
            var bishops = enemyArmy.FindAll(p => p.Type == PieceType.Bishop);
            var knights = enemyArmy.FindAll(p => p.Type == PieceType.Knight);
            var pawns = enemyArmy.FindAll(p => p.Type == PieceType.Pawn);

            int enemyBackRank = boardSize - 1;
            int enemyPawnRank = boardSize - 2;
            int centerFile = boardSize / 2;

            // Place king
            if (king != null)
            {
                PlaceEnemyPiece(king, new Position(centerFile, enemyBackRank));
            }

            // Place back row pieces
            int[] backRowOrder = { -1, 1, -2, 2, -3, 3, -4, 4 };
            var backRowPieces = new List<PieceInstance>();
            backRowPieces.AddRange(queens);
            backRowPieces.AddRange(bishops);
            backRowPieces.AddRange(knights);
            backRowPieces.AddRange(rooks);

            int orderIndex = 0;
            foreach (var piece in backRowPieces)
            {
                if (orderIndex >= backRowOrder.Length) break;

                int file = centerFile + backRowOrder[orderIndex];
                if (file >= 0 && file < boardSize)
                {
                    var pos = new Position(file, enemyBackRank);
                    if (!board.HasPiece(pos))
                    {
                        PlaceEnemyPiece(piece, pos);
                    }
                }
                orderIndex++;
            }

            // Place pawns
            int pawnIndex = 0;
            for (int f = 0; f < boardSize && pawnIndex < pawns.Count; f++)
            {
                var pos = new Position(f, enemyPawnRank);
                if (!board.HasPiece(pos))
                {
                    PlaceEnemyPiece(pawns[pawnIndex], pos);
                    pawnIndex++;
                }
            }

            // Place remaining enemies randomly
            foreach (var piece in enemyArmy)
            {
                if (!piece.PlacedPosition.HasValue)
                {
                    var pos = FindEmptyEnemyPosition();
                    if (pos.HasValue)
                    {
                        PlaceEnemyPiece(piece, pos.Value);
                    }
                }
            }

            Debug.Log($"[BoardSetup] Placed {enemyArmy.Count} enemy pieces");
            return enemyArmy;
        }

        private void PlaceEnemyPiece(PieceInstance piece, Position pos)
        {
            piece.SetPosition(pos);
            board.SetPiece(pos, piece.ToPiece(Color.Black));
        }

        private Position? FindEmptyPlayerPosition()
        {
            // Search for empty position in player's area
            for (int r = 0; r < playerSetupRows; r++)
            {
                for (int f = 0; f < boardSize; f++)
                {
                    var pos = new Position(f, r);
                    if (!board.HasPiece(pos))
                        return pos;
                }
            }
            return null;
        }

        private Position? FindEmptyEnemyPosition()
        {
            // Search for empty position in enemy's area
            for (int r = boardSize - 1; r >= boardSize - playerSetupRows; r--)
            {
                for (int f = 0; f < boardSize; f++)
                {
                    var pos = new Position(f, r);
                    if (!board.HasPiece(pos))
                        return pos;
                }
            }
            return null;
        }

        /// <summary>
        /// Validate that setup is complete (king placed, etc.)
        /// </summary>
        public bool ValidateSetup()
        {
            var king = runState.OwnedPieces.FirstOrDefault(p => p.IsKing);
            if (king == null || !king.PlacedPosition.HasValue)
            {
                Debug.LogError("[BoardSetup] King must be placed!");
                return false;
            }

            // Count placed pieces
            int placedCount = runState.GetPlacedPieces().Count;
            Debug.Log($"[BoardSetup] Setup validated: {placedCount} pieces placed");
            
            return true;
        }

        /// <summary>
        /// Get the final board ready for trial
        /// </summary>
        public Board GetFinalBoard()
        {
            return board;
        }
    }
}
