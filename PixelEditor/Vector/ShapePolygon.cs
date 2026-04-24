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

        public int[] ActiveHandleIndicies { get; set; } = [];

        public override List<Point> ControlPoints()
        {
            var result = new List<Point>();
            foreach (var p in Points)
            {
                result.Add(new Point((int)p.X, (int)p.Y));
            }
            return result;
        }
    }
}
