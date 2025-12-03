using System.Collections.Generic;
using UnityEngine;

namespace Chess.DEBUG
{
    using Core;
    using Pieces;
    using Rules;

    /// <summary>
    /// Debug script to test move generation
    /// ONLY FOR TESTING - DELETE BEFORE PRODUCTION
    /// </summary>
    public class MoveGeneratorTest : MonoBehaviour
    {
        public void TestPawnCapture()
        {
            // Create a test board with White pawn at (4, 4) and Black knight at (5, 5)
            var board = new Board(8);
            board.SetPiece(new Position(4, 4), new Piece(PieceType.Pawn, Color.White));
            board.SetPiece(new Position(5, 5), new Piece(PieceType.Knight, Color.Black));
            board.SetPiece(new Position(3, 0), new Piece(PieceType.King, Color.White));
            board.SetPiece(new Position(3, 7), new Piece(PieceType.King, Color.Black));

            Debug.Log("=== PAWN CAPTURE TEST ===");
            Debug.Log("White pawn at (4,4), Black knight at (5,5)");
            Debug.Log(board.ToString());

            // Get pawn moves
            var pawnMoves = PieceMoveGenerator.GetPseudoLegalMoves(board, new Position(4, 4));
            Debug.Log($"Pawn pseudo-legal moves: {pawnMoves.Count}");
            foreach (var move in pawnMoves)
            {
                var target = board.GetPiece(move.To);
                if (target != null)
                    Debug.Log($"  {move.From} → {move.To} (CAPTURE {target.Color} {target.Type})");
                else
                    Debug.Log($"  {move.From} → {move.To}");
            }

            // Get legal moves (with check validation)
            var rules = new ChessRules(board);
            var legalMoves = rules.GetLegalMoves(Color.White);
            Debug.Log($"White legal moves: {legalMoves.Count}");
            foreach (var move in legalMoves)
            {
                var target = board.GetPiece(move.To);
                if (target != null)
                    Debug.Log($"  {move.From} → {move.To} (CAPTURE {target.Color} {target.Type})");
                else
                    Debug.Log($"  {move.From} → {move.To}");
            }

            // Check if capture move exists
            var captureMove = pawnMoves.Find(m => m.To.File == 5 && m.To.Rank == 5);
            if (captureMove != null)
                Debug.Log("✓ Capture move FOUND in pseudo-legal");
            else
                Debug.LogError("❌ Capture move NOT found in pseudo-legal");

            var legalCapture = legalMoves.Find(m => m.To.File == 5 && m.To.Rank == 5);
            if (legalCapture != null)
                Debug.Log("✓ Capture move FOUND in legal moves");
            else
                Debug.LogError("❌ Capture move NOT found in legal moves");
        }

        private void Start()
        {
            TestPawnCapture();
        }
    }
}
