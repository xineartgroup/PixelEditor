namespace PixelEditor.Vector
{
    public class ShapeRect : BaseShape
    {
        public ShapeRect()
        {
            X = 0;
            Y = 0;
            Width = 0;
            Height = 0;
        }

        public ShapeRect(float x, float y, float width, float height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }

        public float X { get; set; }
        public float Y { get; set; }
        public float Width { get; set; }
        public float Height { get; set; }
        public float Rx { get; set; } // Corner radius X
        public float Ry { get; set; } // Corner radius Y
    }
}
