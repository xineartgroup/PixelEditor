using System.Drawing.Imaging;

namespace PixelEditor
{
    public struct Paint
    {
        private Bitmap? bitmap = null;
        private Bitmap? brush = null;

        public Paint()
        {
        }

        public void SetColor(Color color)
        {
            if (bitmap != null)
            {
                brush?.Dispose();
                brush = new Bitmap(bitmap.Width, bitmap.Height, PixelFormat.Format32bppArgb);
                brush.MakeTransparent(Color.White);
                for (int i = 192; i < 256; i++)
                {
                    brush.MakeTransparent(Color.FromArgb(i, i, i)); //Make all gray-scale pixels that are nearly white transparent
                }

                for (int y = 0; y < bitmap.Height; y++)
                {
                    for (int x = 0; x < bitmap.Width; x++)
                    {
                        Color sourcePixel = bitmap.GetPixel(x, y);

                        if (sourcePixel.A > 0)
                        {
                            float blend = sourcePixel.GetBrightness();

                            int a = (int)(255 - (255 * blend));

                            brush.SetPixel(x, y, Color.FromArgb(a, color));
                        }
                        else
                        {
                            brush.SetPixel(x, y, Color.Transparent);
                        }
                    }
                }
            }
        }

        public Bitmap? Brush
        {
            readonly get => brush;
            set
            {
                if (value != null)
                {
                    bitmap = value;
                    SetColor(Color.Black); // update brush
                }
                else
                {
                    bitmap = null;
                    brush = null;
                }
            }
        }
    }
}