namespace PixelEditor.Vector
{
    public abstract class BaseShape
    {
        public float LineWidth { get; set; } = 1.0f;
        public Color LineColor { get; set; } = Color.Black;
        public Color FillColor { get; set; } = Color.White;
        public float Opacity { get; set; } = 1.0f;
        public float StrokeOpacity { get; set; } = 1.0f;
        public float Rotation { get; set; } = 0.0f;
        public bool HasGradientFill { get; set; } = false;
        public bool HasGradientStroke { get; set; } = false;
        public GradientInfo? GradientStroke { get; set; } = null;
        public GradientInfo? GradientFill { get; set; } = null;
    }
}
