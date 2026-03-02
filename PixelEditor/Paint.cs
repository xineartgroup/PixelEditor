using System.Drawing.Imaging;

namespace PixelEditor
{
    public struct Paint
    {
        private Bitmap? brush = null;

        private Color color = Color.Black;

        public string Name = "";

        public Paint()
        {
        }

        public Color Color
        {
            set
            {
                color = value;
                if (brush != null)
                {
                    Brush = brush; //Re-apply the brush to make it transparent again, since changing the color may have made some pixels non-transparent

                    var result = new Bitmap(brush.Width, brush.Height, PixelFormat.Format32bppArgb);

                    for (int y = 0; y < brush.Height; y++)
                    {
                        for (int x = 0; x < brush.Width; x++)
                        {
                            Color sourcePixel = brush.GetPixel(x, y);

                            if (sourcePixel.A > 0)
                            {
                                float blend = sourcePixel.GetBrightness();

                                int a = (int)(255 - (255 * blend));
                                //int r = (int)(color.R * (1 - blend) + 255 * blend);
                                //int g = (int)(color.G * (1 - blend) + 255 * blend);
                                //int b = (int)(color.B * (1 - blend) + 255 * blend);

                                //result.SetPixel(x, y, Color.FromArgb(a, r, g, b));

                                result.SetPixel(x, y, Color.FromArgb(a, color));
                            }
                            else
                            {
                                result.SetPixel(x, y, Color.Transparent);
                            }
                        }
                    }

                    brush.Dispose();
                    brush = result;
                }
            }
        }

        public Bitmap? Brush
        {
            readonly get => brush;
            set
            {
                brush = value;
                if (brush != null)
                {
                    brush.MakeTransparent(Color.White);
                    for (int i = 192; i < 256; i++)
                    {
                        brush.MakeTransparent(Color.FromArgb(i, i, i)); //Make all gray-scale pixels that are nearly white transparent
                    }
                }
            }
        }
    }
}