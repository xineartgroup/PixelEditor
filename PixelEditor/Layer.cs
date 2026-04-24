using PixelEditor.Vector;
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
        private List<BaseShape> addedShapeSelections = [];
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

        public List<BaseShape> AddedShapeSelections { get => addedShapeSelections; set => SetProperty(ref addedShapeSelections, value); }

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
                List<(RectangleF[] Handles, Matrix Transform)> handlesToDraw = [];
                List<(RectangleF[] Handles, Matrix Transform)> activeHandlesToDraw = [];
                bool foundSelected = false;
                RectangleF groupBounds = RectangleF.Empty;

                foreach (BaseShape shape in shapes)
                {
                    bool isCurrent = (shape == currentShape);
                    if (isCurrent) foundSelected = true;

                    var (handles, activeHandles) = DrawShape(shape, g, isCurrent);

                    if (isCurrent && handles.Length > 0)
                        handlesToDraw.Add((handles, g.Transform.Clone()));

                    if (isCurrent && activeHandles.Length > 0)
                        activeHandlesToDraw.Add((activeHandles, g.Transform.Clone()));

                    if (addedShapeSelections.Contains(shape) || shape == currentShape)
                    {
                        RectangleF bounds = GetShapeBounds(shape, g.Transform);
                        groupBounds = groupBounds.IsEmpty ? bounds : RectangleF.Union(groupBounds, bounds);
                    }
                }

                if (currentShape != null && !foundSelected)
                {
                    var (handles, activeHandles) = DrawShape(currentShape, g, true);
                    if (handles.Length > 0)
                        handlesToDraw.Add((handles, g.Transform.Clone()));
                    if (activeHandles.Length > 0)
                        activeHandlesToDraw.Add((activeHandles, g.Transform.Clone()));
                }

                using SolidBrush handleBrush = new(Color.LightBlue);
                using SolidBrush activeHandleBrush = new(Color.Red);
                using Pen handlePen = new(Color.Red, 2);

                foreach (var (Handles, Transform) in handlesToDraw)
                {
                    Matrix original = g.Transform;
                    g.Transform = Transform;
                    foreach (var handle in Handles)
                    {
                        g.FillRectangle(handleBrush, handle);
                        g.DrawRectangle(handlePen, handle.X, handle.Y, handle.Width, handle.Height);
                    }
                    g.Transform = original;
                    Transform.Dispose();
                }

                foreach (var (Handles, Transform) in activeHandlesToDraw)
                {
                    Matrix original = g.Transform;
                    g.Transform = Transform;
                    foreach (var handle in Handles)
                    {
                        g.FillRectangle(activeHandleBrush, handle);
                        g.DrawRectangle(handlePen, handle.X, handle.Y, handle.Width, handle.Height);
                    }
                    g.Transform = original;
                    Transform.Dispose();
                }

                if (!groupBounds.IsEmpty)
                {
                    g.ResetTransform();
                    g.DrawRectangle(handlePen, groupBounds.X, groupBounds.Y, groupBounds.Width, groupBounds.Height);
                }
            }
            return image;
        }

        public static (RectangleF[], RectangleF[]) DrawShape(BaseShape shape, Graphics g, bool isSelected = false)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            using Pen pen = new(shape.LineColor, shape.LineWidth);
            using SolidBrush brush = new(shape.FillColor);
            pen.DashStyle = shape.DashStyle;

            int size = Math.Max(Document.Width, Document.Height) / 150;
            size = Math.Clamp(size, 2, 20);
            float offset = size / 2f;
            List<RectangleF> handles = [];
            List<RectangleF> activeHandles = [];

            if (shape is ShapeRect rect)
            {
                Matrix originalTransform = g.Transform;

                ApplyRotation(g, rect.X, rect.Y, rect.Width, rect.Height, rect.Rotation);
                g.FillRectangle(brush, rect.X, rect.Y, rect.Width, rect.Height);
                g.DrawRectangle(pen, rect.X, rect.Y, rect.Width, rect.Height);

                if (isSelected)
                {
                    handles.AddRange([
                        new(rect.X - offset, rect.Y - offset, size, size),
                new(rect.X + rect.Width - offset, rect.Y - offset, size, size),
                new(rect.X - offset, rect.Y + rect.Height - offset, size, size),
                new(rect.X + rect.Width - offset, rect.Y + rect.Height - offset, size, size)
                    ]);
                }

                g.Transform = originalTransform;

                Console.WriteLine($"Rect: x={rect.X} y={rect.Y} w={rect.Width} h={rect.Height}");
            }
            else if (shape is ShapeEllipse ellipse)
            {
                Matrix originalTransform = g.Transform;

                ApplyRotation(g, ellipse.X, ellipse.Y, ellipse.Width, ellipse.Height, ellipse.Rotation);
                g.FillEllipse(brush, ellipse.X, ellipse.Y, ellipse.Width, ellipse.Height);
                g.DrawEllipse(pen, ellipse.X, ellipse.Y, ellipse.Width, ellipse.Height);

                if (isSelected)
                {
                    handles.AddRange([
                        new(ellipse.X - offset, ellipse.Y - offset, size, size),
                new(ellipse.X + ellipse.Width - offset, ellipse.Y - offset, size, size),
                new(ellipse.X - offset, ellipse.Y + ellipse.Height - offset, size, size),
                new(ellipse.X + ellipse.Width - offset, ellipse.Y + ellipse.Height - offset, size, size)
                    ]);
                }

                g.Transform = originalTransform;

                Console.WriteLine($"Ellipse: x={ellipse.X} y={ellipse.Y} w={ellipse.Width} h={ellipse.Height}");
            }
            else if (shape is ShapePolygon polygon && polygon.Points.Count > 1)
            {
                if (polygon.IsClosed) { g.FillPolygon(brush, polygon.Points.ToArray()); g.DrawPolygon(pen, polygon.Points.ToArray()); }
                else { g.DrawLines(pen, polygon.Points.ToArray()); }

                if (isSelected)
                {
                    handles.AddRange(polygon.Points.Select(p => new RectangleF(p.X - offset, p.Y - offset, size, size)));

                    if (polygon.ActiveHandleIndicies != null)
                    {
                        foreach (int idx in polygon.ActiveHandleIndicies)
                        {
                            if (idx >= 0 && idx < polygon.Points.Count)
                            {
                                activeHandles.Add(new RectangleF(polygon.Points[idx].X - offset, polygon.Points[idx].Y - offset, size, size));
                            }
                        }
                    }
                }
            }
            else if (shape is ShapeText text)
            {
                Matrix originalTransform = g.Transform;

                using Font font = CreateScaledFont(text);
                ApplyRotation(g, text.X, text.Y, text.Width, text.Height, text.Rotation);
                g.DrawString(text.Content, font, brush, text.X, text.Y);

                if (isSelected)
                {
                    handles.AddRange([
                        new(text.X - offset, text.Y - offset, size, size),
                new(text.X + text.Width - offset, text.Y - offset, size, size),
                new(text.X - offset, text.Y + text.Height - offset, size, size),
                new(text.X + text.Width - offset, text.Y + text.Height - offset, size, size)
                    ]);
                }

                g.Transform = originalTransform;

                Console.WriteLine($"Text: {font.Size} x={text.X} y={text.Y}");
            }
            else if (shape is ShapePath path)
            {
                using GraphicsPath gPath = BuildGraphicsPath(path);
                g.FillPath(brush, gPath);
                g.DrawPath(pen, gPath);

                if (isSelected)
                {
                    int pointIndex = 0;
                    foreach (var segment in path.PathSegments)
                    {
                        foreach (var p in segment.InputPoints)
                        {
                            handles.Add(new RectangleF(p.X - offset, p.Y - offset, size, size));
                            pointIndex++;
                        }
                    }

                    if (path.ActiveHandleIndicies != null)
                    {
                        pointIndex = 0;
                        foreach (var segment in path.PathSegments)
                        {
                            foreach (var p in segment.InputPoints)
                            {
                                if (Array.IndexOf(path.ActiveHandleIndicies, pointIndex) >= 0)
                                {
                                    activeHandles.Add(new RectangleF(p.X - offset, p.Y - offset, size, size));
                                }
                                pointIndex++;
                            }
                        }
                    }
                }
            }

            return ([.. handles], [.. activeHandles]);
        }

        private static RectangleF GetShapeBounds(BaseShape shape, Matrix transform)
        {
            using GraphicsPath path = new();
            if (shape is ShapeRect r) path.AddRectangle(new RectangleF(r.X, r.Y, r.Width, r.Height));
            else if (shape is ShapeEllipse e) path.AddEllipse(new RectangleF(e.X, e.Y, e.Width, e.Height));
            else if (shape is ShapePolygon p && p.Points.Count > 1) path.AddPolygon(p.Points.ToArray());
            else if (shape is ShapeText t) path.AddRectangle(new RectangleF(t.X, t.Y, t.Width, t.Height));
            else if (shape is ShapePath sp && sp.PathSegments.Count > 0)
            {
                using var pth = BuildGraphicsPath(sp);
                if (pth.PointCount > 0)
                {
                    path.AddPath(pth, false);
                }
            }

            path.Transform(transform);
            return path.GetBounds();
        }

        private static void ApplyRotation(Graphics g, float x, float y, float w, float h, float rotation)
        {
            float centerX = x + w / 2;
            float centerY = y + h / 2;
            g.TranslateTransform(centerX, centerY);
            g.RotateTransform(rotation);
            g.TranslateTransform(-centerX, -centerY);
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

        public void RaiseShape(BaseShape shape)
        {
            int index = shapes.IndexOf(shape);
            if (index >= 0 && index < shapes.Count - 1)
            {
                shapes.RemoveAt(index);
                shapes.Insert(index + 1, shape);
                OnLayerChanged?.Invoke(GetBounds());
            }
        }

        public void LowerShape(BaseShape shape)
        {
            int index = shapes.IndexOf(shape);
            if (index > 0)
            {
                shapes.RemoveAt(index);
                shapes.Insert(index - 1, shape);
                OnLayerChanged?.Invoke(GetBounds());
            }
        }

        public void ShapeToTop(BaseShape shape)
        {
            int index = shapes.IndexOf(shape);
            if (index >= 0 && index < shapes.Count - 1)
            {
                shapes.RemoveAt(index);
                shapes.Add(shape);
                OnLayerChanged?.Invoke(GetBounds());
            }
        }

        public void ShapeToBottom(BaseShape shape)
        {
            int index = shapes.IndexOf(shape);
            if (index > 0)
            {
                shapes.RemoveAt(index);
                shapes.Insert(0, shape);
                OnLayerChanged?.Invoke(GetBounds());
            }
        }

        public void DuplicateShape(BaseShape shape)
        {
            BaseShape? duplicate = null;

            if (shape is ShapeRect rect)
            {
                duplicate = new ShapeRect(rect.X + 10, rect.Y + 10, rect.Width, rect.Height)
                {
                    LineColor = rect.LineColor,
                    LineWidth = rect.LineWidth,
                    FillColor = rect.FillColor,
                    DashStyle = rect.DashStyle
                };
            }
            else if (shape is ShapeEllipse ellipse)
            {
                duplicate = new ShapeEllipse(ellipse.X + 10, ellipse.Y + 10, ellipse.Width, ellipse.Height)
                {
                    LineColor = ellipse.LineColor,
                    LineWidth = ellipse.LineWidth,
                    FillColor = ellipse.FillColor,
                    DashStyle = ellipse.DashStyle
                };
            }
            else if (shape is ShapePolygon polygon)
            {
                List<PointF> points = [];
                foreach (var p in polygon.Points)
                {
                    points.Add(new PointF(p.X + 10, p.Y + 10));
                }
                duplicate = new ShapePolygon(points, polygon.IsClosed)
                {
                    LineColor = polygon.LineColor,
                    LineWidth = polygon.LineWidth,
                    FillColor = polygon.FillColor,
                    DashStyle = polygon.DashStyle
                };
            }
            else if (shape is ShapeText text)
            {
                duplicate = new ShapeText(text.Content, text.X + 10, text.Y + 10, text.Width, text.Height, text.FontFamily, text.FontSize)
                {
                    IsBold = text.IsBold,
                    IsItalic = text.IsItalic,
                    LineColor = text.LineColor,
                    LineWidth = text.LineWidth,
                    FillColor = text.FillColor,
                    DashStyle = text.DashStyle
                };
            }
            else if (shape is ShapePath path)
            {
                List<PathSegment> segments = [];
                foreach (var segment in path.PathSegments)
                {
                    List<PointF> offsetPoints = [];
                    foreach (var p in segment.InputPoints)
                    {
                        offsetPoints.Add(new PointF(p.X + 10, p.Y + 10));
                    }
                    segments.Add(new PathSegment(segment.PathType, offsetPoints));
                }
                duplicate = new ShapePath(segments)
                {
                    LineColor = path.LineColor,
                    LineWidth = path.LineWidth,
                    FillColor = path.FillColor,
                    DashStyle = path.DashStyle
                };
            }

            if (duplicate != null)
            {
                int index = shapes.IndexOf(shape);
                shapes.Insert(index + 1, duplicate);
                OnLayerChanged?.Invoke(GetBounds());
            }
        }

        public void FlipShapeHorizomtally(BaseShape shape)
        {
            if (shape is ShapeRect rect)
            {
                rect.X += rect.Width;
                rect.Width = -rect.Width;
            }
            else if (shape is ShapeEllipse ellipse)
            {
                ellipse.X += ellipse.Width;
                ellipse.Width = -ellipse.Width;
            }
            else if (shape is ShapePolygon polygon)
            {
                float minX = polygon.Points.Min(p => p.X);
                float maxX = polygon.Points.Max(p => p.X);
                float centerX = (minX + maxX) / 2;

                for (int i = 0; i < polygon.Points.Count; i++)
                {
                    polygon.Points[i] = new PointF(2 * centerX - polygon.Points[i].X, polygon.Points[i].Y);
                }
            }
            else if (shape is ShapeText text)
            {
                text.X += text.Width;
                text.Width = -text.Width;
            }
            else if (shape is ShapePath path)
            {
                float minX = float.MaxValue;
                float maxX = float.MinValue;

                foreach (var segment in path.PathSegments)
                {
                    foreach (var p in segment.InputPoints)
                    {
                        minX = Math.Min(minX, p.X);
                        maxX = Math.Max(maxX, p.X);
                    }
                }

                float centerX = (minX + maxX) / 2;

                foreach (var segment in path.PathSegments)
                {
                    for (int i = 0; i < segment.InputPoints.Count; i++)
                    {
                        segment.InputPoints[i] = new PointF(2 * centerX - segment.InputPoints[i].X, segment.InputPoints[i].Y);
                    }
                }
            }

            OnLayerChanged?.Invoke(GetBounds());
        }

        public void FlipShapeVertically(BaseShape shape)
        {
            if (shape is ShapeRect rect)
            {
                rect.Y += rect.Height;
                rect.Height = -rect.Height;
            }
            else if (shape is ShapeEllipse ellipse)
            {
                ellipse.Y += ellipse.Height;
                ellipse.Height = -ellipse.Height;
            }
            else if (shape is ShapePolygon polygon)
            {
                float minY = polygon.Points.Min(p => p.Y);
                float maxY = polygon.Points.Max(p => p.Y);
                float centerY = (minY + maxY) / 2;

                for (int i = 0; i < polygon.Points.Count; i++)
                {
                    polygon.Points[i] = new PointF(polygon.Points[i].X, 2 * centerY - polygon.Points[i].Y);
                }
            }
            else if (shape is ShapeText text)
            {
                text.Y += text.Height;
                text.Height = -text.Height;
            }
            else if (shape is ShapePath path)
            {
                float minY = float.MaxValue;
                float maxY = float.MinValue;

                foreach (var segment in path.PathSegments)
                {
                    foreach (var p in segment.InputPoints)
                    {
                        minY = Math.Min(minY, p.Y);
                        maxY = Math.Max(maxY, p.Y);
                    }
                }

                float centerY = (minY + maxY) / 2;

                foreach (var segment in path.PathSegments)
                {
                    for (int i = 0; i < segment.InputPoints.Count; i++)
                    {
                        segment.InputPoints[i] = new PointF(segment.InputPoints[i].X, 2 * centerY - segment.InputPoints[i].Y);
                    }
                }
            }

            OnLayerChanged?.Invoke(GetBounds());
        }

        public void RotateShape(BaseShape shape, float angle)
        {
            float radians = (float)(angle * Math.PI / 180);
            float cos = (float)Math.Cos(radians);
            float sin = (float)Math.Sin(radians);

            if (shape is ShapeRect rect)
            {
                rect.Rotation = (rect.Rotation + angle) % 360;
                if (rect.Rotation < 0) rect.Rotation += 360;
            }
            else if (shape is ShapeEllipse ellipse)
            {
                ellipse.Rotation = (ellipse.Rotation + angle) % 360;
                if (ellipse.Rotation < 0) ellipse.Rotation += 360;
            }
            else if (shape is ShapePolygon polygon)
            {
                float centerX = polygon.Points.Average(p => p.X);
                float centerY = polygon.Points.Average(p => p.Y);

                for (int i = 0; i < polygon.Points.Count; i++)
                {
                    polygon.Points[i] = RotatePoint(polygon.Points[i], new PointF(centerX, centerY), cos, sin);
                }
            }
            else if (shape is ShapeText text)
            {
                text.Rotation = (text.Rotation + angle) % 360;
                if (text.Rotation < 0) text.Rotation += 360;
            }
            else if (shape is ShapePath path)
            {
                float centerX = 0;
                float centerY = 0;
                int pointCount = 0;

                foreach (var segment in path.PathSegments)
                {
                    foreach (var p in segment.InputPoints)
                    {
                        centerX += p.X;
                        centerY += p.Y;
                        pointCount++;
                    }
                }

                if (pointCount > 0)
                {
                    centerX /= pointCount;
                    centerY /= pointCount;

                    foreach (var segment in path.PathSegments)
                    {
                        for (int i = 0; i < segment.InputPoints.Count; i++)
                        {
                            segment.InputPoints[i] = RotatePoint(segment.InputPoints[i], new PointF(centerX, centerY), cos, sin);
                        }
                    }
                }
            }

            OnLayerChanged?.Invoke(GetBounds());
        }

        private static PointF RotatePoint(PointF point, PointF center, float cos, float sin)
        {
            float dx = point.X - center.X;
            float dy = point.Y - center.Y;
            return new PointF(center.X + dx * cos - dy * sin, center.Y + dx * sin + dy * cos);
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