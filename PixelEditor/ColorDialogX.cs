using System.ComponentModel;
using System.Drawing.Drawing2D;

namespace PixelEditor
{
    public partial class ColorDialogX : Form
    {
        private Color _color = Color.Red;
        private float _hue = 0;
        private float _saturation = 1;
        private float _brightness = 1;
        private static Cursor? _cursor = null;

        public Action<Color>? OnColorAccepted = null;

        public static bool IsEyeDropping { get; set; } = false;

        public static PictureBox Canvas { get; set; } = new();

        public ColorDialogX()
        {
            InitializeComponent();
            TopMost = true;
        }

        [Category("Appearance")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public Color Color
        {
            get => _color;
            set
            {
                _color = value;
                UpdateHSBFromColor();
                tkbHue.Value = (int)_hue;
                picColorBox.Invalidate();
                if (picMouseDownPreview.BackColor != _color)
                    picMouseDownPreview.BackColor = _color;
            }
        }

        private bool ShouldSerializeColor() => _color != Color.Red;
        private void ResetColor() => Color = Color.Red;

        private void TkbHue_Scroll(object sender, EventArgs e)
        {
            _hue = tkbHue.Value;
            UpdateColor();
            picColorBox.Invalidate();
        }

        private void PicColorBox_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                Color clickedColor = GetColorFromMouse(e.X, e.Y);
                picMouseDownPreview.BackColor = clickedColor;
                _saturation = Math.Clamp((float)e.X / picColorBox.Width, 0, 1);
                _brightness = Math.Clamp(1 - ((float)e.Y / picColorBox.Height), 0, 1);
                UpdateColor();
                picColorBox.Invalidate();
            }
        }

        private void PicColorBox_MouseMove(object sender, MouseEventArgs e)
        {
            Color hoveredColor = GetColorFromMouse(e.X, e.Y);
            picMouseMovePreview.BackColor = hoveredColor;
            _saturation = Math.Clamp((float)e.X / picColorBox.Width, 0, 1);
            _brightness = Math.Clamp(1 - ((float)e.Y / picColorBox.Height), 0, 1);
        }

        private Color GetColorFromMouse(int x, int y)
        {
            float s = Math.Clamp((float)x / picColorBox.Width, 0, 1);
            float b = Math.Clamp(1 - ((float)y / picColorBox.Height), 0, 1);
            return ColorFromHSB(_hue, s, b);
        }

        private void PicColorBox_Paint(object sender, PaintEventArgs e)
        {
            using (var brushHue = new LinearGradientBrush(picColorBox.ClientRectangle, Color.White, ColorFromHSB(_hue, 1, 1), 0f))
            {
                e.Graphics.FillRectangle(brushHue, picColorBox.ClientRectangle);
            }

            using var brushBlack = new LinearGradientBrush(picColorBox.ClientRectangle, Color.Transparent, Color.Black, 90f);
            e.Graphics.FillRectangle(brushBlack, picColorBox.ClientRectangle);

            int x = (int)(_saturation * (picColorBox.Width - 1));
            int y = (int)((1 - _brightness) * (picColorBox.Height - 1));

            int size = 8; // Size of the square
            Rectangle rect = new (x - size / 2, y - size / 2, size, size);

            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            using Pen whitePen = new(Color.White, 1.5f);
            using Pen blackPen = new(Color.Black, 1.5f);
            e.Graphics.DrawRectangle(blackPen, rect);

            rect.Inflate(-1, -1);
            e.Graphics.DrawRectangle(whitePen, rect);
        }

        private void UpdateColor()
        {
            _color = ColorFromHSB(_hue, _saturation, _brightness);
        }

        private void UpdateHSBFromColor()
        {
            _hue = _color.GetHue();
            _saturation = _color.GetSaturation();
            _brightness = _color.GetBrightness();
        }

        private static Color ColorFromHSB(float h, float s, float b)
        {
            int hi = Convert.ToInt32(Math.Floor(h / 60)) % 6;
            double f = h / 60 - Math.Floor(h / 60);
            double v = b * 255;
            int p = Convert.ToInt32(v * (1 - s));
            int q = Convert.ToInt32(v * (1 - f * s));
            int t = Convert.ToInt32(v * (1 - (1 - f) * s));
            int vi = Convert.ToInt32(v);

            if (hi == 0) return Color.FromArgb(255, vi, t, p);
            if (hi == 1) return Color.FromArgb(255, q, vi, p);
            if (hi == 2) return Color.FromArgb(255, p, vi, t);
            if (hi == 3) return Color.FromArgb(255, p, q, vi);
            if (hi == 4) return Color.FromArgb(255, t, p, vi);
            return Color.FromArgb(255, vi, p, q);
        }

        private void BtnOK_Click(object sender, EventArgs e)
        {
            if (_cursor != null)
                Canvas.Cursor = new Cursor(_cursor.Handle);
            IsEyeDropping = false;
            DialogResult = DialogResult.OK;
            OnColorAccepted?.Invoke(_color);
            Hide();
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            if (_cursor != null)
                Canvas.Cursor = new Cursor(_cursor.Handle);
            IsEyeDropping = false;
            DialogResult = DialogResult.Cancel;
            Hide();
        }

        private void BtnEyeDropper_Click(object sender, EventArgs e)
        {
            UpdateCursor(btnEyeDropper.Image);
            IsEyeDropping = true;
        }

        private static void UpdateCursor(Image? image)
        {
            if (image != null)
            {
                _cursor = new Cursor(Canvas.Cursor.Handle);
                Bitmap bitmap = new(image, 24, 24);
                bitmap.MakeTransparent(Color.White);
                for (int i = 128; i < 256; i++)
                {
                    bitmap.MakeTransparent(Color.FromArgb(i, i, i));
                }
                for (int y = 0; y < bitmap.Height; y++)
                {
                    for (int x = 0; x < bitmap.Width; x++)
                    {
                        Color sourcePixel = bitmap.GetPixel(x, y);

                        Color tL = x > 0 && y > 0 ? bitmap.GetPixel(x - 1, y - 1) : Color.Transparent;
                        Color tM = y > 0 ? bitmap.GetPixel(x, y - 1) : Color.Transparent;
                        Color tR = x < bitmap.Width - 1 && y > 0 ? bitmap.GetPixel(x + 1, y - 1) : Color.Transparent;
                        Color cL = x > 0 ? bitmap.GetPixel(x - 1, y) : Color.Transparent;
                        Color cR = x < bitmap.Width - 1 ? bitmap.GetPixel(x + 1, y) : Color.Transparent;
                        Color bL = x > 0 && y < bitmap.Height - 1 ? bitmap.GetPixel(x - 1, y + 1) : Color.Transparent;
                        Color bM = y < bitmap.Height - 1 ? bitmap.GetPixel(x, y + 1) : Color.Transparent;
                        Color bR = x < bitmap.Width - 1 && y < bitmap.Height - 1 ? bitmap.GetPixel(x + 1, y + 1) : Color.Transparent;

                        if ((tL.A == 0 || tM.A == 0 || tR.A == 0 || cL.A == 0 || cR.A == 0 || bL.A == 0 || bM.A == 0 || bR.A == 0) && sourcePixel.A > 0)
                        {
                            bitmap.SetPixel(x, y, Color.Black);
                        }
                    }
                }
                Canvas.Cursor = GraphicsUtility.GetCursor(new Bitmap(bitmap, 24, 24), 0, 0);
            }
        }
    }
}