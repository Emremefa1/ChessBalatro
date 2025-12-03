using UnityEngine;

namespace Chess.Core
{
    /// <summary>
    /// Represents a position on the chess board using coordinates (file, rank)
    /// </summary>
    [System.Serializable]
    public struct Position
    {
        public int File { get; private set; } // 0-7 for 8x8, can be extended
        public int Rank { get; private set; } // 0-7 for 8x8, can be extended

        public Position(int file, int rank)
        {
            File = file;
            Rank = rank;
        }

        public bool IsValid(int boardSize)
        {
            return File >= 0 && File < boardSize && Rank >= 0 && Rank < boardSize;
        }

        public override bool Equals(object obj)
        {
            if (obj is Position pos)
                return File == pos.File && Rank == pos.Rank;
            return false;
        }

        public override int GetHashCode()
        {
            return File * 31 + Rank;
        }

        public override string ToString()
        {
            return $"({File}, {Rank})";
        }

        public static bool operator ==(Position a, Position b) => a.Equals(b);
        public static bool operator !=(Position a, Position b) => !a.Equals(b);
    }
}
