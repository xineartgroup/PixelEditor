using PixelEditor.Vector;

namespace PixelEditor
{
    public static class StrokeRegionTracer
    {
        public static List<ShapePolygon> TraceStrokes(ColorGrid grid,
            float colorTolerance = 0.15f,
            float simplificationTolerance = 1.0f,
            bool fillBoundaryGaps = true)
        {
            int minRegionSize = colorTolerance >= 0.15f ? 4 : colorTolerance >= 0.075f ? 2 : 0;

            var regions = FindColorRegions(grid, colorTolerance, minRegionSize);

            var strokes = new List<ShapePolygon>();
            foreach (var region in regions)
            {
                region.Boundary = fillBoundaryGaps
                    ? ExtractBoundaryWithGapFilling(region, grid)
                    : ExtractRegionBoundary(region, grid);

                if (region.Boundary.Count >= 2)
                {
                    var stroke = RegionToStroke(region, grid, simplificationTolerance);
                    stroke.IsClosed = true;
                    strokes.Add(stroke);
                }
            }

            return strokes;
        }

        private static List<PointF> ExtractRegionBoundary(ColorRegion region, ColorGrid grid)
        {
            bool[,] mask = StrokeUtility.CreateRegionMask(region, grid);
            PointF? startPoint = StrokeUtility.FindBoundaryStart(mask, grid);

            if (!startPoint.HasValue)
                return [];

            return StrokeUtility.TraceMooreBoundary((int)startPoint.Value.X, (int)startPoint.Value.Y, mask, grid);
        }

        private static List<PointF> ExtractBoundaryWithGapFilling(ColorRegion region, ColorGrid grid)
        {
            float gapColorTolerance = 0.3f;
            int gapSearchDistance = 1;

            var baseBoundary = ExtractRegionBoundary(region, grid);
            if (baseBoundary.Count == 0)
                return baseBoundary;

            bool[,] regionMask = StrokeUtility.CreateRegionMask(region, grid);

            var gapPixels = FindBoundaryGapPixels(baseBoundary, regionMask, grid, gapSearchDistance, gapColorTolerance);

            if (gapPixels.Count == 0)
                return baseBoundary;

            bool[,] expandedMask = ExpandMaskWithGaps(regionMask, gapPixels, grid);

            return TraceBoundaryOfExpandedRegion(expandedMask, grid);
        }

        private static List<PointF> FindBoundaryGapPixels(List<PointF> boundary, bool[,] regionMask, ColorGrid grid, int searchDistance, float colorTolerance)
        {
            var gapPixels = new List<PointF>();
            var visitedGaps = new bool[grid.Width, grid.Height];

            Color boundaryColor = StrokeUtility.CalculateBoundaryColor(boundary, grid);

            PointF[] searchDirections = StrokeUtility.GetSearchDirections();

            foreach (var boundaryPoint in boundary)
            {
                for (int dist = 1; dist <= searchDistance; dist++)
                {
                    foreach (var dir in searchDirections)
                    {
                        PointF candidate = new(
                            boundaryPoint.X + dir.X * dist,
                            boundaryPoint.Y + dir.Y * dist);

                        if (candidate.X < 0 || candidate.X >= grid.Width ||
                            candidate.Y < 0 || candidate.Y >= grid.Height)
                            continue;

                        if (visitedGaps[(int)candidate.X, (int)candidate.Y] ||
                            regionMask[(int)candidate.X, (int)candidate.Y])
                            continue;

                        if (IsPotentialGapPixel(candidate, boundaryColor, grid, colorTolerance))
                        {
                            gapPixels.Add(candidate);
                            visitedGaps[(int)candidate.X, (int)candidate.Y] = true;
                        }
                    }
                }
            }

            return gapPixels;
        }

