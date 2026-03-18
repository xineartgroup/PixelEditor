namespace PixelEditor
{
    public class SelectionPolygon
    {
        public bool Inwards = true;

        public List<Point> Points { get; set; } = [];

        public SelectionPolygon()
        {
        }

        public SelectionPolygon(bool inwards, List<Point> points)
        {
            Inwards = inwards;
            Points = points;
        }
    }
}
