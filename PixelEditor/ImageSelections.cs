namespace PixelEditor
{
    public static class ImageSelections
    {
        private static readonly List<SelectionPolygon> selectionPolygons = [];
        private static RectangleF selectionBounds;
        private static PointF selectionCenter;

        public const float ROTATION_HANDLE_SIZE = 15;
        public const float SCALE_HANDLE_SIZE = 10;

        public static RectangleF GetSelectionBounds()
        {
            return selectionBounds;
        }

        public static PointF GetSelectionCenter()
        {
            return selectionCenter;
        }

        public static bool ContainsSelection()
        {
            for (int i = 0; i < selectionPolygons.Count; i++)
            {
                if (selectionPolygons[i].Points.Count > 0)
                {
                    return true;
                }
            }
            return false;
        }

        public static void AddSelectionPoint(Point worldPoint)
        {
            if (selectionPolygons.Count == 0)
            {
                selectionPolygons.Add(new SelectionPolygon());
            }
            selectionPolygons[^1].Points.Add(worldPoint);
        }

        public static void AddSelectionPoints(IEnumerable<Point> worldPoints)
        {
            if (selectionPolygons.Count == 0)
            {
                selectionPolygons.Add(new SelectionPolygon());
            }
            selectionPolygons[^1].Points.AddRange(worldPoints);
        }

        public static void UpdateSelectionPoint(int index, Point worldPoint)
        {
            if (selectionPolygons.Count > 0)
            {
                selectionPolygons[^1].Points[index] = worldPoint;
            }
        }

        public static void IncreaseSelectionPolygons(bool[,]? mask, Point point, bool adding = true)
        {
            selectionPolygons.Add(new SelectionPolygon(mask, point, adding, []));
        }

        public static void ClearSelections()
        {
            selectionPolygons.Clear();
        }

        public static void InvertSelections(int x, int y, int width, int height)
        {
            bool[,] mask = new bool[width, height];

            foreach (var poly in selectionPolygons)
            {
                int rows1 = mask.GetLength(0);
                int cols1 = mask.GetLength(1);
                if (poly.Mask != null)
                {
                    if (rows1 != poly.Mask.GetLength(0) || cols1 != poly.Mask.GetLength(1))
                    {
                        mask = ExtendMask(mask, Math.Max(rows1, poly.Mask.GetLength(0)), Math.Max(cols1, poly.Mask.GetLength(1)));
                    }
                    AddMask(mask, poly.Mask, poly.Adding);
                }
                else
                {
                    if (rows1 < width || cols1 < height)
                    {
                        mask = ExtendMask(mask, width, height);
                    }
                    FillPolygonInMask(mask, [.. poly.Points.Select(p => new Point(p.X, p.Y))], poly.Adding);
                }
            }

            for (int xM = 0; xM < width; xM++)
            {
                for (int yM = 0; yM < height; yM++)
                {
                    mask[xM, yM] = !mask[xM, yM];
                }
            }

            List<SelectionPolygon> invertedPolygons = GetSelectionPointsFromMask(mask, Point.Empty, true);

            selectionPolygons.Clear();
            foreach (var outline in invertedPolygons)
            {
                SelectionPolygon selectionPolygon = new(outline.Mask, outline.SelectionPoint, outline.Adding, [.. outline.Points.Select(p => new Point(p.X + x, p.Y + y))]);
                selectionPolygons.Add(selectionPolygon);
            }
        }

        public static void GrowSelections(int x, int y, int width, int height, int count)
        {
            bool[,] mask = new bool[width, height];

            foreach (var poly in selectionPolygons)
            {
                int rows1 = mask.GetLength(0);
                int cols1 = mask.GetLength(1);
                if (poly.Mask != null)
                {
                    if (rows1 != poly.Mask.GetLength(0) || cols1 != poly.Mask.GetLength(1))
                    {
                        mask = ExtendMask(mask, Math.Max(rows1, poly.Mask.GetLength(0)), Math.Max(cols1, poly.Mask.GetLength(1)));
                    }
                    AddMask(mask, poly.Mask, poly.Adding);
                }
                else
                {
                    if (rows1 < width || cols1 < height)
                    {
                        mask = ExtendMask(mask, width, height);
                    }
                    FillPolygonInMask(mask, [.. poly.Points.Select(p => new Point(p.X, p.Y))], poly.Adding);
                }
            }

            for (int step = 0; step < count; step++)
            {
                bool[,] tempMask = (bool[,])mask.Clone();
                for (int my = 0; my < height; my++)
                {
                    for (int mx = 0; mx < width; mx++)
                    {
                        if (!mask[mx, my])
                        {
                            if ((mx > 0 && mask[mx - 1, my]) ||
                                (mx < width - 1 && mask[mx + 1, my]) ||
                                (my > 0 && mask[mx, my - 1]) ||
                                (my < height - 1 && mask[mx, my + 1]))
                            {
                                tempMask[mx, my] = true;
                            }
                        }
                    }
                }
                mask = tempMask;
            }

            List<SelectionPolygon> invertedPolygons = GetSelectionPointsFromMask(mask, Point.Empty, true);

            selectionPolygons.Clear();
            foreach (var outline in invertedPolygons)
            {
                SelectionPolygon selectionPolygon = new(outline.Mask, outline.SelectionPoint, outline.Adding, [.. outline.Points.Select(p => new Point(p.X + x, p.Y + y))]);
                selectionPolygons.Add(selectionPolygon);
            }
        }

        public static void ShrinkSelections(int x, int y, int width, int height, int count)
        {
            bool[,] mask = new bool[width, height];

            foreach (var poly in selectionPolygons)
            {
                int rows1 = mask.GetLength(0);
                int cols1 = mask.GetLength(1);
                if (poly.Mask != null)
                {
                    if (rows1 != poly.Mask.GetLength(0) || cols1 != poly.Mask.GetLength(1))
                    {
                        mask = ExtendMask(mask, Math.Max(rows1, poly.Mask.GetLength(0)), Math.Max(cols1, poly.Mask.GetLength(1)));
                    }
                    AddMask(mask, poly.Mask, poly.Adding);
                }
                else
                {
                    if (rows1 < width || cols1 < height)
                    {
                        mask = ExtendMask(mask, width, height);
                    }
                    FillPolygonInMask(mask, [.. poly.Points.Select(p => new Point(p.X, p.Y))], poly.Adding);
                }
            }

            for (int step = 0; step < count; step++)
            {
                bool[,] tempMask = new bool[width, height];
                for (int my = 0; my < height; my++)
                {
                    for (int mx = 0; mx < width; mx++)
                    {
                        if (mask[mx, my])
                        {
                            if (mx > 0 && mx < width - 1 && my > 0 && my < height - 1 &&
                                mask[mx - 1, my] && mask[mx + 1, my] &&
                                mask[mx, my - 1] && mask[mx, my + 1])
                            {
                                tempMask[mx, my] = true;
                            }
                        }
                    }
                }
                mask = tempMask;
            }

            List<SelectionPolygon> invertedPolygons = GetSelectionPointsFromMask(mask, Point.Empty, true);

            selectionPolygons.Clear();
            foreach (var outline in invertedPolygons)
            {
                SelectionPolygon selectionPolygon = new(outline.Mask, outline.SelectionPoint, outline.Adding, [.. outline.Points.Select(p => new Point(p.X + x, p.Y + y))]);
                selectionPolygons.Add(selectionPolygon);
            }
        }

        public static List<SelectionPolygon> GetSelections()
        {
            return selectionPolygons;
        }

        public static List<Point> GetLastSelection()
        {
            if (selectionPolygons.Count > 0)
            {
                return selectionPolygons[^1].Points;
            }
            return [];
        }

        public static void MoveSelection(float worldDx, float worldDy)
        {
            for (int i = 0; i < selectionPolygons.Count; i++)
            {
                for (int j = 0; j < selectionPolygons[i].Points.Count; j++)
                {
                    selectionPolygons[i].Points[j] = new Point(
                        (int)(selectionPolygons[i].Points[j].X + worldDx),
                        (int)(selectionPolygons[i].Points[j].Y + worldDy));
                }
            }
            CalculateSelectionBounds();
        }

        public static void MasAndMergeSelections(int x, int y, int width, int height)
        {
            //if (selectionPolygons.Count < 2) return;

            if (selectionPolygons.Count > 100) return; // Too many regions will slow it down, no need to merge.

            bool[,] mask = new bool[width, height];

            foreach (var poly in selectionPolygons)
            {
                if (poly.Mask != null)
                {
                    if (width != poly.Mask.GetLength(0) || height != poly.Mask.GetLength(1))
                    {
                        mask = ExtendMask(mask, Math.Max(width, poly.Mask.GetLength(0)), Math.Max(height, poly.Mask.GetLength(1)));
                    }
                    AddMask(mask, poly.Mask, poly.Adding);
                }
                else
                {
                    FillPolygonInMask(mask, [.. poly.Points.Select(p => new Point(p.X, p.Y))], poly.Adding);
                }
            }

            Point point = (selectionPolygons.Count > 0) ? selectionPolygons[^1].SelectionPoint : Point.Empty;

            List <SelectionPolygon> mergedOutlines = GetSelectionPointsFromMask(mask, point, true);

            if (mergedOutlines.Count < selectionPolygons.Count)
            {
                selectionPolygons.Clear();
                foreach (var outline in mergedOutlines)
                {
                    SelectionPolygon selectionPolygon = new(outline.Mask, outline.SelectionPoint, outline.Adding, [.. outline.Points.Select(p => new Point(p.X + x, p.Y + y))]);
                    selectionPolygons.Add(selectionPolygon);
                }
            }
            else
            {
                for (int i = 0; i < selectionPolygons.Count; i++)
                {
                    selectionPolygons[i].Mask = mask; //all polygons share 1 mask
                }
            }
        }

        public static void CalculateSelectionBounds()
        {
            if (!ContainsSelection()) return;

            float minX = float.MaxValue, minY = float.MaxValue;
            float maxX = float.MinValue, maxY = float.MinValue;

            var selectionPoints = GetSelections();
            for (int i = 0; i < selectionPoints.Count; i++)
            {
                foreach (var point in selectionPoints[i].Points)
                {
                    minX = Math.Min(minX, point.X);
                    minY = Math.Min(minY, point.Y);
                    maxX = Math.Max(maxX, point.X);
                    maxY = Math.Max(maxY, point.Y);
                }
            }

            selectionBounds = new RectangleF(minX, minY, maxX - minX, maxY - minY);
            selectionCenter = new PointF(
                selectionBounds.X + selectionBounds.Width / 2,
                selectionBounds.Y + selectionBounds.Height / 2
            );
        }

        public static float Distance(Point p1, PointF p2)
        {
            float dx = p1.X - p2.X;
            float dy = p1.Y - p2.Y;
            return (float)Math.Sqrt(dx * dx + dy * dy);
        }

        public static float Distance(PointF p1, PointF p2)
        {
            float dx = p1.X - p2.X;
            float dy = p1.Y - p2.Y;
            return (float)Math.Sqrt(dx * dx + dy * dy);
        }

        public static int IsPointInSelection(Point worldPoint)
        {
            for (int i = 0; i < selectionPolygons.Count; i++)
            {
                if (IsPointInPolygon(worldPoint, selectionPolygons[i]))
                {
                    return i;
                }
            }
            return -1;
        }

        public static bool IsPointInPolygon(Point point, SelectionPolygon polygon)
        {
            int n = polygon.Points.Count;
            bool inside = false;
            for (int i = 0, j = n - 1; i < n; j = i++)
            {
                if ((polygon.Points[i].Y > point.Y) != (polygon.Points[j].Y > point.Y) &&
                    (point.X < (polygon.Points[j].X - polygon.Points[i].X) * (point.Y - polygon.Points[i].Y) / (polygon.Points[j].Y - polygon.Points[i].Y) + polygon.Points[i].X))
                {
                    inside = !inside;
                }
            }
            return inside;
        }

        public static float GetScreenToWorldScale(int worldWidth, int worldHeight, int canvasWidth, int canvasHeight, float zoom)
        {
            float aspectRatio = (float)worldWidth / worldHeight;
            float containerAspectRatio = (float)canvasWidth / canvasHeight;
            float scaledWidth = aspectRatio > containerAspectRatio
                ? canvasWidth * zoom
                : canvasHeight * zoom * aspectRatio;
            return scaledWidth / worldWidth;
        }

        public static bool[,] ExtendMask(bool[,] originalMask, int additionalWidth, int additionalHeight)
        {
            int oldRows = originalMask.GetLength(0);
            int oldCols = originalMask.GetLength(1);

            int newRows = oldRows + additionalHeight;
            int newCols = oldCols + additionalWidth;

            bool[,] newMask = new bool[newRows, newCols]; // New arrays in C# are initialized to 'false' by default

            for (int i = 0; i < oldRows; i++)
            {
                for (int j = 0; j < oldCols; j++)
                {
                    newMask[i, j] = originalMask[i, j];
                }
            }

            return newMask;
        }

        public static void AddMask(bool[,] mask1, bool[,] mask2, bool adding)
        {
            int rows1 = mask1.GetLength(0);
            int cols1 = mask1.GetLength(1);
            int rows2 = mask2.GetLength(0);
            int cols2 = mask2.GetLength(1);

            if (rows1 != rows2 || cols1 != cols2)
            {
                return;
            }

            for (int i = 0; i < rows1; i++)
            {
                for (int j = 0; j < cols1; j++)
                {
                    if (adding)
                    {
                        mask1[i, j] = mask1[i, j] || mask2[i, j]; // Logical OR: If either is true, result is true
                    }
                    else
                    {
                        if (mask2[i, j])
                        {
                            mask1[i, j] = false; // Subtraction: If mask2 is true, force mask1 to false
                        }
                    }
                }
            }
        }

        public static void FillPolygonInMask(bool[,] mask, List<Point> points, bool fill)
        {
            int width = mask.GetLength(0);
            int height = mask.GetLength(1);

            if (points.Count == 0) return;

            int minY = points.Min(p => p.Y);
            int maxY = points.Max(p => p.Y);

            for (int y = minY; y <= maxY; y++)
            {
                List<int> xIntersections = [];
                for (int i = 0; i < points.Count; i++)
                {
                    Point p1 = points[i];
                    Point p2 = points[(i + 1) % points.Count];

                    if ((p1.Y <= y && p2.Y > y) || (p2.Y <= y && p1.Y > y))
                    {
                        int x = p1.X + (y - p1.Y) * (p2.X - p1.X) / (p2.Y - p1.Y);
                        xIntersections.Add(x);
                    }
                }
                xIntersections.Sort();

                for (int i = 0; i < xIntersections.Count; i += 2)
                {
                    int startX = Math.Max(0, xIntersections[i]);
                    int endX = Math.Min(width - 1, xIntersections[i + 1]);
                    for (int x = startX; x <= endX; x++)
                    {
                        if (x >= 0 && x < width && y >= 0 && y < height)
                        {
                            mask[x, y] = fill;
                        }
                    }
                }
            }
        }

        public static List<SelectionPolygon> GetSelectionPointsFromMask(bool[,] mask, Point position, bool adding)
        {
            List<SelectionPolygon> regions = [];

            if (mask == null)
                return regions;

            bool inner = true;

            int width = mask.GetLength(0);
            int height = mask.GetLength(1);

            if (position != Point.Empty &&
                position.X >= 0 && position.X < width &&
                position.Y >= 0 && position.Y < height)
            {
                inner = mask[position.X, position.Y] != mask[0, 0];
            }

            int[] dx = [1, 0, -1, 0];
            int[] dy = [0, 1, 0, -1];

            bool[,] visited = new bool[width, height];

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (mask[x, y] == inner && !visited[x, y])
                    {
                        Point? start = null;

                        if (y == 0 || mask[x, y - 1] != inner)
                        {
                            start = new Point(x, y);
                        }

                        if (!start.HasValue)
                            continue;

                        SelectionPolygon outline = new();
                        int currentX = start.Value.X;
                        int currentY = start.Value.Y;
                        int dir = 0;

                        outline.Points.Add(new Point(currentX, currentY));
                        visited[currentX, currentY] = true;

                        do
                        {
                            int leftDir = (dir + 3) % 4;
                            int checkX = currentX + dx[leftDir];
                            int checkY = currentY + dy[leftDir];

                            bool pixelLeft = checkX >= 0 && checkX < width &&
                                            checkY >= 0 && checkY < height &&
                                            mask[checkX, checkY] == inner;

                            if (pixelLeft)
                            {
                                dir = leftDir;
                                currentX += dx[dir];
                                currentY += dy[dir];
                                outline.Points.Add(new Point(currentX, currentY));
                                visited[currentX, currentY] = true;
                            }
                            else
                            {
                                int forwardX = currentX + dx[dir];
                                int forwardY = currentY + dy[dir];

                                bool pixelForward = forwardX >= 0 && forwardX < width &&
                                                   forwardY >= 0 && forwardY < height &&
                                                   mask[forwardX, forwardY] == inner;

                                if (pixelForward)
                                {
                                    currentX = forwardX;
                                    currentY = forwardY;
                                    outline.Points.Add(new Point(currentX, currentY));
                                    visited[currentX, currentY] = true;
                                }
                                else
                                {
                                    dir = (dir + 1) % 4;
                                }
                            }

                        } while (!(currentX == start.Value.X && currentY == start.Value.Y && dir == 0));

                        if (outline.Points.Count > 2)
                        {
                            outline.SelectionPoint = position;
                            outline.Adding = adding;
                            outline.Mask = mask;
                            regions.Add(outline);
                        }
                    }
                }
            }

            return regions;
        }

        public static bool IsContainedWithinFast(SelectionPolygon inner, SelectionPolygon outer)
        {
            var thisBounds = GetBounds(inner);
            var otherBounds = GetBounds(outer);

            if (!IsContainedWithin(thisBounds, otherBounds))
                return false;

            if (inner.Points.Count < 20)
            {
                foreach (var point in inner.Points)
                {
                    if (!ContainsPoint(outer, point))
                        return false;
                }
                return true;
            }

            return IsContainedByEdgeTest(inner, outer);
        }

        private static bool IsContainedByEdgeTest(SelectionPolygon inner, SelectionPolygon outer)
        {
            Point interiorPoint = GetInteriorPoint(inner);

            if (!ContainsPoint(outer, interiorPoint))
                return false;

            for (int i = 0; i < inner.Points.Count; i++)
            {
                var p1 = inner.Points[i];
                var p2 = inner.Points[(i + 1) % inner.Points.Count];

                Point outsidePoint = GetPointOutsideEdge(p1, p2);

                if (ContainsPoint(outer, outsidePoint))
                {
                    if (!IsEdgeInside(outer, p1, p2))
                        return false;
                }
            }

            return true;
        }

        private static Rectangle GetBounds(SelectionPolygon polygon)
        {
            if (polygon.Points.Count == 0)
                return Rectangle.Empty;

            int minX = polygon.Points.Min(p => p.X);
            int minY = polygon.Points.Min(p => p.Y);
            int maxX = polygon.Points.Max(p => p.X);
            int maxY = polygon.Points.Max(p => p.Y);

            return new Rectangle(minX, minY, maxX - minX, maxY - minY);
        }

        public static bool ContainsPoint(SelectionPolygon polygon, Point point)
        {
            if (polygon.Points.Count < 3)
                return false;

            bool inside = false;
            for (int i = 0, j = polygon.Points.Count - 1; i < polygon.Points.Count; j = i++)
            {
                if (((polygon.Points[i].Y > point.Y) != (polygon.Points[j].Y > point.Y)) &&
                    (point.X < (polygon.Points[j].X - polygon.Points[i].X) * (point.Y - polygon.Points[i].Y) / (polygon.Points[j].Y - polygon.Points[i].Y) + polygon.Points[i].X))
                {
                    inside = !inside;
                }
            }
            return inside;
        }

        private static Point GetInteriorPoint(SelectionPolygon polygon)
        {
            if (polygon.Points.Count == 0)
                return Point.Empty;

            int sumX = 0, sumY = 0;
            foreach (var point in polygon.Points)
            {
                sumX += point.X;
                sumY += point.Y;
            }

            return new Point(sumX / polygon.Points.Count, sumY / polygon.Points.Count);
        }

        private static Point GetPointOutsideEdge(Point p1, Point p2)
        {
            int dx = p2.X - p1.X;
            int dy = p2.Y - p1.Y;

            int perpX = -dy;
            int perpY = dx;

            double length = Math.Sqrt(perpX * perpX + perpY * perpY);
            if (length > 0)
            {
                perpX = (int)(perpX / length * 2);
                perpY = (int)(perpY / length * 2);
            }

            int midX = (p1.X + p2.X) / 2;
            int midY = (p1.Y + p2.Y) / 2;

            return new Point(midX + perpX, midY + perpY);
        }

        private static bool IsEdgeInside(SelectionPolygon outer, Point p1, Point p2)
        {
            int steps = 5;
            for (int i = 1; i < steps; i++)
            {
                double t = (double)i / steps;
                int x = (int)(p1.X + t * (p2.X - p1.X));
                int y = (int)(p1.Y + t * (p2.Y - p1.Y));

                if (!ContainsPoint(outer, new Point(x, y)))
                    return false;
            }
            return true;
        }

        private static bool IsContainedWithin(Rectangle inner, Rectangle outer)
        {
            return inner.Left >= outer.Left &&
                   inner.Right <= outer.Right &&
                   inner.Top >= outer.Top &&
                   inner.Bottom <= outer.Bottom;
        }
    }
}