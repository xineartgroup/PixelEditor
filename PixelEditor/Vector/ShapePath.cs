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
    }
}
