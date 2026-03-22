using System.Drawing.Imaging;

namespace PixelEditor
{
    public static class WarpEngine
    {
        public static Bitmap? WarpSnapshot { get; set; }

        public static unsafe void ApplyForwardWarp(Bitmap source, Bitmap dest, PointF center, float dx, float dy, float radius)
        {
            int width = source.Width;
            int height = source.Height;

            if (dest.Width != width || dest.Height != height)
                return;

            float r2 = radius * radius;

            BitmapData srcData = source.LockBits(
                new Rectangle(0, 0, width, height),
                ImageLockMode.ReadOnly,
                PixelFormat.Format32bppArgb);

            BitmapData destData = dest.LockBits(
                new Rectangle(0, 0, width, height),
                ImageLockMode.ReadWrite,
                PixelFormat.Format32bppArgb);

            int srcStride = srcData.Stride;
            int destStride = destData.Stride;
            nint srcScan0 = srcData.Scan0;
            nint destScan0 = destData.Scan0;

            int minX = (int)Math.Max(0, center.X - radius);
            int maxX = (int)Math.Min(width - 1, center.X + radius);
            int minY = (int)Math.Max(0, center.Y - radius);
            int maxY = (int)Math.Min(height - 1, center.Y + radius);

            Parallel.For(minY, maxY + 1, y =>
            {
                unsafe
                {
                    byte* srcBase = (byte*)srcScan0;
                    byte* destBase = (byte*)destScan0;
                    byte* dRow = destBase + y * destStride;

                    for (int x = minX; x <= maxX; x++)
                    {
                        float vx = x - center.X;
                        float vy = y - center.Y;
                        float dist2 = vx * vx + vy * vy;

                        if (dist2 >= r2) continue;

                        float weight = 1.0f - MathF.Sqrt(dist2) / radius;
                        weight *= weight;

                        int sX = (int)Math.Clamp(x - dx * weight, 0, width - 1);
                        int sY = (int)Math.Clamp(y - dy * weight, 0, height - 1);

                        byte* samplePtr = srcBase + sY * srcStride + sX * 4;
                        byte* dPtr = dRow + x * 4;

                        *(int*)dPtr = *(int*)samplePtr; // copy all 4 bytes in one int write
                    }
                }
            });

            source.UnlockBits(srcData);
            dest.UnlockBits(destData);
        }
    }
}