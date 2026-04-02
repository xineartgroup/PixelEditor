namespace PixelEditor.Vector
{
    public class ShapePolygon : BaseShape
    {
        public bool IsClosed { get; set; } = false;

        public List<PointF> Points { get; set; } = [];
    }
}
