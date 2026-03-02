namespace PixelEditor
{
    public class ColorGrid
    {
        private readonly int[] _pixels = [];
        public int Width { get; } = 0;
        public int Height { get; } = 0;

        public ColorGrid(int width, int height)
        {
            Width = width;
            Height = height;
            _pixels = new int[Width * Height];
        }

        public ColorGrid(ColorGrid cache)
        {
            Width = cache.Width;
            Height = cache.Height;
            _pixels = new int[Width * Height];
            Array.Copy(cache._pixels, _pixels, cache._pixels.Length);
        }

        public int this[int x, int y]
        {
            get
            {
                ValidateBounds(x, y);
                return _pixels[y * Width + x];
            }
            set
            {
                ValidateBounds(x, y);
                _pixels[y * Width + x] = value;
            }
        }

        public int this[int index]
        {
            get
            {
                if (index >= 0 && index < _pixels.Length)
                    return _pixels[index];
                return Color.Transparent.ToArgb();
            }
            set
            {
                if (index >= 0 && index < _pixels.Length)
                    _pixels[index] = value;
            }
        }

        public int[] GetRawPixels() => _pixels;

        private void ValidateBounds(int x, int y)
        {
            if (x < 0 || x >= Width || y < 0 || y >= Height)
                throw new IndexOutOfRangeException($"Coordinates ({x},{y}) are out of bounds for grid {Width}x{Height}.");
        }
    }
}