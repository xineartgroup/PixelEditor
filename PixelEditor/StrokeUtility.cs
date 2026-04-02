using PixelEditor.Vector;

namespace PixelEditor
{
    public static class StrokeUtility
    {
        public static PointF[] GetSearchDirections()
        {
            return
            [
                new PointF(-1, -1), new PointF(0, -1), new PointF(1, -1),
                new PointF(-1, 0),                    new PointF(1, 0),
                new PointF(-1, 1),  new PointF(0, 1),  new PointF(1, 1)
            ];
        }

        public static bool[,] GetMaskOutline(ColorGrid grid)
        {
            bool[,] mask = new bool[grid.Width, grid.Height];
            bool[,] maskOutline = new bool[grid.Width, grid.Height];

            for (float threshold = 0.0f; threshold < 0.6f; threshold += 0.1f)
            {
                mask = MaskDarkPixels(grid, threshold, mask);

                int[] dx = [-1, 1, 0, 0, -1, -1, 1, 1];
                int[] dy = [0, 0, -1, 1, -1, 1, -1, 1];

                for (int x = 0; x < grid.Width; x++)
                {
                    for (int y = 0; y < grid.Height; y++)
                    {
                        if (!mask[x, y]) continue;

                        for (int d = 0; d < 4; d++)
                        {
                            int nx = x + dx[d];
                            int ny = y + dy[d];

                            // If neighbor is out of bounds or false, this pixel is on the outline
                            if (nx < 0 || nx >= grid.Width || ny < 0 || ny >= grid.Height || !mask[nx, ny])
                            {
                                maskOutline[x, y] = true;
                                break;
                            }
                        }
                    }
                }
            }

            return maskOutline;
        }

        public static bool[,] MaskDarkPixels(ColorGrid grid, float threshold, bool[,] mask)
        {
            float luminanceThreshold = threshold * 255.0f;

            for (int y = 0; y < grid.Height; y++)
            {
                for (int x = 0; x < grid.Width; x++)
                {
                    Color color = Color.FromArgb(grid[x, y]);
                    float luminance = 0.299f * color.R + 0.587f * color.G + 0.114f * color.B;
                    mask[x, y] = luminance <= luminanceThreshold;
                }
            }

            return mask;
        }

        public static bool[,] CreateRegionMask(ColorRegion region, ColorGrid grid)
        {
            bool[,] mask = new bool[grid.Width, grid.Height];
            foreach (var point in region.Pixels)
            {
                mask[(int)point.X, (int)point.Y] = true;
            }
            return mask;
        }

        public static float ColorDistance(Color c1, Color c2)
        {
            float dr = (c1.R - c2.R) / 255.0f;
            float dg = (c1.G - c2.G) / 255.0f;
            float db = (c1.B - c2.B) / 255.0f;
            return (float)Math.Sqrt((dr * dr + dg * dg + db * db) / 3);
        }

        public static Rectangle GetBounds(List<PointF> points)
        {
            if (points.Count == 0)
                return new Rectangle(0, 0, 0, 0);

            int minX = int.MaxValue, minY = int.MaxValue;
            int maxX = int.MinValue, maxY = int.MinValue;

            foreach (var point in points)
            {
                minX = (int)Math.Min(minX, point.X);
                minY = (int)Math.Min(minY, point.Y);
                maxX = (int)Math.Max(maxX, point.X);
                maxY = (int)Math.Max(maxY, point.Y);
            }

            return new Rectangle(minX, minY, maxX - minX + 1, maxY - minY + 1);
        }

        public static Rectangle UpdateBounds(Rectangle currentBounds, PointF point)
        {
            if (currentBounds == Rectangle.Empty)
                return new Rectangle((int)point.X, (int)point.Y, 1, 1);

            int left = (int)Math.Min(currentBounds.Left, point.X);
            int top = (int)Math.Min(currentBounds.Top, point.Y);
            int right = (int)Math.Max(currentBounds.Right, point.X + 1);
            int bottom = (int)Math.Max(currentBounds.Bottom, point.Y + 1);

            return new Rectangle(left, top, right - left, bottom - top);
        }

