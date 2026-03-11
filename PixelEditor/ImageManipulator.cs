using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace PixelEditor
{
    public static class ImageManipulator
    {
        public static Bitmap FillColor(Image image, ImageBlending blend, Color color, float opacity, Point startPoint, List<Point> selectionPoints, Control canvas, float zoom, PointF imageOffset)
        {
            Bitmap bitmap = new(image);
            int width = bitmap.Width;
            int height = bitmap.Height;
            Rectangle rect = new(0, 0, width, height);
            BitmapData data = bitmap.LockBits(rect, ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);

            int bytes = data.Stride * height;
            byte[] pixels = new byte[bytes];
            Marshal.Copy(data.Scan0, pixels, 0, bytes);

            float aspectRatio = (float)width / height;
            float containerAspectRatio = (float)canvas.Width / canvas.Height;
            float scaledWidth, scaledHeight;

            if (aspectRatio > containerAspectRatio)
            {
                scaledWidth = canvas.Width * zoom;
                scaledHeight = scaledWidth / aspectRatio;
            }
            else
            {
                scaledHeight = canvas.Height * zoom;
                scaledWidth = scaledHeight * aspectRatio;
            }

            float imageX = ((canvas.Width - scaledWidth) / 2) + imageOffset.X;
            float imageY = ((canvas.Height - scaledHeight) / 2) + imageOffset.Y;

            PointF CanvasToImage(Point p) => new(
                (p.X - imageX) / scaledWidth * width,
                (p.Y - imageY) / scaledHeight * height
            );

            PointF startF = CanvasToImage(startPoint);
            Point startI = new((int)startF.X, (int)startF.Y);

            bool usePointSelection = selectionPoints.Count > 2;
            byte[] mask = [];

            if (usePointSelection)
            {
                mask = new byte[width * height];
                using Bitmap maskBmp = new(width, height, PixelFormat.Format32bppArgb);
                using Graphics g = Graphics.FromImage(maskBmp);
                g.Clear(Color.Transparent);
                PointF[] pts = [.. selectionPoints.Select(CanvasToImage)];
                using (Brush b = new SolidBrush(Color.White))
                    g.FillPolygon(b, pts);

                BitmapData mData = maskBmp.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
                unsafe
                {
                    byte* ptr = (byte*)mData.Scan0;
                    for (int i = 0; i < width * height; i++)
                        if (ptr[i * 4 + 3] > 0) mask[i] = 1;
                }
                maskBmp.UnlockBits(mData);
            }

            void ApplyBlend(int idx, byte cr, byte cg, byte cb, float alpha, float invAlpha)
            {
                byte b = pixels[idx];
                byte g = pixels[idx + 1];
                byte r = pixels[idx + 2];
                byte br, bg, bb;

                switch (blend)
                {
                    case ImageBlending.Multiply:
                        br = (byte)(r * cr / 255); bg = (byte)(g * cg / 255); bb = (byte)(b * cb / 255);
                        break;
                    case ImageBlending.Screen:
                        br = (byte)(255 - (255 - r) * (255 - cr) / 255); bg = (byte)(255 - (255 - g) * (255 - cg) / 255); bb = (byte)(255 - (255 - b) * (255 - cb) / 255);
                        break;
                    case ImageBlending.Overlay:
                        br = (byte)(r < 128 ? (2 * r * cr / 255) : (255 - 2 * (255 - r) * (255 - cr) / 255));
                        bg = (byte)(g < 128 ? (2 * g * cg / 255) : (255 - 2 * (255 - g) * (255 - cg) / 255));
                        bb = (byte)(b < 128 ? (2 * b * cb / 255) : (255 - 2 * (255 - b) * (255 - cb) / 255));
                        break;
                    case ImageBlending.Difference:
                        br = (byte)Math.Abs(r - cr); bg = (byte)Math.Abs(g - cg); bb = (byte)Math.Abs(b - cb);
                        break;
                    case ImageBlending.Add:
                        br = (byte)Math.Min(255, r + cr); bg = (byte)Math.Min(255, g + cg); bb = (byte)Math.Min(255, b + cb);
                        break;
                    case ImageBlending.Subtract:
                        br = (byte)Math.Max(0, r - cr); bg = (byte)Math.Max(0, g - cg); bb = (byte)Math.Max(0, b - cb);
                        break;
                    case ImageBlending.Darken:
                        br = Math.Min(r, cr); bg = Math.Min(g, cg); bb = Math.Min(b, cb);
                        break;
                    case ImageBlending.Lighten:
                        br = Math.Max(r, cr); bg = Math.Max(g, cg); bb = Math.Max(b, cb);
                        break;
                    default:
                        br = cr; bg = cg; bb = cb;
                        break;
                }

                pixels[idx] = (byte)(bb * alpha + b * invAlpha);
                pixels[idx + 1] = (byte)(bg * alpha + g * invAlpha);
                pixels[idx + 2] = (byte)(br * alpha + r * invAlpha);
            }

            if (usePointSelection)
            {
                // Check if start point is within image bounds
                if (startI.X < 0 || startI.X >= width || startI.Y < 0 || startI.Y >= height)
                {
                    bitmap.UnlockBits(data);
                    return bitmap;
                }

                // Determine if start point is inside or outside the selection polygon
                bool startInsideSelection = mask[startI.Y * width + startI.X] == 1;

                // If start is inside, we fill inside the polygon
                // If start is outside, we fill outside the polygon
                bool fillInside = startInsideSelection;

                int startIdx = (startI.Y * width + startI.X) * 4;
                byte tr = pixels[startIdx + 2], tg = pixels[startIdx + 1], tb = pixels[startIdx];

                bool[] visited = new bool[width * height];
                Stack<Point> stack = new();
                stack.Push(startI);

                while (stack.Count > 0)
                {
                    Point p = stack.Pop();
                    int x = p.X, y = p.Y;

                    int lx = x;
                    while (lx > 0 && !visited[y * width + (lx - 1)])
                    {
                        int pixelMask = mask[y * width + (lx - 1)];
                        // Only allow movement if:
                        // - For fillInside: pixel must be inside selection (mask == 1)
                        // - For fillOutside: pixel must be outside selection (mask == 0)
                        if ((fillInside && pixelMask == 0) || (!fillInside && pixelMask == 1))
                            break;

                        int pix = (y * width + (lx - 1)) * 4;
                        if (Math.Abs(pixels[pix + 2] - tr) > 10 || Math.Abs(pixels[pix + 1] - tg) > 10 || Math.Abs(pixels[pix] - tb) > 10)
                            break;
                        lx--;
                    }

                    int rx = x;
                    while (rx < width - 1 && !visited[y * width + (rx + 1)])
                    {
                        int pixelMask = mask[y * width + (rx + 1)];
                        if ((fillInside && pixelMask == 0) || (!fillInside && pixelMask == 1))
                            break;

                        int pix = (y * width + (rx + 1)) * 4;
                        if (Math.Abs(pixels[pix + 2] - tr) > 10 || Math.Abs(pixels[pix + 1] - tg) > 10 || Math.Abs(pixels[pix] - tb) > 10)
                            break;
                        rx++;
                    }

                    for (int i = lx; i <= rx; i++)
                    {
                        visited[y * width + i] = true;
                        ApplyBlend((y * width + i) * 4, color.R, color.G, color.B, opacity, 1 - opacity);

                        if (y > 0)
                        {
                            int up = (y - 1) * width + i;
                            if (!visited[up])
                            {
                                int pixelMask = mask[up];
                                if ((fillInside && pixelMask == 1) || (!fillInside && pixelMask == 0))
                                {
                                    int pix = up * 4;
                                    if (Math.Abs(pixels[pix + 2] - tr) < 10 && Math.Abs(pixels[pix + 1] - tg) < 10 && Math.Abs(pixels[pix] - tb) < 10)
                                        stack.Push(new Point(i, y - 1));
                                }
                            }
                        }
                        if (y < height - 1)
                        {
                            int dn = (y + 1) * width + i;
                            if (!visited[dn])
                            {
                                int pixelMask = mask[dn];
                                if ((fillInside && pixelMask == 1) || (!fillInside && pixelMask == 0))
                                {
                                    int pix = dn * 4;
                                    if (Math.Abs(pixels[pix + 2] - tr) < 10 && Math.Abs(pixels[pix + 1] - tg) < 10 && Math.Abs(pixels[pix] - tb) < 10)
                                        stack.Push(new Point(i, y + 1));
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                float a = opacity, ia = 1 - opacity;
                byte cr = color.R, cg = color.G, cb = color.B;
                for (int i = 0; i < bytes; i += 4) ApplyBlend(i, cr, cg, cb, a, ia);
            }

            Marshal.Copy(pixels, 0, data.Scan0, bytes);
            bitmap.UnlockBits(data);
            return bitmap;
        }

        public static Bitmap AdjustContrast(Image image, float contrast)
        {
            Bitmap bitmap = new(image);
            Rectangle rect = new(0, 0, bitmap.Width, bitmap.Height);
            BitmapData data = bitmap.LockBits(rect, ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);

            byte[] pixels = new byte[data.Stride * bitmap.Height];
            Marshal.Copy(data.Scan0, pixels, 0, pixels.Length);

            for (int i = 0; i < pixels.Length; i += 4)
            {
                // Apply contrast to RGB only, preserve alpha
                for (int j = 0; j < 3; j++)
                {
                    int value = (int)(((pixels[i + j] - 128) * contrast) + 128);
                    pixels[i + j] = ClampToByte(value);
                }
            }

            Marshal.Copy(pixels, 0, data.Scan0, pixels.Length);
            bitmap.UnlockBits(data);
            return bitmap;
        }

        public static Bitmap AdjustBrightness(Image image, float brightness)
        {
            Bitmap bitmap = new(image);
            Rectangle rect = new(0, 0, bitmap.Width, bitmap.Height);
            BitmapData data = bitmap.LockBits(rect, ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);

            byte[] pixels = new byte[data.Stride * bitmap.Height];
            Marshal.Copy(data.Scan0, pixels, 0, pixels.Length);

            for (int i = 0; i < pixels.Length; i += 4)
            {
                // Apply brightness to RGB only
                for (int j = 0; j < 3; j++)
                {
                    int value = (int)(pixels[i + j] * brightness);
                    pixels[i + j] = ClampToByte(value);
                }
            }

            Marshal.Copy(pixels, 0, data.Scan0, pixels.Length);
            bitmap.UnlockBits(data);
            return bitmap;
        }

        public static Bitmap InvertColors(Image image)
        {
            Bitmap bitmap = new(image);
            Rectangle rect = new(0, 0, bitmap.Width, bitmap.Height);
            BitmapData data = bitmap.LockBits(rect, ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);

            byte[] pixels = new byte[data.Stride * bitmap.Height];
            Marshal.Copy(data.Scan0, pixels, 0, pixels.Length);

            for (int i = 0; i < pixels.Length; i += 4)
            {
                // Invert RGB, preserve alpha
                pixels[i] = (byte)(255 - pixels[i]);     // Blue
                pixels[i + 1] = (byte)(255 - pixels[i + 1]); // Green
                pixels[i + 2] = (byte)(255 - pixels[i + 2]); // Red
            }

            Marshal.Copy(pixels, 0, data.Scan0, pixels.Length);
            bitmap.UnlockBits(data);
            return bitmap;
        }

        public static Bitmap Grayscale(Image image)
        {
            Bitmap bitmap = new(image);
            Rectangle rect = new(0, 0, bitmap.Width, bitmap.Height);
            BitmapData data = bitmap.LockBits(rect, ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);

            byte[] pixels = new byte[data.Stride * bitmap.Height];
            Marshal.Copy(data.Scan0, pixels, 0, pixels.Length);

            for (int i = 0; i < pixels.Length; i += 4)
            {
                // Using standard grayscale conversion
                byte blue = pixels[i];
                byte green = pixels[i + 1];
                byte red = pixels[i + 2];

                int grayValue = (int)(red * 0.3 + green * 0.59 + blue * 0.11);
                byte grayByte = ClampToByte(grayValue);

                pixels[i] = grayByte;     // Blue
                pixels[i + 1] = grayByte; // Green
                pixels[i + 2] = grayByte; // Red
            }

            Marshal.Copy(pixels, 0, data.Scan0, pixels.Length);
            bitmap.UnlockBits(data);
            return bitmap;
        }

        public static Bitmap GaussianBlur(Image image, int radius)
        {
            if (radius <= 0) return new Bitmap(image);

            Bitmap src = new(image);
            Bitmap dst = new(src.Width, src.Height);

            int size = radius * 2 + 1;
            double[] kernel = CreateGaussianKernel(radius);

            Bitmap temp = HorizontalBlur(src, kernel, size);
            VerticalBlur(temp, dst, kernel, size);

            src.Dispose();
            temp.Dispose();

            return dst;
        }

        private static double[] CreateGaussianKernel(int radius)
        {
            int size = radius * 2 + 1;
            double[] kernel = new double[size];
            double sigma = radius / 2.0;
            double sum = 0;

            for (int i = 0; i < size; i++)
            {
                int x = i - radius;
                kernel[i] = Math.Exp(-(x * x) / (2 * sigma * sigma));
                sum += kernel[i];
            }

            for (int i = 0; i < size; i++)
                kernel[i] /= sum;

            return kernel;
        }

        private static Bitmap HorizontalBlur(Bitmap src, double[] kernel, int size)
        {
            Bitmap dst = new(src.Width, src.Height);
            int radius = size / 2;

            BitmapData srcData = src.LockBits(new Rectangle(0, 0, src.Width, src.Height),
                ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            BitmapData dstData = dst.LockBits(new Rectangle(0, 0, dst.Width, dst.Height),
                ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);

            int bytesPerPixel = 4;
            int srcStride = srcData.Stride;
            int dstStride = dstData.Stride;
            int height = src.Height;
            int width = src.Width;

            unsafe
            {
                byte* srcPtr = (byte*)srcData.Scan0;
                byte* dstPtr = (byte*)dstData.Scan0;

                for (int y = 0; y < height; y++)
                {
                    byte* srcRow = srcPtr + y * srcStride;
                    byte* dstRow = dstPtr + y * dstStride;

                    for (int x = 0; x < width; x++)
                    {
                        double b = 0, g = 0, r = 0, a = 0;
                        double weightSum = 0;

                        for (int i = 0; i < size; i++)
                        {
                            int nx = x + (i - radius);
                            if (nx >= 0 && nx < width)
                            {
                                byte* pixel = srcRow + nx * bytesPerPixel;
                                double weight = kernel[i];
                                b += pixel[0] * weight;
                                g += pixel[1] * weight;
                                r += pixel[2] * weight;
                                a += pixel[3] * weight;
                                weightSum += weight;
                            }
                        }

                        if (weightSum > 0)
                        {
                            b /= weightSum;
                            g /= weightSum;
                            r /= weightSum;
                            a /= weightSum;
                        }

                        byte* dstPixel = dstRow + x * bytesPerPixel;
                        dstPixel[0] = (byte)b;
                        dstPixel[1] = (byte)g;
                        dstPixel[2] = (byte)r;
                        dstPixel[3] = (byte)a;
                    }
                }
            }

            src.UnlockBits(srcData);
            dst.UnlockBits(dstData);

            return dst;
        }

        private static void VerticalBlur(Bitmap src, Bitmap dst, double[] kernel, int size)
        {
            int radius = size / 2;

            BitmapData srcData = src.LockBits(new Rectangle(0, 0, src.Width, src.Height),
                ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            BitmapData dstData = dst.LockBits(new Rectangle(0, 0, dst.Width, dst.Height),
                ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);

            int bytesPerPixel = 4;
            int srcStride = srcData.Stride;
            int dstStride = dstData.Stride;
            int height = src.Height;
            int width = src.Width;

            unsafe
            {
                byte* srcPtr = (byte*)srcData.Scan0;
                byte* dstPtr = (byte*)dstData.Scan0;

                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        double b = 0, g = 0, r = 0, a = 0;
                        double weightSum = 0;

                        for (int i = 0; i < size; i++)
                        {
                            int ny = y + (i - radius);
                            if (ny >= 0 && ny < height)
                            {
                                byte* pixel = srcPtr + ny * srcStride + x * bytesPerPixel;
                                double weight = kernel[i];
                                b += pixel[0] * weight;
                                g += pixel[1] * weight;
                                r += pixel[2] * weight;
                                a += pixel[3] * weight;
                                weightSum += weight;
                            }
                        }

                        if (weightSum > 0)
                        {
                            b /= weightSum;
                            g /= weightSum;
                            r /= weightSum;
                            a /= weightSum;
                        }

                        byte* dstPixel = dstPtr + y * dstStride + x * bytesPerPixel;
                        dstPixel[0] = (byte)b;
                        dstPixel[1] = (byte)g;
                        dstPixel[2] = (byte)r;
                        dstPixel[3] = (byte)a;
                    }
                }
            }

            src.UnlockBits(srcData);
            dst.UnlockBits(dstData);
        }

        private static byte ClampToByte(int value)
        {
            return (byte)(value < 0 ? 0 : value > 255 ? 255 : value);
        }
    }
}