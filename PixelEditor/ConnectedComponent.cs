namespace PixelEditor
{
    public class ConnectedComponent
    {
        public int Label { get; }

        public List<PointF> Points { get; } = [];

        public Rectangle Bounds { get; set; }

        public ConnectedComponent()
        {
            Label = 0;
            Points = [];
            Bounds = new Rectangle(int.MaxValue, int.MaxValue, 0, 0);
        }

        public ConnectedComponent(int label)
        {
            Label = label;
            Points = [];
            Bounds = new Rectangle(int.MaxValue, int.MaxValue, 0, 0);
        }

        public void AddPoint(PointF point)
        {
            Points.Add(point);
            UpdateBounds(point);
        }

        private void UpdateBounds(PointF point)
        {
            int left = (int)Math.Min(Bounds.Left, point.X);
            int top = (int)Math.Min(Bounds.Top, point.Y);
            int right = (int)Math.Max(Bounds.Right, point.X + 1);
            int bottom = (int)Math.Max(Bounds.Bottom, point.Y + 1);
            Bounds = new Rectangle(left, top, right - left, bottom - top);
        }
    }
}