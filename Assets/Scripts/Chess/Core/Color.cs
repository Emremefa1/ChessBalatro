namespace Chess.Core
{
    public enum Color
    {
        White = 0,
        Black = 1
    }

    public static class ColorExtensions
    {
        public static Color Opposite(this Color color)
        {
            return color == Color.White ? Color.Black : Color.White;
        }
    }
}
