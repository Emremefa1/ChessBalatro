using UnityEngine;
using System.Collections.Generic;

namespace Chess.UI
{
    using Core;
    using Rules;

    /// <summary>
    /// Manages input and interaction with the chess board
    /// </summary>
    public class ChessBoardInput : MonoBehaviour
    {
        public delegate void OnMoveAttempted(Move move);
        public event OnMoveAttempted MoveAttempted;

        private ChessRules rules;
        private Position? selectedPosition;
        private HashSet<Position> highlightedMoves = new();

        public void Initialize(ChessRules chessRules)
        {
            rules = chessRules;
        }

        public void HandleBoardClick(Position clickedPosition)
        {
            if (selectedPosition == null)
            {
                // Select a piece
                var piece = rules.Board.GetPiece(clickedPosition);
                if (piece != null) // && piece.Color == currentPlayer
                {
                    selectedPosition = clickedPosition;
                    HighlightAvailableMoves(clickedPosition);
                }
            }
            else if (clickedPosition == selectedPosition)
            {
                // Deselect
                selectedPosition = null;
                highlightedMoves.Clear();
            }
            else
            {
                // Attempt move
                var move = new Move(selectedPosition.Value, clickedPosition);
                MoveAttempted?.Invoke(move);
                selectedPosition = null;
                highlightedMoves.Clear();
            }
        }

        private void HighlightAvailableMoves(Position pos)
        {
            highlightedMoves.Clear();
            var legalMoves = rules.GetLegalMoves(rules.Board.GetPiece(pos).Color);
            foreach (var move in legalMoves)
            {
                if (move.From == pos)
                    highlightedMoves.Add(move.To);
            }
        }

        public Position? GetSelectedPosition() => selectedPosition;
        public HashSet<Position> GetHighlightedMoves() => highlightedMoves;
    }
}
