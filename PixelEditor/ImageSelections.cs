using System.Windows.Forms.VisualStyles;

namespace PixelEditor
{
    public static class ImageSelections
    {
        private static readonly List<List<Point>> selectionPolygons = [];
        private static bool isInnerSelection = true;
        private static RectangleF selectionBounds;
        private static PointF selectionCenter;

        public const float ROTATION_HANDLE_SIZE = 20;
        public const float SCALE_HANDLE_SIZE = 15;

        public static bool IsInnerSelection()
        {
            return isInnerSelection;
        }

        public static bool IsOverRotationHandle(Point screenPoint, int canvasWidth, int canvasHeight)
        {
            PointF worldHandle = new(
                GetSelectionCenter().X,
                GetSelectionBounds().Y - ROTATION_HANDLE_SIZE / GetScreenToWorldScale(canvasWidth, canvasHeight)
            );
            Point screenHandle = LayersManipulator.WorldToScreen(Point.Round(worldHandle), canvasWidth, canvasHeight);
            float distance = Distance(screenPoint, screenHandle);
            return distance < ROTATION_HANDLE_SIZE;
        }

        public static bool IsOverScaleHandle(Point screenPoint, int canvasWidth, int canvasHeight, out string handle)
        {
            handle = "";

            var corners = new[]
            {
                new { Name = "topLeft",     World = new PointF(GetSelectionBounds().X,     GetSelectionBounds().Y) },
                new { Name = "topRight",    World = new PointF(GetSelectionBounds().Right,  GetSelectionBounds().Y) },
                new { Name = "bottomLeft",  World = new PointF(GetSelectionBounds().X,     GetSelectionBounds().Bottom) },
                new { Name = "bottomRight", World = new PointF(GetSelectionBounds().Right,  GetSelectionBounds().Bottom) }
            };

            foreach (var corner in corners)
            {
                Point screenCorner = LayersManipulator.WorldToScreen(Point.Round(corner.World), canvasWidth, canvasHeight);
                if (Distance(screenPoint, screenCorner) < SCALE_HANDLE_SIZE)
                {
                    handle = corner.Name;
                    return true;
                }
            }

            return false;
        }

        public static RectangleF GetSelectionBounds()
        {
            return selectionBounds;
        }

        public static RectangleF GetSelectionBoundsScreen(int canvasWidth, int canvasHeight)
        {
            Point topLeft = LayersManipulator.WorldToScreen(new Point((int)selectionBounds.X, (int)selectionBounds.Y), canvasWidth, canvasHeight);
            Point bottomRight = LayersManipulator.WorldToScreen(new Point((int)selectionBounds.Right, (int)selectionBounds.Bottom), canvasWidth, canvasHeight);
            return new RectangleF(topLeft.X, topLeft.Y, bottomRight.X - topLeft.X, bottomRight.Y - topLeft.Y);
        }

        public static PointF GetSelectionCenter()
        {
            return selectionCenter;
        }

        public static PointF GetSelectionCenterScreen(int canvasWidth, int canvasHeight)
        {
            Point p = LayersManipulator.WorldToScreen(Point.Round(selectionCenter), canvasWidth, canvasHeight);
            return p;
        }

        public static bool ContainsSelection()
        {
            for (int i = 0; i < selectionPolygons.Count; i++)
            {
                if (selectionPolygons[i].Count > 0)
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
                selectionPolygons.Add([]);
            }
            selectionPolygons[^1].Add(worldPoint);
        }

        public static void UpdateSelectionPoint(int index, Point worldPoint)
        {
            if (selectionPolygons.Count > 0)
            {
                selectionPolygons[^1][index] = worldPoint;
            }
        }

        public static void IncreaseSelectionPoints()
        {
            selectionPolygons.Add([]);
        }

        public static void ClearSelections()
        {
            selectionPolygons.Clear();
        }

        public static List<List<Point>> GetSelections()
        {
            return selectionPolygons;
        }

