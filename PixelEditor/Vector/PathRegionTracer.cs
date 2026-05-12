namespace PixelEditor.Vector
{
    public static class PathRegionTracer
    {
        public static List<ShapePolygon> TraceStrokes(ColorGrid grid,
            float colorTolerance = 0.15f,
            float simplificationTolerance = 0.75f,
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
            bool[,] mask = PathManipulator.CreateRegionMask(region, grid);
            PointF? startPoint = PathManipulator.FindBoundaryStart(mask, grid);

            if (!startPoint.HasValue)
                return [];

            return PathManipulator.TraceMooreBoundary((int)startPoint.Value.X, (int)startPoint.Value.Y, mask, grid);
        }

        private static List<PointF> ExtractBoundaryWithGapFilling(ColorRegion region, ColorGrid grid)
        {
            float gapColorTolerance = 0.3f;
            int gapSearchDistance = 1;

            var baseBoundary = ExtractRegionBoundary(region, grid);
            if (baseBoundary.Count == 0)
                return baseBoundary;

            bool[,] regionMask = PathManipulator.CreateRegionMask(region, grid);

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

            Color boundaryColor = PathManipulator.CalculateBoundaryColor(boundary, grid);

            PointF[] searchDirections = PathManipulator.GetSearchDirections();

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
            float colorDiff = PathManipulator.ColorDistance(boundaryColor, pixelColor);
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
                    if (PathManipulator.ColorDistance(color, neighborColor) <= tolerance)
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
            PointF? startPoint = PathManipulator.FindBoundaryStart(expandedMask, grid);
            if (!startPoint.HasValue)
                return [];

            return PathManipulator.TraceMooreBoundary((int)startPoint.Value.X, (int)startPoint.Value.Y, expandedMask, grid);
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

                            if (PathManipulator.ColorDistance(seedColor, currentColor) <= colorTolerance)
                            {
                                visited[(int)point.X, (int)point.Y] = true;
                                region.Pixels.Add(point);
                                region.Bounds = PathManipulator.UpdateBounds(region.Bounds, point);

                                foreach (var (dx, dy) in neighborOffsets)
                                {
                                    stack.Push(new PointF(point.X + dx, point.Y + dy));
                                }
                            }
                        }

                        if (region.Pixels.Count >= minRegionSize)
                        {
                            region.AverageColor = PathManipulator.CalculateAverageColor(region.Pixels, grid);
                            regions.Add(region);
                        }
                        else
                        {
                            region.AverageColor = PathManipulator.CalculateAverageColor(region.Pixels, grid);
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

            points = PathManipulator.SimplifyPath(points, simplificationTolerance);

            foreach (var point in points)
            {
                stroke.Points.Add(point);
            }

            stroke.LineColor = PathManipulator.CalculateBoundaryColor(region.Boundary, grid);
            stroke.FillColor = region.AverageColor;

            return stroke;
        }
    }
}