using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace PixelEditor
{
    public static class ManipulatorLighting
    {
        public static unsafe Bitmap InvertColors(Image image)
        {
            Bitmap bitmap = new(image);
            Rectangle rect = new(0, 0, bitmap.Width, bitmap.Height);
            BitmapData data = bitmap.LockBits(rect, ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);

            int height = bitmap.Height;
            int stride = data.Stride;
            byte* ptr = (byte*)data.Scan0;

            Parallel.For(0, height, y =>
            {
                byte* row = ptr + y * stride;
                for (int x = 0; x < stride; x += 4)
                {
                    row[x] = (byte)(255 - row[x]);
                    row[x + 1] = (byte)(255 - row[x + 1]);
                    row[x + 2] = (byte)(255 - row[x + 2]);
                }
            });

            bitmap.UnlockBits(data);
            return bitmap;
        }

        public static unsafe Bitmap Grayscale(Image image)
        {
            Bitmap bitmap = new(image);
            Rectangle rect = new(0, 0, bitmap.Width, bitmap.Height);
            BitmapData data = bitmap.LockBits(rect, ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);

            int height = bitmap.Height;
            int stride = data.Stride;
            byte* ptr = (byte*)data.Scan0;

            Parallel.For(0, height, y =>
            {
                byte* row = ptr + y * stride;
                for (int x = 0; x < stride; x += 4)
                {
                    int grayValue = (int)(row[x + 2] * 0.3 + row[x + 1] * 0.59 + row[x] * 0.11);
                    byte grayByte = grayValue < 0 ? (byte)0 : grayValue > 255 ? (byte)255 : (byte)grayValue;
                    row[x] = grayByte;
                    row[x + 1] = grayByte;
                    row[x + 2] = grayByte;
                }
            });

            bitmap.UnlockBits(data);
            return bitmap;
        }

        public static unsafe Bitmap ApplyLighting(Image image, float brightness, float contrast, float exposure, float shadows, float highlights, float vignetteStrength)
        {
            Bitmap bitmap = new(image);
            Rectangle rect = new(0, 0, bitmap.Width, bitmap.Height);
            BitmapData data = bitmap.LockBits(rect, ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);

            int width = bitmap.Width;
            int height = bitmap.Height;
            int stride = data.Stride;
            byte* ptr = (byte*)data.Scan0;

            float centerX = width / 2f;
            float centerY = height / 2f;
            float maxDistance = MathF.Sqrt(centerX * centerX + centerY * centerY);

            Parallel.For(0, height, y =>
            {
                byte* row = ptr + y * stride;
                for (int x = 0; x < width; x++)
                {
                    byte* pixel = row + x * 4;

                    float dx = x - centerX;
                    float dy = y - centerY;
                    float distance = MathF.Sqrt(dx * dx + dy * dy);
                    float normalizedDistance = distance / maxDistance;
                    float vignette = Math.Clamp(MathF.Pow(1.0f - normalizedDistance * vignetteStrength, 2.0f), 0, 1);

                    for (int j = 0; j < 3; j++)
                    {
                        float color = pixel[j] / 255f;

                        color *= exposure;
                        color *= brightness;
                        color = ((color - 0.5f) * contrast) + 0.5f;

                        float shadowWeight = MathF.Pow(1.0f - Math.Clamp(color, 0, 1), 3.0f);
                        color += (shadows - 1.0f) * shadowWeight;

                        float highlightWeight = MathF.Pow(Math.Clamp(color, 0, 1), 3.0f);
                        color += (highlights - 1.0f) * highlightWeight;

                        color *= vignette;

                        pixel[j] = (byte)Math.Clamp(color * 255, 0, 255);
                    }
                }
            });

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

        public static unsafe Bitmap HorizontalBlur(Bitmap src, double[] kernel, int size)
        {
            Bitmap dst = new(src.Width, src.Height);
            int radius = size / 2;

            BitmapData srcData = src.LockBits(new Rectangle(0, 0, src.Width, src.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            BitmapData dstData = dst.LockBits(new Rectangle(0, 0, dst.Width, dst.Height), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);

            int bytesPerPixel = 4;
            int srcStride = srcData.Stride;
            int dstStride = dstData.Stride;
            int height = src.Height;
            int width = src.Width;

            byte* srcPtr = (byte*)srcData.Scan0;
            byte* dstPtr = (byte*)dstData.Scan0;

            Parallel.For(0, height, y =>
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
            });

            src.UnlockBits(srcData);
            dst.UnlockBits(dstData);

            return dst;
        }

        public static unsafe Bitmap VerticalBlur(Bitmap src, double[] kernel, int size)
        {
            Bitmap dst = new(src.Width, src.Height);
            int radius = size / 2;

            BitmapData srcData = src.LockBits(new Rectangle(0, 0, src.Width, src.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            BitmapData dstData = dst.LockBits(new Rectangle(0, 0, dst.Width, dst.Height), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);

            int bytesPerPixel = 4;
            int srcStride = srcData.Stride;
            int dstStride = dstData.Stride;
            int height = src.Height;
            int width = src.Width;

            byte* srcPtr = (byte*)srcData.Scan0;
            byte* dstPtr = (byte*)dstData.Scan0;

            Parallel.For(0, width, x =>
            {
                for (int y = 0; y < height; y++)
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
            });

            src.UnlockBits(srcData);
            dst.UnlockBits(dstData);

            return dst;
        }

        public static unsafe Bitmap AdjustColorBalance(Image image, float saturation, float warmth, float tint)
        {
            Bitmap bitmap = new(image);
            Rectangle rect = new(0, 0, bitmap.Width, bitmap.Height);
            BitmapData data = bitmap.LockBits(rect, ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);

            int height = bitmap.Height;
            int stride = data.Stride;
            byte* ptr = (byte*)data.Scan0;

            Parallel.For(0, height, y =>
            {
                byte* row = ptr + y * stride;
                for (int x = 0; x < stride; x += 4)
                {
                    byte* pixel = row + x;

                    float b = pixel[0] - warmth;
                    float g = pixel[1] + tint;
                    float r = pixel[2] + warmth;

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

                        pixel[2] = (byte)Math.Clamp(HueToRgb(p, q, h + 1f / 3f) * 255, 0, 255);
                        pixel[1] = (byte)Math.Clamp(HueToRgb(p, q, h) * 255, 0, 255);
                        pixel[0] = (byte)Math.Clamp(HueToRgb(p, q, h - 1f / 3f) * 255, 0, 255);
                    }
                    else
                    {
                        pixel[2] = (byte)(rf * 255);
                        pixel[1] = (byte)(gf * 255);
                        pixel[0] = (byte)(bf * 255);
                    }
                }
            });

            bitmap.UnlockBits(data);
            return bitmap;
        }

        public static unsafe Bitmap AdjustSharpness(Image image, float strength)
        {
            Bitmap source = new(image);
            Bitmap result = new(source.Width, source.Height, PixelFormat.Format32bppArgb);

            Rectangle rect = new(0, 0, source.Width, source.Height);
            BitmapData srcData = source.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            BitmapData resData = result.LockBits(rect, ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);

            int width = source.Width;
            int height = source.Height;
            int stride = srcData.Stride;

            byte* srcPtr = (byte*)srcData.Scan0;
            byte* resPtr = (byte*)resData.Scan0;

            Parallel.For(1, height - 1, y =>
            {
                byte* srcRow = srcPtr + y * stride;
                byte* resRow = resPtr + y * stride;

                for (int x = 1; x < width - 1; x++)
                {
                    byte* srcPixel = srcRow + x * 4;
                    byte* resPixel = resRow + x * 4;

                    for (int c = 0; c < 3; c++)
                    {
                        float center = srcPixel[c];
                        float neighbors = srcPixel[c - 4] + srcPixel[c + 4] + srcPixel[c - stride] + srcPixel[c + stride];

                        float sharp = center + (center * 4 - neighbors) * strength;
                        resPixel[c] = (byte)Math.Clamp(sharp, 0, 255);
                    }
                    resPixel[3] = srcPixel[3];
                }
            });

            source.UnlockBits(srcData);
            result.UnlockBits(resData);
            return result;
        }

        public static unsafe Bitmap DarkImage(Image image)
        {
            Bitmap bitmap = new(image);
            Rectangle rect = new(0, 0, bitmap.Width, bitmap.Height);
            BitmapData data = bitmap.LockBits(rect, ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);

            int height = bitmap.Height;
            int stride = data.Stride;
            byte* ptr = (byte*)data.Scan0;

            for (float threshold = 0.0f; threshold < 1.0; threshold += 0.1f)
            {
                Parallel.For(0, height, y =>
                {
                    byte* row = ptr + y * stride;
                    for (int x = 0; x < stride; x += 4)
                    {
                        byte* pixel = row + x;
                        float brightness = (pixel[2] + pixel[1] + pixel[0]) / (3.0f * 255.0f);

                        if (brightness < threshold)
                        {
                            pixel[0] = 0;
                            pixel[1] = 0;
                            pixel[2] = 0;
                        }
                    }
                });
            }

            bitmap.UnlockBits(data);
            return bitmap;
        }

        public static unsafe Bitmap CropFromCenter(Image image, int newWidth, int newHeight)
        {
            if (image == null)
                return new Bitmap(newWidth, newHeight);

            Bitmap newBitmap = new(newWidth, newHeight, PixelFormat.Format32bppArgb);

            using (Bitmap sourceBitmap = new(image))
            {
                BitmapData sourceData = sourceBitmap.LockBits(new Rectangle(0, 0, sourceBitmap.Width, sourceBitmap.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
                BitmapData destData = newBitmap.LockBits(new Rectangle(0, 0, newWidth, newHeight), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);

                try
                {
                    int sourceX = Math.Max(0, (sourceBitmap.Width - newWidth) / 2);
                    int sourceY = Math.Max(0, (sourceBitmap.Height - newHeight) / 2);
                    int sourceWidth = Math.Min(newWidth, sourceBitmap.Width);
                    int sourceHeight = Math.Min(newHeight, sourceBitmap.Height);

                    int destX = Math.Max(0, (newWidth - sourceWidth) / 2);
                    int destY = Math.Max(0, (newHeight - sourceHeight) / 2);

                    byte* sourcePtr = (byte*)sourceData.Scan0;
                    byte* destPtr = (byte*)destData.Scan0;

                    int sourceStride = sourceData.Stride;
                    int destStride = destData.Stride;

                    Parallel.For(0, sourceHeight, y =>
                    {
                        int sourceRow = (sourceY + y) * sourceStride;
                        int destRow = (destY + y) * destStride;

                        for (int x = 0; x < sourceWidth; x++)
                        {
                            int sourceCol = (sourceX + x) * 4;
                            int destCol = (destX + x) * 4;

                            destPtr[destRow + destCol] = sourcePtr[sourceRow + sourceCol];
                            destPtr[destRow + destCol + 1] = sourcePtr[sourceRow + sourceCol + 1];
                            destPtr[destRow + destCol + 2] = sourcePtr[sourceRow + sourceCol + 2];
                            destPtr[destRow + destCol + 3] = sourcePtr[sourceRow + sourceCol + 3];
                        }
                    });
                }
                finally
                {
                    sourceBitmap.UnlockBits(sourceData);
                    newBitmap.UnlockBits(destData);
                }
            }

            return newBitmap;
        }

        public static Bitmap ResizeImage(Image image, int width, int height)
        {
            var destRect = new Rectangle(0, 0, width, height);
            var destImage = new Bitmap(width, height);

            destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            using (var graphics = Graphics.FromImage(destImage))
            {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                using (var wrapMode = new ImageAttributes())
                {
                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                    graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }

            return destImage;
        }

        public static unsafe Bitmap MaskImage(Bitmap image, Bitmap mask)
        {
            if (image.Width != mask.Width || image.Height != mask.Height)
                return image;

            Bitmap result = new(image.Width, image.Height, PixelFormat.Format32bppArgb);

            Rectangle rect = new(0, 0, image.Width, image.Height);
            BitmapData imageData = image.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            BitmapData maskData = mask.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            BitmapData resultData = result.LockBits(rect, ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);

            int height = image.Height;
            int width = image.Width;
            int imageStride = imageData.Stride;
            int maskStride = maskData.Stride;
            int resultStride = resultData.Stride;

            byte* imagePtr = (byte*)imageData.Scan0;
            byte* maskPtr = (byte*)maskData.Scan0;
            byte* resultPtr = (byte*)resultData.Scan0;

            Parallel.For(0, height, y =>
            {
                byte* imageRow = imagePtr + y * imageStride;
                byte* maskRow = maskPtr + y * maskStride;
                byte* resultRow = resultPtr + y * resultStride;

                for (int x = 0; x < width; x++)
                {
                    byte* imagePixel = imageRow + x * 4;
                    byte* maskPixel = maskRow + x * 4;
                    byte* resultPixel = resultRow + x * 4;

                    byte maskValue = (byte)((maskPixel[0] + maskPixel[1] + maskPixel[2]) / 3);

                    resultPixel[0] = imagePixel[0];
                    resultPixel[1] = imagePixel[1];
                    resultPixel[2] = imagePixel[2];
                    resultPixel[3] = maskValue;
                }
            });

            image.UnlockBits(imageData);
            mask.UnlockBits(maskData);
            result.UnlockBits(resultData);

            return result;
        }

        public static unsafe bool[,] MagicWandSelect(Image image, Point startPosition, float threshold, string selectBy = "All")
        {
            Bitmap bmp = (Bitmap)image;
            int width = bmp.Width;
            int height = bmp.Height;
            bool[,] mask = new bool[width, height];
            bool[,] visited = new bool[width, height];

            if (startPosition.X < 0 || startPosition.X >= width || startPosition.Y < 0 || startPosition.Y >= height)
                return mask;

            BitmapData data = bmp.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            int stride = data.Stride;
            byte* ptr = (byte*)data.Scan0;

            byte* sPtr = ptr + (startPosition.Y * stride) + (startPosition.X * 4);
            byte sB = sPtr[0], sG = sPtr[1], sR = sPtr[2], sA = sPtr[3];
            float sBr = (0.2126f * sR + 0.7152f * sG + 0.0722f * sB) / 255f;

            Queue<Point> pixels = new();
            pixels.Enqueue(startPosition);
            visited[startPosition.X, startPosition.Y] = true;
            mask[startPosition.X, startPosition.Y] = true;

            int[] dx = [0, 0, 1, -1];
            int[] dy = [1, -1, 0, 0];

            while (pixels.Count > 0)
            {
                Point current = pixels.Dequeue();

                for (int i = 0; i < 4; i++)
                {
                    int nx = current.X + dx[i];
                    int ny = current.Y + dy[i];

                    if (nx >= 0 && nx < width && ny >= 0 && ny < height && !visited[nx, ny])
                    {
                        visited[nx, ny] = true;
                        byte* cP = ptr + (ny * stride) + (nx * 4);
                        byte b = cP[0], g = cP[1], r = cP[2], a = cP[3];

                        bool match = selectBy switch
                        {
                            "Red" => Math.Abs(sR - r) / 255f <= threshold,
                            "Green" => Math.Abs(sG - g) / 255f <= threshold,
                            "Blue" => Math.Abs(sB - b) / 255f <= threshold,
                            "Alpha" => Math.Abs(sA - a) / 255f <= threshold,
                            "Brightness" => Math.Abs(sBr - ((0.2126f * r + 0.7152f * g + 0.0722f * b) / 255f)) <= threshold,
                            _ => Math.Sqrt(Math.Pow(sR - r, 2) + Math.Pow(sG - g, 2) + Math.Pow(sB - b, 2)) / 441.6729559300637 <= threshold
                        };

                        mask[nx, ny] = true;
                        if (match)
                            pixels.Enqueue(new Point(nx, ny));
                    }
                }
            }

            bmp.UnlockBits(data);
            return Dilate(mask, width, height, 2);
        }

        public static unsafe bool[,] ByColorSelect(Image image, Point startPosition, float threshold)
        {
            using Bitmap bitmap = new(image);
            int width = bitmap.Width;
            int height = bitmap.Height;
            bool[,] mask = new bool[width, height];

            if (startPosition.X < 0 || startPosition.X >= width || startPosition.Y < 0 || startPosition.Y >= height)
                return mask;

            BitmapData data = bitmap.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            int stride = data.Stride;
            byte* ptr = (byte*)data.Scan0;

            int seedIdx = (startPosition.Y * stride) + (startPosition.X * 4);
            byte seedB = ptr[seedIdx];
            byte seedG = ptr[seedIdx + 1];
            byte seedR = ptr[seedIdx + 2];

            float maxDiffSq = (threshold * 255) * (threshold * 255);

            Parallel.For(0, height, y =>
            {
                byte* row = ptr + (y * stride);
                for (int x = 0; x < width; x++)
                {
                    int offset = x * 4;
                    int db = row[offset] - seedB;
                    int dg = row[offset + 1] - seedG;
                    int dr = row[offset + 2] - seedR;

                    float diffSq = (dr * dr) + (dg * dg) + (db * db);

                    if (diffSq <= maxDiffSq)
                    {
                        mask[x, y] = true;
                    }
                }
            });

            bitmap.UnlockBits(data);
            return mask;
        }

        public static unsafe bool HasTransparentPixels(Bitmap? selectedAreaBitmap)
        {
            if (selectedAreaBitmap == null) return false;

            Rectangle rect = new(0, 0, selectedAreaBitmap.Width, selectedAreaBitmap.Height);
            BitmapData bmpData = selectedAreaBitmap.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

            try
            {
                byte* ptr = (byte*)bmpData.Scan0;
                int bytes = bmpData.Stride * bmpData.Height;

                for (int i = 3; i < bytes; i += 4)
                    if (ptr[i] < 255)
                        return true;
            }
            finally
            {
                selectedAreaBitmap.UnlockBits(bmpData);
            }

            return false;
        }

        private static bool[,] Dilate(bool[,] mask, int width, int height, int radius = 1)
        {
            if (radius <= 0) return mask;

            bool[,] horizontalPass = new bool[width, height];
            bool[,] result = new bool[width, height];

            Parallel.For(0, height, y =>
            {
                for (int x = 0; x < width; x++)
                {
                    if (mask[x, y])
                    {
                        int start = Math.Max(0, x - radius);
                        int end = Math.Min(width - 1, x + radius);
                        for (int nx = start; nx <= end; nx++)
                            horizontalPass[nx, y] = true;
                    }
                }
            });

            Parallel.For(0, width, x =>
            {
                for (int y = 0; y < height; y++)
                {
                    if (horizontalPass[x, y])
                    {
                        int start = Math.Max(0, y - radius);
                        int end = Math.Min(height - 1, y + radius);
                        for (int ny = start; ny <= end; ny++)
                            result[x, ny] = true;
                    }
                }
            });

            return result;
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
    }
}