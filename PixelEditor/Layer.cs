using System.Drawing.Drawing2D;

namespace PixelEditor
{
    public class Layer(string name, bool isVisible)
    {
        private int _x = 0;
        private int _y = 0;
        private int _scaleWidth = 1;
        private int _scaleHeight = 1;
        private int _opacity = 100;
        private bool _isVisible = isVisible;
        private bool _redFilter = false;
        private bool _greenFilter = false;
        private bool _blueFilter = false;
        private LayerChannel _channel = LayerChannel.RGB;
        private LayerBlending _blendMode = LayerBlending.Normal;

        public event Action<Rectangle>? OnLayerChanged;

        public Image? image = null;
        public string Name = name;

        public int X { get => _x; set => SetProperty(ref _x, value); }

        public int Y { get => _y; set => SetProperty(ref _y, value); }

        public int ScaleWidth { get => _scaleWidth; set => SetProperty(ref _scaleWidth, Math.Max(1, value)); }

        public int ScaleHeight { get => _scaleHeight; set => SetProperty(ref _scaleHeight, Math.Max(1, value)); }

        public int Opacity { get => _opacity; set => SetProperty(ref _opacity, Math.Clamp(value, 0, 100)); }

        public bool IsVisible { get => _isVisible; set => SetProperty(ref _isVisible, value); }

        public bool RedFilter { get => _redFilter; set => SetProperty(ref _redFilter, value); }

        public bool GreenFilter { get => _greenFilter; set => SetProperty(ref _greenFilter, value); }

        public bool BlueFilter { get => _blueFilter; set => SetProperty(ref _blueFilter, value); }

        public LayerChannel Channel { get => _channel; set => SetProperty(ref _channel, value); }

        public LayerBlending BlendMode { get => _blendMode; set => SetProperty(ref _blendMode, value); }

        private void SetProperty<T>(ref T field, T value)
        {
            if (Equals(field, value)) return;

            OnLayerChanged?.Invoke(GetBounds());
            field = value;
            OnLayerChanged?.Invoke(GetBounds());
        }

        public Rectangle GetBounds()
        {
            if (image == null) return Rectangle.Empty;
            return new Rectangle(_x, _y, image.Width * _scaleWidth, image.Height * _scaleHeight);
        }

        public Image? GetProxyImage(int step)
        {
            if (image == null || step <= 1) return image;

            int div = (int)Math.Pow(2, step - 1);
            int nw = Math.Max(1, image.Width / div);
            int nh = Math.Max(1, image.Height / div);

            Bitmap proxy = new(nw, nh);
            using (Graphics g = Graphics.FromImage(proxy))
            {
                g.InterpolationMode = InterpolationMode.NearestNeighbor;
                g.DrawImage(image, 0, 0, nw, nh);
            }
            return proxy;
        }

        public void ResizeContainer(int containerWidth, int containerHeight)
        {
            if (image == null) return;
            if (containerWidth <= 0 || containerHeight <= 0) return;

            Bitmap newImage = new(containerWidth, containerHeight);
            using (Graphics g = Graphics.FromImage(newImage))
            {
                g.Clear(Color.Transparent);
                g.DrawImage(image, 0, 0, containerWidth, containerHeight);
            }

            image.Dispose();
            image = newImage;
            OnLayerChanged?.Invoke(GetBounds());
        }
    }
}