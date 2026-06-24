namespace PixelEditor
{
    public class PaintBrush(string name, Bitmap stamp)
    {
        public string Name { get; set; } = name;
        public Bitmap Stamp { get; set; } = stamp;
        public float Size { get; set; } = 0.1f;
        public float Opacity { get; set; } = 1.0f;
        public float Smoothness { get; set; } = 0.6f;
        public float Hardness { get; set; } = 1.0f;
        public float Spacing { get; set; } = 0.1f;
        public float Randomness { get; set; } = 0.0f;
        public bool PressuredOpacity { get; set; } = false;
        public bool PressuredSize { get; set; } = true;
        public bool Tilt { get; set; } = false;
        public bool Rotation { get; set; } = false;

    }
}