        public static string PrintPoints(List<PointF> points)
        {
            return string.Join(", ", points.Select(p => $"({p.X:F1}, {p.Y:F1})"));
        }

        public static List<PointF> SimplifyDiagonalStraightLines(List<PointF> points,
            float lineDeviationThreshold = 1.5f, float curveThreshold = 3.0f, int minStraightRun = 5,int curveWindowSize = 10)
        {
            if (points.Count < 3) return [.. points];

            // PASS 1: Identify and mark all curve regions
            bool[] isCurveRegion = IdentifyCurveRegions(points, lineDeviationThreshold, curveThreshold, curveWindowSize);

            // PASS 2: Simplify only the non-curve regions
            return SimplifyNonCurveRegions(points, isCurveRegion, minStraightRun, lineDeviationThreshold);
        }

        private static bool[] IdentifyCurveRegions(List<PointF> points, float lineDeviationThreshold, float curveThreshold, int windowSize)
        {
            bool[] isCurve = new bool[points.Count];

            // Scan through with a sliding window to detect curves
            for (int i = 0; i < points.Count - windowSize; i++)
            {
                int end = Math.Min(i + windowSize, points.Count - 1);

                // Check if this window contains a curve
                float lineDeviation = CalculateLineDeviation(points, i, end);

                if (lineDeviation > lineDeviationThreshold)
                {
                    // High deviation - check if it's a curve
                    float curveError = CalculateCurveError(points, i, end);

                    if (curveError <= curveThreshold)
                    {
                        // This is a curve! Mark all points in this window
                        for (int j = i; j <= end; j++)
                        {
                            isCurve[j] = true;
                        }
                    }
                }
            }

            // Expand curve regions slightly to be safe
            bool[] expanded = new bool[points.Count];
            for (int i = 0; i < points.Count; i++)
            {
                if (isCurve[i])
                {
                    // Mark neighbors as curve too (2 points before and after)
                    for (int j = Math.Max(0, i - 2); j <= Math.Min(points.Count - 1, i + 2); j++)
                    {
                        expanded[j] = true;
                    }
                }
            }

            return expanded;
        }

        private static List<PointF> SimplifyNonCurveRegions(List<PointF> points, bool[] isCurveRegion, int minStraightRun, float lineDeviationThreshold)
        {
            List<PointF> result = [];
            int i = 0;

            while (i < points.Count)
            {
                // Always keep the current point
                result.Add(points[i]);

                if (i >= points.Count - 1)
                {
                    break;
                }

                // If we're in a curve region, keep all points
                if (isCurveRegion[i])
                {
                    i++;
                    continue;
                }

                // We're in a non-curve region - find the longest straight segment
                int segmentEnd = i + 1;

                // Extend as far as possible while:
                // 1. Staying in non-curve regions
                // 2. Maintaining straightness
                for (int j = i + 2; j < points.Count; j++)
                {
                    // Stop if we hit a curve region
                    if (isCurveRegion[j])
                    {
                        break;
                    }

                    // Check if still straight
                    float lineDeviation = CalculateLineDeviation(points, i, j);
                    if (lineDeviation <= lineDeviationThreshold)
                    {
                        segmentEnd = j;
                    }
                    else
                    {
                        break;
                    }
                }

                int segmentLength = segmentEnd - i + 1;

                if (segmentLength >= minStraightRun)
                {
                    // Long straight segment - skip all intermediate points
                    i = segmentEnd;
                }
                else
                {
                    // Too short - keep moving point by point
                    i++;
                }
            }

            return result;
        }

