namespace PixelEditor
{
    public class ColorRegion
    {
        public List<PointF> Pixels { get; set; } = [];
        public Color AverageColor { get; set; } = Color.Empty;
        public Rectangle Bounds { get; set; } = new Rectangle(0, 0, 0, 0);
        public List<PointF> Boundary { get; set; } = [];
    }
}
