namespace PixelEditor
{
    public class SelectionPolygon
    {
        public bool[,]? Mask { get; set; } = null;

        public Point SelectionPoint { get; set; } = Point.Empty;

        public bool Adding { get; set; } = true;

        public List<Point> Points { get; set; } = [];

        public SelectionPolygon()
        {
            Mask = null;
            SelectionPoint = Point.Empty;
            Adding = true;
            Points = [];
        }

        public SelectionPolygon(bool[,]? mask, Point selectionPoint, bool adding, List<Point> points)
        {
            Mask = mask;
            SelectionPoint = selectionPoint;
            Adding = adding;
            Points = points;
        }

        public override string ToString()
        {
            return $"Count: {Points.Count}. " + (Adding ? "Adding." : "Subtracting.");
        }
    }
}
