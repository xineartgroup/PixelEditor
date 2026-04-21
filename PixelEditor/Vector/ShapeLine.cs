namespace PixelEditor.Vector
{
    public class ShapeLine : BaseShape
    {
        public PointF StartPoint { get; set; }
        public PointF EndPoint { get; set; }

        public override List<Point> ControlPoints()
        {
            return
            [
                new Point((int)StartPoint.X, (int)StartPoint.Y),
                new Point((int)EndPoint.X, (int)EndPoint.Y)
            ];
        }
    }
}
