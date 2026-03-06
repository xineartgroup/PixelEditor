using System.Drawing.Imaging;

namespace PixelEditor
{
    public static class LayerManipulator
    {
        public static int Width { get; set; } = 1920;
        public static int Height { get; set; } = 1080;
        public static ColorGrid Screen { get; set; } = new(0, 0);
        public static Dictionary<string, (ColorGrid Cache, int Hash)> LayerCache { get; set; } = [];
        public static HashSet<Rectangle> DirtyRegions { get; set; } = [];

        private static ColorGrid? _backgroundBuffer;
        private static ColorGrid? _activeBuffer;

        private static int _backgroundHash = 0;
        private static int _cachedSelectedIndex = -1;

        private static Bitmap? _screenBitmap;
        private static Bitmap? _canvasBitmap;

        public static void InvalidateCompositeBuffers()
        {
            _backgroundBuffer = null;
            _activeBuffer = null;
            _backgroundHash = 0;
            _cachedSelectedIndex = -1;
        }

        public static void UpdateBuffers()
        {
            _backgroundHash = 0;
            _cachedSelectedIndex = -1;
            _backgroundBuffer = null;
            _activeBuffer = null;
        }

        public static void PopulateColorGrid(List<Layer> layers, int selectedLayerIndex = -1)
        {
            if (_canvasBitmap == null || _canvasBitmap.Width != Width || _canvasBitmap.Height != Height)
                PopulateBackgroundImage();

            if (selectedLayerIndex >= 0 && selectedLayerIndex < layers.Count)
            {
                PopulateColorGridOptimized(layers, selectedLayerIndex);
                return;
            }

            Screen = new ColorGrid(Width, Height);
            InitializeScreenWithBackground();

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

        private static void ApplyCachedLayer(ColorGrid screen, ColorGrid source, Rectangle bounds,
            int screenW, int screenH, LayerBlending mode)
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

        private static void ApplyCachedLayerRegion(ColorGrid screen, ColorGrid source, Rectangle sourceBounds,
            Rectangle region, LayerBlending mode)
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

        private static int Composite(int src, int srcA, int bg, LayerBlending mode)
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
                case LayerBlending.Multiply:
                    blendedR = (srcR * bgR) / 255;
                    blendedG = (srcG * bgG) / 255;
                    blendedB = (srcB * bgB) / 255;
                    break;
                case LayerBlending.Screen:
                    blendedR = 255 - ((255 - srcR) * (255 - bgR) / 255);
                    blendedG = 255 - ((255 - srcG) * (255 - bgG) / 255);
                    blendedB = 255 - ((255 - srcB) * (255 - bgB) / 255);
                    break;
                case LayerBlending.Add:
                    blendedR = Math.Min(255, srcR + bgR);
                    blendedG = Math.Min(255, srcG + bgG);
                    blendedB = Math.Min(255, srcB + bgB);
                    break;
                default:
                    blendedR = srcR;
                    blendedG = srcG;
                    blendedB = srcB;
                    break;
            }

            if (srcA == 255 && mode == LayerBlending.Normal)
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
    }
}