        public static List<PathSegment> GenerateSegments(List<PointF> points, float lineThreshold = 1.5f, float curveThreshold = 3.0f)
        {
            List<PathSegment> segments = [];
            if (points == null || points.Count < 2) return segments;

            // PASS 1: Generate segments using original algorithm
            int startIdx = 0;
            for (int i = 1; i < points.Count; i++)
            {
                float lineDeviation = CalculateLineDeviation(points, startIdx, i);

                if (lineDeviation <= lineThreshold)
                {
                    continue;
                }

                if (i - startIdx > 2)
                {
                    float curveError = CalculateCurveError(points, startIdx, i - 1);
                    if (curveError <= curveThreshold)
                    {
                        ProcessSegment(points, startIdx, i - 1, true, segments);
                        startIdx = i - 1;
                    }
                    else
                    {
                        ProcessSegment(points, startIdx, i - 1, false, segments);
                        startIdx = i - 1;
                    }
                }
            }

            ProcessSegment(points, startIdx, points.Count - 1, false, segments);

            // PASS 2: Simplify consecutive line segments
            segments = SimplifyLineSegments(segments);

            return segments;
        }

        private static List<PathSegment> SimplifyLineSegments(List<PathSegment> segments)
        {
            if (segments.Count < 2) return segments;

            List<PathSegment> result = [];
            int i = 0;

            while (i < segments.Count)
            {
                PathSegment current = segments[i];

                // Always keep M and C segments as-is
                if (current.PathType == "M" || current.PathType == "C")
                {
                    result.Add(current);
                    i++;
                    continue;
                }

                // For L segments, try to merge consecutive ones into a single line
                if (current.PathType == "L")
                {
                    List<PointF> linePoints =
                    [
                        current.InputPoints[0], // Start point
                        current.InputPoints[1], // End point
                    ];

                    // Collect consecutive L segments
                    int j = i + 1;
                    while (j < segments.Count && segments[j].PathType == "L")
                    {
                        linePoints.Add(segments[j].InputPoints[1]);
                        j++;
                    }

                    // Check if all these points form a straight line
                    if (linePoints.Count >= 3)
                    {
                        float deviation = CalculateLineDeviationForPoints(linePoints);

                        // If they're nearly straight, replace with single line segment
                        if (deviation <= 1.5f)
                        {
                            PathSegment simplifiedLine = new()
                            {
                                PathType = "L",
                                InputPoints = [linePoints[0], linePoints[^1]]
                            };
                            result.Add(simplifiedLine);
                            i = j; // Skip all the merged segments
                            continue;
                        }
                    }

                    // Not simplifiable - keep the original segment
                    result.Add(current);
                    i++;
                }
                else
                {
                    result.Add(current);
                    i++;
                }
            }

            return result;
        }

        private static void ProcessSegment(List<PointF> points, int startIdx, int endIdx, bool isCurve, List<PathSegment> segments)
        {
            if (startIdx == 0)
            {
                PathSegment moveSegment = new()
                {
                    PathType = "M",
                    InputPoints = [points[startIdx]]
                };
                segments.Add(moveSegment);
            }

            if (isCurve && endIdx - startIdx >= 3)
            {
                PointF p0 = points[startIdx];
                PointF p3 = points[endIdx];
                PointF p1 = EstimateControlPoint(points, startIdx, endIdx, true);
                PointF p2 = EstimateControlPoint(points, startIdx, endIdx, false);

                PathSegment curveSegment = new()
                {
                    PathType = "C",
                    InputPoints = [p0, p1, p2, p3]
                };
                segments.Add(curveSegment);
            }
            else
            {
                for (int i = startIdx + 1; i <= endIdx; i++)
                {
                    PathSegment lineSegment = new()
                    {
                        PathType = "L",
                        InputPoints = [points[i - 1], points[i]]
                    };
                    segments.Add(lineSegment);
                }
            }
        }

        private static float CalculateLineDeviationForPoints(List<PointF> points)
        {
            if (points.Count < 3) return 0;

            PointF p1 = points[0];
            PointF p2 = points[^1];

            float maxDeviation = 0;
            float lineLength = (float)Math.Sqrt(Math.Pow(p2.X - p1.X, 2) + Math.Pow(p2.Y - p1.Y, 2));

            if (lineLength < 0.01f) return 0;

            for (int i = 1; i < points.Count - 1; i++)
            {
                PointF p = points[i];
                float distance = DistanceFromLine(p, p1, p2);
                maxDeviation = Math.Max(maxDeviation, distance);
            }

            return maxDeviation;
        }