        private static bool IsPotentialGapPixel(PointF point, Color boundaryColor, ColorGrid grid, float colorTolerance)
        {
            Color pixelColor = Color.FromArgb(grid[(int)point.X, (int)point.Y]);

            // Check 1: Not too different from boundary color
            float colorDiff = StrokeUtility.ColorDistance(boundaryColor, pixelColor);
            if (colorDiff > colorTolerance)
                return false;

            // Check 2: Not too close to white (optional - adjust threshold as needed)
            float whiteness = (pixelColor.R + pixelColor.G + pixelColor.B) / (3.0f * 255.0f);
            if (whiteness > 0.95) // 95% white or brighter
                return false;

            // Check 3: Not an isolated pixel (has neighbors of similar color)
            return HasSimilarNeighbors(point, pixelColor, grid, colorTolerance * 1.5f);
        }

        private static bool HasSimilarNeighbors(PointF point, Color color, ColorGrid grid, float tolerance)
        {
            int similarNeighbors = 0;

            // Check 4-connected neighbors
            PointF[] neighbors =
            [
                new(-1, 0), new(1, 0),
                new(0, -1), new(0, 1)
            ];

            foreach (var dir in neighbors)
            {
                PointF neighbor = new(point.X + dir.X, point.Y + dir.Y);

                if (neighbor.X >= 0 && neighbor.X < grid.Width &&
                    neighbor.Y >= 0 && neighbor.Y < grid.Height)
                {
                    Color neighborColor = Color.FromArgb(grid[(int)neighbor.X, (int)neighbor.Y]);
                    if (StrokeUtility.ColorDistance(color, neighborColor) <= tolerance)
                    {
                        similarNeighbors++;
                    }
                }
            }

            // Need at least 1 similar neighbor (prevents isolated noise)
            return similarNeighbors >= 1;
        }

        private static bool[,] ExpandMaskWithGaps(bool[,] originalMask, List<PointF> gapPixels, ColorGrid grid)
        {
            int width = grid.Width;
            int height = grid.Height;
            bool[,] expandedMask = new bool[width, height];

            // Copy original region
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    expandedMask[x, y] = originalMask[x, y];
                }
            }

            // Add gap pixels
            foreach (var gap in gapPixels)
            {
                expandedMask[(int)gap.X, (int)gap.Y] = true;
            }

