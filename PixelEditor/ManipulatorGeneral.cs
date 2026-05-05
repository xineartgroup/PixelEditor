using PixelEditor.Vector;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace PixelEditor
{
    public static class ManipulatorGeneral
    {
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
            float aspectRatio = (float)Document.Width / Document.Height;
            float containerAspectRatio = (float)width / height;

            float scaledWidth = aspectRatio > containerAspectRatio
                ? width * Document.Zoom
                : height * Document.Zoom * aspectRatio;

            return scaledWidth / Document.Width;
        }

        public static Point ScreenToWorld(Point screenPt, int width, int height)
        {
            float aspectRatio = (float)Document.Width / Document.Height;
            float containerAspectRatio = (float)width / height;

            float scaledWidth, scaledHeight;
            if (aspectRatio > containerAspectRatio)
            {
                scaledWidth = width * Document.Zoom;
                scaledHeight = scaledWidth / aspectRatio;
            }
            else
            {
                scaledHeight = height * Document.Zoom;
                scaledWidth = scaledHeight * aspectRatio;
            }

            float centerX = (width - scaledWidth) / 2;
            float centerY = (height - scaledHeight) / 2;

            float ratio = scaledWidth / Document.Width;

            int worldX = (int)((screenPt.X - (centerX + Document.ImageOffset.X)) / ratio);
            int worldY = (int)((screenPt.Y - (centerY + Document.ImageOffset.Y)) / ratio);

            return new Point(worldX, worldY);
        }

        public static Point WorldToScreen(Point worldPt, int width, int height)
        {
            float aspectRatio = (float)Document.Width / Document.Height;
            float containerAspectRatio = (float)width / height;

            float scaledWidth, scaledHeight;

            if (aspectRatio > containerAspectRatio)
            {
                scaledWidth = width * Document.Zoom;
                scaledHeight = scaledWidth / aspectRatio;
            }
            else
            {
                scaledHeight = height * Document.Zoom;
                scaledWidth = scaledHeight * aspectRatio;
            }

            float centerX = (width - scaledWidth) / 2;
            float centerY = (height - scaledHeight) / 2;

            float ratio = scaledWidth / Document.Width;

            int screenX = (int)(worldPt.X * ratio + (centerX + Document.ImageOffset.X));
            int screenY = (int)(worldPt.Y * ratio + (centerY + Document.ImageOffset.Y));

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
            if (_canvasBitmap == null || _canvasBitmap.Width != Document.Width || _canvasBitmap.Height != Document.Height)
                PopulateBackgroundImage();

            bool canUseCache = false;

            if (selectedLayerIndex >= 0 && selectedLayerIndex < layers.Count)
            {
                PopulateColorGrid(layers, selectedLayerIndex);
                canUseCache = true;
                return;
            }

            Screen = new ColorGrid(Document.Width, Document.Height);

            if (includeBackground)
            {
                InitializeScreenWithBackground();
            }

            for (int i = layers.Count - 1; i >= 0; i--)
            {
                var layer = layers[i];
                layer.UseImageCache = i != selectedLayerIndex || canUseCache;
                Image? image = layer.Image;
                layer.UseImageCache = false;
                if (!layer.IsVisible || image == null) continue;

                int displayWidth = image.Width * layer.ScaleWidth;
                int displayHeight = image.Height * layer.ScaleHeight;
                Rectangle layerBounds = new(layer.X, layer.Y, displayWidth, displayHeight);

                int stateHash = HashCode.Combine(
                    layer.X, layer.Y, layer.ScaleWidth, layer.ScaleHeight,
                    image.GetHashCode(), layer.Opacity, layer.BlendMode);

                bool isDirty = DirtyRegions.Any(r => r.IntersectsWith(layerBounds));

                if (!isDirty && LayerCache.TryGetValue(layer.Name, out var cached) && cached.Hash == stateHash)
                {
                    ApplyCachedLayer(Screen, cached.Cache, layerBounds, Document.Width, Document.Height, layer.BlendMode);
                    continue;
                }

                ColorGrid layerBuffer = RasterizeLayer(layer, displayWidth, displayHeight, useCachedImage: i != selectedLayerIndex);
                LayerCache[layer.Name] = (layerBuffer, stateHash);
                ApplyCachedLayer(Screen, layerBuffer, layerBounds, Document.Width, Document.Height, layer.BlendMode);
            }

            DirtyRegions.Clear();
        }

        private static void PopulateColorGrid(List<Layer> layers, int selectedLayerIndex)
        {
            if (Screen.Width != Document.Width || Screen.Height != Document.Height)
                Screen = new ColorGrid(Document.Width, Document.Height);

            int bgHash = ComputeGroupHash(layers, selectedLayerIndex + 1, layers.Count);
            bool bgValid = _backgroundBuffer != null && _cachedSelectedIndex == selectedLayerIndex && _backgroundHash == bgHash;

            Rectangle compositeRegion = new(0, 0, Document.Width, Document.Height);
            if (DirtyRegions.Count > 0)
            {
                Rectangle combined = Rectangle.Empty;
                foreach (var region in DirtyRegions)
                    combined = combined.IsEmpty ? region : Rectangle.Union(combined, region);
                combined.Intersect(new Rectangle(0, 0, Document.Width, Document.Height));
                if (!combined.IsEmpty)
                    compositeRegion = combined;
            }

            if (!bgValid)
            {
                _backgroundBuffer = new ColorGrid(Document.Width, Document.Height);
                InitializeColorGridWithBackground(_backgroundBuffer);

                for (int i = layers.Count - 1; i > selectedLayerIndex; i--)
                {
                    var layer = layers[i];
                    layer.UseImageCache = true;
                    Image? image = layer.Image;
                    layer.UseImageCache = false;
                    if (!layer.IsVisible || image == null) continue;

                    int dw = image.Width * layer.ScaleWidth;
                    int dh = image.Height * layer.ScaleHeight;
                    Rectangle lb = new(layer.X, layer.Y, dw, dh);

                    int hash = HashCode.Combine(layer.X, layer.Y, layer.ScaleWidth, layer.ScaleHeight,
                        image.GetHashCode(), layer.Opacity, layer.BlendMode);

                    if (!LayerCache.TryGetValue(layer.Name, out var cached) || cached.Hash != hash)
                    {
                        cached = (RasterizeLayer(layer, dw, dh, useCachedImage: i != selectedLayerIndex), hash);
                        LayerCache[layer.Name] = cached;
                    }

                    ApplyCachedLayer(_backgroundBuffer, cached.Cache, lb, Document.Width, Document.Height, layer.BlendMode);
                }

                _backgroundHash = bgHash;
            }

            _cachedSelectedIndex = selectedLayerIndex;

            int[] screenPixels = Screen.GetRawPixels();
            int[] bgPixels = _backgroundBuffer!.GetRawPixels();

            for (int y = compositeRegion.Top; y < compositeRegion.Bottom; y++)
            {
                int rowBase = y * Document.Width;
                Array.Copy(bgPixels, rowBase + compositeRegion.Left, screenPixels, rowBase + compositeRegion.Left, compositeRegion.Width);
            }

            var activeLayer = layers[selectedLayerIndex];
            Image? image1 = activeLayer.Image;
            if (activeLayer.IsVisible && image1 != null)
            {
                int dw = image1.Width * activeLayer.ScaleWidth;
                int dh = image1.Height * activeLayer.ScaleHeight;
                Rectangle activeBounds = new(activeLayer.X, activeLayer.Y, dw, dh);
                ColorGrid activeBuffer = RasterizeLayer(activeLayer, dw, dh, useCachedImage: true);

                if (activeBounds.IntersectsWith(compositeRegion))
                    ApplyCachedLayerRegion(Screen, activeBuffer, activeBounds, compositeRegion, activeLayer.BlendMode);
            }

            for (int i = selectedLayerIndex - 1; i >= 0; i--)
            {
                var layer = layers[i];
                layer.UseImageCache = true;
                Image? image = layer.Image;
                layer.UseImageCache = false;
                if (!layer.IsVisible || image == null) continue;

                int dw = image.Width * layer.ScaleWidth;
                int dh = image.Height * layer.ScaleHeight;
                Rectangle lb = new(layer.X, layer.Y, dw, dh);

                int hash = HashCode.Combine(layer.X, layer.Y, layer.ScaleWidth, layer.ScaleHeight,
                    image.GetHashCode(), layer.Opacity, layer.BlendMode);

                if (!LayerCache.TryGetValue(layer.Name, out var cached) || cached.Hash != hash)
                {
                    cached = (RasterizeLayer(layer, dw, dh, useCachedImage: i != selectedLayerIndex), hash);
                    LayerCache[layer.Name] = cached;
                }

                ApplyCachedLayerRegion(Screen, cached.Cache, lb, compositeRegion, layer.BlendMode);
            }

            DirtyRegions.Clear();
        }

        public static void PopulateBackgroundImage()
        {
            _canvasBitmap = new Bitmap(Document.Width, Document.Height);

            using Graphics g = Graphics.FromImage(_canvasBitmap);

            int size = Math.Max(Document.Width, Document.Height);

            int squareSize = size > 512 ? 20 : size > 256 ? 10 : 5;

            Color lightColor = Color.LightGray;
            Color darkColor = Color.White;

            for (int x = 0; x < Document.Width; x += squareSize)
            {
                for (int y = 0; y < Document.Height; y += squareSize)
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

        public static Bitmap? ExtractSelectedArea(Layer selectedLayer)
        {
            Image? image = selectedLayer.Image;
            if (image == null) return null;

            int width = image.Width;
            int height = image.Height;

            var selections = SelectionsManipulator.GetSelections();
            var selectionPolygons = selections.Where(s => s.Points.Count >= 3).ToList();

            bool[,] mask = new bool[width, height];

            foreach (var poly in selectionPolygons)
            {
                if (poly.Mask != null)
                {
                    int maskWidth = poly.Mask.GetLength(0);
                    int maskHeight = poly.Mask.GetLength(1);

                    for (int y = 0; y < height; y++)
                    {
                        for (int x = 0; x < width; x++)
                        {
                            int globalX = x + selectedLayer.X;
                            int globalY = y + selectedLayer.Y;

                            if (globalX >= 0 && globalX < maskWidth && globalY >= 0 && globalY < maskHeight)
                            {
                                if (poly.Mask[globalX, globalY])
                                {
                                    mask[x, y] = poly.Adding; // Apply the state (add or subtract)
                                }
                            }
                        }
                    }
                }
                else
                {
                    var localPoints = poly.Points
                        .Select(p => new Point((int)(p.X - selectedLayer.X), (int)(p.Y - selectedLayer.Y)))
                        .ToList();

                    SelectionsManipulator.FillPolygonInMask(mask, localPoints, true);
                }
            }

            int minX = width, maxX = 0, minY = height, maxY = 0;
            bool areaFound = false;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (mask[x, y])
                    {
                        if (x < minX) minX = x;
                        if (x > maxX) maxX = x;
                        if (y < minY) minY = y;
                        if (y > maxY) maxY = y;
                        areaFound = true;
                    }
                }
            }

            if (!areaFound) return null;

            int cropWidth = maxX - minX + 1;
            int cropHeight = maxY - minY + 1;
            Bitmap result = new(cropWidth, cropHeight, PixelFormat.Format32bppArgb);

            using (Bitmap sourceBitmap = new(image))
            {
                BitmapData sourceData = sourceBitmap.LockBits(
                    new Rectangle(0, 0, width, height),
                    ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

                BitmapData resultData = result.LockBits(
                    new Rectangle(0, 0, cropWidth, cropHeight),
                    ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);

                int sStride = sourceData.Stride;
                int rStride = resultData.Stride;
                IntPtr sPtr = sourceData.Scan0;
                IntPtr rPtr = resultData.Scan0;

                try
                {
                    Parallel.For(0, cropHeight, y =>
                    {
                        int sourceY = y + minY;
                        unsafe
                        {
                            byte* sRow = (byte*)sPtr + (sourceY * sStride);
                            byte* rRow = (byte*)rPtr + (y * rStride);

                            for (int x = 0; x < cropWidth; x++)
                            {
                                int sourceX = x + minX;
                                int rOffset = x * 4;

                                if (mask[sourceX, sourceY])
                                {
                                    int sOffset = sourceX * 4;
                                    *(uint*)(rRow + rOffset) = *(uint*)(sRow + sOffset);
                                }
                                else
                                {
                                    *(uint*)(rRow + rOffset) = 0;
                                }
                            }
                        }
                    });
                }
                finally
                {
                    sourceBitmap.UnlockBits(sourceData);
                    result.UnlockBits(resultData);
                }
            }

            return result;
        }

        public static Bitmap? CutSelectionFromLayer(Layer selectedLayer)
        {
            Image? image = selectedLayer.Image;
            if (image == null) return null;

            bool emptyHole = selectedLayer.FillType == FillType.Transparency;

            int width = image.Width;
            int height = image.Height;
            Bitmap result = new(image);

            byte fillA = emptyHole ? (byte)0 : (byte)255;
            byte fillR = 255; byte fillG = 255; byte fillB = 255;

            var selections = SelectionsManipulator.GetSelections();
            var selectionPolygons = selections.Where(s => s.Points.Count >= 3).ToList();

            if (selectionPolygons.Count == 0) return result;

            bool[,] mask = new bool[width, height];

            foreach (var poly in selectionPolygons)
            {
                if (poly.Mask != null)
                {
                    int maskWidth = poly.Mask.GetLength(0);
                    int maskHeight = poly.Mask.GetLength(1);

                    for (int y = 0; y < height; y++)
                    {
                        for (int x = 0; x < width; x++)
                        {
                            int globalX = x + selectedLayer.X;
                            int globalY = y + selectedLayer.Y;

                            if (globalX >= 0 && globalX < maskWidth && globalY >= 0 && globalY < maskHeight)
                            {
                                if (poly.Mask[globalX, globalY])
                                {
                                    mask[x, y] = poly.Adding;
                                }
                            }
                        }
                    }
                }
                else
                {
                    var localPoints = poly.Points
                        .Select(p => new Point((int)(p.X - selectedLayer.X), (int)(p.Y - selectedLayer.Y)))
                        .ToList();

                    SelectionsManipulator.FillPolygonInMask(mask, localPoints, true);
                }
            }

            BitmapData data = result.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadWrite, PixelFormat.Format32bppPArgb);
            int stride = data.Stride;
            IntPtr ptr = data.Scan0;

            try
            {
                Parallel.For(0, height, y =>
                {
                    unsafe
                    {
                        byte* row = (byte*)ptr + (y * stride);

                        for (int x = 0; x < width; x++)
                        {
                            if (mask[x, y])
                            {
                                int offset = x * 4;
                                row[offset] = fillB;
                                row[offset + 1] = fillG;
                                row[offset + 2] = fillR;
                                row[offset + 3] = fillA;
                            }
                        }
                    }
                });
            }
            finally
            {
                result.UnlockBits(data);
            }

            return result;
        }

        public static Bitmap? FillColor(Layer selectedLayer, int canvasWidth, int canvasHeight, ImageBlending blend, Color color, float opacity, Point startPoint, SelectionPolygon selectionPoints)
        {
            if (selectedLayer.Image == null)
                return null;

            Bitmap bitmap = new(selectedLayer.Image);
            int width = bitmap.Width;
            int height = bitmap.Height;
            Rectangle rect = new(0, 0, width, height);
            BitmapData data = bitmap.LockBits(rect, ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);

            int bytes = data.Stride * height;
            byte[] pixels = new byte[bytes];
            Marshal.Copy(data.Scan0, pixels, 0, bytes);

            float screenAspectRatio = Document.Width / Document.Height;
            float containerAspectRatio = (float)canvasWidth / canvasHeight;
            float screenScaledWidth, screenScaledHeight;

            if (screenAspectRatio > containerAspectRatio)
            {
                screenScaledWidth = canvasWidth * Document.Zoom;
                screenScaledHeight = screenScaledWidth / screenAspectRatio;
            }
            else
            {
                screenScaledHeight = canvasHeight * Document.Zoom;
                screenScaledWidth = screenScaledHeight * screenAspectRatio;
            }

            float screenOriginX = ((canvasWidth - screenScaledWidth) / 2) + Document.ImageOffset.X;
            float screenOriginY = ((canvasHeight - screenScaledHeight) / 2) + Document.ImageOffset.Y;

            PointF WorldToImage(Point p)
            {
                float layerPixelX = (p.X - selectedLayer.X) / selectedLayer.ScaleWidth;
                float layerPixelY = (p.Y - selectedLayer.Y) / selectedLayer.ScaleHeight;
                return new PointF(layerPixelX, layerPixelY);
            }

            Point startI = ScreenToWorld(startPoint, canvasWidth, canvasHeight);
            startI.X -= selectedLayer.X;
            startI.Y -= selectedLayer.Y;

            if (startI.X < 0 || startI.X >= width || startI.Y < 0 || startI.Y >= height)
            {
                bitmap.UnlockBits(data);
                return bitmap;
            }

            bool usePointSelection = selectionPoints.Points.Count > 2;
            byte[] mask = [];

            if (usePointSelection)
            {
                mask = new byte[width * height];
                using Bitmap maskBmp = new(width, height, PixelFormat.Format32bppArgb);
                using (Graphics g = Graphics.FromImage(maskBmp))
                {
                    g.Clear(Color.Transparent);
                    PointF[] pts = [.. selectionPoints.Points.Select(WorldToImage)];
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

        public static Bitmap? FillColor(Layer selectedLayer, ImageBlending blend, Color color, float opacity, bool[,]? mask)
        {
            if (selectedLayer.Image == null)
                return null;

            Bitmap bitmap = new(selectedLayer.Image);
            int width = bitmap.Width;
            int height = bitmap.Height;
            Rectangle rect = new(0, 0, width, height);
            BitmapData data = bitmap.LockBits(rect, ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);

            int bytes = data.Stride * height;
            byte[] pixels = new byte[bytes];
            Marshal.Copy(data.Scan0, pixels, 0, bytes);

            float a = opacity;
            float ia = 1.0f - opacity;
            byte cr = color.R, cg = color.G, cb = color.B;

            if (mask != null)
            {
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        if (x < mask.GetLength(0) && y < mask.GetLength(1) && mask[x, y])
                        {
                            int i = (y * data.Stride) + (x * 4);
                            ApplyBlend(i, cr, cg, cb, a, ia, blend, pixels, color.A);
                        }
                    }
                }
            }

            Marshal.Copy(pixels, 0, data.Scan0, bytes);
            bitmap.UnlockBits(data);
            return bitmap;
        }

        private static void ApplyBlend(int idx, byte cr, byte cg, byte cb, float alpha, float invAlpha, ImageBlending blend, byte[] pixels, byte colorA)
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
            // Maintain or additive alpha
            pixels[idx + 3] = (byte)Math.Min(255, pixels[idx + 3] + (colorA * alpha));
        }

        public static Layer MergeLayers(Layer top, Layer bottom)
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

        public static ColorGrid RasterizeLayer(Layer layer, int displayWidth, int displayHeight, bool useCachedImage = false)
        {
            ColorGrid layerBuffer = new(displayWidth, displayHeight);
            int[] pixels = layerBuffer.GetRawPixels();
            float masterOpacity = layer.Opacity / 100f;
            LayerChannel channel = layer.Channel;

            unsafe
            {
                layer.UseImageCache = useCachedImage;
                using Bitmap bmp = new(layer.Image!);
                layer.UseImageCache = false;
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

        public static ColorGrid RasterizeImage(Image? image)
        {
            if (image != null)
            {
                int width = image.Width;
                int height = image.Height;

                ColorGrid grid = new(width, height);
                int[] pixels = grid.GetRawPixels();

                using Bitmap bmp = image is Bitmap bitmap ? bitmap : new Bitmap(image);

                unsafe
                {
                    BitmapData data = bmp.LockBits(
                        new Rectangle(0, 0, width, height),
                        ImageLockMode.ReadOnly,
                        PixelFormat.Format32bppArgb);

                    try
                    {
                        if (data.Stride == width * 4)
                        {
                            fixed (int* pDest = pixels)
                            {
                                Buffer.MemoryCopy((void*)data.Scan0, pDest, pixels.Length * 4, pixels.Length * 4);
                            }
                        }
                        else
                        {
                            byte* pSrc = (byte*)data.Scan0;
                            fixed (int* pDest = pixels)
                            {
                                byte* pDestByte = (byte*)pDest;
                                for (int y = 0; y < height; y++)
                                {
                                    Buffer.MemoryCopy(
                                        pSrc + (y * data.Stride),
                                        pDestByte + (y * width * 4),
                                        width * 4,
                                        width * 4);
                                }
                            }
                        }
                    }
                    finally
                    {
                        bmp.UnlockBits(data);
                    }
                }

                return grid;
            }

            return new ColorGrid(0, 0);
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

        private static void InitializeScreenWithBackground()
        {
            if (_canvasBitmap == null) return;

            var bgData = _canvasBitmap.LockBits(
                new Rectangle(0, 0, Document.Width, Document.Height),
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

                    for (int y = 0; y < Document.Height; y++)
                    {
                        int rowOffset = y * stride;
                        int screenRowOffset = y * Document.Width;

                        for (int x = 0; x < Document.Width; x++)
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
                new Rectangle(0, 0, Document.Width, Document.Height),
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

                    for (int y = 0; y < Document.Height; y++)
                    {
                        int rowOffset = y * stride;
                        int gridRowOffset = y * Document.Width;

                        for (int x = 0; x < Document.Width; x++)
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
                var l = layers[i]; // You are not including the selected layer's image hash in the group hash
                l.UseImageCache = true;
                hc.Add(l.X);
                hc.Add(l.Y);
                hc.Add(l.ScaleWidth);
                hc.Add(l.ScaleHeight);
                hc.Add(l.Image?.GetHashCode() ?? 0);
                hc.Add(l.Opacity);
                hc.Add(l.BlendMode);
                hc.Add(l.IsVisible);
                l.UseImageCache = false;
            }
            return hc.ToHashCode();
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

        public static bool IsOverRotationHandle(Point screenPoint, int worldWidth, int worldHeight, int canvasWidth, int canvasHeight, float zoom)
        {
            PointF worldHandle = new(
                SelectionsManipulator.GetSelectionCenter().X,
                SelectionsManipulator.GetSelectionBounds().Y - SelectionsManipulator.ROTATION_HANDLE_SIZE / SelectionsManipulator.GetScreenToWorldScale(worldWidth, worldHeight, canvasWidth, canvasHeight, zoom)
            );
            Point screenHandle = WorldToScreen(Point.Round(worldHandle), canvasWidth, canvasHeight);
            float distance = Utility.VectorDist(screenPoint, screenHandle);
            return distance < SelectionsManipulator.ROTATION_HANDLE_SIZE;
        }

        public static RectangleF GetSelectionBoundsScreen(int canvasWidth, int canvasHeight)
        {
            Point topLeft = WorldToScreen(new Point((int)SelectionsManipulator.GetSelectionBounds().X, (int)SelectionsManipulator.GetSelectionBounds().Y), canvasWidth, canvasHeight);
            Point bottomRight = WorldToScreen(new Point((int)SelectionsManipulator.GetSelectionBounds().Right, (int)SelectionsManipulator.GetSelectionBounds().Bottom), canvasWidth, canvasHeight);
            return new RectangleF(topLeft.X, topLeft.Y, bottomRight.X - topLeft.X, bottomRight.Y - topLeft.Y);
        }

        public static PointF GetSelectionCenterScreen(int canvasWidth, int canvasHeight)
        {
            Point p = WorldToScreen(Point.Round(SelectionsManipulator.GetSelectionCenter()), canvasWidth, canvasHeight);
            return p;
        }

        public static float CalculateRotationAngle(Point screenMousePosition, int canvasWidth, int canvasHeight)
        {
            PointF screenCenter = GetSelectionCenterScreen(canvasWidth, canvasHeight);
            float dx = screenMousePosition.X - screenCenter.X;
            float dy = screenMousePosition.Y - screenCenter.Y;
            return (float)(Math.Atan2(dy, dx) * 180 / Math.PI);
        }

        public static bool IsOverScaleHandle(Point screenPoint, int canvasWidth, int canvasHeight, out string handle)
        {
            handle = "";

            RectangleF b = SelectionsManipulator.GetSelectionBounds();

            float midX = (b.Left + b.Right) / 2f;
            float midY = (b.Top + b.Bottom) / 2f;

            var handles = new[]
            {
                new { Name = "topLeft",     World = new PointF(b.Left,  b.Top) },
                new { Name = "topRight",    World = new PointF(b.Right, b.Top) },
                new { Name = "bottomLeft",  World = new PointF(b.Left,  b.Bottom) },
                new { Name = "bottomRight", World = new PointF(b.Right, b.Bottom) },
                new { Name = "topMid",      World = new PointF(midX,    b.Top) },
                new { Name = "bottomMid",   World = new PointF(midX,    b.Bottom) },
                new { Name = "leftMid",     World = new PointF(b.Left,  midY) },
                new { Name = "rightMid",    World = new PointF(b.Right, midY) },
            };

            foreach (var h in handles)
            {
                Point screenCorner = WorldToScreen(Point.Round(h.World), canvasWidth, canvasHeight);
                if (Utility.VectorDist(screenPoint, screenCorner) < SelectionsManipulator.SCALE_HANDLE_SIZE)
                {
                    handle = h.Name;
                    return true;
                }
            }

            return false;
        }

        public static PointF GetOppositeAnchor(string handle)
        {
            RectangleF b = SelectionsManipulator.GetSelectionBounds();
            float midX = (b.Left + b.Right) / 2f;
            float midY = (b.Top + b.Bottom) / 2f;

            return handle switch
            {
                "topLeft" => new PointF(b.Right, b.Bottom),
                "topRight" => new PointF(b.Left, b.Bottom),
                "bottomLeft" => new PointF(b.Right, b.Top),
                "bottomRight" => new PointF(b.Left, b.Top),
                "topMid" => new PointF(midX, b.Bottom),
                "bottomMid" => new PointF(midX, b.Top),
                "leftMid" => new PointF(b.Right, midY),
                "rightMid" => new PointF(b.Left, midY),
                _ => new PointF(midX, midY),
            };
        }

        public static (bool affectsX, bool affectsY) GetHandleAxes(string handle) => handle switch
        {
            "topMid" or "bottomMid" => (false, true),
            "leftMid" or "rightMid" => (true, false),
            _ => (true, true),   // corners affect both
        };

        public static bool IsPointInShape(Point p, BaseShape shape)
        {
            if (shape is ShapeRect r)
            {
                return p.X >= r.X && p.X <= r.X + r.Width &&
                       p.Y >= r.Y && p.Y <= r.Y + r.Height;
            }

            if (shape is ShapeEllipse el)
            {
                if (el.Rx == 0 || el.Ry == 0) return false;

                float dx = p.X - el.Cx;
                float dy = p.Y - el.Cy;

                float rxSq = el.Rx * el.Rx;
                float rySq = el.Ry * el.Ry;

                return (dx * dx * rySq) + (dy * dy * rxSq) <= rxSq * rySq;
            }

            if (shape is ShapePolygon pg)
            {
                List<PointF> polygon = pg.Points;
                if (polygon.Count < 3) return false;

                bool isInside = false;
                for (int i = 0, j = polygon.Count - 1; i < polygon.Count; j = i++)
                {
                    if (((polygon[i].Y > p.Y) != (polygon[j].Y > p.Y)) &&
                        (p.X < (polygon[j].X - polygon[i].X) * (p.Y - polygon[i].Y) / (polygon[j].Y - polygon[i].Y) + polygon[i].X))
                    {
                        isInside = !isInside;
                    }
                }
                return isInside;
            }

            if (shape is ShapeText t)
            {
                return p.X >= t.X && p.X <= t.X + t.Width &&
                       p.Y >= t.Y && p.Y <= t.Y + t.Height;
            }

            if (shape is ShapePath path)
            {
                using GraphicsPath gPath = Layer.BuildGraphicsPath(path);

                if (gPath.IsVisible(p))
                {
                    return true;
                }

                using Pen testPen = new(Color.Black, Math.Max(5, path.LineWidth));
                if (gPath.IsOutlineVisible(p, testPen))
                {
                    return true;
                }
            }

            return false;
        }

        public static DashStyle GetDashStyle(string strStyle)
        {
            return strStyle switch
            {
                "Solid" => DashStyle.Solid,
                "Dash" => DashStyle.Dash,
                "Dot" => DashStyle.Dot,
                "DashDot" => DashStyle.DashDot,
                "DashDotDot" => DashStyle.DashDotDot,
                "Custom" => DashStyle.Custom,
                _ => DashStyle.Solid,
            };
        }

        public static Color PickColorAtPoint(Layer selectedLayer, PointF startPoint)
        {
            int localX = (int)(startPoint.X - selectedLayer.X);
            int localY = (int)(startPoint.Y - selectedLayer.Y);

            if (selectedLayer.Image == null)
            {
                return Color.Transparent;
            }

            using Bitmap? bmp = new (selectedLayer.Image);

            if (localX >= 0 && localX < bmp.Width && localY >= 0 && localY < bmp.Height)
            {
                return bmp.GetPixel(localX, localY);
            }

            return Color.Transparent;
        }
    }
}