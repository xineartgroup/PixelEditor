namespace PixelEditor.Vector
{
    public class ShapeText : BaseShape
    {
        public ShapeText()
        {
            Content = string.Empty;
            X = 0.0f;
            Y = 0.0f;
            Width = 0.0f;
            Height = 0.0f;
            FontFamily = "Arial";
            FontSize = 12.0f;
        }

        public ShapeText(string content, float x, float y, float width, float height, string fontFamily, float fontSize)
        {
            Content = content;
            X = x;
            Y = y;
            Width = width;
            Height = height;
            FontFamily = fontFamily;
            FontSize = fontSize;
        }

        public string Content { get; set; } = string.Empty;
        public float X { get; set; } = 0.0f;
        public float Y { get; set; } = 0.0f;
        public float Width { get; set; } = 0.0f;
        public float Height { get; set; } = 0.0f;
        public float FontSize { get; set; } = 12.0f;
        public bool IsBold { get; internal set; } = false;
        public bool IsItalic { get; internal set; } = false;
        public string FontFamily { get; set; } = "Arial";

        public override List<Point> ControlPoints()
        {
            return
            [
                new Point((int)X, (int)Y),
                new Point((int)(X + Width), (int)Y),
                new Point((int)(X + Width), (int)(Y + Height)),
                new Point((int)X, (int)(Y + Height))
            ];
        }

    }
}
