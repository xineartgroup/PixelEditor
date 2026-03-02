using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace PixelEditor
{
    public class PaintingEngine
    {
        private Image? targetImage = null;
        private Paint currentBrush;

        private float[,]? accumulationBuffer;
        private int bufferWidth, bufferHeight;

        public void SetBrush(Paint brush)
        {
            currentBrush = brush;
        }

        public void SetTarget(Image? image)
        {
            targetImage = image;
        }

        public void BeginStroke()
        {
            if (targetImage == null) return;

            bufferWidth = targetImage.Width;
            bufferHeight = targetImage.Height;
            accumulationBuffer = new float[bufferWidth, bufferHeight];
        }

        public void PaintLine(Point start, Point end, float brushScale = 1.0f, float opacity = 1.0f)
        {
            if (currentBrush.Brush == null || targetImage == null) return;

            BeginStroke(); // Auto-begin stroke if not already started

            float distance = Distance(start, end);

            float brushSize = currentBrush.Brush.Width * brushScale; // Adaptive step size based on brush size
            float step = Math.Max(0.5f, brushSize * 0.3f); // 30% overlap

            Random random = new(start.X * 1000 + start.Y); // Use a stable random for jitter

            for (float t = 0; t <= distance; t += step)
            {
                float lerp = distance == 0 ? 0 : t / distance;
                int x = (int)(start.X + (end.X - start.X) * lerp);
                int y = (int)(start.Y + (end.Y - start.Y) * lerp);

                float jitter = 0.9f + (float)(random.NextDouble() * 0.2f); // Vary opacity slightly for more natural strokes
                PaintAt(new Point(x, y), brushScale, opacity * jitter);
            }

            if (distance > step)
            {
                PaintAt(end, brushScale, opacity); // Ensure we paint at the end point
            }

            EndStroke();
        }

        public void PaintAt(Point location, float brushScale = 1.0f, float opacity = 1.0f)
        {
            if (currentBrush.Brush == null || targetImage == null || accumulationBuffer == null)
                return;

            int brushWidth = (int)(currentBrush.Brush.Width * brushScale);
            int brushHeight = (int)(currentBrush.Brush.Height * brushScale);

            int x = location.X - brushWidth / 2;
            int y = location.Y - brushHeight / 2;

            using Bitmap brushStamp = GetBrushStamp(brushScale, brushWidth, brushHeight);
            Bitmap targetBitmap = (Bitmap)targetImage;
            Rectangle targetRect = new(0, 0, targetBitmap.Width, targetBitmap.Height);
            BitmapData targetData = targetBitmap.LockBits(targetRect, ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);

            Rectangle brushRect = new(0, 0, brushStamp.Width, brushStamp.Height);
            BitmapData brushData = brushStamp.LockBits(brushRect, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

            unsafe
            {
                byte* targetPtr = (byte*)targetData.Scan0;
                byte* brushPtr = (byte*)brushData.Scan0;

                int targetStride = targetData.Stride;
                int brushStride = brushData.Stride;

                int startX = Math.Max(0, x);
                int startY = Math.Max(0, y);
                int endX = Math.Min(targetImage.Width, x + brushWidth);
                int endY = Math.Min(targetImage.Height, y + brushHeight);

                for (int py = startY; py < endY; py++)
                {
                    for (int px = startX; px < endX; px++)
                    {
                        int brushPx = px - x;
                        int brushPy = py - y;

                        byte* brushPixel = brushPtr + brushPy * brushStride + brushPx * 4;
                        byte brushAlpha = brushPixel[3]; // Get brush alpha (softness)

                        if (brushAlpha == 0) continue;

                        float brushIntensity = brushAlpha / 255f;

                        float strokeIntensity = brushIntensity * opacity;

                        accumulationBuffer[px, py] += strokeIntensity;
                        accumulationBuffer[px, py] = Math.Min(accumulationBuffer[px, py], 1.0f);

                        byte* targetPixel = targetPtr + py * targetStride + px * 4;

                        float finalOpacity = accumulationBuffer[px, py];

                        // Blend brush color with existing pixel
                        float invOpacity = 1 - finalOpacity;
                        targetPixel[0] = (byte)(targetPixel[0] * invOpacity + brushPixel[0] * finalOpacity);
                        targetPixel[1] = (byte)(targetPixel[1] * invOpacity + brushPixel[1] * finalOpacity);
                        targetPixel[2] = (byte)(targetPixel[2] * invOpacity + brushPixel[2] * finalOpacity);
                    }
                }
            }

            targetBitmap.UnlockBits(targetData);
            brushStamp.UnlockBits(brushData);
        }

        private Bitmap GetBrushStamp(float brushScale, int brushWidth, int brushHeight)
        {
            if (currentBrush.Brush == null) return new Bitmap(1, 1); // Return a dummy brush if none set
            if (brushScale == 1.0f)
            {
                return new Bitmap(currentBrush.Brush); // Return a clone so we don't modify the original
            }
            else
            {
                Bitmap resized = new(brushWidth, brushHeight, PixelFormat.Format32bppArgb);
                using (Graphics g = Graphics.FromImage(resized))
                {
                    g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    g.DrawImage(currentBrush.Brush, 0, 0, brushWidth, brushHeight);
                }
                return resized;
            }
        }

        public void EndStroke()
        {
            accumulationBuffer = null;
        }

        private static float Distance(Point p1, Point p2)
        {
            return (float)Math.Sqrt(Math.Pow(p2.X - p1.X, 2) + Math.Pow(p2.Y - p1.Y, 2));
        }
    }
}