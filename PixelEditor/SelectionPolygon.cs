namespace PixelEditor
{
    public class SelectionPolygon
    {
        public Point SelectionPoint { get; set; } = Point.Empty;

        public bool Inner { get; set; } = true;

        public bool Adding { get; set; } = true;

        public List<Point> Points { get; set; } = [];

        public SelectionPolygon()
        {
            SelectionPoint = Point.Empty;
            Inner = true;
            Adding = true;
            Points = [];
        }

        public SelectionPolygon(Point selectionPoint, bool inner, bool adding, List<Point> points)
        {
            SelectionPoint = selectionPoint;
            Inner = inner;
            Adding = adding;
            Points = points;
        }

        public override string ToString()
        {
            return $"Count: {Points.Count}. " + (Inner ? "Inner. " : "Outer. ") + (Adding ? "Adding." : "Subtracting.");
        }
    }
}
