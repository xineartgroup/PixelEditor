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
        private bool _useImageCache = false;
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
        private List<ImageAdjustment> adjustments = [];

        public event Action<Rectangle>? OnLayerChanged;

        public static LayerSelectionType LayerSelectionType { get; set; } = LayerSelectionType.Shape;

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

        public bool UseImageCache { get => _useImageCache; set => SetProperty(ref _useImageCache, value); }

        public LayerType LayerType { get => _layerType; set => SetProperty(ref _layerType, value); }

        public LayerChannel Channel { get => _channel; set => SetProperty(ref _channel, value); }

        public ImageBlending BlendMode { get => _blendMode; set => SetProperty(ref _blendMode, value); }

        public FillType FillType { get => _fillType; set => SetProperty(ref _fillType, value); }

        public Color FillColor { get => _fillColor; set => SetProperty(ref _fillColor, value); }

        public List<ImageAdjustment> Adjustments { get => adjustments; set => SetProperty(ref adjustments, value); }

        public List<BaseShape> Shapes { get => shapes; set => SetProperty(ref shapes, value); }

        public List<BaseShape> AddedShapeSelections { get => addedShapeSelections; set => SetProperty(ref addedShapeSelections, value); }

        public BaseShape? CurrentShape { get => currentShape; set => SetProperty(ref currentShape, value); }

        public bool IsNewShape(BaseShape shape)
        {
            return !shapes.Contains(shape);
        }

        private Image? GetImageComposite()
        {
            if (LayerType == LayerType.Vector)
            {
                if (!_useImageCache)
                {
                    Console.WriteLine("Getting vector image composite");
                    _image = DrawVectorImage(new Bitmap(Document.Width, Document.Height));
                }
                return _image;
            }
            else
            {
                Image? image = null;

                if (_imageMask == null)
                {
                    image = _image;
                }
                else
                {
                    if (_image != null)
                    {
                        image = ManipulatorLighting.MaskImage((Bitmap)_image, (Bitmap)_imageMask);
                    }
                }

                if (image != null)
                {
                    foreach (var adjustment in adjustments)
                    {
                        if (!adjustment.IsActive)
                            continue;

                        if (adjustment.Name == "Brightness")
                        {
                            float brightness = adjustment.Values[0];
                            image = ManipulatorLighting.ApplyLighting(image, brightness, 1, 1, 1, 1, 0);
                        }
                        else if (adjustment.Name == "Contrast")
                        {
                            float contrast = adjustment.Values[0];
                            image = ManipulatorLighting.ApplyLighting(image, 1, contrast, 1, 1, 1, 0);
                        }
                        else if (adjustment.Name == "Exposure")
                        {
                            float exposure = adjustment.Values[0];
                            image = ManipulatorLighting.ApplyLighting(image, 1, 1, exposure, 1, 1, 0);
                        }
                        else if (adjustment.Name == "Highlights")
                        {
                            float highlights = adjustment.Values[0];
                            image = ManipulatorLighting.ApplyLighting(image, 1, 1, 1, 1, highlights, 0);
                        }
                        else if (adjustment.Name == "Shadows")
                        {
                            float shadows = adjustment.Values[0];
                            image = ManipulatorLighting.ApplyLighting(image, 1, 1, 1, shadows, 1, 0);
                        }
                        else if (adjustment.Name == "Vignette")
                        {
                            float vignetteStrength = adjustment.Values[0];
                            image = ManipulatorLighting.ApplyLighting(image, 1, 1, 1, 1, 1, vignetteStrength);
                        }
                        else if (adjustment.Name == "Saturation")
                        {
                            float saturation = adjustment.Values[0];
                            image = ManipulatorLighting.AdjustColorBalance(image, saturation, 0, 0);
                        }
                        else if (adjustment.Name == "Warmth")
                        {
                            float warmth = adjustment.Values[0];
                            image = ManipulatorLighting.AdjustColorBalance(image, 0, warmth, 0);
                        }
                        else if (adjustment.Name == "Tint")
                        {
                            float tint = adjustment.Values[0];
                            image = ManipulatorLighting.AdjustColorBalance(image, 0, 0, tint);
                        }
                        else if (adjustment.Name == "Sharpness")
                        {
                            float sharpness = adjustment.Values[0] * 100;
                            image = ManipulatorLighting.AdjustSharpness(image, sharpness);
                        }
                        else if (adjustment.Name == "Blur")
                        {
                            float sizeX = adjustment.Values[0] * 100;
                            float sizeY = adjustment.Values[1] * 100;
                            if (sizeX == 0) sizeX = 1;
                            if (sizeY == 0) sizeY = 1;
                            image = ManipulatorLighting.GaussianBlur(image, (int)sizeX, (int)sizeY);
                        }
                    }
                }

                return image;
            }
        }

        private Image? DrawVectorImage(Image temp)
        {
            Image? image = new Bitmap(temp);
            using (Graphics g = Graphics.FromImage(image))
            {
                g.SmoothingMode = SmoothingMode.None;
                g.InterpolationMode = InterpolationMode.Low;
                g.PixelOffsetMode = PixelOffsetMode.None;

                List<(RectangleF[] Handles, Matrix Transform)> handlesToDraw = [];
                List<(RectangleF[] ControlHandles, Matrix Transform)> controlHandlesToDraw = [];
                List<(RectangleF[] ActiveHandles, Matrix Transform)> activeHandlesToDraw = [];
                List<(RectangleF[] RotationHandles, Matrix Transform, PointF Center)> rotationHandlesToDraw = [];
                List<(RectangleF[] BezierHandles, Matrix Transform)> bezierHandlesToDraw = [];
                List<((PointF Start, PointF End)[] Lines, Matrix Transform)> bezierLinesToDraw = [];
                bool foundSelected = false;
                RectangleF groupBounds = RectangleF.Empty;

                foreach (BaseShape shape in shapes)
                {
                    if (shape.Visible)
                    {
                        bool isCurrent = (shape == currentShape);
                        if (isCurrent) foundSelected = true;

                        var (handles, controlHandles, activeHandles, rotationHandles, bezierHandles, bezierLines, center) = DrawShape(shape, g, isCurrent);

                        if (isCurrent && handles.Length > 0)
                            handlesToDraw.Add((handles, g.Transform.Clone()));

                        if (isCurrent && controlHandles.Length > 0)
                            controlHandlesToDraw.Add((controlHandles, g.Transform.Clone()));

                        if (isCurrent && activeHandles.Length > 0)
                            activeHandlesToDraw.Add((activeHandles, g.Transform.Clone()));

                        if (isCurrent && rotationHandles.Length > 0)
                            rotationHandlesToDraw.Add((rotationHandles, g.Transform.Clone(), center));

                        if (isCurrent && bezierHandles.Length > 0)
                            bezierHandlesToDraw.Add((bezierHandles, g.Transform.Clone()));

                        if (isCurrent && bezierLines.Length > 0)
                            bezierLinesToDraw.Add((bezierLines, g.Transform.Clone()));

                        if (addedShapeSelections.Contains(shape) || shape == currentShape)
                        {
                            RectangleF bounds = GetShapeBounds(shape, g.Transform);
                            groupBounds = groupBounds.IsEmpty ? bounds : RectangleF.Union(groupBounds, bounds);
                        }
                    }
                }

                if (currentShape != null && !foundSelected)
                {
                    var (handles, controlHandles, activeHandles, rotationHandles, bezierHandles, bezierLines, center) = DrawShape(currentShape, g, true);
                    if (handles.Length > 0)
                        handlesToDraw.Add((handles, g.Transform.Clone()));
                    if (controlHandles.Length > 0)
                        controlHandlesToDraw.Add((controlHandles, g.Transform.Clone()));
                    if (activeHandles.Length > 0)
                        activeHandlesToDraw.Add((activeHandles, g.Transform.Clone()));
                    if (rotationHandles.Length > 0)
                        rotationHandlesToDraw.Add((rotationHandles, g.Transform.Clone(), center));
                    if (bezierHandles.Length > 0)
                        bezierHandlesToDraw.Add((bezierHandles, g.Transform.Clone()));
                    if (bezierLines.Length > 0)
                        bezierLinesToDraw.Add((bezierLines, g.Transform.Clone()));
                }

                int penSize = Math.Max(Document.Width, Document.Height) < 800 ? 1 : 2;

                using SolidBrush handleBrush = new(Color.LightBlue);
                using SolidBrush controlHandleBrush = new(Color.White);
                using SolidBrush activeHandleBrush = new(Color.Red);
                using SolidBrush rotateBrush = new(Color.Gold);
                using SolidBrush bezierHandleBrush = new(Color.LightGreen);
                using Pen handlePen = new(Color.Red, penSize);
                using Pen bezierPen = new(Color.Green, penSize);
                using Pen rotationPen = new(Color.White, penSize);
                using Pen bezierLinePen = new(Color.Gray, penSize) { DashStyle = DashStyle.Dash };

                foreach (var (Lines, Transform) in bezierLinesToDraw)
                {
                    Matrix original = g.Transform;
                    g.Transform = Transform;
                    foreach (var (Start, End) in Lines)
                    {
                        g.DrawLine(bezierLinePen, Start, End);
                    }
                    g.Transform = original;
                    Transform.Dispose();
                }

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

                foreach (var (Handles, Transform) in controlHandlesToDraw)
                {
                    Matrix original = g.Transform;
                    g.Transform = Transform;
                    foreach (var handle in Handles)
                    {
                        g.FillRectangle(controlHandleBrush, handle);
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

                foreach (var (BezierHandles, Transform) in bezierHandlesToDraw)
                {
                    Matrix original = g.Transform;
                    g.Transform = Transform;
                    foreach (var handle in BezierHandles)
                    {
                        g.FillEllipse(bezierHandleBrush, handle);
                        g.DrawEllipse(bezierPen, handle);
                    }
                    g.Transform = original;
                    Transform.Dispose();
                }

                foreach (var (RotationHandles, Transform, Center) in rotationHandlesToDraw)
                {
                    Matrix original = g.Transform;
                    g.Transform = Transform;

                    float handleCenterY = Center.Y - SelectionsManipulator.ROTATION_HANDLE_SIZE;
                    g.DrawLine(rotationPen, Center.X, Center.Y, Center.X, handleCenterY);

                    foreach (var handle in RotationHandles)
                    {
                        g.FillEllipse(rotateBrush, handle);
                        g.DrawEllipse(rotationPen, handle);
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

        public static (RectangleF[] Handles, RectangleF[] ControlHandles, RectangleF[] ActiveHandles, RectangleF[] RotationHandles, RectangleF[] BezierHandles, (PointF Start, PointF End)[] BezierLines, PointF Center) DrawShape(BaseShape shape, Graphics g, bool isSelected = false)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            using Pen pen = new(shape.LineColor, shape.LineWidth);
            using SolidBrush brush = new(shape.FillColor);
            pen.DashStyle = shape.DashStyle;

            int size = Math.Max(Document.Width, Document.Height) < 400 ? 2 : Math.Max(Document.Width, Document.Height) < 800 ? 4 : Math.Max(Document.Width, Document.Height) < 1200 ? 6 : 12;
            float offset = size / 2f;

            List<RectangleF> handles = [];
            List<RectangleF> controlHandles = [];
            List<RectangleF> activeHandles = [];
            List<RectangleF> rotationHandles = [];
            List<RectangleF> bezierHandles = [];
            List<(PointF Start, PointF End)> bezierLines = [];
            PointF center = PointF.Empty;

            if (isSelected)
            {
                RectangleF bounds = GetShapeBounds(shape, new Matrix());
                handles.AddRange([
                    new(bounds.X - offset, bounds.Y - offset, size, size),
                    new(bounds.X + bounds.Width - offset, bounds.Y - offset, size, size),
                    new(bounds.X - offset, bounds.Y + bounds.Height - offset, size, size),
                    new(bounds.X + bounds.Width - offset, bounds.Y + bounds.Height - offset, size, size)
                ]);

                center = new PointF(bounds.X + bounds.Width / 2, bounds.Y + bounds.Height / 2);

                float rotationHandleSize = SelectionsManipulator.ROTATION_HANDLE_SIZE;
                float rotOffset = rotationHandleSize / 2f;
                rotationHandles.Add(new RectangleF(
                    center.X - rotOffset,
                    center.Y - SelectionsManipulator.ROTATION_HANDLE_SIZE - rotOffset,
                    rotationHandleSize,
                    rotationHandleSize
                ));
            }

            int penSize = Math.Max(Document.Width, Document.Height) < 800 ? 1 : 2;

            using SolidBrush controlHandleBrush = new(Color.White);
            using SolidBrush activeHandleBrush = new(Color.Red);
            using Pen handlePen = new(Color.Red, penSize);

            if (shape is ShapeRect rect)
            {
                Matrix originalTransform = g.Transform;
                ApplyRotation(g, rect.X, rect.Y, rect.Width, rect.Height, rect.Rotation, rect.HasCustomRotationCenter, rect.RotationCenterX, rect.RotationCenterY);
                g.FillRectangle(brush, rect.X, rect.Y, rect.Width, rect.Height);
                g.DrawRectangle(pen, rect.X, rect.Y, rect.Width, rect.Height);
                g.Transform = originalTransform;
            }
            else if (shape is ShapeEllipse ellipse)
            {
                Matrix originalTransform = g.Transform;
                ApplyRotation(g, ellipse.X, ellipse.Y, ellipse.Width, ellipse.Height, ellipse.Rotation, ellipse.HasCustomRotationCenter, ellipse.RotationCenterX, ellipse.RotationCenterY);
                g.FillEllipse(brush, ellipse.X, ellipse.Y, ellipse.Width, ellipse.Height);
                g.DrawEllipse(pen, ellipse.X, ellipse.Y, ellipse.Width, ellipse.Height);
                g.Transform = originalTransform;
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

                if (isSelected && LayerSelectionType == LayerSelectionType.Point)
                {
                    foreach (var p in polygon.Points)
                    {
                        var hRect = new RectangleF(p.X - offset, p.Y - offset, size, size);
                        controlHandles.Add(hRect);
                        g.FillRectangle(controlHandleBrush, hRect);
                        g.DrawRectangle(handlePen, hRect.X, hRect.Y, hRect.Width, hRect.Height);
                    }

                    if (polygon.ActiveHandleIndicies != null)
                    {
                        foreach (int idx in polygon.ActiveHandleIndicies)
                        {
                            if (idx >= 0 && idx < polygon.Points.Count)
                            {
                                var hRect = new RectangleF(polygon.Points[idx].X - offset, polygon.Points[idx].Y - offset, size, size);
                                activeHandles.Add(hRect);
                                g.FillRectangle(activeHandleBrush, hRect);
                                g.DrawRectangle(handlePen, hRect.X, hRect.Y, hRect.Width, hRect.Height);
                            }
                        }
                    }
                }
            }
            else if (shape is ShapePath path)
            {
                using GraphicsPath gPath = BuildGraphicsPath(path);
                g.FillPath(brush, gPath);
                g.DrawPath(pen, gPath);

                if (isSelected && LayerSelectionType == LayerSelectionType.Point)
                {
                    int pointIndex = 0;
                    foreach (var segment in path.PathSegments)
                    {
                        string type = segment.PathType.ToUpper();
                        bool isBezier = (type == "C" || type == "Q");

                        for (int i = 0; i < segment.InputPoints.Count; i++)
                        {
                            var p = segment.InputPoints[i];
                            var hRect = new RectangleF(p.X - offset, p.Y - offset, size, size);

                            bool isBezierControlPoint = isBezier && ((type == "C" && (i == 1 || i == 2)) || (type == "Q" && i == 1));

                            if (isBezierControlPoint)
                            {
                                bezierHandles.Add(hRect);

                                if (type == "C" && i == 1 && segment.InputPoints.Count >= 4)
                                    bezierLines.Add((segment.InputPoints[0], p));
                                else if (type == "C" && i == 2 && segment.InputPoints.Count >= 4)
                                    bezierLines.Add((segment.InputPoints[3], p));
                                else if (type == "Q" && i == 1 && segment.InputPoints.Count >= 3)
                                {
                                    bezierLines.Add((segment.InputPoints[0], p));
                                    bezierLines.Add((segment.InputPoints[2], p));
                                }
                            }
                            else
                            {
                                controlHandles.Add(hRect);
                                g.FillRectangle(controlHandleBrush, hRect);
                                g.DrawRectangle(handlePen, hRect.X, hRect.Y, hRect.Width, hRect.Height);
                            }

                            if (path.ActiveHandleIndicies != null && Array.IndexOf(path.ActiveHandleIndicies, pointIndex) >= 0)
                            {
                                activeHandles.Add(hRect);
                                g.FillRectangle(activeHandleBrush, hRect);
                                g.DrawRectangle(handlePen, hRect.X, hRect.Y, hRect.Width, hRect.Height);
                            }
                            pointIndex++;
                        }
                    }
                }
            }
            else if (shape is ShapeText text)
            {
                if (text.Width > 0 && text.Height > 0 && !string.IsNullOrEmpty(text.Content))
                {
                    //Console.WriteLine($"=== DrawShape Text Debug ===");
                    //Console.WriteLine($"Content: '{text.Content}'");
                    //Console.WriteLine($"Position: ({text.X}, {text.Y})");
                    //Console.WriteLine($"Size: {text.Width} x {text.Height}");
                    //Console.WriteLine($"FontSize: {text.FontSize}");
                    //Console.WriteLine($"TransformScale: {text.TransformScale}");
                    //Console.WriteLine($"Rotation: {text.Rotation}");
                    //Console.WriteLine($"Measurement Unit: {text.MeasurementUnit}");
                    //Console.WriteLine($"============================");

                    Matrix originalTransform = g.Transform;
                    ApplyRotation(g, text.X, text.Y, text.Width, text.Height, text.Rotation, text.HasCustomRotationCenter, text.RotationCenterX, text.RotationCenterY);

                    RectangleF layoutRect = new(text.X, text.Y, text.Width, text.Height);

                    using (GraphicsPath textPath = new())
                    {
                        FontStyle style = GetFontStyle(text);

                        float renderFontSize = text.FontSize;

                        using (Font font = new (text.FontFamily, renderFontSize, style))
                        {
                            using (GraphicsPath measurePath = new())
                            {
                                measurePath.AddString(text.Content, font.FontFamily, (int)font.Style,
                                                      font.SizeInPoints, new PointF(0, 0),
                                                      StringFormat.GenericDefault);
                                RectangleF measureBounds = measurePath.GetBounds();
                                //Console.WriteLine($"MeasureBounds at {renderFontSize}pt: X={measureBounds.X}, Y={measureBounds.Y}, W={measureBounds.Width}, H={measureBounds.Height}");
                            }

                            textPath.AddString(
                                text.Content,
                                font.FontFamily,
                                (int)font.Style,
                                font.SizeInPoints,
                                new PointF(0, 0),
                                StringFormat.GenericDefault
                            );
                        }

                        RectangleF textBounds = textPath.GetBounds();

                        //Console.WriteLine($"=== TextBounds Debug ===");
                        //Console.WriteLine($"Content: '{text.Content}'");
                        //Console.WriteLine($"Font size: {renderFontSize}pt");
                        //Console.WriteLine($"Font family: {text.FontFamily}");
                        //Console.WriteLine($"TextBounds: X={textBounds.X}, Y={textBounds.Y}, W={textBounds.Width}, H={textBounds.Height}");
                        //Console.WriteLine($"Target rect: X={text.X}, Y={text.Y}, W={text.Width}, H={text.Height}");
                        //Console.WriteLine($"ScaleX needed: {text.Width / textBounds.Width}");
                        //Console.WriteLine($"ScaleY needed: {text.Height / textBounds.Height}");
                        //Console.WriteLine($"Graphics DPI: {g.DpiX}, {g.DpiY}");
                        //Console.WriteLine($"=========================");

                        if (textBounds.Width > 0 && textBounds.Height > 0)
                        {
                            PointF[] destPoints =
                            [
                                new PointF(layoutRect.Left, layoutRect.Top),
                                new PointF(layoutRect.Right, layoutRect.Top),
                                new PointF(layoutRect.Left, layoutRect.Bottom)
                            ];

                            using Matrix stretchMatrix = new(textBounds, destPoints);
                            textPath.Transform(stretchMatrix);
                        }

                        g.FillPath(brush, textPath);
                        g.DrawPath(pen, textPath);
                    }

                    g.Transform = originalTransform;
                }
            }

            return ([.. handles], [.. controlHandles], [.. activeHandles], [.. rotationHandles], [.. bezierHandles], [.. bezierLines], center);
        }

        public static RectangleF GetShapeBounds(BaseShape shape, Matrix transform)
        {
            using GraphicsPath path = new();
            if (shape is ShapeRect r)
            {
                path.AddRectangle(new RectangleF(r.X, r.Y, r.Width, r.Height));
            }
            else if (shape is ShapeEllipse e)
            {
                path.AddEllipse(new RectangleF(e.X, e.Y, e.Width, e.Height));
            }
            else if (shape is ShapePolygon p && p.Points.Count > 1)
            {
                if (p.Points.Count > 2)
                {
                    path.AddPolygon(p.Points.ToArray());
                }
            }
            else if (shape is ShapeText t)
            {
                path.AddRectangle(new RectangleF(t.X, t.Y, t.Width, t.Height));
            }
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

        private static void ApplyRotation(Graphics g, float x, float y, float w, float h, float rotation,
            bool hasCustomCenter = false, float customCx = 0, float customCy = 0)
        {
            float centerX = hasCustomCenter ? customCx : x + w / 2;
            float centerY = hasCustomCenter ? customCy : y + h / 2;
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
                        if (pts.Count >= 2)
                        {
                            gPath.StartFigure();
                            currentPoint = pts[1];
                        }
                        else if (pts.Count == 1)
                        {
                            gPath.StartFigure();
                            currentPoint = pts[0];
                        }
                        break;

                    case "L":
                        if (pts.Count >= 2)
                        {
                            gPath.AddLine(pts[0], pts[1]);
                            currentPoint = pts[1];
                        }
                        else if (pts.Count == 1)
                        {
                            gPath.AddLine(currentPoint, pts[0]);
                            currentPoint = pts[0];
                        }
                        break;

                    case "H":
                        if (pts.Count >= 2)
                        {
                            PointF dest = new(pts[1].X, pts[0].Y);
                            gPath.AddLine(pts[0], dest);
                            currentPoint = dest;
                        }
                        else if (pts.Count == 1)
                        {
                            PointF dest = new(pts[0].X, currentPoint.Y);
                            gPath.AddLine(currentPoint, dest);
                            currentPoint = dest;
                        }
                        break;

                    case "V":
                        if (pts.Count >= 2)
                        {
                            PointF dest = new(pts[0].X, pts[1].Y);
                            gPath.AddLine(pts[0], dest);
                            currentPoint = dest;
                        }
                        else if (pts.Count == 1)
                        {
                            PointF dest = new(currentPoint.X, pts[0].Y);
                            gPath.AddLine(currentPoint, dest);
                            currentPoint = dest;
                        }
                        break;

                    case "C":
                        if (pts.Count >= 4)
                        {
                            // Carried point stored: pts[0]=start, pts[1]=cp1, pts[2]=cp2, pts[3]=end
                            gPath.AddBezier(pts[0], pts[1], pts[2], pts[3]);
                            currentPoint = pts[3];
                        }
                        else if (pts.Count == 3)
                        {
                            // No carried point: pts[0]=cp1, pts[1]=cp2, pts[2]=end
                            gPath.AddBezier(currentPoint, pts[0], pts[1], pts[2]);
                            currentPoint = pts[2];
                        }
                        break;

                    case "S":
                        if (pts.Count >= 4)
                        {
                            gPath.AddBezier(pts[0], pts[1], pts[2], pts[3]);
                            currentPoint = pts[3];
                        }
                        else if (pts.Count == 3)
                        {
                            gPath.AddBezier(currentPoint, pts[0], pts[1], pts[2]);
                            currentPoint = pts[2];
                        }
                        break;

                    case "Q":
                        if (pts.Count >= 3)
                        {
                            // Carried point stored: pts[0]=start, pts[1]=cp1, pts[2]=end
                            PointF cp1 = new((pts[0].X + 2 * pts[1].X) / 3, (pts[0].Y + 2 * pts[1].Y) / 3);
                            PointF cp2 = new((pts[2].X + 2 * pts[1].X) / 3, (pts[2].Y + 2 * pts[1].Y) / 3);
                            gPath.AddBezier(pts[0], cp1, cp2, pts[2]);
                            currentPoint = pts[2];
                        }
                        else if (pts.Count == 2)
                        {
                            // No carried point: pts[0]=cp1, pts[1]=end
                            PointF cp1 = new((currentPoint.X + 2 * pts[0].X) / 3, (currentPoint.Y + 2 * pts[0].Y) / 3);
                            PointF cp2 = new((pts[1].X + 2 * pts[0].X) / 3, (pts[1].Y + 2 * pts[0].Y) / 3);
                            gPath.AddBezier(currentPoint, cp1, cp2, pts[1]);
                            currentPoint = pts[1];
                        }
                        break;

                    case "T":
                        if (pts.Count >= 3)
                        {
                            PointF cp1 = new((pts[0].X + 2 * pts[1].X) / 3, (pts[0].Y + 2 * pts[1].Y) / 3);
                            PointF cp2 = new((pts[2].X + 2 * pts[1].X) / 3, (pts[2].Y + 2 * pts[1].Y) / 3);
                            gPath.AddBezier(pts[0], cp1, cp2, pts[2]);
                            currentPoint = pts[2];
                        }
                        else if (pts.Count == 2)
                        {
                            PointF cp1 = new((currentPoint.X + 2 * pts[0].X) / 3, (currentPoint.Y + 2 * pts[0].Y) / 3);
                            PointF cp2 = new((pts[1].X + 2 * pts[0].X) / 3, (pts[1].Y + 2 * pts[0].Y) / 3);
                            gPath.AddBezier(currentPoint, cp1, cp2, pts[1]);
                            currentPoint = pts[1];
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

        public static FontStyle GetFontStyle(ShapeText text)
        {
            FontStyle style = FontStyle.Regular;
            if (text.IsBold) style |= FontStyle.Bold;
            if (text.IsItalic) style |= FontStyle.Italic;
            return style;
        }

        private void SetProperty<T>(ref T field, T value)
        {
            if (_useImageCache) _useImageCache = false;

            if (Equals(field, value)) return;

            T oldValue = field;
            field = value;

            if (!Equals(oldValue, value))
            {
                OnLayerChanged?.Invoke(GetBounds());
            }
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

        public static BaseShape? GetDuplicate(BaseShape shape, int spacing = 0)
        {
            if (shape is ShapeRect rect)
            {
                return new ShapeRect(rect.X + spacing, rect.Y + spacing, rect.Width, rect.Height)
                {
                    Rx = rect.Rx,
                    Ry = rect.Ry,
                    LineColor = rect.LineColor,
                    LineWidth = rect.LineWidth,
                    FillColor = rect.FillColor,
                    DashStyle = rect.DashStyle,
                    RotationCenterX = rect.RotationCenterX,
                    RotationCenterY = rect.RotationCenterY,
                    HasCustomRotationCenter = rect.HasCustomRotationCenter,
                };
            }
            else if (shape is ShapeEllipse ellipse)
            {
                return new ShapeEllipse(ellipse.X + spacing, ellipse.Y + spacing, ellipse.Width, ellipse.Height)
                {
                    LineColor = ellipse.LineColor,
                    LineWidth = ellipse.LineWidth,
                    FillColor = ellipse.FillColor,
                    DashStyle = ellipse.DashStyle,
                    RotationCenterX = ellipse.RotationCenterX,
                    RotationCenterY = ellipse.RotationCenterY,
                    HasCustomRotationCenter = ellipse.HasCustomRotationCenter,
                };
            }
            else if (shape is ShapePolygon polygon)
            {
                List<PointF> points = [];
                foreach (var p in polygon.Points)
                {
                    points.Add(new PointF(p.X + spacing, p.Y + spacing));
                }
                return new ShapePolygon(points, polygon.IsClosed)
                {
                    LineColor = polygon.LineColor,
                    LineWidth = polygon.LineWidth,
                    FillColor = polygon.FillColor,
                    DashStyle = polygon.DashStyle,
                    RotationCenterX = polygon.RotationCenterX,
                    RotationCenterY = polygon.RotationCenterY,
                    HasCustomRotationCenter = polygon.HasCustomRotationCenter,
                };
            }
            else if (shape is ShapeText text)
            {
                return new ShapeText(text.Content, text.X + spacing, text.Y + spacing, text.Width, text.Height, text.FontFamily, text.FontSize, text.MeasurementUnit)
                {
                    IsBold = text.IsBold,
                    IsItalic = text.IsItalic,
                    LineColor = text.LineColor,
                    LineWidth = text.LineWidth,
                    FillColor = text.FillColor,
                    DashStyle = text.DashStyle,
                    FontSize = text.FontSize,
                    FontFamily = text.FontFamily,
                    MeasurementUnit = text.MeasurementUnit,
                    RotationCenterX = text.RotationCenterX,
                    RotationCenterY = text.RotationCenterY,
                    HasCustomRotationCenter = text.HasCustomRotationCenter,
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
                        offsetPoints.Add(new PointF(p.X + spacing, p.Y + spacing));
                    }
                    segments.Add(new PathSegment(segment.PathType, offsetPoints));
                }
                return new ShapePath(segments)
                {
                    LineColor = path.LineColor,
                    LineWidth = path.LineWidth,
                    FillColor = path.FillColor,
                    DashStyle = path.DashStyle,
                    RotationCenterX = path.RotationCenterX,
                    RotationCenterY = path.RotationCenterY,
                    HasCustomRotationCenter = path.HasCustomRotationCenter,
                };
            }

            return null;
        }

        public void DuplicateShape(BaseShape shape)
        {
            BaseShape? duplicate = GetDuplicate(shape, 10);

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

        public static RectangleF GetPolygonBounds(ShapePolygon polygon)
        {
            if (polygon.Points.Count == 0) return RectangleF.Empty;

            float minX = polygon.Points.Min(p => p.X);
            float minY = polygon.Points.Min(p => p.Y);
            float maxX = polygon.Points.Max(p => p.X);
            float maxY = polygon.Points.Max(p => p.Y);

            return new RectangleF(minX, minY, maxX - minX, maxY - minY);
        }

        public static RectangleF GetPathBounds(ShapePath path)
        {
            if (path.PathSegments.Count == 0) return RectangleF.Empty;

            float minX = float.MaxValue;
            float minY = float.MaxValue;
            float maxX = float.MinValue;
            float maxY = float.MinValue;

            foreach (var segment in path.PathSegments)
            {
                foreach (var point in segment.InputPoints)
                {
                    minX = Math.Min(minX, point.X);
                    minY = Math.Min(minY, point.Y);
                    maxX = Math.Max(maxX, point.X);
                    maxY = Math.Max(maxY, point.Y);
                }
            }

            return new RectangleF(minX, minY, maxX - minX, maxY - minY);
        }

        public static void ResizePolygon(ShapePolygon polygon, RectangleF newBounds)
        {
            if (polygon.Points.Count == 0) return;

            var oldBounds = GetPolygonBounds(polygon);
            if (oldBounds.Width == 0 || oldBounds.Height == 0) return;

            float scaleX = newBounds.Width / oldBounds.Width;
            float scaleY = newBounds.Height / oldBounds.Height;

            for (int i = 0; i < polygon.Points.Count; i++)
            {
                float relativeX = polygon.Points[i].X - oldBounds.X;
                float relativeY = polygon.Points[i].Y - oldBounds.Y;

                polygon.Points[i] = new PointF(
                    newBounds.X + relativeX * scaleX,
                    newBounds.Y + relativeY * scaleY
                );
            }
        }

        public static void ResizePath(ShapePath path, RectangleF newBounds)
        {
            var oldBounds = GetPathBounds(path);
            if (oldBounds.Width == 0 || oldBounds.Height == 0) return;

            float scaleX = newBounds.Width / oldBounds.Width;
            float scaleY = newBounds.Height / oldBounds.Height;

            foreach (var segment in path.PathSegments)
            {
                for (int i = 0; i < segment.InputPoints.Count; i++)
                {
                    float relativeX = segment.InputPoints[i].X - oldBounds.X;
                    float relativeY = segment.InputPoints[i].Y - oldBounds.Y;

                    segment.InputPoints[i] = new PointF(
                        newBounds.X + relativeX * scaleX,
                        newBounds.Y + relativeY * scaleY
                    );
                }
            }
        }

        public static void UpdateShapePositionOrSize(BaseShape shape, float? x, float? y, float? width, float? height)
        {
            using Matrix identity = new();
            RectangleF bounds = Layer.GetShapeBounds(shape, identity);

            float targetX = x ?? bounds.X;
            float targetY = y ?? bounds.Y;
            float targetWidth = width ?? bounds.Width;
            float targetHeight = height ?? bounds.Height;

            if (shape is ShapeRect r)
            {
                r.X = targetX;
                r.Y = targetY;
                r.Width = targetWidth;
                r.Height = targetHeight;
            }
            else if (shape is ShapeEllipse e)
            {
                e.X = targetX;
                e.Y = targetY;
                e.Width = targetWidth;
                e.Height = targetHeight;
            }
            else if (shape is ShapeText t)
            {
                t.X = targetX;
                t.Y = targetY;
                t.Width = targetWidth;
                t.Height = targetHeight;
            }
            else if (shape is ShapePolygon p)
            {
                Layer.ResizePolygon(p, new RectangleF(targetX, targetY, targetWidth, targetHeight));
            }
            else if (shape is ShapePath sp)
            {
                Layer.ResizePath(sp, new RectangleF(targetX, targetY, targetWidth, targetHeight));
            }
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
                _fillColor = _fillColor,
                _layerType = _layerType,
                shapes = [.. shapes.Select(s => GetDuplicate(s) ?? s)]

            };

            if (Image != null)
            {
                clone.Image = new Bitmap(Image);
            }

            return clone;
        }
    }
}