namespace PixelEditor.Vector
{
    public class ShapeText : BaseShape
    {
        public string Content { get; set; } = string.Empty;
        public float X { get; set; } = 0.0f;
        public float Y { get; set; } = 0.0f;
        public float Width { get; set; }
        public float Height { get; set; }
        public float FontSize { get; set; } = 12.0f;
        public bool IsBold { get; internal set; } = false;
        public bool IsItalic { get; internal set; } = false;
        public string FontFamily { get; set; } = "Arial";
    }
}
