using UnityEngine;

namespace ChessBalatro
{
    public class ChessPiece : MonoBehaviour
    {
        [Header("Piece Data")]
        public PieceData pieceData;
        
        [Header("Visual Components")]
        public SpriteRenderer spriteRenderer;
        
        [Header("Position")]
        public Vector2Int boardPosition;
        
        [Header("2.5D Settings")]
        public float pieceHeight = 0.1f;

        private void Awake()
        {
            if (spriteRenderer == null)
                spriteRenderer = GetComponent<SpriteRenderer>();
        }

        public void Initialize(PieceData data, Vector2Int position)
        {
            pieceData = new PieceData(data.type, data.color, data.sprite);
            boardPosition = position;
            
            if (spriteRenderer != null && data.sprite != null)
            {
                spriteRenderer.sprite = data.sprite;
            }
            
            // Set 2.5D position
            transform.position = new Vector3(position.x, pieceHeight, position.y);
            
            // Name the piece for easier debugging
            gameObject.name = $"{data.color}_{data.type}_{position.x}_{position.y}";
        }

        public void SetBoardPosition(Vector2Int newPosition)
        {
            boardPosition = newPosition;
            transform.position = new Vector3(newPosition.x, pieceHeight, newPosition.y);
        }        public bool IsValidMove(Vector2Int targetPosition, ChessBoard board)
        {
            // Basic bounds checking
            if (targetPosition.x < 0 || targetPosition.x >= 8 || 
                targetPosition.y < 0 || targetPosition.y >= 8)
                return false;

            // Check if there's a piece of the same color at the target position
            ChessPiece targetPiece = board.GetPieceAt(targetPosition);
            if (targetPiece != null && targetPiece.pieceData.color == pieceData.color)
                return false;

            // Implement piece-specific movement rules
            return IsValidMoveForPieceType(targetPosition, board);
        }
        
        private bool IsValidMoveForPieceType(Vector2Int targetPosition, ChessBoard board)
        {
            Vector2Int currentPos = boardPosition;
            int deltaX = targetPosition.x - currentPos.x;
            int deltaY = targetPosition.y - currentPos.y;
            
            switch (pieceData.type)
            {
                case PieceType.Pawn:
                    return IsValidPawnMove(targetPosition, board);
                case PieceType.Rook:
                    return IsValidRookMove(targetPosition, board);
                case PieceType.Knight:
                    return IsValidKnightMove(targetPosition);
                case PieceType.Bishop:
                    return IsValidBishopMove(targetPosition, board);
                case PieceType.Queen:
                    return IsValidQueenMove(targetPosition, board);                case PieceType.King:
                    return IsValidKingMove(targetPosition, board);
                default:
                    return false;
            }
        }
        
        private bool IsValidPawnMove(Vector2Int targetPosition, ChessBoard board)
        {
            Vector2Int currentPos = boardPosition;
            int deltaX = targetPosition.x - currentPos.x;
            int deltaY = targetPosition.y - currentPos.y;
            int direction = pieceData.color == PieceColor.White ? 1 : -1;
            
            // Forward move
            if (deltaX == 0)
            {
                if (deltaY == direction)
                {
                    // Single step forward
                    return board.GetPieceAt(targetPosition) == null;
                }
                else if (deltaY == 2 * direction)
                {
                    // Double step from starting position
                    bool isStartingPosition = (pieceData.color == PieceColor.White && currentPos.y == 1) ||
                                            (pieceData.color == PieceColor.Black && currentPos.y == 6);
                    return isStartingPosition && 
                           board.GetPieceAt(targetPosition) == null &&
                           board.GetPieceAt(new Vector2Int(currentPos.x, currentPos.y + direction)) == null;
                }
            }
            // Diagonal capture
            else if (Mathf.Abs(deltaX) == 1 && deltaY == direction)
            {
                ChessPiece targetPiece = board.GetPieceAt(targetPosition);
                return targetPiece != null && targetPiece.pieceData.color != pieceData.color;
            }
            
            return false;
        }
        
        private bool IsValidRookMove(Vector2Int targetPosition, ChessBoard board)
        {
            Vector2Int currentPos = boardPosition;
            int deltaX = targetPosition.x - currentPos.x;
            int deltaY = targetPosition.y - currentPos.y;
            
            // Must move in straight line (horizontal or vertical)
            if (deltaX != 0 && deltaY != 0)
                return false;
            
            // Check path is clear
            return IsPathClear(currentPos, targetPosition, board);
        }
        
        private bool IsValidKnightMove(Vector2Int targetPosition)
        {
            Vector2Int currentPos = boardPosition;
            int deltaX = Mathf.Abs(targetPosition.x - currentPos.x);
            int deltaY = Mathf.Abs(targetPosition.y - currentPos.y);
            
            return (deltaX == 2 && deltaY == 1) || (deltaX == 1 && deltaY == 2);
        }
        
        private bool IsValidBishopMove(Vector2Int targetPosition, ChessBoard board)
        {
            Vector2Int currentPos = boardPosition;
            int deltaX = Mathf.Abs(targetPosition.x - currentPos.x);
            int deltaY = Mathf.Abs(targetPosition.y - currentPos.y);
            
            // Must move diagonally
            if (deltaX != deltaY)
                return false;
            
            // Check path is clear
            return IsPathClear(currentPos, targetPosition, board);
        }
        
        private bool IsValidQueenMove(Vector2Int targetPosition, ChessBoard board)
        {
            return IsValidRookMove(targetPosition, board) || IsValidBishopMove(targetPosition, board);
        }
          private bool IsValidKingMove(Vector2Int targetPosition, ChessBoard board)
        {
            Vector2Int currentPos = boardPosition;
            int deltaX = Mathf.Abs(targetPosition.x - currentPos.x);
            int deltaY = Mathf.Abs(targetPosition.y - currentPos.y);
            
            // Basic king movement - one square in any direction
            if (!(deltaX <= 1 && deltaY <= 1 && (deltaX + deltaY > 0)))
                return false;
            
            // CRITICAL: Kings cannot move to squares under attack
            PieceColor enemyColor = pieceData.color == PieceColor.White ? PieceColor.Black : PieceColor.White;
            if (board.IsPositionUnderAttack(targetPosition, enemyColor))
            {
                return false;
            }
            
            return true;
        }
        
        private bool IsPathClear(Vector2Int from, Vector2Int to, ChessBoard board)
        {
            Vector2Int direction = new Vector2Int(
                to.x > from.x ? 1 : (to.x < from.x ? -1 : 0),
                to.y > from.y ? 1 : (to.y < from.y ? -1 : 0)
            );
            
            Vector2Int current = from + direction;
            
            while (current != to)
            {
                if (board.GetPieceAt(current) != null)
                    return false;
                current += direction;
            }
            
            return true;
        }
    }
}
