using PixelEditor.Vector;
using System.Drawing.Drawing2D;
using System.Reflection.Emit;

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
        private LayerType _layerType = LayerType.Image;
        private LayerChannel _channel = LayerChannel.RGB;
        private ImageBlending _blendMode = ImageBlending.Normal;
        private FillType _fillType = FillType.Color;
        private Color _fillColor = Color.White;
        private Image? _image = null;
        private Image? _imageMask = null;
        private List<BaseShape> shapes = [];
        private BaseShape? currentShape = null;

        public event Action<Rectangle>? OnLayerChanged;

        public Image? GetBasicImage()
        {
            return _image;
        }

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

        public LayerType LayerType { get => _layerType; set => SetProperty(ref _layerType, value); }

        public LayerChannel Channel { get => _channel; set => SetProperty(ref _channel, value); }

        public ImageBlending BlendMode { get => _blendMode; set => SetProperty(ref _blendMode, value); }

        public FillType FillType { get => _fillType; set => SetProperty(ref _fillType, value); }

        public Color FillColor { get => _fillColor; set => SetProperty(ref _fillColor, value); }

        public List<BaseShape> Shapes { get => shapes; set => SetProperty(ref shapes, value); }

        public BaseShape? CurrentShape { get => currentShape; set => SetProperty(ref currentShape, value); }

        private Image? GetImageComposite()
        {
            if (LayerType == LayerType.Vector)
            {
                return DrawVectorImage(new Bitmap(Document.Width, Document.Height));
            }
            else
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
        }

        public bool IsNewShape(BaseShape shape)
        {
            return !shapes.Contains(shape);
        }

        private Image? DrawVectorImage(Image temp)
        {
            Image? image = new Bitmap(temp);
            using (Graphics g = Graphics.FromImage(image))
            {
                bool foundSelected = false;
                foreach (BaseShape shape in shapes)
                {
                    if (shape == currentShape)
                    {
                        DrawShape(shape, g, true);
                        foundSelected = true;
                    }
                    else
                    {
                        DrawShape(shape, g);
                    }
                }
                if (currentShape != null && !foundSelected)
                {
                    DrawShape(currentShape, g, true);
                }
            }
            return image;
        }

        public static void DrawShape(BaseShape shape, Graphics g, bool isSelected = false)
        {
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            using Pen pen = new(shape.LineColor, shape.LineWidth);
            using SolidBrush brush = new(shape.FillColor);

            pen.DashStyle = shape.DashStyle;

            const int size = 10;
            const int offset = size / 2;

            if (shape is ShapeRect rect)
            {
                g.FillRectangle(brush, rect.X, rect.Y, rect.Width, rect.Height);
                g.DrawRectangle(pen, rect.X, rect.Y, rect.Width, rect.Height);

                if (isSelected)
                {
                    using SolidBrush handleBrush = new(Color.LightBlue);
                    using Pen handlePen = new(Color.Red, 2);

                    RectangleF[] handles =
                    [
                        new(rect.X - offset, rect.Y - offset, size, size),
                        new(rect.X + rect.Width - offset, rect.Y - offset, size, size),
                        new(rect.X - offset, rect.Y + rect.Height - offset, size, size),
                        new(rect.X + rect.Width - offset, rect.Y + rect.Height - offset, size, size)
                    ];

                    foreach (var h in handles)
                    {
                        g.FillRectangle(handleBrush, h);
                        g.DrawRectangle(handlePen, h.X, h.Y, h.Width, h.Height);
                    }
                }
            }
            else if (shape is ShapeEllipse ellipse)
            {
                g.FillEllipse(brush, ellipse.X, ellipse.Y, ellipse.Width, ellipse.Height);
                g.DrawEllipse(pen, ellipse.X, ellipse.Y, ellipse.Width, ellipse.Height);

                if (isSelected)
                {
                    using SolidBrush handleBrush = new(Color.LightBlue);
                    using Pen handlePen = new(Color.Red, 2);

                    RectangleF[] handles =
                    [
                        new(ellipse.X - offset, ellipse.Y - offset, size, size),
                        new(ellipse.X + ellipse.Width - offset, ellipse.Y - offset, size, size),
                        new(ellipse.X - offset, ellipse.Y + ellipse.Height - offset, size, size),
                        new(ellipse.X + ellipse.Width - offset, ellipse.Y + ellipse.Height - offset, size, size)
                    ];

                    foreach (var h in handles)
                    {
                        g.FillRectangle(handleBrush, h);
                        g.DrawRectangle(handlePen, h.X, h.Y, h.Width, h.Height);
                    }
                }
            }
            else if (shape is ShapePolygon polygon && polygon.Points.Count > 1)
            {
                if (polygon.IsClosed)
                {
                    g.FillPolygon(brush, polygon.Points.ToArray());
                    g.DrawPolygon(pen, polygon.Points.ToArray());
                }
                else
                {
                    g.DrawLines(pen, polygon.Points.ToArray());
                }

                if (isSelected)
                {
                    using SolidBrush handleBrush = new(Color.LightBlue);
                    using Pen handlePen = new(Color.Red, 2);

                    foreach (var p in polygon.Points)
                    {
                        RectangleF handle = new(p.X - offset, p.Y - offset, size, size);
                        g.FillRectangle(handleBrush, handle);
                        g.DrawRectangle(handlePen, handle.X, handle.Y, handle.Width, handle.Height);
                    }
                }
            }
            else if (shape is ShapeText text)
            {
                using Font font = CreateScaledFont(text);
                g.DrawString(text.Content, font, brush, text.X, text.Y);

                if (isSelected)
                {
                    using SolidBrush handleBrush = new(Color.LightBlue);
                    using Pen handlePen = new(Color.Red, 2);

                    RectangleF[] handles =
                    [
                        new(text.X - offset, text.Y - offset, size, size),
                        new(text.X + text.Width - offset, text.Y - offset, size, size),
                        new(text.X - offset, text.Y + text.Height - offset, size, size),
                        new(text.X + text.Width - offset, text.Y + text.Height - offset, size, size)
                    ];

                    foreach (var h in handles)
                    {
                        g.FillRectangle(handleBrush, h);
                        g.DrawRectangle(handlePen, h.X, h.Y, h.Width, h.Height);
                    }
                }
            }
            else if (shape is ShapePath path)
            {
                using GraphicsPath gPath = BuildGraphicsPath(path);

                g.FillPath(brush, gPath);
                g.DrawPath(pen, gPath);

                if (isSelected)
                {
                    using SolidBrush handleBrush = new(Color.LightBlue);
                    using Pen handlePen = new(Color.Red, 2);

                    foreach (var segment in path.PathSegments)
                    {
                        foreach (var p in segment.InputPoints)
                        {
                            RectangleF handle = new(p.X - offset, p.Y - offset, size, size);
                            g.FillRectangle(handleBrush, handle);
                            g.DrawRectangle(handlePen, handle.X, handle.Y, handle.Width, handle.Height);
                        }
                    }
                }
            }
        }

        public static GraphicsPath BuildGraphicsPath(ShapePath path)
        {
            GraphicsPath gPath = new();
            PointF currentPoint = PointF.Empty;

            foreach (var segment in path.PathSegments)
            {
                string type = segment.PathType.ToUpper();
                var pts = segment.InputPoints;

                switch (type)
                {
                    case "M":
                        if (pts.Count > 0)
                        {
                            gPath.StartFigure();
                            currentPoint = pts[^1];
                        }
                        break;

                    case "L":
                        if (pts.Count > 0)
                        {
                            gPath.AddLine(currentPoint, pts[0]);
                            currentPoint = pts[^1];
                        }
                        break;

                    case "H":
                        if (pts.Count > 0)
                        {
                            gPath.AddLine(currentPoint, new PointF(pts[0].X, currentPoint.Y));
                            currentPoint = new PointF(pts[0].X, currentPoint.Y);
                        }
                        break;

                    case "V":
                        if (pts.Count > 0)
                        {
                            gPath.AddLine(currentPoint, new PointF(currentPoint.X, pts[0].Y));
                            currentPoint = new PointF(currentPoint.X, pts[0].Y);
                        }
                        break;

                    case "C":
                        if (pts.Count >= 4)
                        {
                            gPath.AddBezier(pts[0], pts[1], pts[2], pts[3]);
                            currentPoint = pts[3];
                        }
                        break;

                    case "Q":
                        if (pts.Count >= 3)
                        {
                            PointF cp1 = new((currentPoint.X + 2 * pts[1].X) / 3, (currentPoint.Y + 2 * pts[1].Y) / 3);
                            PointF cp2 = new((pts[2].X + 2 * pts[1].X) / 3, (pts[2].Y + 2 * pts[1].Y) / 3);
                            gPath.AddBezier(currentPoint, cp1, cp2, pts[2]);
                            currentPoint = pts[2];
                        }
                        break;

                    case "A":
                        List<PointF> arcPoints = segment.GetPoints();
                        if (arcPoints.Count > 1)
                        {
                            gPath.AddCurve(arcPoints.ToArray());
                            currentPoint = arcPoints[^1];
                        }
                        break;

                    case "Z":
                        gPath.CloseFigure();
                        break;
                }
            }
            return gPath;
        }

        public static Font CreateScaledFont(ShapeText text)
        {
            using Font tempFont = new(text.FontFamily, text.FontSize, GetFontStyle(text));

            using StringFormat stringFormat = new()
            {
                Alignment = StringAlignment.Near,
                LineAlignment = StringAlignment.Near,
                Trimming = StringTrimming.None
            };

            using Graphics g = Graphics.FromImage(new Bitmap(1, 1));

            SizeF textSize = g.MeasureString(text.Content, tempFont, new SizeF(text.Width, text.Height), stringFormat);

            float widthScale = text.Width > 0 ? text.Width / textSize.Width : 1f;
            float heightScale = text.Height > 0 ? text.Height / textSize.Height : 1f;

            float scale = Math.Min(widthScale, heightScale);

            float scaledFontSize = Math.Max(4, Math.Min(text.FontSize * scale, 4000));

            return new Font(text.FontFamily, scaledFontSize, GetFontStyle(text));
        }

        private static FontStyle GetFontStyle(ShapeText text)
        {
            FontStyle style = FontStyle.Regular;
            if (text.IsBold) style |= FontStyle.Bold;
            if (text.IsItalic) style |= FontStyle.Italic;
            return style;
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