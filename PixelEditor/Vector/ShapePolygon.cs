namespace PixelEditor.Vector
{
    public class ShapePolygon : BaseShape
    {
        public ShapePolygon()
        {
            Points = [];
            IsClosed = false;
        }

        public ShapePolygon(List<PointF> points, bool isClosed)
        {
            Points = points;
            IsClosed = isClosed;
        }

        public bool IsClosed { get; set; } = false;

        public List<PointF> Points { get; set; } = [];
    }
}
