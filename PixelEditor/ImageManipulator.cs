using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace PixelEditor
{
    public static class ImageManipulator
    {
        public static Bitmap CropFromCenter(Image sourceImage, int newWidth, int newHeight)
        {
            if (sourceImage == null)
                throw new ArgumentNullException(nameof(sourceImage));

            Bitmap newBitmap = new Bitmap(newWidth, newHeight, PixelFormat.Format32bppArgb);

            using (Bitmap sourceBitmap = new Bitmap(sourceImage))
            {
                BitmapData sourceData = sourceBitmap.LockBits(
                    new Rectangle(0, 0, sourceBitmap.Width, sourceBitmap.Height),
                    ImageLockMode.ReadOnly,
                    PixelFormat.Format32bppArgb);

                BitmapData destData = newBitmap.LockBits(
                    new Rectangle(0, 0, newWidth, newHeight),
                    ImageLockMode.WriteOnly,
                    PixelFormat.Format32bppArgb);

                try
                {
                    int sourceX = Math.Max(0, (sourceBitmap.Width - newWidth) / 2);
                    int sourceY = Math.Max(0, (sourceBitmap.Height - newHeight) / 2);
                    int sourceWidth = Math.Min(newWidth, sourceBitmap.Width);
                    int sourceHeight = Math.Min(newHeight, sourceBitmap.Height);

                    int destX = Math.Max(0, (newWidth - sourceWidth) / 2);
                    int destY = Math.Max(0, (newHeight - sourceHeight) / 2);

                    unsafe
                    {
                        byte* sourcePtr = (byte*)sourceData.Scan0;
                        byte* destPtr = (byte*)destData.Scan0;

                        for (int y = 0; y < sourceHeight; y++)
                        {
                            int sourceRow = (sourceY + y) * sourceData.Stride;
                            int destRow = (destY + y) * destData.Stride;

                            for (int x = 0; x < sourceWidth; x++)
                            {
                                int sourceCol = (sourceX + x) * 4;
                                int destCol = (destX + x) * 4;

                                destPtr[destRow + destCol] = sourcePtr[sourceRow + sourceCol];
                                destPtr[destRow + destCol + 1] = sourcePtr[sourceRow + sourceCol + 1];
                                destPtr[destRow + destCol + 2] = sourcePtr[sourceRow + sourceCol + 2];
                                destPtr[destRow + destCol + 3] = sourcePtr[sourceRow + sourceCol + 3];
                            }
                        }
                    }
                }
                finally
                {
                    sourceBitmap.UnlockBits(sourceData);
                    newBitmap.UnlockBits(destData);
                }
            }

            return newBitmap;
        }

        public static Image? GetImage(Color color, int width, int height)
        {
            if (width <= 0 || height <= 0) return null;

            Bitmap bitmap = new(width, height, PixelFormat.Format32bppArgb);
            Rectangle rect = new(0, 0, bitmap.Width, bitmap.Height);
            BitmapData data = bitmap.LockBits(rect, ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);

            int totalBytes = data.Stride * bitmap.Height;
            byte[] pixels = new byte[totalBytes];

            for (int i = 0; i < pixels.Length; i += 4)
            {
                pixels[i] = color.B;
                pixels[i + 1] = color.G;
                pixels[i + 2] = color.R;
                pixels[i + 3] = color.A;
            }

            Marshal.Copy(pixels, 0, data.Scan0, pixels.Length);
            bitmap.UnlockBits(data);

            return bitmap;
        }

        public static Bitmap FillColor(Image image, int imgX, int imgY, float screenWidth, float screenHeight, float layerScaleWidth, float layerScaleHeight, ImageBlending blend, Color color, float opacity, Point startPoint, List<Point> selectionPoints, Control canvas, float zoom, PointF imageOffset)
        {
            Bitmap bitmap = new(image);
            int width = bitmap.Width;
            int height = bitmap.Height;
            Rectangle rect = new(0, 0, width, height);
            BitmapData data = bitmap.LockBits(rect, ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);

            int bytes = data.Stride * height;
            byte[] pixels = new byte[bytes];
            Marshal.Copy(data.Scan0, pixels, 0, bytes);

            float screenAspectRatio = screenWidth / screenHeight;
            float containerAspectRatio = (float)canvas.Width / canvas.Height;
            float screenScaledWidth, screenScaledHeight;

            if (screenAspectRatio > containerAspectRatio)
            {
                screenScaledWidth = canvas.Width * zoom;
                screenScaledHeight = screenScaledWidth / screenAspectRatio;
            }
            else
            {
                screenScaledHeight = canvas.Height * zoom;
                screenScaledWidth = screenScaledHeight * screenAspectRatio;
            }

            float screenOriginX = ((canvas.Width - screenScaledWidth) / 2) + imageOffset.X;
            float screenOriginY = ((canvas.Height - screenScaledHeight) / 2) + imageOffset.Y;

            PointF ScreenToImage(Point p)
            {
                float screenCoordX = (p.X - screenOriginX) / screenScaledWidth * screenWidth;
                float screenCoordY = (p.Y - screenOriginY) / screenScaledHeight * screenHeight;

                float layerPixelX = (screenCoordX - imgX) / layerScaleWidth;
                float layerPixelY = (screenCoordY - imgY) / layerScaleHeight;

                return new PointF(layerPixelX, layerPixelY);
            }

            PointF WorldToImage(Point p)
            {
                float layerPixelX = (p.X - imgX) / layerScaleWidth;
                float layerPixelY = (p.Y - imgY) / layerScaleHeight;
                return new PointF(layerPixelX, layerPixelY);
            }

            PointF startF = ScreenToImage(startPoint);
            Point startI = new((int)Math.Floor(startF.X), (int)Math.Floor(startF.Y));

            if (startI.X < 0 || startI.X >= width || startI.Y < 0 || startI.Y >= height)
            {
                bitmap.UnlockBits(data);
                return bitmap;
            }

            bool usePointSelection = selectionPoints.Count > 2;
            byte[] mask = [];

            if (usePointSelection)
            {
                mask = new byte[width * height];
                using Bitmap maskBmp = new(width, height, PixelFormat.Format32bppArgb);
                using (Graphics g = Graphics.FromImage(maskBmp))
                {
                    g.Clear(Color.Transparent);
                    PointF[] pts = [.. selectionPoints.Select(WorldToImage)];
                    using Brush b = new SolidBrush(Color.White);
                    g.FillPolygon(b, pts);
                }

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
                        br = (byte)Math.Min(r, cr); bg = (byte)Math.Min(g, cg); bb = (byte)Math.Min(b, cb);
                        break;
                    case ImageBlending.Lighten:
                        br = (byte)Math.Max(r, cr); bg = (byte)Math.Max(g, cg); bb = (byte)Math.Max(b, cb);
                        break;
                    default:
                        br = cr; bg = cg; bb = cb;
                        break;
                }

                pixels[idx] = (byte)(bb * alpha + b * invAlpha);
                pixels[idx + 1] = (byte)(bg * alpha + g * invAlpha);
                pixels[idx + 2] = (byte)(br * alpha + r * invAlpha);
                pixels[idx + 3] = (byte)Math.Min(255, pixels[idx + 3] + color.A * alpha);
            }

            if (usePointSelection)
            {
                bool startInsideSelection = mask[startI.Y * width + startI.X] == 1;
                bool fillInside = startInsideSelection;

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
                        if ((fillInside && pixelMask == 0) || (!fillInside && pixelMask == 1)) break;
                        lx--;
                    }

                    int rx = x;
                    while (rx < width - 1 && !visited[y * width + (rx + 1)])
                    {
                        int pixelMask = mask[y * width + (rx + 1)];
                        if ((fillInside && pixelMask == 0) || (!fillInside && pixelMask == 1)) break;
                        rx++;
                    }

                    for (int i = lx; i <= rx; i++)
                    {
                        visited[y * width + i] = true;
                        ApplyBlend((y * width + i) * 4, color.R, color.G, color.B, opacity, 1 - opacity);

                        if (y > 0)
                        {
                            int up = (y - 1) * width + i;
                            if (!visited[up] && ((fillInside && mask[up] == 1) || (!fillInside && mask[up] == 0)))
                                stack.Push(new Point(i, y - 1));
                        }
                        if (y < height - 1)
                        {
                            int dn = (y + 1) * width + i;
                            if (!visited[dn] && ((fillInside && mask[dn] == 1) || (!fillInside && mask[dn] == 0)))
                                stack.Push(new Point(i, y + 1));
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

        public static Bitmap ApplyLighting(Image image, float brightness, float contrast, float exposure, float shadows, float highlights, float vignetteStrength)
        {
            Bitmap bitmap = new(image);
            Rectangle rect = new(0, 0, bitmap.Width, bitmap.Height);
            BitmapData data = bitmap.LockBits(rect, ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);

            byte[] pixels = new byte[data.Stride * bitmap.Height];
            Marshal.Copy(data.Scan0, pixels, 0, pixels.Length);

            float centerX = bitmap.Width / 2f;
            float centerY = bitmap.Height / 2f;
            float maxDistance = MathF.Sqrt(centerX * centerX + centerY * centerY);

            for (int y = 0; y < bitmap.Height; y++)
            {
                for (int x = 0; x < bitmap.Width; x++)
                {
                    int i = y * data.Stride + x * 4;

                    float dx = x - centerX;
                    float dy = y - centerY;
                    float distance = MathF.Sqrt(dx * dx + dy * dy);
                    float normalizedDistance = distance / maxDistance;
                    float vignette = Math.Clamp(MathF.Pow(1.0f - normalizedDistance * vignetteStrength, 2.0f), 0, 1);

                    for (int j = 0; j < 3; j++)
                    {
                        float color = pixels[i + j] / 255f;

                        color *= exposure;
                        color *= brightness;
                        color = ((color - 0.5f) * contrast) + 0.5f;

                        float shadowWeight = MathF.Pow(1.0f - Math.Clamp(color, 0, 1), 3.0f);
                        color += (shadows - 1.0f) * shadowWeight;

                        float highlightWeight = MathF.Pow(Math.Clamp(color, 0, 1), 3.0f);
                        color += (highlights - 1.0f) * highlightWeight;

                        color *= vignette;

                        pixels[i + j] = (byte)Math.Clamp(color * 255, 0, 255);
                    }
                }
            }

            Marshal.Copy(pixels, 0, data.Scan0, pixels.Length);
            bitmap.UnlockBits(data);
            return bitmap;
        }

        public static Bitmap GaussianBlur(Image image, int sizeX, int sizeY)
        {
            int radius = (sizeX + sizeY) / 2;
            if (radius <= 0) return new Bitmap(image);

            Bitmap src = new(image);

            double[] kernel = CreateGaussianKernel(radius);

            Bitmap temp = HorizontalBlur(src, kernel, sizeX);
            Bitmap dst = VerticalBlur(temp, kernel, sizeY);

            src.Dispose();
            temp.Dispose();

            return dst;
        }

        public static Bitmap HorizontalBlur(Bitmap src, double[] kernel, int size)
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

        public static Bitmap VerticalBlur(Bitmap src, double[] kernel, int size)
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

            return dst;
        }

        public static Bitmap AdjustColorBalance(Image image, float saturation, float warmth, float tint)
        {
            Bitmap bitmap = new(image);
            Rectangle rect = new(0, 0, bitmap.Width, bitmap.Height);
            BitmapData data = bitmap.LockBits(rect, ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);

            byte[] pixels = new byte[data.Stride * bitmap.Height];
            Marshal.Copy(data.Scan0, pixels, 0, pixels.Length);

            for (int i = 0; i < pixels.Length; i += 4)
            {
                float b = pixels[i] - warmth;
                float g = pixels[i + 1] + tint;
                float r = pixels[i + 2] + warmth;

                float rf = Math.Clamp(r / 255f, 0, 1);
                float gf = Math.Clamp(g / 255f, 0, 1);
                float bf = Math.Clamp(b / 255f, 0, 1);

                float max = Math.Max(rf, Math.Max(gf, bf));
                float min = Math.Min(rf, Math.Min(gf, bf));
                float l = (max + min) / 2f;

                if (max != min && saturation != 1.0f)
                {
                    float d = max - min;
                    float s = l > 0.5f ? d / (2f - max - min) : d / (max + min);
                    float h;

                    if (max == rf) h = (gf - bf) / d + (gf < bf ? 6 : 0);
                    else if (max == gf) h = (bf - rf) / d + 2;
                    else h = (rf - gf) / d + 4;
                    h /= 6f;

                    s = Math.Clamp(s * saturation, 0, 1);

                    float q = l < 0.5f ? l * (1 + s) : l + s - l * s;
                    float p = 2 * l - q;

                    pixels[i + 2] = (byte)Math.Clamp(HueToRgb(p, q, h + 1f / 3f) * 255, 0, 255);
                    pixels[i + 1] = (byte)Math.Clamp(HueToRgb(p, q, h) * 255, 0, 255);
                    pixels[i] = (byte)Math.Clamp(HueToRgb(p, q, h - 1f / 3f) * 255, 0, 255);
                }
                else
                {
                    pixels[i + 2] = (byte)(rf * 255);
                    pixels[i + 1] = (byte)(gf * 255);
                    pixels[i] = (byte)(bf * 255);
                }
            }

            Marshal.Copy(pixels, 0, data.Scan0, pixels.Length);
            bitmap.UnlockBits(data);
            return bitmap;
        }

        public static Bitmap AdjustSharpness(Image image, float strength)
        {
            Bitmap source = new(image);
            Bitmap result = new(source.Width, source.Height, PixelFormat.Format32bppArgb);

            Rectangle rect = new(0, 0, source.Width, source.Height);
            BitmapData srcData = source.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            BitmapData resData = result.LockBits(rect, ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);

            byte[] srcPixels = new byte[srcData.Stride * source.Height];
            byte[] resPixels = new byte[resData.Stride * source.Height];

            Marshal.Copy(srcData.Scan0, srcPixels, 0, srcPixels.Length);

            int width = source.Width;
            int height = source.Height;
            int stride = srcData.Stride;

            for (int y = 1; y < height - 1; y++)
            {
                for (int x = 1; x < width - 1; x++)
                {
                    int i = y * stride + x * 4;

                    for (int c = 0; c < 3; c++) // B, G, R channels
                    {
                        float center = srcPixels[i + c];
                        float neighbors = srcPixels[i + c - 4] +           // Left
                                          srcPixels[i + c + 4] +           // Right
                                          srcPixels[i + c - stride] +      // Top
                                          srcPixels[i + c + stride];       // Bottom

                        float sharp = center + (center * 4 - neighbors) * strength;
                        resPixels[i + c] = (byte)Math.Clamp(sharp, 0, 255);
                    }
                    resPixels[i + 3] = srcPixels[i + 3]; // Copy Alpha
                }
            }

            Marshal.Copy(resPixels, 0, resData.Scan0, resPixels.Length);
            source.UnlockBits(srcData);
            result.UnlockBits(resData);
            return result;
        }

        public static unsafe bool[,] MagicWandSelect(Image image, Point startPosition, float threshold, string selectBy = "All")
        {
            Bitmap bmp = (Bitmap)image;
            int width = bmp.Width;
            int height = bmp.Height;
            bool[,] mask = new bool[width, height];

            if (startPosition.X < 0 || startPosition.X >= width || startPosition.Y < 0 || startPosition.Y >= height)
                return mask;

            BitmapData data = bmp.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            int stride = data.Stride;
            byte* ptr = (byte*)data.Scan0;

            byte* sPtr = ptr + (startPosition.Y * stride) + (startPosition.X * 4);
            byte sB = sPtr[0], sG = sPtr[1], sR = sPtr[2], sA = sPtr[3];
            float sBr = (0.2126f * sR + 0.7152f * sG + 0.0722f * sB) / 255f;

            Queue<Point> pixels = [];
            pixels.Enqueue(startPosition);
            mask[startPosition.X, startPosition.Y] = true;

            int[] dx = [0, 0, 1, -1, 1, 1, -1, -1];
            int[] dy = [1, -1, 0, 0, 1, -1, 1, -1];

            while (pixels.Count > 0)
            {
                Point current = pixels.Dequeue();

                for (int i = 0; i < 8; i++)
                {
                    int nx = current.X + dx[i];
                    int ny = current.Y + dy[i];

                    if (nx >= 0 && nx < width && ny >= 0 && ny < height && !mask[nx, ny])
                    {
                        byte* cP = ptr + (ny * stride) + (nx * 4);
                        byte b = cP[0], g = cP[1], r = cP[2], a = cP[3];

                        bool match = false;
                        switch (selectBy)
                        {
                            case "Red": match = Math.Abs(sR - r) / 255f <= threshold; break;
                            case "Green": match = Math.Abs(sG - g) / 255f <= threshold; break;
                            case "Blue": match = Math.Abs(sB - b) / 255f <= threshold; break;
                            case "Alpha": match = Math.Abs(sA - a) / 255f <= threshold; break;
                            case "Brightness":
                                float br = (0.2126f * r + 0.7152f * g + 0.0722f * b) / 255f;
                                match = Math.Abs(sBr - br) <= threshold;
                                break;
                            case "All":
                            default:
                                // Max Euclidean distance for RGB is sqrt(255^2 * 3) ≈ 441.67
                                double diff = Math.Sqrt(Math.Pow(sR - r, 2) + Math.Pow(sG - g, 2) + Math.Pow(sB - b, 2));
                                match = (diff / 441.6729559300637) <= threshold;
                                break;
                        }

                        if (match)
                        {
                            mask[nx, ny] = true;
                            pixels.Enqueue(new Point(nx, ny));
                        }
                    }
                }
            }

            bmp.UnlockBits(data);
            return mask;
        }

        public static bool HasTransparentPixels(Bitmap? selectedAreaBitmap)
        {
            if (selectedAreaBitmap == null) return false;

            Rectangle rect = new(0, 0, selectedAreaBitmap.Width, selectedAreaBitmap.Height);
            BitmapData bmpData = selectedAreaBitmap.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

            try
            {
                unsafe
                {
                    byte* ptr = (byte*)bmpData.Scan0;
                    int bytes = bmpData.Stride * bmpData.Height;

                    for (int i = 3; i < bytes; i += 4)
                    {
                        if (ptr[i] < 255)
                        {
                            return true;
                        }
                    }
                }
            }
            finally
            {
                selectedAreaBitmap.UnlockBits(bmpData);
            }

            return false;
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

        private static float HueToRgb(float p, float q, float t)
        {
            if (t < 0) t += 1;
            if (t > 1) t -= 1;
            if (t < 1f / 6f) return p + (q - p) * 6 * t;
            if (t < 1f / 2f) return q;
            if (t < 2f / 3f) return p + (q - p) * (2f / 3f - t) * 6;
            return p;
        }

        private static byte ClampToByte(int value)
        {
            return (byte)(value < 0 ? 0 : value > 255 ? 255 : value);
        }
    }
}