using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace PixelEditor.Vector
{
    public static class ShapeToPathConverter
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
                _ => throw new NotSupportedException("Unsupported shape type")
            };
        }

        public static ShapePath Convert(ShapeLine line)
        {
            ShapePath path = CreateBase(line);
            path.PathSegments.Add(new PathSegment { PathType = "M", InputPoints = [line.StartPoint] });
            path.PathSegments.Add(new PathSegment { PathType = "L", InputPoints = [line.EndPoint] });
            return path;
        }

        public static ShapePath Convert(ShapeRect rect)
        {
            ShapePath path = CreateBase(rect);

            if (rect.Rx > 0 || rect.Ry > 0)
            {
                float rx = Math.Min(rect.Rx, rect.Width / 2f);
                float ry = Math.Min(rect.Ry, rect.Height / 2f);

                path.PathSegments.Add(new PathSegment { PathType = "M", InputPoints = [new PointF(rect.X + rx, rect.Y)] });
                path.PathSegments.Add(new PathSegment { PathType = "H", InputPoints = [new PointF(rect.X + rect.Width - rx, rect.Y)] });
                path.PathSegments.Add(new PathSegment { PathType = "A", InputPoints = [new PointF(rect.X + rect.Width - rx, rect.Y), new PointF(rx, ry), new PointF(0, 0), new PointF(0, 1), new PointF(rect.X + rect.Width, rect.Y + ry)] });
                path.PathSegments.Add(new PathSegment { PathType = "V", InputPoints = [new PointF(rect.X + rect.Width, rect.Y + rect.Height - ry)] });
                path.PathSegments.Add(new PathSegment { PathType = "A", InputPoints = [new PointF(rect.X + rect.Width, rect.Y + rect.Height - ry), new PointF(rx, ry), new PointF(0, 0), new PointF(0, 1), new PointF(rect.X + rect.Width - rx, rect.Y + rect.Height)] });
                path.PathSegments.Add(new PathSegment { PathType = "H", InputPoints = [new PointF(rect.X + rx, rect.Y + rect.Height)] });
                path.PathSegments.Add(new PathSegment { PathType = "A", InputPoints = [new PointF(rect.X + rx, rect.Y + rect.Height), new PointF(rx, ry), new PointF(0, 0), new PointF(0, 1), new PointF(rect.X, rect.Y + rect.Height - ry)] });
                path.PathSegments.Add(new PathSegment { PathType = "V", InputPoints = [new PointF(rect.X, rect.Y + ry)] });
                path.PathSegments.Add(new PathSegment { PathType = "A", InputPoints = [new PointF(rect.X, rect.Y + ry), new PointF(rx, ry), new PointF(0, 0), new PointF(0, 1), new PointF(rect.X + rx, rect.Y)] });
            }
            else
            {
                path.PathSegments.Add(new PathSegment { PathType = "M", InputPoints = [new PointF(rect.X, rect.Y)] });
                path.PathSegments.Add(new PathSegment { PathType = "L", InputPoints = [new PointF(rect.X + rect.Width, rect.Y)] });
                path.PathSegments.Add(new PathSegment { PathType = "L", InputPoints = [new PointF(rect.X + rect.Width, rect.Y + rect.Height)] });
                path.PathSegments.Add(new PathSegment { PathType = "L", InputPoints = [new PointF(rect.X, rect.Y + rect.Height)] });
            }

            path.PathSegments.Add(new PathSegment { PathType = "Z", InputPoints = [] });
            return path;
        }

        public static ShapePath Convert(ShapeEllipse ellipse)
        {
            ShapePath path = CreateBase(ellipse);
            float rx = ellipse.Rx;
            float ry = ellipse.Ry;
            float cx = ellipse.Cx;
            float cy = ellipse.Cy;

            path.PathSegments.Add(new PathSegment { PathType = "M", InputPoints = [new PointF(cx, cy - ry)] });
            path.PathSegments.Add(new PathSegment { PathType = "A", InputPoints = [new PointF(cx, cy - ry), new PointF(rx, ry), new PointF(0, 0), new PointF(0, 1), new PointF(cx, cy + ry)] });
            path.PathSegments.Add(new PathSegment { PathType = "A", InputPoints = [new PointF(cx, cy + ry), new PointF(rx, ry), new PointF(0, 0), new PointF(0, 1), new PointF(cx, cy - ry)] });
            path.PathSegments.Add(new PathSegment { PathType = "Z", InputPoints = [] });

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

                PointF startPoint = new(simplifiedContour[0].X + text.X, simplifiedContour[0].Y + text.Y);
                path.PathSegments.Add(new PathSegment { PathType = "M", InputPoints = [startPoint] });

                for (int i = 1; i < simplifiedContour.Count; i++)
                {
                    PointF nextPoint = new(simplifiedContour[i].X + text.X, simplifiedContour[i].Y + text.Y);
                    path.PathSegments.Add(new PathSegment { PathType = "L", InputPoints = [nextPoint] });
                }

                path.PathSegments.Add(new PathSegment { PathType = "Z", InputPoints = [] });
            }

            return path;
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
                g.DrawString(text.Content, font, brush, 0, 0);
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
                        mask[x, y] = alpha > 127;
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
    }
}