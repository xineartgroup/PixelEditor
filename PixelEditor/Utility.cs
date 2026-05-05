namespace PixelEditor
{
    public static class Utility
    {
        public static float VectorDistance(PointF a, PointF b)
        {
            return (float)Math.Sqrt(Math.Pow(b.X - a.X, 2) + Math.Pow(b.Y - a.Y, 2));
        }
    }
}