        private static float CalculateLineDeviation(List<PointF> points, int start, int end)
        {
            if (end - start < 2) return 0;

            PointF p1 = points[start];
            PointF p2 = points[end];

            float maxDeviation = 0;
            float lineLength = (float)Math.Sqrt(Math.Pow(p2.X - p1.X, 2) + Math.Pow(p2.Y - p1.Y, 2));

            if (lineLength < 0.01f) return 0;

            for (int i = start + 1; i < end; i++)
            {
                PointF p = points[i];
                float distance = DistanceFromLine(p, p1, p2);
                maxDeviation = Math.Max(maxDeviation, distance);
            }

            return maxDeviation;
        }

        private static float CalculateCurveError(List<PointF> points, int start, int end)
        {
            if (end - start < 3) return float.MaxValue;

            PointF p0 = points[start];
            PointF p3 = points[end];
            PointF p1 = EstimateControlPoint(points, start, end, true);
            PointF p2 = EstimateControlPoint(points, start, end, false);

            float totalError = 0;
            int count = 0;

            for (int i = start + 1; i < end; i++)
            {
                PointF p = points[i];
                float t = (float)(i - start) / (end - start);

                float xt = (float)(Math.Pow(1 - t, 3) * p0.X + 3 * Math.Pow(1 - t, 2) * t * p1.X + 3 * (1 - t) * t * t * p2.X + Math.Pow(t, 3) * p3.X);
                float yt = (float)(Math.Pow(1 - t, 3) * p0.Y + 3 * Math.Pow(1 - t, 2) * t * p1.Y + 3 * (1 - t) * t * t * p2.Y + Math.Pow(t, 3) * p3.Y);

                float dx = xt - p.X;
                float dy = yt - p.Y;
                totalError += (float)Math.Sqrt(dx * dx + dy * dy);
                count++;
            }

            return count > 0 ? totalError / count : float.MaxValue;
        }

        private static PointF EstimateControlPoint(List<PointF> points, int start, int end, bool first)
        {
            if (first)
            {
                int lookahead = Math.Min(Math.Max(3, (end - start) / 3), end - start);
                float dx = 0, dy = 0;
                int count = 0;

                for (int i = 1; i <= lookahead; i++)
                {
                    dx += points[start + i].X - points[start].X;
                    dy += points[start + i].Y - points[start].Y;
                    count++;
                }

                dx /= count;
                dy /= count;
                float length = (float)Math.Sqrt(dx * dx + dy * dy);

                if (length < 0.01f) return points[start];

                float segmentLength = (float)Math.Sqrt(
                    Math.Pow(points[end].X - points[start].X, 2) +
                    Math.Pow(points[end].Y - points[start].Y, 2)
                );
                float controlDistance = segmentLength * 0.33f;

                return new PointF(
                    points[start].X + (dx / length) * controlDistance,
                    points[start].Y + (dy / length) * controlDistance
                );
            }
            else
            {
                int lookback = Math.Min(Math.Max(3, (end - start) / 3), end - start);
                float dx = 0, dy = 0;
                int count = 0;

                for (int i = 1; i <= lookback; i++)
                {
                    dx += points[end].X - points[end - i].X;
                    dy += points[end].Y - points[end - i].Y;
                    count++;
                }

                dx /= count;
                dy /= count;
                float length = (float)Math.Sqrt(dx * dx + dy * dy);

                if (length < 0.01f) return points[end];

                float segmentLength = (float)Math.Sqrt(
                    Math.Pow(points[end].X - points[start].X, 2) +
                    Math.Pow(points[end].Y - points[start].Y, 2)
                );
                float controlDistance = segmentLength * 0.33f;

                return new PointF(
                    points[end].X - (dx / length) * controlDistance,
                    points[end].Y - (dy / length) * controlDistance
                );
            }
        }

