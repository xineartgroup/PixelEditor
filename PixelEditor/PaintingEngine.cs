using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace PixelEditor
{
    public static class PaintingEngine
    {
        private static Bitmap? strokeBase;
        private static Bitmap? targetBitmap;
        private static Paint currentBrush;

        private static float[,]? strokeCoverage;
        private static int bufferWidth;
        private static int bufferHeight;

        private static readonly Random strokeRand = new();

        public static void SetBrush(Paint brush) => currentBrush = brush;

        public static void SetTarget(Image? image)
        {
            targetBitmap = image as Bitmap;
            Console.WriteLine($"Target set: {targetBitmap?.Width}x{targetBitmap?.Height}");
        }

        public static void BeginStroke()
        {
            if (targetBitmap == null)
                return;

            try
            {
                bufferWidth = targetBitmap.Width;
                bufferHeight = targetBitmap.Height;

                strokeBase?.Dispose();
                strokeBase = new Bitmap(targetBitmap);

                if (strokeCoverage == null || strokeCoverage.GetLength(0) != bufferWidth || strokeCoverage.GetLength(1) != bufferHeight)
                    strokeCoverage = new float[bufferWidth, bufferHeight];
                else
                    Array.Clear(strokeCoverage, 0, strokeCoverage.Length);
            }
            catch (Exception ex)
            {
                targetBitmap = null;
                strokeBase = null;
                strokeCoverage = null;
                Console.WriteLine($"Error initializing stroke: {ex.Message}");
                return;
            }
        }

        public static void EndStroke()
        {
            strokeBase?.Dispose();
            strokeBase = null;
            strokeCoverage = null;
        }

        public static void PaintStroke(Point start, Point end, float brushScale = 1.0f, float opacity = 1.0f,
            float tiltX = 0f, float tiltY = 0f, float baseRotation = 0f, int randomRotation = 360,
            bool isErasing = false, bool[,]? mask = null)
        {
            if (currentBrush.Brush == null || targetBitmap == null)
                return;

            float distance = Distance(start, end);

            float GetCurrentRotation()
            {
                if (randomRotation > 0)
                {
                    return (baseRotation + strokeRand.Next(0, randomRotation)) % 360;
                }
                return baseRotation;
            }

            if (distance < 1f)
            {
                PaintAt(end, brushScale, opacity, tiltX, tiltY, GetCurrentRotation(), isErasing, mask);
                return;
            }

            float brushSize = currentBrush.Brush.Width * brushScale;
            float step = Math.Max(1.0f, brushSize * 0.2f);

            for (float t = 0; t <= distance; t += step)
            {
                float lerp = t / distance;
                int x = (int)Math.Round(start.X + (end.X - start.X) * lerp);
                int y = (int)Math.Round(start.Y + (end.Y - start.Y) * lerp);

                PaintAt(new Point(x, y), brushScale, opacity, tiltX, tiltY, GetCurrentRotation(), isErasing, mask);
            }

            PaintAt(end, brushScale, opacity, tiltX, tiltY, GetCurrentRotation(), isErasing, mask);
        }

        private static void PaintAt(Point location, float brushScale, float opacity, float tiltX, float tiltY, float rotation, bool isErasing, bool[,]? mask = null)
        {
            if (currentBrush.Brush == null || targetBitmap == null || strokeBase == null || strokeCoverage == null)
                return;

            using Bitmap brushStamp = GetBrushStamp(brushScale, tiltX, tiltY, rotation);

            // Center the custom-sized stamp on the current coordinate
            int x0 = location.X - brushStamp.Width / 2;
            int y0 = location.Y - brushStamp.Height / 2;

            Rectangle targetRect = new(0, 0, targetBitmap.Width, targetBitmap.Height);
            BitmapData targetData = targetBitmap.LockBits(targetRect, ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
            BitmapData baseData = strokeBase.LockBits(targetRect, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

            Rectangle brushRect = new(0, 0, brushStamp.Width, brushStamp.Height);
            BitmapData brushData = brushStamp.LockBits(brushRect, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

            unsafe
            {
                byte* targetPtr = (byte*)targetData.Scan0;
                byte* basePtr = (byte*)baseData.Scan0;
                byte* brushPtr = (byte*)brushData.Scan0;

                int targetStride = targetData.Stride;
                int brushStride = brushData.Stride;

                int startX = Math.Max(0, x0);
                int startY = Math.Max(0, y0);
                int endX = Math.Min(targetBitmap.Width, x0 + brushStamp.Width);
                int endY = Math.Min(targetBitmap.Height, y0 + brushStamp.Height);

                for (int y = startY; y < endY; y++)
                {
                    for (int x = startX; x < endX; x++)
                    {
                        if (mask != null && !mask[x, y])
                            continue;

                        int bx = x - x0;
                        int by = y - y0;

                        byte* brushPixel = brushPtr + by * brushStride + bx * 4;

                        float brushAlpha = brushPixel[3] / 255f;
                        if (brushAlpha <= 0f)
                            continue;

                        float desired = brushAlpha * opacity;
                        float current = strokeCoverage[x, y];

                        if (current >= desired)
                            continue;

                        strokeCoverage[x, y] = desired;

                        byte* basePixel = basePtr + y * targetStride + x * 4;
                        byte* targetPixel = targetPtr + y * targetStride + x * 4;

                        float inv = 1f - desired;
                        float baseAlpha = basePixel[3] / 255f;
                        float outAlpha = isErasing ? baseAlpha * (1f - desired) : desired + baseAlpha * inv;

                        if (outAlpha > 0)
                        {
                            float baseWeight = baseAlpha * inv;
                            float totalWeight = desired + baseWeight;

                            targetPixel[0] = (byte)((brushPixel[0] * desired + basePixel[0] * baseWeight) / totalWeight);
                            targetPixel[1] = (byte)((brushPixel[1] * desired + basePixel[1] * baseWeight) / totalWeight);
                            targetPixel[2] = (byte)((brushPixel[2] * desired + basePixel[2] * baseWeight) / totalWeight);
                        }
                        targetPixel[3] = (byte)(outAlpha * 255);
                    }
                }
            }

            targetBitmap.UnlockBits(targetData);
            strokeBase.UnlockBits(baseData);
            brushStamp.UnlockBits(brushData);
        }

        private static Bitmap GetBrushStamp(float scale, float tiltX, float tiltY, float rotation)
        {
            if (currentBrush.Brush == null)
                return new Bitmap(1, 1);

            float origW = currentBrush.Brush.Width * scale;
            float origH = currentBrush.Brush.Height * scale;

            using Matrix matrix = new();

            matrix.Scale(scale, scale);

            float shearX = MathF.Tan(tiltX * MathF.PI / 180f) * 0.5f;
            float shearY = MathF.Tan(tiltY * MathF.PI / 180f) * 0.5f;
            matrix.Shear(shearX, shearY);

            matrix.Rotate(rotation);

            PointF[] corners = [
                new PointF(0, 0),
                new PointF(currentBrush.Brush.Width, 0),
                new PointF(currentBrush.Brush.Width, currentBrush.Brush.Height),
                new PointF(0, currentBrush.Brush.Height)
            ];
            matrix.TransformPoints(corners);

            float minX = corners.Min(p => p.X);
            float maxX = corners.Max(p => p.X);
            float minY = corners.Min(p => p.Y);
            float maxY = corners.Max(p => p.Y);

            int boundWidth = Math.Max(1, (int)MathF.Ceiling(maxX - minX));
            int boundHeight = Math.Max(1, (int)MathF.Ceiling(maxY - minY));

            Bitmap transformedStamp = new(boundWidth, boundHeight, PixelFormat.Format32bppArgb);
            using (Graphics g = Graphics.FromImage(transformedStamp))
            {
                g.InterpolationMode = InterpolationMode.Bilinear; // InterpolationMode.HighQualityBicubic;
                g.SmoothingMode = SmoothingMode.HighQuality;

                g.TranslateTransform(-minX, -minY);
                g.MultiplyTransform(matrix);

                g.DrawImage(currentBrush.Brush, 0, 0);
            }

            return transformedStamp;
        }

        public static float Distance(Point p1, Point p2)
        {
            float dx = p2.X - p1.X;
            float dy = p2.Y - p1.Y;
            return MathF.Sqrt(dx * dx + dy * dy);
        }

        public static PointF CatmullRomPoint(PointF p0, PointF p1, PointF p2, PointF p3, float t)
        {
            float t2 = t * t;
            float t3 = t2 * t;

            float x = 0.5f * (
                (2 * p1.X) +
                (-p0.X + p2.X) * t +
                (2 * p0.X - 5 * p1.X + 4 * p2.X - p3.X) * t2 +
                (-p0.X + 3 * p1.X - 3 * p2.X + p3.X) * t3
            );

            float y = 0.5f * (
                (2 * p1.Y) +
                (-p0.Y + p2.Y) * t +
                (2 * p0.Y - 5 * p1.Y + 4 * p2.Y - p3.Y) * t2 +
                (-p0.Y + 3 * p1.Y - 3 * p2.Y + p3.Y) * t3
            );

            return new PointF(x, y);
        }
    }
}