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
        private static int _colorIndex = 0;
        private static Cursor? _cursor = null;
        private static List<PictureBox> _pictureBoxesInOrder = [];

        public Action<Color>? OnColorAccepted = null;

        public static bool IsEyeDropping { get; set; } = false;

        public static PictureBox Canvas { get; set; } = new();

        public static List<Color> ColorPallettes { get; set; } = [];

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
                tkbSaturation.Value = (int)(_saturation * tkbSaturation.Maximum);
                tkbValue.Value = (int)(_brightness * tkbValue.Maximum);
                picColorBox.Invalidate();
                if (picMouseDownPreview.BackColor != _color)
                    picMouseDownPreview.BackColor = _color;
            }
        }

        private void ColorDialogX_Load(object sender, EventArgs e)
        {
            if (_pictureBoxesInOrder.Count == 0)
            {
                _pictureBoxesInOrder = [.. groupBox1.Controls.OfType<PictureBox>()
                .Where(pic => pic.Name.StartsWith("picSave"))
                .OrderBy(pic => pic.Name)];

                foreach (var ctrl in groupBox1.Controls)
                {
                    if (ctrl is PictureBox pic)
                    {
                        pic.BackColor = Color.Transparent;
                    }
                }

                _pictureBoxesInOrder[_colorIndex].BorderStyle = BorderStyle.Fixed3D;
            }
        }

        private void ColorDialogX_VisibleChanged(object sender, EventArgs e)
        {
            if (Visible)
            {
                if (ColorPallettes.Count > 0)
                {
                    for (int i = 0; i < _pictureBoxesInOrder.Count; i++)
                    {
                        if (i < ColorPallettes.Count)
                        {
                            _pictureBoxesInOrder[i].BackColor = ColorPallettes[i];
                        }
                        else
                        {
                            _pictureBoxesInOrder[i].BackColor = Color.Transparent;
                        }
                    }
                }
            }
        }

        private void TkbHue_Scroll(object sender, EventArgs e)
        {
            _hue = tkbHue.Value;
            UpdateColor();
            picColorBox.Invalidate();
            picSaturation.Invalidate();
            picValue.Invalidate();
        }

        private void PicColorBox_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                _saturation = Math.Clamp((float)e.X / picColorBox.Width, 0f, 1f);
                _brightness = Math.Clamp(1f - ((float)e.Y / picColorBox.Height), 0f, 1f);

                tkbSaturation.Value = (int)(_saturation * tkbSaturation.Maximum);
                tkbValue.Value = (int)(_brightness * tkbValue.Maximum);

                UpdateColor();

                Color clickedColor = GetColorFromMouse(e.X, e.Y);
                picMouseDownPreview.BackColor = clickedColor;

                picColorBox.Invalidate();
                picSaturation.Invalidate();
                picValue.Invalidate();
            }
        }

        private void PicColorBox_MouseMove(object sender, MouseEventArgs e)
        {
            Color hoveredColor = GetColorFromMouse(e.X, e.Y);
            picMouseMovePreview.BackColor = hoveredColor;

            if (e.Button == MouseButtons.Left)
            {
                _saturation = Math.Clamp((float)e.X / picColorBox.Width, 0f, 1f);
                _brightness = Math.Clamp(1f - ((float)e.Y / picColorBox.Height), 0f, 1f);

                tkbSaturation.Value = (int)(_saturation * tkbSaturation.Maximum);
                tkbValue.Value = (int)(_brightness * tkbValue.Maximum);

                UpdateColor();

                picMouseDownPreview.BackColor = _color;

                picColorBox.Invalidate();
                picSaturation.Invalidate();
                picValue.Invalidate();
            }
        }

        private Color GetColorFromMouse(int x, int y)
        {
            float s = Math.Clamp((float)x / picColorBox.Width, 0, 1);
            float b = Math.Clamp(1 - ((float)y / picColorBox.Height), 0, 1);
            return ColorFromHSB(_hue, s, b);
        }

        private void PicColorRange_MouseDown(object sender, MouseEventArgs e)
        {
            tkbHue.Value = Math.Clamp((picColorRange.Height - e.Y) * 360 / picColorRange.Height, 0, 359);
            TkbHue_Scroll(sender, e);
        }

        private void PicColorRange_Paint(object sender, PaintEventArgs e)
        {
            for (int y = 0; y < picColorRange.Height; y++)
            {
                float hue = 360f * (picColorRange.Height - y) / picColorRange.Height;
                if (hue >= 360) hue = 359.99f;
                Color color = ColorFromHSB(hue, 1, 1);

                using var pen = new Pen(color);
                e.Graphics.DrawLine(pen, 0, y, picColorRange.Width, y);
            }
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
            Rectangle rect = new(x - size / 2, y - size / 2, size, size);

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
            string strColor = $"#{_color.R:X2}{_color.G:X2}{_color.B:X2}";
            if (txtColor.Text != strColor)
            {
                txtColor.Text = strColor;
            }
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

        private void TxtColor_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                TxtColor_TextChanged(sender, e);
            }
        }

        private void TxtColor_TextChanged(object sender, EventArgs e)
        {
            try
            {
                Color = Color.FromArgb(
                    255,
                    Convert.ToInt32(txtColor.Text.Substring(1, 2), 16),
                    Convert.ToInt32(txtColor.Text.Substring(3, 2), 16),
                    Convert.ToInt32(txtColor.Text.Substring(5, 2), 16)
                );
            }
            catch
            {
                UpdateColor();
            }
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

        private void TkbSaturation_Scroll(object sender, EventArgs e)
        {
            _saturation = Math.Clamp((float)tkbSaturation.Value / tkbSaturation.Maximum, 0f, 1f);
            UpdateColor();
            picColorBox.Invalidate();
            picSaturation.Invalidate();
            picValue.Invalidate();
        }

        private void TkbValue_Scroll(object sender, EventArgs e)
        {
            _brightness = Math.Clamp((float)tkbValue.Value / tkbValue.Maximum, 0f, 1f);
            UpdateColor();
            picColorBox.Invalidate();
            picSaturation.Invalidate();
            picValue.Invalidate();
        }

        private void PicSaturation_MouseDown(object sender, MouseEventArgs e)
        {
            tkbSaturation.Value = Math.Clamp((picSaturation.Height - e.Y) * tkbSaturation.Maximum / picSaturation.Height, 0, tkbSaturation.Maximum);
            TkbSaturation_Scroll(sender, e);
        }

        private void PicSaturation_Paint(object sender, PaintEventArgs e)
        {
            for (int y = 0; y < picSaturation.Height; y++)
            {
                float s = Math.Clamp((float)(picSaturation.Height - y) / picSaturation.Height, 0f, 1f);
                Color color = ColorFromHSB(_hue, s, _brightness);

                using var pen = new Pen(color);
                e.Graphics.DrawLine(pen, 0, y, picSaturation.Width, y);
            }
        }

        private void PicValue_MouseDown(object sender, MouseEventArgs e)
        {
            tkbValue.Value = Math.Clamp((picValue.Height - e.Y) * tkbValue.Maximum / picValue.Height, 0, tkbValue.Maximum);
            TkbValue_Scroll(sender, e);
        }

        private void PicValue_Paint(object sender, PaintEventArgs e)
        {
            for (int y = 0; y < picValue.Height; y++)
            {
                float v = Math.Clamp((float)(picValue.Height - y) / picValue.Height, 0f, 1f);
                Color color = ColorFromHSB(_hue, _saturation, v);

                using var pen = new Pen(color);
                e.Graphics.DrawLine(pen, 0, y, picValue.Width, y);
            }
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            _pictureBoxesInOrder[_colorIndex].BackColor = _color;
            _pictureBoxesInOrder[_colorIndex].BorderStyle = BorderStyle.FixedSingle;
            if (_colorIndex < _pictureBoxesInOrder.Count - 1)
            {
                _colorIndex++;
            }
            else
            {
                _colorIndex = 0;
            }
            _pictureBoxesInOrder[_colorIndex].BorderStyle = BorderStyle.Fixed3D;

            if (ColorPallettes.Count > 0)
            {
                ColorPallettes[_colorIndex] = _color;
            }
            else
            {
                ColorPallettes.Add(_color);
            }

            if (ColorPallettes.Count != _pictureBoxesInOrder.Count)
            {
                ColorPallettes.Clear();
                ColorPallettes.AddRange(new Color[_pictureBoxesInOrder.Count]);
            }

            for (int i = 0; i < _pictureBoxesInOrder.Count; i++)
            {
                if (_pictureBoxesInOrder[i].BackColor != ColorPallettes[i])
                {
                    ColorPallettes[i] = _pictureBoxesInOrder[i].BackColor;
                }
            }
        }

        private void PicSave_Click(object sender, EventArgs e)
        {
            if (sender is PictureBox pic)
            {
                if (pic.BackColor != Color.Transparent)
                {
                    Color = pic.BackColor;
                }

                for (int i = 0; i < _pictureBoxesInOrder.Count; i++)
                {
                    if (_pictureBoxesInOrder[i] == pic)
                    {
                        _colorIndex = i;
                    }
                    else if (_pictureBoxesInOrder[i].BorderStyle == BorderStyle.Fixed3D)
                    {
                        _pictureBoxesInOrder[i].BorderStyle = BorderStyle.FixedSingle;
                    }
                }

                pic.BorderStyle = BorderStyle.Fixed3D;
            }
        }

        private void ColorDialogX_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;

                if (_cursor != null)
                    Canvas.Cursor = new Cursor(_cursor.Handle);
                IsEyeDropping = false;
                DialogResult = DialogResult.Cancel;

                Hide();
            }
        }
    }
}