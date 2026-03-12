using System.Windows.Forms.VisualStyles;

namespace PixelEditor
{
    public struct Edge(Point a, Point b)
    {
        public Point A = a, B = b;
    }

    public static class ImageSelections
    {
        private static readonly List<List<Point>> selectionPoints = [];

        public static bool IsInnerSelection { get; set; }

        public static bool ContainsSelection()
        {
            for (int i = 0; i < selectionPoints.Count; i++)
            {
                if (selectionPoints[i].Count > 0)
                {
                    return true;
                }
            }
            return false;
        }

        public static void AddSelectionPoint(Point point)
        {
            if (selectionPoints.Count == 0)
            {
                selectionPoints.Add([]);
            }
            selectionPoints[^1].Add(point);
        }

        public static void UpdateSelectionPoint(int index, Point point)
        {
            if (selectionPoints.Count > 0)
            {
                selectionPoints[^1][index] = point;
            }
        }

        public static void IncreaseSelectionPoints()
        {
            selectionPoints.Add([]);
        }

        public static void ClearSelections()
        {
            selectionPoints.Clear();
        }

        public static List<List<Point>> GetSelections()
        {
            return selectionPoints;
        }

        public static List<Point> GetLastSelection()
        {
            if (selectionPoints.Count > 0)
            {
                return selectionPoints[^1];
            }
            return [];
        }

        public static void MoveSelection(float dx, float dy)
        {
            for (int i = 0; i < selectionPoints.Count; i++)
            {
                for (int j = 0; j < selectionPoints[i].Count; j++)
                {
                    selectionPoints[i][j] = new Point((int)(selectionPoints[i][j].X + dx), (int)(selectionPoints[i][j].Y + dy));
                }
            }
        }

        public static void MergeIntersections()
        {
            if (selectionPoints.Count < 2) return;

            int minX = int.MaxValue, minY = int.MaxValue;
            int maxX = int.MinValue, maxY = int.MinValue;

            foreach (var poly in selectionPoints)
            {
                foreach (var p in poly)
                {
                    if (p.X < minX) minX = p.X;
                    if (p.Y < minY) minY = p.Y;
                    if (p.X > maxX) maxX = p.X;
                    if (p.Y > maxY) maxY = p.Y;
                }
            }

            int padding = 2;
            int width = (maxX - minX) + (padding * 2);
            int height = (maxY - minY) + (padding * 2);

            bool[,] mask = new bool[width, height];

            foreach (var poly in selectionPoints)
            {
                List<Point> offsetPoly = poly.Select(p => new Point(p.X - minX + padding, p.Y - minY + padding)).ToList();
                FillPolygonInMask(mask, offsetPoly);
            }

            List<List<Point>> mergedOutlines = GetSelectionPointsFromMask(mask, true);

            selectionPoints.Clear();
            foreach (var outline in mergedOutlines)
            {
                selectionPoints.Add([.. outline.Select(p => new Point(p.X + minX - padding, p.Y + minY - padding))]);
            }
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

        public static int IsPointInSelection(Point point)
        {
            for (int i = 0; i < selectionPoints.Count; i++)
            {
                if (IsPointInPolygon(point, selectionPoints[i]))
                {
                    return i;
                }
            }
            return -1;
        }

        private static bool IsPointInPolygon(Point point, List<Point> points)
        {
            int n = points.Count;
            bool inside = false;
            for (int i = 0, j = n - 1; i < n; j = i++)
            {
                if ((points[i].Y > point.Y) != (points[j].Y > point.Y) &&
                    (point.X < (points[j].X - points[i].X) * (point.Y - points[i].Y) / (points[j].Y - points[i].Y) + points[i].X))
                {
                    inside = !inside;
                }
            }
            return inside;
        }

        public static List<List<Point>> GetSelectionPointsFromMask(bool[,] mask, bool inner)
        {
            List<List<Point>> regions = [];

            if (mask == null)
                return regions;

            int width = mask.GetLength(0);
            int height = mask.GetLength(1);

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

                        List<Point> outline = [];
                        int currentX = start.Value.X;
                        int currentY = start.Value.Y;
                        int dir = 0;

                        outline.Add(new Point(currentX, currentY));
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
                                outline.Add(new Point(currentX, currentY));
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
                                    outline.Add(new Point(currentX, currentY));
                                    visited[currentX, currentY] = true;
                                }
                                else
                                {
                                    dir = (dir + 1) % 4;
                                }
                            }

                        } while (!(currentX == start.Value.X && currentY == start.Value.Y && dir == 0));

                        regions.Add(outline);
                    }
                }
            }

            return regions;
        }
    }
}
