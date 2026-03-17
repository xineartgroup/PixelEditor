using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace PixelEditor
{
    public static class LayersManipulator
    {
        public static PointF ImageOffset { get; set; } = new(0, 0);

        public static float Zoom { get; set; } = 0.95f;

        public static int Width { get; set; } = 1920;

        public static int Height { get; set; } = 1080;

        public static ColorGrid Screen { get; set; } = new(0, 0);

        public static Dictionary<string, (ColorGrid Cache, int Hash)> LayerCache { get; set; } = [];

        public static HashSet<Rectangle> DirtyRegions { get; set; } = [];

        private static ColorGrid? _backgroundBuffer;

        private static int _backgroundHash = 0;
        private static int _cachedSelectedIndex = -1;

        private static Bitmap? _screenBitmap;
        private static Bitmap? _canvasBitmap;

        public static float ScreenToWorldRatio(int width, int height)
        {
            float aspectRatio = (float)Width / Height;
            float containerAspectRatio = (float)width / height;

            float scaledWidth = aspectRatio > containerAspectRatio
                ? width * Zoom
                : height * Zoom * aspectRatio;

            return scaledWidth / Width;
        }

        public static Point ScreenToWorld(Point screenPt, int width, int height)
        {
            float aspectRatio = (float)Width / Height;
            float containerAspectRatio = (float)width / height;

            float scaledWidth, scaledHeight;
            if (aspectRatio > containerAspectRatio)
            {
                scaledWidth = width * Zoom;
                scaledHeight = scaledWidth / aspectRatio;
            }
            else
            {
                scaledHeight = height * Zoom;
                scaledWidth = scaledHeight * aspectRatio;
            }

            float centerX = (width - scaledWidth) / 2;
            float centerY = (height - scaledHeight) / 2;

            float ratio = scaledWidth / Width;

            int worldX = (int)((screenPt.X - (centerX + ImageOffset.X)) / ratio);
            int worldY = (int)((screenPt.Y - (centerY + ImageOffset.Y)) / ratio);

            return new Point(worldX, worldY);
        }

        public static Point WorldToScreen(Point worldPt, int width, int height)
        {
            float aspectRatio = (float)Width / Height;
            float containerAspectRatio = (float)width / height;

            float scaledWidth, scaledHeight;

            if (aspectRatio > containerAspectRatio)
            {
                scaledWidth = width * Zoom;
                scaledHeight = scaledWidth / aspectRatio;
            }
            else
            {
                scaledHeight = height * Zoom;
                scaledWidth = scaledHeight * aspectRatio;
            }

            float centerX = (width - scaledWidth) / 2;
            float centerY = (height - scaledHeight) / 2;

            float ratio = scaledWidth / Width;

            int screenX = (int)(worldPt.X * ratio + (centerX + ImageOffset.X));
            int screenY = (int)(worldPt.Y * ratio + (centerY + ImageOffset.Y));

            return new Point(screenX, screenY);
        }

        public static void InvalidateCompositeBuffers()
        {
            _backgroundBuffer = null;
            _backgroundHash = 0;
            _cachedSelectedIndex = -1;
        }

        public static void UpdateBuffers()
        {
            _backgroundHash = 0;
            _cachedSelectedIndex = -1;
            _backgroundBuffer = null;
        }

        public static void PopulateColorGrid(List<Layer> layers, int selectedLayerIndex = -1, bool includeBackground = true)
        {
            if (_canvasBitmap == null || _canvasBitmap.Width != Width || _canvasBitmap.Height != Height)
                PopulateBackgroundImage();

            if (selectedLayerIndex >= 0 && selectedLayerIndex < layers.Count)
            {
                PopulateColorGridOptimized(layers, selectedLayerIndex);
                return;
            }

            Screen = new ColorGrid(Width, Height);

            if (includeBackground)
            {
                InitializeScreenWithBackground();
            }

            for (int i = layers.Count - 1; i >= 0; i--)
            {
                var layer = layers[i];
                if (!layer.IsVisible || layer.Image == null) continue;

                int displayWidth = layer.Image.Width * layer.ScaleWidth;
                int displayHeight = layer.Image.Height * layer.ScaleHeight;
                Rectangle layerBounds = new(layer.X, layer.Y, displayWidth, displayHeight);

                int stateHash = HashCode.Combine(
                    layer.X, layer.Y, layer.ScaleWidth, layer.ScaleHeight,
                    layer.Image.GetHashCode(), layer.Opacity, layer.BlendMode);

                bool isDirty = DirtyRegions.Any(r => r.IntersectsWith(layerBounds));

                if (!isDirty && LayerCache.TryGetValue(layer.Name, out var cached) && cached.Hash == stateHash)
                {
                    ApplyCachedLayer(Screen, cached.Cache, layerBounds, Width, Height, layer.BlendMode);
                    continue;
                }

                ColorGrid layerBuffer = RasterizeLayer(layer, displayWidth, displayHeight);
                LayerCache[layer.Name] = (layerBuffer, stateHash);
                ApplyCachedLayer(Screen, layerBuffer, layerBounds, Width, Height, layer.BlendMode);
            }

            DirtyRegions.Clear();
        }

        private static void PopulateColorGridOptimized(List<Layer> layers, int selectedLayerIndex)
        {
            if (Screen.Width != Width || Screen.Height != Height)
                Screen = new ColorGrid(Width, Height);

            int bgHash = ComputeGroupHash(layers, selectedLayerIndex + 1, layers.Count);
            bool bgValid = _backgroundBuffer != null && _cachedSelectedIndex == selectedLayerIndex && _backgroundHash == bgHash;

            Rectangle compositeRegion = new(0, 0, Width, Height);
            if (DirtyRegions.Count > 0)
            {
                Rectangle combined = Rectangle.Empty;
                foreach (var region in DirtyRegions)
                    combined = combined.IsEmpty ? region : Rectangle.Union(combined, region);
                combined.Intersect(new Rectangle(0, 0, Width, Height));
                if (!combined.IsEmpty)
                    compositeRegion = combined;
            }

            if (!bgValid)
            {
                _backgroundBuffer = new ColorGrid(Width, Height);
                InitializeColorGridWithBackground(_backgroundBuffer);

                for (int i = layers.Count - 1; i > selectedLayerIndex; i--)
                {
                    var layer = layers[i];
                    if (!layer.IsVisible || layer.Image == null) continue;

                    int dw = layer.Image.Width * layer.ScaleWidth;
                    int dh = layer.Image.Height * layer.ScaleHeight;
                    Rectangle lb = new(layer.X, layer.Y, dw, dh);

                    int hash = HashCode.Combine(layer.X, layer.Y, layer.ScaleWidth, layer.ScaleHeight,
                        layer.Image.GetHashCode(), layer.Opacity, layer.BlendMode);

                    if (!LayerCache.TryGetValue(layer.Name, out var cached) || cached.Hash != hash)
                    {
                        cached = (RasterizeLayer(layer, dw, dh), hash);
                        LayerCache[layer.Name] = cached;
                    }

                    ApplyCachedLayer(_backgroundBuffer, cached.Cache, lb, Width, Height, layer.BlendMode);
                }

                _backgroundHash = bgHash;
            }

            _cachedSelectedIndex = selectedLayerIndex;

            int[] screenPixels = Screen.GetRawPixels();
            int[] bgPixels = _backgroundBuffer!.GetRawPixels();

            for (int y = compositeRegion.Top; y < compositeRegion.Bottom; y++)
            {
                int rowBase = y * Width;
                Array.Copy(bgPixels, rowBase + compositeRegion.Left, screenPixels, rowBase + compositeRegion.Left, compositeRegion.Width);
            }

            var activeLayer = layers[selectedLayerIndex];
            if (activeLayer.IsVisible && activeLayer.Image != null)
            {
                int dw = activeLayer.Image.Width * activeLayer.ScaleWidth;
                int dh = activeLayer.Image.Height * activeLayer.ScaleHeight;
                Rectangle activeBounds = new(activeLayer.X, activeLayer.Y, dw, dh);
                ColorGrid activeBuffer = RasterizeLayer(activeLayer, dw, dh);

                if (activeBounds.IntersectsWith(compositeRegion))
                    ApplyCachedLayerRegion(Screen, activeBuffer, activeBounds, compositeRegion, activeLayer.BlendMode);
            }

            for (int i = selectedLayerIndex - 1; i >= 0; i--)
            {
                var layer = layers[i];
                if (!layer.IsVisible || layer.Image == null) continue;

                int dw = layer.Image.Width * layer.ScaleWidth;
                int dh = layer.Image.Height * layer.ScaleHeight;
                Rectangle lb = new(layer.X, layer.Y, dw, dh);

                int hash = HashCode.Combine(layer.X, layer.Y, layer.ScaleWidth, layer.ScaleHeight,
                    layer.Image.GetHashCode(), layer.Opacity, layer.BlendMode);

                if (!LayerCache.TryGetValue(layer.Name, out var cached) || cached.Hash != hash)
                {
                    cached = (RasterizeLayer(layer, dw, dh), hash);
                    LayerCache[layer.Name] = cached;
                }

                ApplyCachedLayerRegion(Screen, cached.Cache, lb, compositeRegion, layer.BlendMode);
            }

            DirtyRegions.Clear();
        }

        public static void PopulateBackgroundImage()
        {
            _canvasBitmap = new Bitmap(Width, Height);

            using Graphics g = Graphics.FromImage(_canvasBitmap);
            int squareSize = 20;
            Color lightColor = Color.LightGray;
            Color darkColor = Color.White;

            for (int x = 0; x < Width; x += squareSize)
            {
                for (int y = 0; y < Height; y += squareSize)
                {
                    if ((x / squareSize + y / squareSize) % 2 == 0)
                    {
                        using SolidBrush brush = new(lightColor);
                        g.FillRectangle(brush, x, y, squareSize, squareSize);
                    }
                    else
                    {
                        using SolidBrush brush = new(darkColor);
                        g.FillRectangle(brush, x, y, squareSize, squareSize);
                    }
                }
            }
        }

        private static void InitializeScreenWithBackground()
        {
            if (_canvasBitmap == null) return;

            var bgData = _canvasBitmap.LockBits(
                new Rectangle(0, 0, Width, Height),
                ImageLockMode.ReadOnly,
                PixelFormat.Format32bppArgb);

            try
            {
                int bytesPerPixel = 4;
                int stride = bgData.Stride;
                IntPtr scan0 = bgData.Scan0;

                unsafe
                {
                    byte* bgPtr = (byte*)scan0;
                    int[] screenPixels = Screen.GetRawPixels();

                    for (int y = 0; y < Height; y++)
                    {
                        int rowOffset = y * stride;
                        int screenRowOffset = y * Width;

                        for (int x = 0; x < Width; x++)
                        {
                            int pixelOffset = rowOffset + x * bytesPerPixel;
                            int bgColor = (bgPtr[pixelOffset + 2] << 16) |
                                          (bgPtr[pixelOffset + 1] << 8) |
                                          (bgPtr[pixelOffset + 0]) |
                                          (bgPtr[pixelOffset + 3] << 24);

                            screenPixels[screenRowOffset + x] = bgColor;
                        }
                    }
                }
            }
            finally
            {
                _canvasBitmap.UnlockBits(bgData);
            }
        }

        private static void InitializeColorGridWithBackground(ColorGrid grid)
        {
            if (_canvasBitmap == null) return;

            var bgData = _canvasBitmap.LockBits(
                new Rectangle(0, 0, Width, Height),
                ImageLockMode.ReadOnly,
                PixelFormat.Format32bppArgb);

            try
            {
                int bytesPerPixel = 4;
                int stride = bgData.Stride;
                IntPtr scan0 = bgData.Scan0;

                unsafe
                {
                    byte* bgPtr = (byte*)scan0;
                    int[] gridPixels = grid.GetRawPixels();

                    for (int y = 0; y < Height; y++)
                    {
                        int rowOffset = y * stride;
                        int gridRowOffset = y * Width;

                        for (int x = 0; x < Width; x++)
                        {
                            int pixelOffset = rowOffset + x * bytesPerPixel;
                            int bgColor = (bgPtr[pixelOffset + 2] << 16) |
                                          (bgPtr[pixelOffset + 1] << 8) |
                                          (bgPtr[pixelOffset + 0]) |
                                          (bgPtr[pixelOffset + 3] << 24);

                            gridPixels[gridRowOffset + x] = bgColor;
                        }
                    }
                }
            }
            finally
            {
                _canvasBitmap.UnlockBits(bgData);
            }
        }

        private static int ComputeGroupHash(List<Layer> layers, int fromInclusive, int toExclusive)
        {
            var hc = new HashCode();
            for (int i = fromInclusive; i < toExclusive; i++)
            {
                var l = layers[i];
                hc.Add(l.X); hc.Add(l.Y); hc.Add(l.ScaleWidth); hc.Add(l.ScaleHeight);
                hc.Add(l.Image?.GetHashCode() ?? 0); hc.Add(l.Opacity); hc.Add(l.BlendMode);
                hc.Add(l.IsVisible);
            }
            return hc.ToHashCode();
        }

        private static ColorGrid RasterizeLayer(Layer layer, int displayWidth, int displayHeight)
        {
            ColorGrid layerBuffer = new(displayWidth, displayHeight);
            int[] pixels = layerBuffer.GetRawPixels();
            float masterOpacity = layer.Opacity / 100f;
            LayerChannel channel = layer.Channel;

            unsafe
            {
                using Bitmap bmp = new(layer.Image!);
                BitmapData data = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height),
                    ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
                byte* ptr = (byte*)data.Scan0;

                for (int ly = 0; ly < displayHeight; ly++)
                {
                    int srcY = ly / layer.ScaleHeight;
                    byte* row = ptr + (srcY * data.Stride);

                    for (int lx = 0; lx < displayWidth; lx++)
                    {
                        int srcX = (lx / layer.ScaleWidth) * 4;

                        byte originalAlpha = row[srcX + 3];
                        if (originalAlpha == 0) continue;

                        byte finalAlpha = (byte)(originalAlpha * masterOpacity);
                        if (finalAlpha == 0) continue;

                        byte b = layer.BlueFilter ? (byte)255 : row[srcX];
                        byte g = layer.GreenFilter ? (byte)255 : row[srcX + 1];
                        byte r = layer.RedFilter ? (byte)255 : row[srcX + 2];

                        pixels[ly * displayWidth + lx] = channel switch
                        {
                            LayerChannel.Red => (finalAlpha << 24) | (r << 16) | (r << 8) | r,
                            LayerChannel.Green => (finalAlpha << 24) | (g << 16) | (g << 8) | g,
                            LayerChannel.Blue => (finalAlpha << 24) | (b << 16) | (b << 8) | b,
                            _ => (finalAlpha << 24) | (r << 16) | (g << 8) | b
                        };
                    }
                }
                bmp.UnlockBits(data);
            }

            return layerBuffer;
        }

        private static void ApplyCachedLayer(ColorGrid screen, ColorGrid source, Rectangle bounds, int screenW, int screenH, ImageBlending mode)
        {
            int startY = Math.Max(0, bounds.Y);
            int endY = Math.Min(screenH, bounds.Y + bounds.Height);
            int startX = Math.Max(0, bounds.X);
            int endX = Math.Min(screenW, bounds.X + bounds.Width);

            int[] screenPixels = screen.GetRawPixels();
            int[] srcPixels = source.GetRawPixels();
            int srcW = source.Width;

            Parallel.For(startY, endY, y =>
            {
                int screenRow = y * screenW;
                int srcRow = (y - bounds.Y) * srcW;
                int srcXOffset = -bounds.X;

                for (int x = startX; x < endX; x++)
                {
                    int src = srcPixels[srcRow + x + srcXOffset];
                    int srcA = (src >> 24) & 0xFF;
                    if (srcA == 0) continue;

                    screenPixels[screenRow + x] = Composite(src, srcA, screenPixels[screenRow + x], mode);
                }
            });
        }

        private static void ApplyCachedLayerRegion(ColorGrid screen, ColorGrid source, Rectangle sourceBounds, Rectangle region, ImageBlending mode)
        {
            int startY = Math.Max(region.Top, Math.Max(0, sourceBounds.Y));
            int endY = Math.Min(region.Bottom, Math.Min(screen.Height, sourceBounds.Bottom));
            int startX = Math.Max(region.Left, Math.Max(0, sourceBounds.X));
            int endX = Math.Min(region.Right, Math.Min(screen.Width, sourceBounds.Right));

            if (startY >= endY || startX >= endX) return;

            int[] screenPixels = screen.GetRawPixels();
            int[] srcPixels = source.GetRawPixels();
            int screenW = screen.Width;
            int srcW = source.Width;
            int srcOffsetX = sourceBounds.X;
            int srcOffsetY = sourceBounds.Y;

            for (int y = startY; y < endY; y++)
            {
                int screenRow = y * screenW;
                int srcRow = (y - srcOffsetY) * srcW - srcOffsetX;

                for (int x = startX; x < endX; x++)
                {
                    int src = srcPixels[srcRow + x];
                    int srcA = (src >> 24) & 0xFF;
                    if (srcA == 0) continue;

                    screenPixels[screenRow + x] = Composite(src, srcA, screenPixels[screenRow + x], mode);
                }
            }
        }

        private static int Composite(int src, int srcA, int bg, ImageBlending mode)
        {
            int srcR = (src >> 16) & 0xFF;
            int srcG = (src >> 8) & 0xFF;
            int srcB = src & 0xFF;

            int bgA = (bg >> 24) & 0xFF;
            int bgR = (bg >> 16) & 0xFF;
            int bgG = (bg >> 8) & 0xFF;
            int bgB = bg & 0xFF;

            int blendedR, blendedG, blendedB;

            switch (mode)
            {
                case ImageBlending.Multiply:
                    blendedR = (srcR * bgR) / 255;
                    blendedG = (srcG * bgG) / 255;
                    blendedB = (srcB * bgB) / 255;
                    break;
                case ImageBlending.Screen:
                    blendedR = 255 - ((255 - srcR) * (255 - bgR) / 255);
                    blendedG = 255 - ((255 - srcG) * (255 - bgG) / 255);
                    blendedB = 255 - ((255 - srcB) * (255 - bgB) / 255);
                    break;
                case ImageBlending.Add:
                    blendedR = Math.Min(255, srcR + bgR);
                    blendedG = Math.Min(255, srcG + bgG);
                    blendedB = Math.Min(255, srcB + bgB);
                    break;
                case ImageBlending.Subtract:
                    blendedR = Math.Max(0, srcR - bgR);
                    blendedG = Math.Max(0, srcG - bgG);
                    blendedB = Math.Max(0, srcB - bgB);
                    break;
                case ImageBlending.Darken:
                    blendedR = Math.Min(srcR, bgR);
                    blendedG = Math.Min(srcG, bgG);
                    blendedB = Math.Min(srcB, bgB);
                    break;
                case ImageBlending.Lighten:
                case ImageBlending.Lighen:
                    blendedR = Math.Max(srcR, bgR);
                    blendedG = Math.Max(srcG, bgG);
                    blendedB = Math.Max(srcB, bgB);
                    break;
                case ImageBlending.Overlay:
                    blendedR = OverlayBlend(srcR, bgR);
                    blendedG = OverlayBlend(srcG, bgG);
                    blendedB = OverlayBlend(srcB, bgB);
                    break;
                case ImageBlending.Difference:
                    blendedR = Math.Abs(srcR - bgR);
                    blendedG = Math.Abs(srcG - bgG);
                    blendedB = Math.Abs(srcB - bgB);
                    break;
                default:
                    blendedR = srcR;
                    blendedG = srcG;
                    blendedB = srcB;
                    break;
            }

            if (srcA == 255 && mode == ImageBlending.Normal)
                return src;

            float fa = srcA / 255f;
            float ba = bgA / 255f;
            float invFa = 1.0f - fa;
            float outA = fa + ba * invFa;

            if (outA <= 0) return 0;

            byte r = (byte)((blendedR * fa + bgR * ba * invFa) / outA);
            byte g = (byte)((blendedG * fa + bgG * ba * invFa) / outA);
            byte b = (byte)((blendedB * fa + bgB * ba * invFa) / outA);

            return ((byte)(outA * 255) << 24) | (r << 16) | (g << 8) | b;
        }

        private static int OverlayBlend(int src, int bg)
        {
            if (bg < 128)
                return (2 * src * bg) / 255;
            else
                return 255 - (2 * (255 - src) * (255 - bg) / 255);
        }

        public static Bitmap GetImage(ColorGrid grid)
        {
            int width = grid.Width;
            int height = grid.Height;
            Bitmap bitmap = new(width, height, PixelFormat.Format32bppArgb);

            BitmapData data = bitmap.LockBits(
                new Rectangle(0, 0, width, height),
                ImageLockMode.WriteOnly,
                PixelFormat.Format32bppArgb
            );

            int[] rawPixels = grid.GetRawPixels();

            System.Runtime.InteropServices.Marshal.Copy(rawPixels, 0, data.Scan0, rawPixels.Length);

            bitmap.UnlockBits(data);
            return bitmap;
        }

        public static Bitmap GetImage(ColorGrid grid, Rectangle dirty)
        {
            if (_screenBitmap == null || _screenBitmap.Width != grid.Width || _screenBitmap.Height != grid.Height)
            {
                _screenBitmap?.Dispose();
                _screenBitmap = new Bitmap(grid.Width, grid.Height, PixelFormat.Format32bppArgb);
            }

            BitmapData data = _screenBitmap.LockBits(
                dirty,
                ImageLockMode.WriteOnly,
                PixelFormat.Format32bppArgb);

            try
            {
                unsafe
                {
                    int* ptr = (int*)data.Scan0;
                    int stride = data.Stride / 4;
                    int[] pixels = grid.GetRawPixels();
                    int gridW = grid.Width;

                    for (int y = 0; y < dirty.Height; y++)
                    {
                        int srcRow = (dirty.Y + y) * gridW + dirty.X;
                        int* dstRow = ptr + y * stride;

                        for (int x = 0; x < dirty.Width; x++)
                            dstRow[x] = pixels[srcRow + x];
                    }
                }
            }
            finally
            {
                _screenBitmap.UnlockBits(data);
            }

            return _screenBitmap;
        }

        public static ColorGrid DarkPixelGrid(bool[,] mask, int width, int height)
        {
            ColorGrid result = new(width, height);
            for (float threshold = 0.0f; threshold < 1.0; threshold += 0.1f)
            {
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        if (mask[x, y])
                            result[x, y] = Color.Black.ToArgb();
                        else
                            result[x, y] = Color.White.ToArgb();
                    }
                }
            }

            return result;
        }

        public static bool AreBitmapsEqual(Image? image1, Image? image2)
        {
            try
            {
                if (ReferenceEquals(image1, image2)) return true;
                if (image1 == null || image2 == null) return false;
                if (image1.Size != image2.Size) return false;

                Bitmap? bmp1 = image1 as Bitmap;
                Bitmap? bmp2 = image2 as Bitmap;

                bool createdBmp1 = false;
                bool createdBmp2 = false;

                try
                {
                    if (bmp1 == null) { bmp1 = new Bitmap(image1); createdBmp1 = true; }
                    if (bmp2 == null) { bmp2 = new Bitmap(image2); createdBmp2 = true; }

                    BitmapData data1 = bmp1.LockBits(new Rectangle(0, 0, bmp1.Width, bmp1.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
                    BitmapData data2 = bmp2.LockBits(new Rectangle(0, 0, bmp2.Width, bmp2.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

                    try
                    {
                        unsafe
                        {
                            int* ptr1 = (int*)data1.Scan0;
                            int* ptr2 = (int*)data2.Scan0;
                            int pixelCount = data1.Width * data1.Height;

                            for (int i = 0; i < pixelCount; i++)
                                if (ptr1[i] != ptr2[i]) return false;
                        }
                        return true;
                    }
                    finally
                    {
                        bmp1.UnlockBits(data1);
                        bmp2.UnlockBits(data2);
                    }
                }
                finally
                {
                    if (createdBmp1) bmp1?.Dispose();
                    if (createdBmp2) bmp2?.Dispose();
                }
            }
            catch
            {
                return false;
            }
        }

        public static unsafe Layer MergeLayers(Layer top, Layer bottom)
        {
            Rectangle topBounds = top.GetBounds();
            Rectangle bottomBounds = bottom.GetBounds();
            Rectangle combinedBounds = Rectangle.Union(topBounds, bottomBounds);

            Layer result = new($"{top.Name}_merged", top.IsVisible)
            {
                X = combinedBounds.X,
                Y = combinedBounds.Y
            };

            if (combinedBounds.IsEmpty) return result;

            Bitmap mergedBitmap = new(combinedBounds.Width, combinedBounds.Height, PixelFormat.Format32bppArgb);
            BitmapData resData = mergedBitmap.LockBits(new Rectangle(0, 0, mergedBitmap.Width, mergedBitmap.Height), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);

            DrawLayerToBuffer(bottom, resData, combinedBounds);
            DrawLayerToBuffer(top, resData, combinedBounds);

            mergedBitmap.UnlockBits(resData);
            result.Image = mergedBitmap;
            return result;
        }

        public static unsafe void DrawLayerToBuffer(Layer layer, BitmapData destData, Rectangle canvasBounds)
        {
            if (layer.Image == null || !layer.IsVisible) return;

            Bitmap srcBitmap = (Bitmap)layer.Image;
            Rectangle srcBounds = layer.GetBounds();
            BitmapData srcData = srcBitmap.LockBits(new Rectangle(0, 0, srcBitmap.Width, srcBitmap.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

            byte* dBase = (byte*)destData.Scan0;
            byte* sBase = (byte*)srcData.Scan0;

            int ox = srcBounds.X - canvasBounds.X;
            int oy = srcBounds.Y - canvasBounds.Y;
            float alphaM = layer.Opacity / 100f;

            for (int y = 0; y < srcBounds.Height; y++)
            {
                int dy = y + oy;
                if (dy < 0 || dy >= canvasBounds.Height) continue;

                for (int x = 0; x < srcBounds.Width; x++)
                {
                    int dx = x + ox;
                    if (dx < 0 || dx >= canvasBounds.Width) continue;

                    int sx = x / layer.ScaleWidth;
                    int sy = y / layer.ScaleHeight;
                    if (sx >= srcBitmap.Width || sy >= srcBitmap.Height) continue;

                    byte* sP = sBase + (sy * srcData.Stride) + (sx * 4);
                    byte* dP = dBase + (dy * destData.Stride) + (dx * 4);

                    float sA = (sP[3] / 255f) * alphaM;
                    float dA = dP[3] / 255f;

                    if (sA <= 0) continue;

                    float rS = sP[2] / 255f, gS = sP[1] / 255f, bS = sP[0] / 255f;
                    float rD = dP[2] / 255f, gD = dP[1] / 255f, bD = dP[0] / 255f;
                    float rR, gR, bR;

                    switch (layer.BlendMode)
                    {
                        case ImageBlending.Multiply:
                            rR = rS * rD; gR = gS * gD; bR = bS * bD;
                            break;
                        case ImageBlending.Screen:
                            rR = 1 - (1 - rS) * (1 - rD); gR = 1 - (1 - gS) * (1 - gD); bR = 1 - (1 - bS) * (1 - bD);
                            break;
                        case ImageBlending.Overlay:
                            rR = rD < 0.5f ? 2 * rS * rD : 1 - 2 * (1 - rS) * (1 - rD);
                            gR = gD < 0.5f ? 2 * gS * gD : 1 - 2 * (1 - gS) * (1 - gD);
                            bR = bD < 0.5f ? 2 * bS * bD : 1 - 2 * (1 - bS) * (1 - bD);
                            break;
                        case ImageBlending.Difference:
                            rR = Math.Abs(rD - rS); gR = Math.Abs(gD - gS); bR = Math.Abs(bD - bS);
                            break;
                        case ImageBlending.Add:
                            rR = rS + rD; gR = gS + gD; bR = bS + bD;
                            break;
                        case ImageBlending.Subtract:
                            rR = rD - rS; gR = gD - gS; bR = bD - bS;
                            break;
                        case ImageBlending.Darken:
                            rR = Math.Min(rS, rD); gR = Math.Min(gS, gD); bR = Math.Min(bS, bD);
                            break;
                        case ImageBlending.Lighen:
                        case ImageBlending.Lighten:
                            rR = Math.Max(rS, rD); gR = Math.Max(gS, gD); bR = Math.Max(bS, bD);
                            break;
                        case ImageBlending.Normal:
                        default:
                            rR = rS; gR = gS; bR = bS;
                            break;
                    }

                    float outA = sA + dA * (1 - sA);
                    if (outA > 0)
                    {
                        dP[2] = (byte)(Math.Clamp((rR * sA + rD * dA * (1 - sA)) / outA, 0, 1) * 255);
                        dP[1] = (byte)(Math.Clamp((gR * sA + gD * dA * (1 - sA)) / outA, 0, 1) * 255);
                        dP[0] = (byte)(Math.Clamp((bR * sA + bD * dA * (1 - sA)) / outA, 0, 1) * 255);
                        dP[3] = (byte)(Math.Clamp(outA, 0, 1) * 255);
                    }
                }
            }
            srcBitmap.UnlockBits(srcData);
        }

        public static bool[,] GetDarkPixels(ColorGrid grid, float start, float end)
        {
            bool[,] mask = new bool[grid.Width, grid.Height];

            for (float threshold = start; threshold < end; threshold += 0.1f)
                mask = MaskDarkPixels(grid, threshold, mask);

            return mask;
        }

        public static bool[,] MaskDarkPixels(ColorGrid grid, float threshold, bool[,] mask)
        {
            float luminanceThreshold = threshold * 255.0f;

            for (int y = 0; y < grid.Height; y++)
            {
                for (int x = 0; x < grid.Width; x++)
                {
                    Color color = Color.FromArgb(grid[x, y]);
                    float luminance = 0.299f * color.R + 0.587f * color.G + 0.114f * color.B;
                    mask[x, y] = luminance <= luminanceThreshold;
                }
            }

            return mask;
        }

        public static Bitmap CropFromCenter(Image sourceImage, int newWidth, int newHeight)
        {
            if (sourceImage == null)
                return new Bitmap(newWidth, newHeight);

            Bitmap newBitmap = new(newWidth, newHeight, PixelFormat.Format32bppArgb);

            using (Bitmap sourceBitmap = new(sourceImage))
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

        public static Bitmap FillColor(Image image, int imgX, int imgY, int canvasWidth, int canvasHeight, float layerScaleWidth, float layerScaleHeight, ImageBlending blend, Color color, float opacity, Point startPoint, List<Point> selectionPoints)
        {
            Bitmap bitmap = new(image);
            int width = bitmap.Width;
            int height = bitmap.Height;
            Rectangle rect = new(0, 0, width, height);
            BitmapData data = bitmap.LockBits(rect, ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);

            int bytes = data.Stride * height;
            byte[] pixels = new byte[bytes];
            Marshal.Copy(data.Scan0, pixels, 0, bytes);

            float screenAspectRatio = Width / Height;
            float containerAspectRatio = (float)canvasWidth / canvasHeight;
            float screenScaledWidth, screenScaledHeight;

            if (screenAspectRatio > containerAspectRatio)
            {
                screenScaledWidth = canvasWidth * Zoom;
                screenScaledHeight = screenScaledWidth / screenAspectRatio;
            }
            else
            {
                screenScaledHeight = canvasHeight * Zoom;
                screenScaledWidth = screenScaledHeight * screenAspectRatio;
            }

            float screenOriginX = ((canvasWidth - screenScaledWidth) / 2) + ImageOffset.X;
            float screenOriginY = ((canvasHeight - screenScaledHeight) / 2) + ImageOffset.Y;

            PointF WorldToImage(Point p)
            {
                float layerPixelX = (p.X - imgX) / layerScaleWidth;
                float layerPixelY = (p.Y - imgY) / layerScaleHeight;
                return new PointF(layerPixelX, layerPixelY);
            }

            Point startI = ScreenToWorld(startPoint, canvasWidth, canvasHeight);
            startI.X -= imgX;
            startI.Y -= imgY;

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
            bool[,] visited = new bool[width, height];

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
                                double diff = Math.Sqrt(Math.Pow(sR - r, 2) + Math.Pow(sG - g, 2) + Math.Pow(sB - b, 2));
                                match = (diff / 441.6729559300637) <= threshold;
                                break;
                        }

                        mask[nx, ny] = true;
                        if (match)
                        {
                            pixels.Enqueue(new Point(nx, ny));
                        }
                    }
                }
            }

            bmp.UnlockBits(data);
            return Dilate(mask, width, height, 2);
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

        private static bool[,] Dilate(bool[,] mask, int width, int height, int radius = 1)
        {
            if (radius <= 0) return mask;

            bool[,] horizontalPass = new bool[width, height];
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (mask[x, y])
                    {
                        int start = Math.Max(0, x - radius);
                        int end = Math.Min(width - 1, x + radius);
                        for (int nx = start; nx <= end; nx++)
                        {
                            horizontalPass[nx, y] = true;
                        }
                    }
                }
            }

            bool[,] result = new bool[width, height];
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (horizontalPass[x, y])
                    {
                        int start = Math.Max(0, y - radius);
                        int end = Math.Min(height - 1, y + radius);
                        for (int ny = start; ny <= end; ny++)
                        {
                            result[x, ny] = true;
                        }
                    }
                }
            }

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

        private static byte ClampToByte(int value)
        {
            return (byte)(value < 0 ? 0 : value > 255 ? 255 : value);
        }
    }
}