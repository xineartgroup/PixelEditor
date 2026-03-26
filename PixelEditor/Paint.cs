using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace PixelEditor
{
    public struct Paint
    {
        private Color strokeColor = Color.Black;
        private Color fillColor = Color.Black;
        private Bitmap? bitmap = null;
        private Bitmap? brush = null;

        public Paint()
        {
        }

        public void Reset(Color color, int radius)
        {
            if (bitmap != null)
            {
                brush?.Dispose();
                brush = SoftenEdges(bitmap, color, radius);
            }
        }

        public readonly int GetRadius()
        {
            return brush != null ? brush.Width / 2 : 0;
        }

        public void SetFillColor(Color color)
        {
            fillColor = color;
        }

        public readonly Color GetFillColor()
        {
            return fillColor;
        }

        public void SetStrokeColor(Color color)
        {
            strokeColor = color;
        }

        public readonly Color GetStrokeColor()
        {
            return strokeColor;
        }

        public Bitmap? Brush
        {
            readonly get => brush;
            set
            {
                if (value != null)
                {
                    brush?.Dispose();
                    bitmap = new Bitmap(value);
                    Reset(Color.Black, 0);
                }
                else
                {
                    bitmap = null;
                    brush = null;
                }
            }
        }
        
        private static Bitmap SoftenEdges(Bitmap input, Color color, int radius)
        {
            int width = input.Width;
            int height = input.Height;

            bool[,] isBrush = GetBrushMask(input);

            int[,] distanceFromEdge = CalculateDistanceFromEdge(isBrush, width, height);

            Bitmap result = new(width, height, PixelFormat.Format32bppArgb);
            BitmapData resultData = result.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);

            try
            {
                byte[] pixels = new byte[resultData.Stride * height];

                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        int index = y * resultData.Stride + x * 4;

                        byte alpha = 0; // Background - transparent

                        if (isBrush[x, y])
                        {
                            int dist = distanceFromEdge[x, y];

                            if (dist >= radius)
                            {
                                alpha = color.A; // Interior - solid color
                            }
                            else
                            {
                                alpha = (byte)(color.A * (float)dist / radius); // Edge - gradient based on distance
                            }
                        }

                        pixels[index + 0] = color.B; // B
                        pixels[index + 1] = color.G; // G
                        pixels[index + 2] = color.R; // R
                        pixels[index + 3] = alpha; // A
                    }
                }

                Marshal.Copy(pixels, 0, resultData.Scan0, pixels.Length);
            }
            finally
            {
                result.UnlockBits(resultData);
            }

            //result.Save("cursor.png", ImageFormat.Png);

            return result;
        }

        private static bool[,] GetBrushMask(Bitmap image)
        {
            int width = image.Width;
            int height = image.Height;
            bool[,] mask = new bool[width, height];

            BitmapData data = image.LockBits(new Rectangle(0, 0, width, height),
                ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);

            try
            {
                byte[] pixels = new byte[data.Stride * height];
                Marshal.Copy(data.Scan0, pixels, 0, pixels.Length);

                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        int index = y * data.Stride + x * 3;
                        // Check if pixel is dark (brush)
                        if (pixels[index] < 128 && pixels[index + 1] < 128 && pixels[index + 2] < 128)
                        {
                            mask[x, y] = true;
                        }
                    }
                }
            }
            finally
            {
                image.UnlockBits(data);
            }

            return mask;
        }

        private static int[,] CalculateDistanceFromEdge(bool[,] mask, int width, int height)
        {
            int[,] distance = new int[width, height];

            // Initialize: 0 for edge pixels, max for others
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (mask[x, y])
                    {
                        // Check if this is an edge pixel (has background neighbor)
                        bool isEdge = false;
                        for (int ny = -1; ny <= 1; ny++)
                        {
                            for (int nx = -1; nx <= 1; nx++)
                            {
                                int checkX = x + nx;
                                int checkY = y + ny;

                                if (checkX >= 0 && checkX < width && checkY >= 0 && checkY < height)
                                {
                                    if (!mask[checkX, checkY])
                                    {
                                        isEdge = true;
                                        break;
                                    }
                                }
                            }
                            if (isEdge) break;
                        }

                        distance[x, y] = isEdge ? 0 : int.MaxValue;
                    }
                    else
                    {
                        distance[x, y] = -1; // Background
                    }
                }
            }

            // Propagate distances inward (multiple passes until stable)
            bool changed;
            do
            {
                changed = false;

                for (int x = 0; x < width; x++)
                {
                    for (int y = 0; y < height; y++)
                    {
                        if (mask[x, y] && distance[x, y] > 0)
                        {
                            int minNeighbor = int.MaxValue;

                            // Check 4-directional neighbors
                            if (x > 0 && distance[x - 1, y] >= 0)
                                minNeighbor = Math.Min(minNeighbor, distance[x - 1, y]);
                            if (x < width - 1 && distance[x + 1, y] >= 0)
                                minNeighbor = Math.Min(minNeighbor, distance[x + 1, y]);
                            if (y > 0 && distance[x, y - 1] >= 0)
                                minNeighbor = Math.Min(minNeighbor, distance[x, y - 1]);
                            if (y < height - 1 && distance[x, y + 1] >= 0)
                                minNeighbor = Math.Min(minNeighbor, distance[x, y + 1]);

                            if (minNeighbor != int.MaxValue && minNeighbor + 1 < distance[x, y])
                            {
                                distance[x, y] = minNeighbor + 1;
                                changed = true;
                            }
                        }
                    }
                }
            } while (changed);

            return distance;
        }
    }
}