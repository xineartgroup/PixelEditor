namespace PixelEditor.Vector
{
    public class ShapeEllipse : BaseShape
    {
        public ShapeEllipse()
        {
            X = 0;
            Y = 0;
            Width = 0;
            Height = 0;
        }

        public ShapeEllipse(float x, float y, float width, float height)
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

        public float Cx => X + (Width / 2f);
        public float Cy => Y + (Height / 2f);
        public float Rx => Width / 2f;
        public float Ry => Height / 2f;
    }
}