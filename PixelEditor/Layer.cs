namespace PixelEditor
{
    public class Layer(string name, bool isVisible)
    {
        private int _x = 0;
        private int _y = 0;
        private int _scaleWidth = 1;
        private int _scaleHeight = 1;
        private int _opacity = 100;
        private string _name = name;
        private bool _isVisible = isVisible;
        private bool _redFilter = false;
        private bool _greenFilter = false;
        private bool _blueFilter = false;
        private LayerChannel _channel = LayerChannel.RGB;
        private ImageBlending _blendMode = ImageBlending.Normal;
        private FillType _fillType = FillType.Color;
        private Color _fillColor = Color.White;
        private Image? _image = null;
        private Image? _imageMask = null;

        public event Action<Rectangle>? OnLayerChanged;

        public Image? Image { get { return GetImageComposite(); } set => SetProperty(ref _image, value); }

        public Image? ImageMask { get => _imageMask; set => SetProperty(ref _imageMask, value); }

        public string Name { get => _name; set => SetProperty(ref _name, value); }

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

        public ImageBlending BlendMode { get => _blendMode; set => SetProperty(ref _blendMode, value); }

        public FillType FillType { get => _fillType; set => SetProperty(ref _fillType, value); }

        public Color FillColor { get => _fillColor; set => SetProperty(ref _fillColor, value); }

        private Image? GetImageComposite()
        {
            if (_imageMask == null)
            {
                return _image;
            }
            else
            {
                if (_image != null)
                    return ManipulatorLighting.MaskImage((Bitmap)_image, (Bitmap)_imageMask);
                else
                    return null;
            }
        }

        private void SetProperty<T>(ref T field, T value)
        {
            if (Equals(field, value)) return;

            OnLayerChanged?.Invoke(GetBounds());
            field = value;
            OnLayerChanged?.Invoke(GetBounds());
        }

        public Rectangle GetBounds()
        {
            if (Image == null) return Rectangle.Empty;
            return new Rectangle(_x, _y, Image.Width * _scaleWidth, Image.Height * _scaleHeight);
        }

        public void ResizeContainer(int containerWidth, int containerHeight)
        {
            if (Image == null) return;
            if (containerWidth <= 0 || containerHeight <= 0) return;

            Bitmap newImage = new(containerWidth, containerHeight);
            using (Graphics g = Graphics.FromImage(newImage))
            {
                g.Clear(Color.Transparent);
                g.DrawImage(Image, 0, 0, containerWidth, containerHeight);
            }

            Image.Dispose();
            Image = newImage;
            OnLayerChanged?.Invoke(GetBounds());
        }

        public Layer Clone()
        {
            Layer clone = new(Name, IsVisible)
            {
                _x = _x,
                _y = _y,
                _scaleWidth = _scaleWidth,
                _scaleHeight = _scaleHeight,
                _opacity = _opacity,
                _redFilter = _redFilter,
                _greenFilter = _greenFilter,
                _blueFilter = _blueFilter,
                _channel = _channel,
                _blendMode = _blendMode,
                _fillType = _fillType,
                _fillColor = _fillColor
            };

            if (Image != null)
            {
                clone.Image = new Bitmap(Image);
            }

            return clone;
        }
    }
}