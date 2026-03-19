namespace PixelEditor
{
    public static class Document
    {
        public static PointF ImageOffset { get; set; } = new(0, 0);

        public static float Zoom { get; set; } = 0.95f;

        public static int Width { get; set; } = 1920;

        public static int Height { get; set; } = 1080;

    }
}
