using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace PixelEditor.Vector
{
    public static class PathManipulator
    {
        public static ShapePath Convert(BaseShape shape)
        {
            return shape switch
            {
                ShapeLine line => Convert(line),
                ShapeRect rect => Convert(rect),
                ShapeEllipse ellipse => Convert(ellipse),
                ShapePolygon polygon => Convert(polygon),
                ShapeText text => Convert(text),
                _ => new ShapePath()
            };
        }

        public static ShapePath Convert(ShapeLine line)
        {
            ShapePath path = CreateBase(line);

            float cx = (line.StartPoint.X + line.EndPoint.X) / 2f;
            float cy = (line.StartPoint.Y + line.EndPoint.Y) / 2f;

            PointF start = RotatePoint(line.StartPoint, cx, cy, line.Rotation);
            PointF end = RotatePoint(line.EndPoint, cx, cy, line.Rotation);

            path.PathSegments.Add(new PathSegment { PathType = "M", InputPoints = [start] });
            path.PathSegments.Add(new PathSegment { PathType = "L", InputPoints = [end] });

            return path;
        }

        public static ShapePath Convert(ShapeRect rect)
        {
            ShapePath path = CreateBase(rect);

            float cx = rect.X + rect.Width / 2f;
            float cy = rect.Y + rect.Height / 2f;

            if (rect.Rx > 0 || rect.Ry > 0)
            {
                float rx = Math.Min(rect.Rx, rect.Width / 2f);
                float ry = Math.Min(rect.Ry, rect.Height / 2f);

                var points = new List<List<PointF>>
                {
                    ([new PointF(rect.X + rx, rect.Y)]),
                    ([new PointF(rect.X + rect.Width - rx, rect.Y)]),
                    ([new PointF(rect.X + rect.Width - rx, rect.Y), new PointF(rx, ry), new PointF(0, 0), new PointF(0, 1), new PointF(rect.X + rect.Width, rect.Y + ry)]),
                    ([new PointF(rect.X + rect.Width, rect.Y + rect.Height - ry)]),
                    ([new PointF(rect.X + rect.Width, rect.Y + rect.Height - ry), new PointF(rx, ry), new PointF(0, 0), new PointF(0, 1), new PointF(rect.X + rect.Width - rx, rect.Y + rect.Height)]),
                    ([new PointF(rect.X + rx, rect.Y + rect.Height)]),
                    ([new PointF(rect.X + rx, rect.Y + rect.Height), new PointF(rx, ry), new PointF(0, 0), new PointF(0, 1), new PointF(rect.X, rect.Y + rect.Height - ry)]),
                    ([new PointF(rect.X, rect.Y + ry)]),
                    ([new PointF(rect.X, rect.Y + ry), new PointF(rx, ry), new PointF(0, 0), new PointF(0, 1), new PointF(rect.X + rx, rect.Y)])
                };

                foreach (var pointList in points)
                {
                    var rotatedPoints = RotatePoints(pointList, cx, cy, rect.Rotation);
                    path.PathSegments.Add(new PathSegment
                    {
                        PathType = GetPathTypeForIndex(points.IndexOf(pointList)),
                        InputPoints = rotatedPoints
                    });
                }
            }
            else
            {
                var points = new List<PointF>
                {
                    new (rect.X, rect.Y),
                    new (rect.X + rect.Width, rect.Y),
                    new (rect.X + rect.Width, rect.Y + rect.Height),
                    new (rect.X, rect.Y + rect.Height)
                };

                var rotatedPoints = RotatePoints(points, cx, cy, rect.Rotation);

                path.PathSegments.Add(new PathSegment { PathType = "M", InputPoints = [rotatedPoints[0]] });
                path.PathSegments.Add(new PathSegment { PathType = "L", InputPoints = [rotatedPoints[1]] });
                path.PathSegments.Add(new PathSegment { PathType = "L", InputPoints = [rotatedPoints[2]] });
                path.PathSegments.Add(new PathSegment { PathType = "L", InputPoints = [rotatedPoints[3]] });
            }

            path.PathSegments.Add(new PathSegment { PathType = "Z", InputPoints = [] });
            return path;
        }

        public static ShapePath Convert(ShapeEllipse ellipse)
        {
            ShapePath path = CreateBase(ellipse);

            float cx = ellipse.Cx;
            float cy = ellipse.Cy;
            float rx = ellipse.Rx;
            float ry = ellipse.Ry;

            if (ellipse.Rotation == 0)
            {
                PointF top = new(cx, cy - ry);
                PointF bottom = new(cx, cy + ry);

                path.PathSegments.Add(new PathSegment { PathType = "M", InputPoints = [top] });
                path.PathSegments.Add(new PathSegment { PathType = "A", InputPoints = [top, new(rx, ry), new PointF(0, 0), new PointF(0, 1), bottom] });
                path.PathSegments.Add(new PathSegment { PathType = "A", InputPoints = [bottom, new(rx, ry), new PointF(0, 0), new PointF(0, 1), top] });
            }
            else
            {
                float angleRad = ellipse.Rotation * (float)(Math.PI / 180.0);
                float cos = (float)Math.Cos(angleRad);
                float sin = (float)Math.Sin(angleRad);

                List<PointF> points = [];
                int segments = 64;

                for (int i = 0; i <= segments; i++)
                {
                    float t = (float)(i * 2 * Math.PI / segments);
                    float x = cx + rx * (float)Math.Cos(t);
                    float y = cy + ry * (float)Math.Sin(t);

                    float dx = x - cx;
                    float dy = y - cy;
                    float rx2 = dx * cos - dy * sin;
                    float ry2 = dx * sin + dy * cos;

                    points.Add(new PointF(cx + rx2, cy + ry2));
                }

                if (points.Count > 0)
                {
                    path.PathSegments.Add(new PathSegment { PathType = "M", InputPoints = [points[0]] });

                    for (int i = 1; i < points.Count; i++)
                    {
                        path.PathSegments.Add(new PathSegment { PathType = "L", InputPoints = [points[i]] });
                    }

                    path.PathSegments.Add(new PathSegment { PathType = "Z", InputPoints = [] });
                }
            }

            return path;
        }

        public static ShapePath Convert(ShapePolygon polygon)
        {
            ShapePath path = CreateBase(polygon);

            if (polygon.Points.Count > 0)
            {
                path.PathSegments.Add(new PathSegment { PathType = "M", InputPoints = [polygon.Points[0]] });

                for (int i = 1; i < polygon.Points.Count; i++)
                {
                    path.PathSegments.Add(new PathSegment { PathType = "L", InputPoints = [polygon.Points[i]] });
                }

                if (polygon.IsClosed)
                {
                    path.PathSegments.Add(new PathSegment { PathType = "Z", InputPoints = [] });
                }
            }

            return path;
        }

        public static ShapePath Convert(ShapeText text)
        {
            ShapePath path = CreateBase(text);

            if (string.IsNullOrEmpty(text.Content)) return path;

            bool[,] mask = CreateTextMask(text);
            List<List<PointF>> outlines = ExtractOutlines(mask);

            foreach (var contour in outlines)
            {
                if (contour.Count < 2) continue;

                List<PointF> simplifiedContour = SimplifyContour(contour, epsilon: 0.5f);
                if (simplifiedContour.Count < 2) continue;

                List<PointF> worldPoints = [];
                foreach (var point in simplifiedContour)
                {
                    worldPoints.Add(new PointF(point.X + text.X, point.Y + text.Y));
                }

                if (text.Rotation != 0)
                {
                    float minX = worldPoints.Min(p => p.X);
                    float minY = worldPoints.Min(p => p.Y);
                    float maxX = worldPoints.Max(p => p.X);
                    float maxY = worldPoints.Max(p => p.Y);
                    float centerX = (minX + maxX) / 2f;
                    float centerY = (minY + maxY) / 2f;

                    List<PointF> rotatedPoints = [];
                    float angleRad = text.Rotation * (float)(Math.PI / 180.0);
                    float cos = (float)Math.Cos(angleRad);
                    float sin = (float)Math.Sin(angleRad);

                    foreach (var point in worldPoints)
                    {
                        float dx = point.X - centerX;
                        float dy = point.Y - centerY;
                        float x = centerX + dx * cos - dy * sin;
                        float y = centerY + dx * sin + dy * cos;
                        rotatedPoints.Add(new PointF(x, y));
                    }
                    worldPoints = rotatedPoints;
                }

                path.PathSegments.Add(new PathSegment { PathType = "M", InputPoints = [worldPoints[0]] });

                for (int i = 1; i < worldPoints.Count; i++)
                {
                    path.PathSegments.Add(new PathSegment { PathType = "L", InputPoints = [worldPoints[i]] });
                }

                path.PathSegments.Add(new PathSegment { PathType = "Z", InputPoints = [] });
            }

            return path;
        }

        public static ShapePath UnionShapePaths(ShapePath shape1, ShapePath shape2)
        {
            if (shape1.PathSegments.Count == 0) return shape2;
            if (shape2.PathSegments.Count == 0) return shape1;

            var allPoints = shape1.PathSegments.Concat(shape2.PathSegments).SelectMany(s => s.GetPoints()).ToList();
            float buffer = Math.Max(shape1.LineWidth, shape2.LineWidth) + 1;
            float minX = allPoints.Min(p => p.X) - buffer;
            float minY = allPoints.Min(p => p.Y) - buffer;
            float maxX = allPoints.Max(p => p.X) + buffer;
            float maxY = allPoints.Max(p => p.Y) + buffer;

            int width = Math.Max(1, (int)Math.Ceiling(maxX - minX));
            int height = Math.Max(1, (int)Math.Ceiling(maxY - minY));

            bool[,] mask1 = CreatePathMask(shape1, minX, minY, width, height);
            bool[,] mask2 = CreatePathMask(shape2, minX, minY, width, height);

            bool[,] combinedMask = new bool[width, height];
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    combinedMask[x, y] = mask1[x, y] || mask2[x, y];
                }
            }

            List<List<PointF>> outlines = ExtractOutlines(combinedMask);
            List<PathSegment> resultSegments = [];

            foreach (var outline in outlines)
            {
                var simplified = SimplifyContour(outline, epsilon: 0.5f);
                for (int i = 0; i < simplified.Count; i++)
                {
                    PointF worldPoint = new(simplified[i].X + minX, simplified[i].Y + minY);
                    resultSegments.Add(new PathSegment(i == 0 ? "M" : "L", [worldPoint]));
                }
                resultSegments.Add(new PathSegment("Z", []));
            }

            shape1.PathSegments = resultSegments;

            return shape1;
        }

        public static ShapePath DifferenceShapePaths(ShapePath shape1, ShapePath shape2)
        {
            if (shape1.PathSegments.Count == 0) return shape2;
            if (shape2.PathSegments.Count == 0) return shape1;

            var allPoints = shape1.PathSegments.Concat(shape2.PathSegments).SelectMany(s => s.GetPoints()).ToList();
            float buffer = Math.Max(shape1.LineWidth, shape2.LineWidth) + 1;
            float minX = allPoints.Min(p => p.X) - buffer;
            float minY = allPoints.Min(p => p.Y) - buffer;
            float maxX = allPoints.Max(p => p.X) + buffer;
            float maxY = allPoints.Max(p => p.Y) + buffer;

            int width = Math.Max(1, (int)Math.Ceiling(maxX - minX));
            int height = Math.Max(1, (int)Math.Ceiling(maxY - minY));

            bool[,] mask1 = CreatePathMask(shape1, minX, minY, width, height);
            bool[,] mask2 = CreatePathMask(shape2, minX, minY, width, height);

            bool[,] combinedMask = new bool[width, height];
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    combinedMask[x, y] = mask1[x, y] && !mask2[x, y];
                }
            }

            List<List<PointF>> outlines = ExtractOutlines(combinedMask);
            List<PathSegment> resultSegments = [];
            
            foreach (var outline in outlines)
            {
                var simplified = SimplifyContour(outline, epsilon: 0.5f);
                for (int i = 0; i < simplified.Count; i++)
                {
                    PointF worldPoint = new(simplified[i].X + minX, simplified[i].Y + minY);
                    resultSegments.Add(new PathSegment(i == 0 ? "M" : "L", [worldPoint]));
                }
                resultSegments.Add(new PathSegment("Z", []));
            }

            shape1.PathSegments = resultSegments;

            return shape1;
        }

        public static ShapePath IntersectionShapePaths(ShapePath shape1, ShapePath shape2)
        {
            if (shape1.PathSegments.Count == 0) return shape2;
            if (shape2.PathSegments.Count == 0) return shape1;

            var allPoints = shape1.PathSegments.Concat(shape2.PathSegments).SelectMany(s => s.GetPoints()).ToList();
            float buffer = Math.Max(shape1.LineWidth, shape2.LineWidth) + 1;
            float minX = allPoints.Min(p => p.X) - buffer;
            float minY = allPoints.Min(p => p.Y) - buffer;
            float maxX = allPoints.Max(p => p.X) + buffer;
            float maxY = allPoints.Max(p => p.Y) + buffer;

            int width = Math.Max(1, (int)Math.Ceiling(maxX - minX));
            int height = Math.Max(1, (int)Math.Ceiling(maxY - minY));

            bool[,] mask1 = CreatePathMask(shape1, minX, minY, width, height);
            bool[,] mask2 = CreatePathMask(shape2, minX, minY, width, height);

            bool[,] combinedMask = new bool[width, height];
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    combinedMask[x, y] = mask1[x, y] && mask2[x, y];
                }
            }

            List<List<PointF>> outlines = ExtractOutlines(combinedMask);
            List<PathSegment> resultSegments = [];

            foreach (var outline in outlines)
            {
                var simplified = SimplifyContour(outline, epsilon: 0.5f);
                for (int i = 0; i < simplified.Count; i++)
                {
                    PointF worldPoint = new(simplified[i].X + minX, simplified[i].Y + minY);
                    resultSegments.Add(new PathSegment(i == 0 ? "M" : "L", [worldPoint]));
                }
                resultSegments.Add(new PathSegment("Z", []));
            }

            shape1.PathSegments = resultSegments;

            return shape1;
        }

        public static ShapePath ExclusionShapePaths(ShapePath shape1, ShapePath shape2)
        {
            if (shape1.PathSegments.Count == 0) return shape2;
            if (shape2.PathSegments.Count == 0) return shape1;

            var allPoints = shape1.PathSegments.Concat(shape2.PathSegments).SelectMany(s => s.GetPoints()).ToList();
            float buffer = Math.Max(shape1.LineWidth, shape2.LineWidth) + 1;
            float minX = allPoints.Min(p => p.X) - buffer;
            float minY = allPoints.Min(p => p.Y) - buffer;
            float maxX = allPoints.Max(p => p.X) + buffer;
            float maxY = allPoints.Max(p => p.Y) + buffer;

            int width = Math.Max(1, (int)Math.Ceiling(maxX - minX));
            int height = Math.Max(1, (int)Math.Ceiling(maxY - minY));

            bool[,] mask1 = CreatePathMask(shape1, minX, minY, width, height);
            bool[,] mask2 = CreatePathMask(shape2, minX, minY, width, height);

            bool[,] combinedMask = new bool[width, height];
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    combinedMask[x, y] = mask1[x, y] ^ mask2[x, y];
                }
            }

            List<List<PointF>> outlines = ExtractOutlines(combinedMask);
            List<PathSegment> resultSegments = [];

            foreach (var outline in outlines)
            {
                var simplified = SimplifyContour(outline, epsilon: 0.5f);
                for (int i = 0; i < simplified.Count; i++)
                {
                    PointF worldPoint = new(simplified[i].X + minX, simplified[i].Y + minY);
                    resultSegments.Add(new PathSegment(i == 0 ? "M" : "L", [worldPoint]));
                }
                resultSegments.Add(new PathSegment("Z", []));
            }

            shape1.PathSegments = resultSegments;

            return shape1;
        }

        private static bool[,] CreatePathMask(ShapePath shape, float offsetX, float offsetY, int width, int height)
        {
            bool[,] mask = new bool[width, height];
            using Bitmap bmp = new(width, height, PixelFormat.Format32bppArgb);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.Clear(Color.Transparent);
                g.SmoothingMode = SmoothingMode.None;
                g.PixelOffsetMode = PixelOffsetMode.None;
                g.TranslateTransform(-offsetX, -offsetY);

                using GraphicsPath path = new();
                path.FillMode = FillMode.Winding;

                List<PointF> currentFigure = [];

                foreach (var seg in shape.PathSegments)
                {
                    List<PointF> segPoints = seg.GetPoints();

                    if (seg.PathType.Equals("M", StringComparison.CurrentCultureIgnoreCase))
                    {
                        if (currentFigure.Count > 1) path.AddLines(currentFigure.ToArray());
                        currentFigure.Clear();
                        currentFigure.AddRange(segPoints);
                    }
                    else if (seg.PathType.Equals("Z", StringComparison.CurrentCultureIgnoreCase))
                    {
                        if (currentFigure.Count > 1)
                        {
                            path.AddLines(currentFigure.ToArray());
                            path.CloseFigure();
                        }
                        currentFigure.Clear();
                    }
                    else
                    {
                        currentFigure.AddRange(segPoints);
                    }
                }

                if (currentFigure.Count > 1) path.AddLines(currentFigure.ToArray());

                if (path.PointCount > 0)
                {
                    using SolidBrush brush = new(Color.Black);
                    g.FillPath(brush, path);

                    using Pen pen = new(Color.Black, 1.0f);
                    g.DrawPath(pen, path);
                }
            }

            BitmapData data = bmp.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            unsafe
            {
                byte* ptr = (byte*)data.Scan0;
                for (int y = 0; y < height; y++)
                {
                    byte* row = ptr + (y * data.Stride);
                    for (int x = 0; x < width; x++)
                    {
                        mask[x, y] = row[x * 4 + 3] > 0;
                    }
                }
            }
            bmp.UnlockBits(data);

            return mask;
        }

        private static bool[,] CreateTextMask(ShapeText text)
        {
            using Font font = Layer.CreateScaledFont(text);

            using Bitmap measureBmp = new(1, 1);
            using Graphics measureG = Graphics.FromImage(measureBmp);
            SizeF size = measureG.MeasureString(text.Content, font);

            int width = Math.Max(1, (int)Math.Ceiling(size.Width));
            int height = Math.Max(1, (int)Math.Ceiling(size.Height));

            using Bitmap bmp = new(width, height, PixelFormat.Format32bppArgb);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.Clear(Color.Transparent);
                g.SmoothingMode = SmoothingMode.None;
                g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SingleBitPerPixelGridFit;

                using SolidBrush brush = new(Color.Black);

                Matrix originalTransform = g.Transform;

                float centerX = text.X + text.Width / 2;
                float centerY = text.Y + text.Height / 2;

                //g.TranslateTransform(centerX, centerY);
                //g.RotateTransform(text.Rotation);
                //g.TranslateTransform(-centerX, -centerY);

                g.DrawString(text.Content, font, brush, 0, 0);

                g.Transform = originalTransform;
            }

            bool[,] mask = new bool[width, height];
            BitmapData data = bmp.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

            unsafe
            {
                byte* ptr = (byte*)data.Scan0;
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        byte* pixel = ptr + (y * data.Stride) + (x * 4);
                        byte alpha = pixel[3];
                        mask[x, y] = alpha > 0;
                    }
                }
            }
            bmp.UnlockBits(data);

            return mask;
        }

        private static List<List<PointF>> ExtractOutlines(bool[,] mask)
        {
            int width = mask.GetLength(0);
            int height = mask.GetLength(1);
            bool[,] visited = new bool[width, height];
            List<List<PointF>> contours = [];

            int[] dx = [0, 1, 1, 1, 0, -1, -1, -1];
            int[] dy = [-1, -1, 0, 1, 1, 1, 0, -1];

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (mask[x, y] && !visited[x, y])
                    {
                        bool isEdge = false;
                        for (int i = 0; i < 8; i++)
                        {
                            int nx = x + dx[i];
                            int ny = y + dy[i];
                            if (nx < 0 || nx >= width || ny < 0 || ny >= height || !mask[nx, ny])
                            {
                                isEdge = true;
                                break;
                            }
                        }

                        if (isEdge)
                        {
                            List<PointF> contour = TraceContour(mask, visited, x, y, dx, dy);
                            if (contour.Count > 2)
                            {
                                contours.Add(contour);
                            }
                        }
                    }
                }
            }
            return contours;
        }

        private static List<PointF> TraceContour(bool[,] mask, bool[,] visited, int startX, int startY, int[] dx, int[] dy)
        {
            List<PointF> contour = [];
            int width = mask.GetLength(0);
            int height = mask.GetLength(1);

            int cx = startX;
            int cy = startY;
            int enteredFrom = 0;

            bool looped = false;
            int maxIterations = width * height;
            int iterations = 0;

            while (!looped && iterations < maxIterations)
            {
                iterations++;
                contour.Add(new PointF(cx, cy));
                visited[cx, cy] = true;

                bool foundNext = false;
                for (int i = 0; i < 8; i++)
                {
                    int dir = (enteredFrom + i) % 8;
                    int nx = cx + dx[dir];
                    int ny = cy + dy[dir];

                    if (nx >= 0 && nx < width && ny >= 0 && ny < height && mask[nx, ny])
                    {
                        cx = nx;
                        cy = ny;
                        enteredFrom = (dir + 5) % 8;
                        foundNext = true;
                        break;
                    }
                }

                if (!foundNext || (cx == startX && cy == startY))
                {
                    looped = true;
                }
            }

            return contour;
        }

        private static List<PointF> SimplifyContour(List<PointF> points, float epsilon)
        {
            if (points.Count < 3) return points;

            List<PointF> simplified = [];

            // Find point with maximum distance from segment connecting first and last points
            int index = -1;
            float maxDist = 0;

            for (int i = 1; i < points.Count - 1; i++)
            {
                float dist = PerpendicularDistance(points[i], points[0], points[^1]);
                if (dist > maxDist)
                {
                    index = i;
                    maxDist = dist;
                }
            }

            // If max distance is greater than epsilon, split recursively
            if (maxDist > epsilon)
            {
                var left = SimplifyContour(points.GetRange(0, index + 1), epsilon);
                var right = SimplifyContour(points.GetRange(index, points.Count - index), epsilon);

                simplified.AddRange(left.GetRange(0, left.Count - 1));
                simplified.AddRange(right);
            }
            else
            {
                simplified.Add(points[0]);
                simplified.Add(points[^1]);
            }

            return simplified;
        }

        private static float PerpendicularDistance(PointF p, PointF lineStart, PointF lineEnd)
        {
            float area = Math.Abs((lineEnd.Y - lineStart.Y) * p.X - (lineEnd.X - lineStart.X) * p.Y + lineEnd.X * lineStart.Y - lineEnd.Y * lineStart.X);
            float bottom = (float)Math.Sqrt(Math.Pow(lineEnd.Y - lineStart.Y, 2) + Math.Pow(lineEnd.X - lineStart.X, 2));
            return area / bottom;
        }

        private static string GetPathTypeForIndex(int index)
        {
            return index switch
            {
                0 => "M",
                1 => "H",
                2 => "A",
                3 => "V",
                4 => "A",
                5 => "H",
                6 => "A",
                7 => "V",
                8 => "A",
                _ => "L"
            };
        }

        private static ShapePath CreateBase(BaseShape source)
        {
            return new ShapePath
            {
                LineWidth = source.LineWidth,
                LineColor = source.LineColor,
                FillColor = source.FillColor,
                DashStyle = source.DashStyle,
                Opacity = source.Opacity,
                StrokeOpacity = source.StrokeOpacity,
                Rotation = source.Rotation,
                HasGradientFill = source.HasGradientFill,
                HasGradientStroke = source.HasGradientStroke,
                GradientFill = source.GradientFill,
                GradientStroke = source.GradientStroke
            };
        }

        private static PointF RotatePoint(PointF point, float cx, float cy, float angleDegrees)
        {
            if (angleDegrees == 0) return point;

            float angleRad = angleDegrees * (float)(Math.PI / 180.0);
            float cos = (float)Math.Cos(angleRad);
            float sin = (float)Math.Sin(angleRad);

            float dx = point.X - cx;
            float dy = point.Y - cy;

            float x = dx * cos - dy * sin;
            float y = dx * sin + dy * cos;

            return new PointF(x + cx, y + cy);
        }

        private static List<PointF> RotatePoints(List<PointF> points, float cx, float cy, float angleDegrees)
        {
            if (angleDegrees == 0) return points;

            List<PointF> rotated = [];
            foreach (var point in points)
            {
                rotated.Add(RotatePoint(point, cx, cy, angleDegrees));
            }
            return rotated;
        }

        private static void ApplyRotationToPath(ShapePath path, float cx, float cy, float rotation)
        {
            if (rotation == 0) return;

            foreach (var segment in path.PathSegments)
            {
                if (segment.InputPoints != null && segment.InputPoints.Count > 0)
                {
                    segment.InputPoints = RotatePoints(segment.InputPoints, cx, cy, rotation);
                }
            }
        }

        private static List<PathSegment> EllipseToBezierCurves(float cx, float cy, float rx, float ry, float rotation)
        {
            List<PathSegment> segments = [];

            const float K = 0.5522847498f;

            float kx = rx * K;
            float ky = ry * K;

            PointF[] quadrants =
            [
                new(cx + rx, cy),
                new(cx, cy - ry),
                new(cx - rx, cy),
                new(cx, cy + ry)
            ];

            PointF[][] controlPoints =
            [
                [new PointF(cx + rx, cy - ky), new PointF(cx + kx, cy - ry), quadrants[1]],
                [new PointF(cx - kx, cy - ry), new PointF(cx - rx, cy - ky), quadrants[2]],
                [new PointF(cx - rx, cy + ky), new PointF(cx - kx, cy + ry), quadrants[3]],
                [new PointF(cx + kx, cy + ry), new PointF(cx + rx, cy + ky), quadrants[0]]
            ];

            if (rotation != 0)
            {
                for (int i = 0; i < quadrants.Length; i++)
                    quadrants[i] = RotatePoint(quadrants[i], cx, cy, rotation);

                for (int i = 0; i < controlPoints.Length; i++)
                {
                    for (int j = 0; j < controlPoints[i].Length; j++)
                    {
                        controlPoints[i][j] = RotatePoint(controlPoints[i][j], cx, cy, rotation);
                    }
                }
            }

            segments.Add(new PathSegment { PathType = "M", InputPoints = [quadrants[0]] });

            foreach (var cp in controlPoints)
            {
                segments.Add(new PathSegment
                {
                    PathType = "C",
                    InputPoints = [cp[0], cp[1], cp[2]]
                });
            }

            segments.Add(new PathSegment { PathType = "Z", InputPoints = [] });

            return segments;
        }
    }
}