        public static List<PointF> SimplifyWithCorners(List<PointF> points, float distanceEpsilon, float angleThresholdDegrees)
        {
            if (points.Count < 3) return [.. points];

            // Convert angle to dot product for faster math. 150 degrees means it's nearly straight.
            // Smaller degrees = sharper corners.
            float cosThreshold = (float)Math.Cos(angleThresholdDegrees * Math.PI / 180.0);

            bool[] keep = new bool[points.Count];
            keep[0] = true;
            keep[points.Count - 1] = true;

            for (int i = 1; i < points.Count - 1; i++)
            {
                if (IsSharpCorner(points[i - 1], points[i], points[i + 1], cosThreshold))
                {
                    keep[i] = true;
                }
            }

            SimplifyRange(points, 0, points.Count - 1, distanceEpsilon, keep);

            List<PointF> result = [];
            for (int i = 0; i < points.Count; i++)
            {
                if (keep[i]) result.Add(points[i]);
            }
            return result;
        }

        private static void SimplifyRange(List<PointF> points, int first, int last, float epsilon, bool[] keep)
        {
            if (last - first < 2) return;

            float maxDist = 0;
            int index = 0;

            for (int i = first + 1; i < last; i++)
            {
                float dist = PerpendicularDistance(points[i], points[first], points[last]);
                if (dist > maxDist)
                {
                    maxDist = dist;
                    index = i;
                }
            }

            if (maxDist > epsilon)
            {
                keep[index] = true;
                SimplifyRange(points, first, index, epsilon, keep);
                SimplifyRange(points, index, last, epsilon, keep);
            }
        }

        private static bool IsSharpCorner(PointF p1, PointF p2, PointF p3, float cosThreshold)
        {
            float v1x = p1.X - p2.X;
            float v1y = p1.Y - p2.Y;
            float v2x = p3.X - p2.X;
            float v2y = p3.Y - p2.Y;

            float mag1 = (float)Math.Sqrt(v1x * v1x + v1y * v1y);
            float mag2 = (float)Math.Sqrt(v2x * v2x + v2y * v2y);

            if (mag1 < 0.001f || mag2 < 0.001f) return false;

            float dot = (v1x * v2x + v1y * v2y) / (mag1 * mag2);

            // In dot product, -1 is a straight line, 0 is 90 degrees, 1 is a 180-degree reversal.
            // We keep the point if the angle is sharper than our threshold.
            return dot > cosThreshold;
        }

        public static List<PointF> TraceMooreBoundary(int startX, int startY, bool[,] mask, ColorGrid grid)
        {
            var boundaryPoints = new List<PointF>();
            int width = grid.Width;
            int height = grid.Height;

            PointF[] directions =
            [
                new(0, -1), new(-1, -1), new(-1, 0),
                new(-1, 1), new(0, 1), new(1, 1),
                new(1, 0), new(1, -1)
            ];

            int currentX = startX;
            int currentY = startY;
            int startDir = 0;
            int maxIterations = width * height * 2;

            do
            {
                boundaryPoints.Add(new PointF(currentX, currentY));

                bool foundNext = false;
                for (int i = 0; i < 8; i++)
                {
                    int dir = (startDir + i) % 8;
                    int nx = (int)(currentX + directions[dir].X);
                    int ny = (int)(currentY + directions[dir].Y);

                    if (nx >= 0 && nx < width && ny >= 0 && ny < height && mask[nx, ny])
                    {
                        currentX = nx;
                        currentY = ny;
                        startDir = (dir + 5) % 8;
                        foundNext = true;
                        break;
                    }
                }

                if (!foundNext) break;

                if (currentX == startX && currentY == startY && boundaryPoints.Count > 1)
                    break;

                if (boundaryPoints.Count > maxIterations)
                    break;

            } while (true);

            return boundaryPoints;
        }

        public static PointF? FindBoundaryStart(bool[,] mask, ColorGrid grid)
        {
            int width = grid.Width;
            int height = grid.Height;

            // Simple scan for first boundary pixel
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (mask[x, y] && IsBoundaryInMask(x, y, mask, grid))
                    {
                        return new PointF(x, y);
                    }
                }
            }

