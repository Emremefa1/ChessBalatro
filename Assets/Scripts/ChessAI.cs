using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace ChessBalatro
{
    public class ChessAI : MonoBehaviour
    {
        [Header("AI Settings")]
        public PieceColor aiColor = PieceColor.Black;
        public float thinkingTime = 1f;
        public int searchDepth = 3;
        
        [Header("AI Personality")]
        [Range(0f, 1f)]
        public float aggressiveness = 0.5f; // How much AI prioritizes attacks
        [Range(0f, 1f)]
        public float defensiveness = 0.5f; // How much AI prioritizes defense
        
        private ChessBoard chessBoard;
        private GameManager gameManager;
        private bool isThinking = false;

        // Piece values for evaluation
        private readonly Dictionary<PieceType, int> pieceValues = new Dictionary<PieceType, int>
        {
            { PieceType.Pawn, 100 },
            { PieceType.Knight, 320 },
            { PieceType.Bishop, 330 },
            { PieceType.Rook, 500 },
            { PieceType.Queen, 900 },
            { PieceType.King, 20000 }
        };        private void Start()
        {
            chessBoard = FindFirstObjectByType<ChessBoard>();
            gameManager = FindFirstObjectByType<GameManager>();
        }

        public void MakeMove()
        {
            if (isThinking) return;
            
            StartCoroutine(ThinkAndMove());
        }

        private System.Collections.IEnumerator ThinkAndMove()
        {
            isThinking = true;
            Debug.Log($"AI ({aiColor}) is thinking...");
            
            yield return new WaitForSeconds(thinkingTime);
            
            Move bestMove = FindBestMove();
            
            if (bestMove != null)
            {
                Debug.Log($"AI moves {bestMove.from} to {bestMove.to}");
                
                // Execute the move
                if (chessBoard.MovePiece(bestMove.from, bestMove.to))
                {
                    // Notify game manager that AI made a move
                    if (gameManager != null)
                    {
                        gameManager.OnAIMoveComplete();
                    }
                }
            }
            else
            {
                Debug.Log("AI couldn't find a valid move!");
            }
            
            isThinking = false;
        }        private Move FindBestMove()
        {
            if (chessBoard == null) return null;
            
            // Initialize virtual board for simulation
            InitializeVirtualBoard();
            usingVirtualBoard = true;
            
            List<Move> allMoves = GetAllValidMoves(aiColor);
            
            if (allMoves.Count == 0)
            {
                usingVirtualBoard = false;
                Debug.LogWarning($"AI ({aiColor}) has no valid moves available!");
                return null;
            }

            Move bestMove = null;
            int bestScore = int.MinValue;

            foreach (Move move in allMoves)
            {
                // Make the move temporarily
                ChessPiece capturedPiece = SimulateMove(move);
                
                // Evaluate the position
                int score = Minimax(searchDepth - 1, int.MinValue, int.MaxValue, false);
                
                // Undo the move
                UndoMove(move, capturedPiece);
                
                if (score > bestScore)
                {
                    bestScore = score;
                    bestMove = move;
                }
            }

            // Reset virtual board after thinking
            usingVirtualBoard = false;
            virtualBoard = null;

            return bestMove;
        }

        private int Minimax(int depth, int alpha, int beta, bool maximizingPlayer)
        {
            if (depth == 0)
            {
                return EvaluatePosition();
            }

            PieceColor currentColor = maximizingPlayer ? aiColor : GetOpponentColor(aiColor);
            List<Move> moves = GetAllValidMoves(currentColor);

            if (moves.Count == 0)
            {
                // No moves available - checkmate or stalemate
                return maximizingPlayer ? -10000 : 10000;
            }

            if (maximizingPlayer)
            {
                int maxEval = int.MinValue;
                foreach (Move move in moves)
                {
                    ChessPiece capturedPiece = SimulateMove(move);
                    int eval = Minimax(depth - 1, alpha, beta, false);
                    UndoMove(move, capturedPiece);
                    
                    maxEval = Mathf.Max(maxEval, eval);
                    alpha = Mathf.Max(alpha, eval);
                    
                    if (beta <= alpha)
                        break; // Alpha-beta pruning
                }
                return maxEval;
            }
            else
            {
                int minEval = int.MaxValue;
                foreach (Move move in moves)
                {
                    ChessPiece capturedPiece = SimulateMove(move);
                    int eval = Minimax(depth - 1, alpha, beta, true);
                    UndoMove(move, capturedPiece);
                    
                    minEval = Mathf.Min(minEval, eval);
                    beta = Mathf.Min(beta, eval);
                    
                    if (beta <= alpha)
                        break; // Alpha-beta pruning
                }
                return minEval;
            }
        }

        private int EvaluatePosition()
        {
            int score = 0;
            
            // Material evaluation
            score += EvaluateMaterial();
            
            // Position evaluation
            score += EvaluatePositions();
            
            // Safety evaluation
            score += EvaluateSafety();
            
            return score;
        }        private int EvaluateMaterial()
        {
            int score = 0;
            
            for (int x = 0; x < chessBoard.boardSize; x++)
            {
                for (int y = 0; y < chessBoard.boardSize; y++)
                {
                    ChessPiece piece = GetVirtualPieceAt(new Vector2Int(x, y));
                    if (piece != null)
                    {
                        int pieceValue = pieceValues[piece.pieceData.type];
                        if (piece.pieceData.color == aiColor)
                        {
                            score += pieceValue;
                        }
                        else
                        {
                            score -= pieceValue;
                        }
                    }
                }
            }
            
            return score;
        }        private int EvaluatePositions()
        {
            int score = 0;
            
            // Prefer central positions
            for (int x = 0; x < chessBoard.boardSize; x++)
            {
                for (int y = 0; y < chessBoard.boardSize; y++)
                {
                    ChessPiece piece = GetVirtualPieceAt(new Vector2Int(x, y));
                    if (piece != null && piece.pieceData.color == aiColor)
                    {
                        // Central squares are more valuable
                        int centerBonus = 0;
                        int distanceFromCenter = Mathf.Abs(x - 3) + Mathf.Abs(y - 3);
                        centerBonus = (6 - distanceFromCenter) * 10;
                        
                        if (piece.pieceData.type == PieceType.Knight || 
                            piece.pieceData.type == PieceType.Bishop)
                        {
                            score += centerBonus;
                        }
                    }
                }
            }
            
            return score;
        }

        private int EvaluateSafety()
        {
            int score = 0;
            
            // Check if king is safe
            Vector2Int aiKingPos = FindKing(aiColor);
            Vector2Int opponentKingPos = FindKing(GetOpponentColor(aiColor));
            
            if (aiKingPos != Vector2Int.one * -1)
            {
                // Penalty for exposed king
                List<Move> attacksOnKing = GetAttacksOnSquare(aiKingPos, GetOpponentColor(aiColor));
                score -= attacksOnKing.Count * 50;
            }
            
            if (opponentKingPos != Vector2Int.one * -1)
            {
                // Bonus for attacking opponent's king
                List<Move> attacksOnOpponentKing = GetAttacksOnSquare(opponentKingPos, aiColor);
                score += attacksOnOpponentKing.Count * 30;
            }
            
            return score;
        }        private List<Move> GetAllValidMoves(PieceColor color)
        {
            List<Move> moves = new List<Move>();
            
            for (int x = 0; x < chessBoard.boardSize; x++)
            {
                for (int y = 0; y < chessBoard.boardSize; y++)
                {
                    ChessPiece piece = GetVirtualPieceAt(new Vector2Int(x, y));
                    if (piece != null && piece.pieceData.color == color)
                    {
                        Vector2Int piecePos = new Vector2Int(x, y);
                        // Use ChessBoard's legal moves which includes check validation
                        List<Vector2Int> legalMoves = chessBoard.GetLegalMovesForPiece(piecePos);
                        foreach (Vector2Int target in legalMoves)
                        {
                            moves.Add(new Move(piecePos, target));
                        }
                    }
                }
            }
            
            return moves;
        }

        private List<Vector2Int> GetPossibleMoves(ChessPiece piece)
        {
            List<Vector2Int> moves = new List<Vector2Int>();
            Vector2Int pos = piece.boardPosition;
            
            switch (piece.pieceData.type)
            {
                case PieceType.Pawn:
                    moves.AddRange(GetPawnMoves(piece));
                    break;
                case PieceType.Rook:
                    moves.AddRange(GetRookMoves(pos));
                    break;
                case PieceType.Knight:
                    moves.AddRange(GetKnightMoves(pos));
                    break;
                case PieceType.Bishop:
                    moves.AddRange(GetBishopMoves(pos));
                    break;
                case PieceType.Queen:
                    moves.AddRange(GetQueenMoves(pos));
                    break;
                case PieceType.King:
                    moves.AddRange(GetKingMoves(pos));
                    break;
            }
            
            // Filter out invalid moves
            return moves.Where(move => IsValidMove(pos, move)).ToList();
        }        private List<Vector2Int> GetPawnMoves(ChessPiece pawn)
        {
            List<Vector2Int> moves = new List<Vector2Int>();
            Vector2Int pos = pawn.boardPosition;
            int direction = pawn.pieceData.color == PieceColor.White ? 1 : -1;
            
            // Forward move
            Vector2Int forward = new Vector2Int(pos.x, pos.y + direction);
            if (chessBoard.IsValidPosition(forward.x, forward.y) && 
                GetVirtualPieceAt(forward) == null)
            {
                moves.Add(forward);
                
                // Double move from starting position
                if ((pawn.pieceData.color == PieceColor.White && pos.y == 1) ||
                    (pawn.pieceData.color == PieceColor.Black && pos.y == 6))
                {
                    Vector2Int doubleForward = new Vector2Int(pos.x, pos.y + 2 * direction);
                    if (chessBoard.IsValidPosition(doubleForward.x, doubleForward.y) &&
                        GetVirtualPieceAt(doubleForward) == null)
                    {
                        moves.Add(doubleForward);
                    }
                }
            }
            
            // Diagonal captures
            Vector2Int captureLeft = new Vector2Int(pos.x - 1, pos.y + direction);
            Vector2Int captureRight = new Vector2Int(pos.x + 1, pos.y + direction);
            
            if (chessBoard.IsValidPosition(captureLeft.x, captureLeft.y))
            {
                ChessPiece target = GetVirtualPieceAt(captureLeft);
                if (target != null && target.pieceData.color != pawn.pieceData.color)
                {
                    moves.Add(captureLeft);
                }
            }
            
            if (chessBoard.IsValidPosition(captureRight.x, captureRight.y))
            {
                ChessPiece target = GetVirtualPieceAt(captureRight);
                if (target != null && target.pieceData.color != pawn.pieceData.color)
                {
                    moves.Add(captureRight);
                }
            }
            
            return moves;
        }        private List<Vector2Int> GetRookMoves(Vector2Int pos)
        {
            List<Vector2Int> moves = new List<Vector2Int>();
            
            // Horizontal and vertical directions
            Vector2Int[] directions = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
            
            foreach (Vector2Int dir in directions)
            {
                for (int i = 1; i < chessBoard.boardSize; i++)
                {
                    Vector2Int target = pos + dir * i;
                    if (!chessBoard.IsValidPosition(target.x, target.y))
                        break;
                    
                    ChessPiece piece = GetVirtualPieceAt(target);
                    if (piece == null)
                    {
                        moves.Add(target);
                    }
                    else
                    {
                        // Can capture opponent piece
                        ChessPiece currentPiece = GetVirtualPieceAt(pos);
                        if (currentPiece != null && piece.pieceData.color != currentPiece.pieceData.color)
                        {
                            moves.Add(target);
                        }
                        break;
                    }
                }
            }
            
            return moves;
        }        private List<Vector2Int> GetKnightMoves(Vector2Int pos)
        {
            List<Vector2Int> moves = new List<Vector2Int>();
            
            Vector2Int[] knightMoves = {
                new Vector2Int(2, 1), new Vector2Int(2, -1),
                new Vector2Int(-2, 1), new Vector2Int(-2, -1),
                new Vector2Int(1, 2), new Vector2Int(1, -2),
                new Vector2Int(-1, 2), new Vector2Int(-1, -2)
            };
            
            foreach (Vector2Int move in knightMoves)
            {
                Vector2Int target = pos + move;
                if (chessBoard.IsValidPosition(target.x, target.y))
                {
                    ChessPiece piece = GetVirtualPieceAt(target);
                    ChessPiece currentPiece = GetVirtualPieceAt(pos);
                    
                    if (currentPiece != null && (piece == null || piece.pieceData.color != currentPiece.pieceData.color))
                    {
                        moves.Add(target);
                    }
                }
            }
            
            return moves;
        }        private List<Vector2Int> GetBishopMoves(Vector2Int pos)
        {
            List<Vector2Int> moves = new List<Vector2Int>();
            
            // Diagonal directions
            Vector2Int[] directions = { 
                new Vector2Int(1, 1), new Vector2Int(1, -1),
                new Vector2Int(-1, 1), new Vector2Int(-1, -1)
            };
            
            foreach (Vector2Int dir in directions)
            {
                for (int i = 1; i < chessBoard.boardSize; i++)
                {
                    Vector2Int target = pos + dir * i;
                    if (!chessBoard.IsValidPosition(target.x, target.y))
                        break;
                    
                    ChessPiece piece = GetVirtualPieceAt(target);
                    if (piece == null)
                    {
                        moves.Add(target);
                    }
                    else
                    {
                        ChessPiece currentPiece = GetVirtualPieceAt(pos);
                        if (currentPiece != null && piece.pieceData.color != currentPiece.pieceData.color)
                        {
                            moves.Add(target);
                        }
                        break;
                    }
                }
            }
            
            return moves;
        }

        private List<Vector2Int> GetQueenMoves(Vector2Int pos)
        {
            List<Vector2Int> moves = new List<Vector2Int>();
            moves.AddRange(GetRookMoves(pos));
            moves.AddRange(GetBishopMoves(pos));
            return moves;
        }        private List<Vector2Int> GetKingMoves(Vector2Int pos)
        {
            List<Vector2Int> moves = new List<Vector2Int>();
            
            for (int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    if (x == 0 && y == 0) continue;
                    
                    Vector2Int target = pos + new Vector2Int(x, y);
                    if (chessBoard.IsValidPosition(target.x, target.y))
                    {
                        ChessPiece piece = GetVirtualPieceAt(target);
                        ChessPiece currentPiece = GetVirtualPieceAt(pos);
                        
                        if (currentPiece != null && (piece == null || piece.pieceData.color != currentPiece.pieceData.color))
                        {
                            moves.Add(target);
                        }
                    }
                }
            }
            
            return moves;
        }        private bool IsValidMove(Vector2Int from, Vector2Int to)
        {
            ChessPiece piece = GetVirtualPieceAt(from);
            if (piece == null) return false;
            
            // First check basic piece movement rules
            if (!piece.IsValidMove(to, chessBoard)) return false;
            
            // Use the ChessBoard's king safety validation
            // This ensures AI doesn't make moves that put its own king in check
            return !chessBoard.WouldMovePutKingInCheck(from, to, piece.pieceData.color);
        }// Virtual board state for simulation - we'll track pieces without moving actual GameObjects
        private ChessPiece[,] virtualBoard;
        private bool usingVirtualBoard = false;        private void InitializeVirtualBoard()
        {
            if (chessBoard == null) return;
            
            virtualBoard = new ChessPiece[chessBoard.boardSize, chessBoard.boardSize];
            
            // Copy current board state to virtual board
            for (int x = 0; x < chessBoard.boardSize; x++)
            {
                for (int y = 0; y < chessBoard.boardSize; y++)
                {
                    virtualBoard[x, y] = chessBoard.GetPieceAt(new Vector2Int(x, y));
                }
            }
        }        private ChessPiece GetVirtualPieceAt(Vector2Int position)
        {
            if (chessBoard == null) return null;
            
            if (usingVirtualBoard && virtualBoard != null)
            {
                if (position.x >= 0 && position.x < chessBoard.boardSize && 
                    position.y >= 0 && position.y < chessBoard.boardSize)
                {
                    return virtualBoard[position.x, position.y];
                }
                return null;
            }
            return chessBoard.GetPieceAt(position);
        }        private ChessPiece SimulateMove(Move move)
        {
            if (!usingVirtualBoard)
            {
                InitializeVirtualBoard();
                usingVirtualBoard = true;
            }

            if (virtualBoard == null) return null;

            // Get pieces from virtual board
            ChessPiece movingPiece = virtualBoard[move.from.x, move.from.y];
            ChessPiece capturedPiece = virtualBoard[move.to.x, move.to.y];
            
            // Simulate the move on virtual board only
            virtualBoard[move.to.x, move.to.y] = movingPiece;
            virtualBoard[move.from.x, move.from.y] = null;
            
            // Update the moving piece's virtual position
            if (movingPiece != null)
            {
                movingPiece.boardPosition = move.to;
            }
            
            return capturedPiece;
        }

        private void UndoMove(Move move, ChessPiece capturedPiece)
        {
            if (!usingVirtualBoard || virtualBoard == null) return;

            // Get the piece that was moved
            ChessPiece movingPiece = virtualBoard[move.to.x, move.to.y];
            
            // Undo the move on virtual board
            virtualBoard[move.from.x, move.from.y] = movingPiece;
            virtualBoard[move.to.x, move.to.y] = capturedPiece;
            
            // Restore the moving piece's original position
            if (movingPiece != null)
            {
                movingPiece.boardPosition = move.from;
            }
        }        private Vector2Int FindKing(PieceColor color)
        {
            for (int x = 0; x < chessBoard.boardSize; x++)
            {
                for (int y = 0; y < chessBoard.boardSize; y++)
                {
                    ChessPiece piece = GetVirtualPieceAt(new Vector2Int(x, y));
                    if (piece != null && piece.pieceData.type == PieceType.King && 
                        piece.pieceData.color == color)
                    {
                        return new Vector2Int(x, y);
                    }
                }
            }
            return Vector2Int.one * -1; // Not found
        }        private List<Move> GetAttacksOnSquare(Vector2Int square, PieceColor attackingColor)
        {
            List<Move> attacks = new List<Move>();
            
            for (int x = 0; x < chessBoard.boardSize; x++)
            {
                for (int y = 0; y < chessBoard.boardSize; y++)
                {
                    ChessPiece piece = GetVirtualPieceAt(new Vector2Int(x, y));
                    if (piece != null && piece.pieceData.color == attackingColor)
                    {
                        List<Vector2Int> moves = GetPossibleMoves(piece);
                        if (moves.Contains(square))
                        {
                            attacks.Add(new Move(new Vector2Int(x, y), square));
                        }
                    }
                }
            }
            
            return attacks;
        }

        private PieceColor GetOpponentColor(PieceColor color)
        {
            return color == PieceColor.White ? PieceColor.Black : PieceColor.White;
        }
    }

    [System.Serializable]
    public class Move
    {
        public Vector2Int from;
        public Vector2Int to;
        
        public Move(Vector2Int from, Vector2Int to)
        {
            this.from = from;
            this.to = to;
        }
    }
}
