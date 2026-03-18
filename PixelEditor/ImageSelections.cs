namespace PixelEditor
{
    public static class ImageSelections
    {
        private static readonly List<SelectionPolygon> selectionPolygons = [];
        private static RectangleF selectionBounds;
        private static PointF selectionCenter;

        public const float ROTATION_HANDLE_SIZE = 15;
        public const float SCALE_HANDLE_SIZE = 10;

        public static bool IsInnerSelection()
        {
            if (selectionPolygons.Count > 0)
            {
                return selectionPolygons[^1].Inwards;
            }
            return true;
        }

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

        public static void UpdateSelectionPoint(int index, Point worldPoint)
        {
            if (selectionPolygons.Count > 0)
            {
                selectionPolygons[^1].Points[index] = worldPoint;
            }
        }

        public static void IncreaseSelectionPolygons(bool inner = true)
        {
            selectionPolygons.Add(new SelectionPolygon(inner, []));
        }

        public static void ClearSelections()
        {
            selectionPolygons.Clear();
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

        public static void MergeIntersections()
        {
            if (IsInnerSelection())
            {
                if (selectionPolygons.Count < 2) return;

                int minX = int.MaxValue, minY = int.MaxValue;
                int maxX = int.MinValue, maxY = int.MinValue;

                foreach (var poly in selectionPolygons)
                {
                    foreach (var p in poly.Points)
                    {
                        if (p.X < minX) minX = p.X;
                        if (p.Y < minY) minY = p.Y;
                        if (p.X > maxX) maxX = p.X;
                        if (p.Y > maxY) maxY = p.Y;
                    }
                }

                int padding = 0;
                int width = (maxX - minX) + (padding * 2);
                int height = (maxY - minY) + (padding * 2);

                bool[,] mask = new bool[width, height];

                foreach (var poly in selectionPolygons)
                {
                    FillPolygonInMask(mask, [.. poly.Points.Select(p => new Point(p.X - minX + padding, p.Y - minY + padding))]);
                }

                List<SelectionPolygon> mergedOutlines = GetSelectionPointsFromMask(mask, Point.Empty);

                if (mergedOutlines.Count < selectionPolygons.Count)
                {
                    selectionPolygons.Clear();
                    foreach (var outline in mergedOutlines)
                    {
                        selectionPolygons.Add(new SelectionPolygon(outline.Inwards, [.. outline.Points.Select(p => new Point(p.X + minX - padding, p.Y + minY - padding))]));
                    }
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

        private static void FillPolygonInMask(bool[,] mask, List<Point> points)
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
                        mask[x, y] = true;
                    }
                }
            }
        }

        public static List<SelectionPolygon> GetSelectionPointsFromMask(bool[,] mask, Point position)
        {
            List<SelectionPolygon> regions = [];

            if (mask == null)
                return regions;

            int width = mask.GetLength(0);
            int height = mask.GetLength(1);

            bool targetValue = false;
            bool backgroundValue = mask[0, 0];

            if (position != Point.Empty &&
                position.X >= 0 && position.X < width &&
                position.Y >= 0 && position.Y < height)
            {
                targetValue = mask[position.X, position.Y];
            }

            bool inner = targetValue;
            bool isInwards = (targetValue != backgroundValue);

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
                            outline.Inwards = isInwards;
                            regions.Add(outline);
                        }
                    }
                }
            }

            return regions;
        }
    }
}