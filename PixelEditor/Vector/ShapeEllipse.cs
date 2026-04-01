namespace PixelEditor.Vector
{
    public class ShapeEllipse : BaseShape
    {
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