            return expandedMask;
        }

        private static List<PointF> TraceBoundaryOfExpandedRegion(bool[,] expandedMask, ColorGrid grid)
        {
            PointF? startPoint = StrokeUtility.FindBoundaryStart(expandedMask, grid);
            if (!startPoint.HasValue)
                return [];

            return StrokeUtility.TraceMooreBoundary((int)startPoint.Value.X, (int)startPoint.Value.Y, expandedMask, grid);
        }

        private static List<ColorRegion> FindColorRegions(ColorGrid grid, float colorTolerance, int minRegionSize)
        {
            var regions = new List<ColorRegion>();
            var stray_regions = new List<ColorRegion>();
            var visited = new bool[grid.Width, grid.Height];

            var neighborOffsets = new (int dx, int dy)[]
            {
                (-1, 0), (1, 0), (0, -1), (0, 1), (-1, -1), (-1, 1), (1, -1), (1, 1)
            };

            for (int y = 0; y < grid.Height; y++)
            {
                for (int x = 0; x < grid.Width; x++)
                {
                    if (!visited[x, y])
                    {
                        var seedColor = Color.FromArgb(grid[x, y]);
                        var region = new ColorRegion();

                        var stack = new Stack<PointF>();
                        stack.Push(new PointF(x, y));

                        while (stack.Count > 0)
                        {
                            var point = stack.Pop();

                            if (point.X < 0 || point.X >= grid.Width ||
                                point.Y < 0 || point.Y >= grid.Height)
                                continue;

                            if (visited[(int)point.X, (int)point.Y])
                                continue;

                            var currentColor = Color.FromArgb(grid[(int)point.X, (int)point.Y]);

                            if (StrokeUtility.ColorDistance(seedColor, currentColor) <= colorTolerance)
                            {
                                visited[(int)point.X, (int)point.Y] = true;
                                region.Pixels.Add(point);
                                region.Bounds = StrokeUtility.UpdateBounds(region.Bounds, point);

                                foreach (var (dx, dy) in neighborOffsets)
                                {
                                    stack.Push(new PointF(point.X + dx, point.Y + dy));
                                }
                            }
                        }

                        if (region.Pixels.Count >= minRegionSize)
                        {
                            region.AverageColor = StrokeUtility.CalculateAverageColor(region.Pixels, grid);
                            regions.Add(region);
                        }
                        else
                        {
                            region.AverageColor = StrokeUtility.CalculateAverageColor(region.Pixels, grid);
                            stray_regions.Add(region);
                        }
                    }
                }
            }

            return regions; // MergeStrayRegions(regions, stray_regions, grid, minRegionSize); // 
        }

        private static ShapePolygon RegionToStroke(ColorRegion region, ColorGrid grid, float simplificationTolerance)
        {
            var stroke = new ShapePolygon();

            var points = region.Boundary;

            foreach (var point in points)
            {
                stroke.Points.Add(point);
            }

            stroke.LineColor = StrokeUtility.CalculateBoundaryColor(region.Boundary, grid);
            stroke.FillColor = region.AverageColor;

            return stroke;
        }

        private static List<ColorRegion> MergeStrayRegions(List<ColorRegion> mainRegions, List<ColorRegion> strayRegions, ColorGrid grid, int minRegionSize, int proximityThreshold = 0)
        {
            var finalRegions = new List<ColorRegion>(mainRegions);
            var remainingStrays = new List<ColorRegion>(strayRegions);

            remainingStrays.Sort((a, b) => b.Pixels.Count.CompareTo(a.Pixels.Count));

            foreach (var stray in remainingStrays.ToList())
            {
                ColorRegion bestTarget = null;
                double bestDistance = double.MaxValue;

                foreach (var main in finalRegions)
                {
                    double dist = GetMinDistanceBetweenBounds(stray.Bounds, main.Bounds);
                    if (dist < bestDistance)
                    {
                        bestDistance = dist;
                        bestTarget = main;
                    }
                }

                foreach (var other in remainingStrays)
                {
                    if (other == stray) continue;
                    double dist = GetMinDistanceBetweenBounds(stray.Bounds, other.Bounds);
                    if (dist < bestDistance)
                    {
                        bestDistance = dist;
                        bestTarget = other;
                    }
                }

                if (bestDistance <= proximityThreshold && bestTarget != null)
                {
                    foreach (var p in stray.Pixels)
                    {
                        bestTarget.Pixels.Add(p);
                        bestTarget.Bounds = StrokeUtility.UpdateBounds(bestTarget.Bounds, p);
                    }

                    bestTarget.AverageColor = StrokeUtility.CalculateAverageColor(bestTarget.Pixels, grid); // Color.Green; // 

                    remainingStrays.Remove(stray);

                    if (!finalRegions.Contains(bestTarget) && bestTarget.Pixels.Count >= minRegionSize)
                    {
                        finalRegions.Add(bestTarget);
                        remainingStrays.Remove(bestTarget);
                    }
                }
            }

            foreach (var stray in remainingStrays)
            {
                if (stray.Pixels.Count >= minRegionSize)
                {
                    finalRegions.Add(stray);
                }
            }

            return finalRegions;
        }

        private static double GetMinDistanceBetweenBounds(Rectangle a, Rectangle b)
        {
            if (a.IntersectsWith(b)) return 0;

            int dx = Math.Max(Math.Max(a.Left - b.Right, b.Left - a.Right), 0);
            int dy = Math.Max(Math.Max(a.Top - b.Bottom, b.Top - a.Bottom), 0);

            return Math.Sqrt(dx * dx + dy * dy);
        }
    }
}