        public static List<Point> GetLastSelection()
        {
            if (selectionPolygons.Count > 0)
            {
                return selectionPolygons[^1];
            }
            return [];
        }

        public static void MoveSelection(float worldDx, float worldDy)
        {
            for (int i = 0; i < selectionPolygons.Count; i++)
            {
                for (int j = 0; j < selectionPolygons[i].Count; j++)
                {
                    selectionPolygons[i][j] = new Point(
                        (int)(selectionPolygons[i][j].X + worldDx),
                        (int)(selectionPolygons[i][j].Y + worldDy));
                }
            }
            CalculateSelectionBounds();
        }

        public static void MergeIntersections()
        {
            if (isInnerSelection)
            {
                if (selectionPolygons.Count < 2) return;

                int minX = int.MaxValue, minY = int.MaxValue;
                int maxX = int.MinValue, maxY = int.MinValue;

                foreach (var poly in selectionPolygons)
                {
                    foreach (var p in poly)
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
                    List<Point> offsetPoly = [.. poly.Select(p => new Point(p.X - minX + padding, p.Y - minY + padding))];
                    FillPolygonInMask(mask, offsetPoly);
                }

                List<List<Point>> mergedOutlines = GetSelectionPointsFromMask(mask, Point.Empty);

                selectionPolygons.Clear();
                foreach (var outline in mergedOutlines)
                {
                    selectionPolygons.Add([.. outline.Select(p => new Point(p.X + minX - padding, p.Y + minY - padding))]);
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
                foreach (var point in selectionPoints[i])
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

        public static float CalculateRotationAngle(Point screenMousePosition, int canvasWidth, int canvasHeight)
        {
            PointF screenCenter = GetSelectionCenterScreen(canvasWidth, canvasHeight);
            float dx = screenMousePosition.X - screenCenter.X;
            float dy = screenMousePosition.Y - screenCenter.Y;
            return (float)(Math.Atan2(dy, dx) * 180 / Math.PI);
        }

        public static int IsPointInSelection(Point screenPoint, int canvasWidth, int canvasHeight)
        {
            Point worldPoint = LayersManipulator.ScreenToWorld(screenPoint, canvasWidth, canvasHeight);
            for (int i = 0; i < selectionPolygons.Count; i++)
            {
                if (IsPointInPolygon(worldPoint, selectionPolygons[i]))
                {
                    return i;
                }
            }
            return -1;
        }

        private static float GetScreenToWorldScale(int canvasWidth, int canvasHeight)
        {
            float aspectRatio = (float)LayersManipulator.Width / LayersManipulator.Height;
            float containerAspectRatio = (float)canvasWidth / canvasHeight;
            float scaledWidth = aspectRatio > containerAspectRatio
                ? canvasWidth * LayersManipulator.Zoom
                : canvasHeight * LayersManipulator.Zoom * aspectRatio;
            return scaledWidth / LayersManipulator.Width;
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

        public static List<List<Point>> GetSelectionPointsFromMask(bool[,] mask, Point position)
        {
            List<List<Point>> regions = [];

            if (mask == null)
                return regions;

            int width = mask.GetLength(0);
            int height = mask.GetLength(1);

            if (position != Point.Empty &&
                position.X >= 0 && position.X < width &&
                position.Y >= 0 && position.Y < height)
            {
                isInnerSelection = mask[position.X, position.Y] != mask[0, 0];
            }

            int[] dx = [1, 0, -1, 0];
            int[] dy = [0, 1, 0, -1];

            bool[,] visited = new bool[width, height];

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (mask[x, y] == isInnerSelection && !visited[x, y])
                    {
                        Point? start = null;

                        if (y == 0 || mask[x, y - 1] != isInnerSelection)
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
                                            mask[checkX, checkY] == isInnerSelection;

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
                                                   mask[forwardX, forwardY] == isInnerSelection;

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