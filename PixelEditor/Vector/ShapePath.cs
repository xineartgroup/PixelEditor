namespace PixelEditor.Vector
{
    public class ShapePath : BaseShape
    {
        public ShapePath()
        {
            PathSegments = [];
        }

        public ShapePath(List<PathSegment> segments)
        {
            PathSegments = segments;
        }

        public List<PathSegment> PathSegments { get; set; } = [];

        public override List<Point> ControlPoints()
        {
            var result = new List<Point>();
            foreach (var segment in PathSegments)
            {
                var points = segment.GetPoints();
                foreach (var p in points)
                {
                    result.Add(new Point((int)p.X, (int)p.Y));
                }
            }
            return result;
        }
    }
}