            return null;
        }

        public static void SimplifyRecursive(List<PointF> points, int start, int end, float tolerance, bool[] keep)
        {
            if (end <= start + 1) return;

            float maxDistance = 0;
            int maxIndex = start;

            // Find point with maximum distance from line segment
            PointF lineStart = points[start];
            PointF lineEnd = points[end];

            for (int i = start + 1; i < end; i++)
            {
                float distance = PerpendicularDistance(points[i], lineStart, lineEnd);
                if (distance > maxDistance)
                {
                    maxDistance = distance;
                    maxIndex = i;
                }
            }

            // If max distance is greater than tolerance, recursively simplify
            if (maxDistance > tolerance)
            {
                keep[maxIndex] = true;
                SimplifyRecursive(points, start, maxIndex, tolerance, keep);
                SimplifyRecursive(points, maxIndex, end, tolerance, keep);
            }
        }

        public static float PerpendicularDistance(PointF point, PointF lineStart, PointF lineEnd)
        {
            float area = Math.Abs(
                (lineEnd.Y - lineStart.Y) * point.X -
                (lineEnd.X - lineStart.X) * point.Y +
                lineEnd.X * lineStart.Y -
                lineEnd.Y * lineStart.X
            );

            float lineLength = (float)Math.Sqrt(
                Math.Pow(lineEnd.X - lineStart.X, 2) +
                Math.Pow(lineEnd.Y - lineStart.Y, 2)
            );

            // Handle zero-length lines
            return lineLength < 0.001 ? 0 : area / lineLength;
        }

        public static List<ShapePolygon> RemoveContainedStrokes(List<ShapePolygon> strokes, float proximityThreshold = 4.0f)
        {
            if (strokes.Count == 0) return strokes;

            var result = new List<ShapePolygon>();
            var removed = new bool[strokes.Count];

            var bounds = strokes.Select(s => GetBounds(s.Points)).ToArray();
            
            if (strokes.Count < 4000)
            {
                for (int i = 0; i < strokes.Count; i++)
                {
                    if (removed[i]) continue;

                    bool isContained = false;

                    for (int j = 0; j < strokes.Count; j++)
                    {
                        if (i == j || removed[j]) continue;

                        if (bounds[j].Contains(bounds[i].X, bounds[i].Y) &&
                            bounds[j].Contains(bounds[i].Right, bounds[i].Bottom))
                        {
                            if (IsFullyContained(strokes[i], strokes[j], proximityThreshold))
                            {
                                isContained = true;
                                removed[i] = true;
                                break;
                            }
                        }
                    }

                    if (!isContained)
                    {
                        result.Add(strokes[i]);
                    }
                }

                return result;
            }
            else
            {
                return strokes;
            }
        }

        public static bool IsFullyContained(ShapePolygon strokeA, ShapePolygon strokeB, float proximityThreshold)
        {
            if (strokeA.Points.Count == 0) return true;
            if (strokeB.Points.Count == 0) return false;

            var lookupB = new HashSet<(int, int)>();
            foreach (var p in strokeB.Points)
            {
                lookupB.Add(((int)Math.Round(p.X), (int)Math.Round(p.Y)));
            }

            int margin = (int)Math.Ceiling(proximityThreshold);

            foreach (var pointA in strokeA.Points)
            {
                int ax = (int)Math.Round(pointA.X);
                int ay = (int)Math.Round(pointA.Y);

                bool foundClose = false;

                for (int x = ax - margin; x <= ax + margin && !foundClose; x++)
                {
                    for (int y = ay - margin; y <= ay + margin && !foundClose; y++)
                    {
                        if (lookupB.Contains((x, y)))
                        {
                            foundClose = true;
                        }
                    }
                }

                if (!foundClose)
                {
                    return false;
                }
            }

            return true;
        }

        public static bool IsBoundaryInMask(int x, int y, bool[,] mask, ColorGrid grid)
        {
            if (!mask[x, y]) return false;

            for (int dy = -1; dy <= 1; dy++)
            {
                for (int dx = -1; dx <= 1; dx++)
                {
                    if (dx == 0 && dy == 0) continue;

                    int nx = x + dx;
                    int ny = y + dy;

                    if (nx < 0 || nx >= grid.Width || ny < 0 || ny >= grid.Height || !mask[nx, ny])
                        return true;
                }
            }
            return false;
        }

        public static Color CalculateAverageColor(List<PointF> pixels, ColorGrid grid)
        {
            if (pixels.Count == 0) return Color.Black;

            long totalA = 0, totalR = 0, totalG = 0, totalB = 0;
            foreach (var point in pixels)
            {
                var color = Color.FromArgb(grid[(int)point.X, (int)point.Y]);
                totalA += color.A;
                totalR += color.R;
                totalG += color.G;
                totalB += color.B;
            }

            int count = pixels.Count;
            return Color.FromArgb(
                (int)(totalA / count),
                (int)(totalR / count),
                (int)(totalG / count),
                (int)(totalB / count)
            );
        }

        public static Color CalculateBoundaryColor(List<PointF> boundary, ColorGrid grid, bool isBoundry = false)
        {
            if (boundary.Count == 0) return Color.Black;

            int r = 0, g = 0, b = 0;
            int samples = Math.Min(boundary.Count, 100);

            Color darkest = Color.White;
            Color ligthest = Color.Black;

            for (int i = 0; i < samples; i++)
            {
                var point = boundary[i * boundary.Count / samples];
                var color = Color.FromArgb(grid[(int)point.X, (int)point.Y]);
                r += color.R;
                g += color.G;
                b += color.B;

                if (color.GetBrightness() < darkest.GetBrightness())
                {
                    darkest = color;
                }

                if (color.GetBrightness() > ligthest.GetBrightness())
                {
                    ligthest = color;
                }
            }

            Color result = Color.FromArgb(
                r / samples,
                g / samples,
                b / samples
            );

            if (isBoundry)
            {
                result = Color.FromArgb(
                    (int)(darkest.R * 0.5f + ligthest.R * 0.5f),
                    (int)(darkest.G * 0.5f + ligthest.G * 0.5f),
                    (int)(darkest.B * 0.5f + ligthest.B * 0.5f)
                );
            }

            return result;
        }

        public static float DistanceFromLine(PointF p, PointF p1, PointF p2)
        {
            float area = Math.Abs((p2.X - p1.X) * (p1.Y - p.Y) - (p1.X - p.X) * (p2.Y - p1.Y));
            float lineLength = (float)Math.Sqrt(Math.Pow(p2.X - p1.X, 2) + Math.Pow(p2.Y - p1.Y, 2));
            return area / lineLength;
        }

        public static PointF[] ConvertToQuadraticBezierPoints(PointF[] controlPoints, float tension = 0.5f)
        {
            if (controlPoints == null)
                return [];

            if (controlPoints.Length < 2)
                return controlPoints;

            var bezierPoints = new List<PointF>
            {
                controlPoints[0]
            };

            if (controlPoints.Length == 2)
            {
                PointF midPoint = new(
                    (controlPoints[0].X + controlPoints[1].X) / 2,
                    (controlPoints[0].Y + controlPoints[1].Y) / 2
                );
                bezierPoints.Add(midPoint);
                bezierPoints.Add(controlPoints[1]);
            }
            else
            {
                for (int i = 0; i < controlPoints.Length - 1; i++)
                {
                    PointF p0 = controlPoints[i];
                    PointF p1 = controlPoints[i + 1];

                    PointF control;

                    if (i == 0)
                    {
                        control = new PointF(
                            p0.X + (p1.X - p0.X) * tension,
                            p0.Y + (p1.Y - p0.Y) * tension
                        );
                    }
                    else if (i == controlPoints.Length - 2)
                    {
                        control = new PointF(
                            p1.X - (p1.X - p0.X) * tension,
                            p1.Y - (p1.Y - p0.Y) * tension
                        );
                    }
                    else
                    {
                        PointF p2 = controlPoints[i + 2];
                        control = new PointF(
                            (p0.X + p1.X + p1.X + p2.X) / 4,
                            (p0.Y + p1.Y + p1.Y + p2.Y) / 4
                        );
                    }

                    bezierPoints.Add(control);
                    bezierPoints.Add(p1);
                }
            }

            return [.. bezierPoints];
        }
    }
}
