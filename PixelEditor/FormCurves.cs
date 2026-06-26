using System.Drawing.Imaging;

namespace PixelEditor
{
    public partial class FormCurves : Form
    {
        public Image? Image = null;
        private Bitmap? _histogramImage = null;
        private Bitmap? _histogramImage_R = null;
        private Bitmap? _histogramImage_G = null;
        private Bitmap? _histogramImage_B = null;
        private readonly Dictionary<string, List<Point>> _originalCurves = [];
        private readonly Dictionary<string, List<Point>> _curves = [];
        private List<Point> _currentPoints = [];
        private string _currentChannel = "RGB";
        private bool _isDragging = false;
        private int _selectedPointIndex = -1;
        private int _darkScale = 0;
        private int _lightScale = 255;
        private const int MinDrawIntervalMs = 32;
        private DateTime _lastDrawTime = DateTime.MinValue;

        public FormCurves()
        {
            InitializeComponent();
            InitializeCurves();
            PopulateChannelComboBox();
        }

        private void InitializeCurves()
        {
            string[] channels = ["RGB", "R", "G", "B"];

            foreach (string channel in channels)
            {
                _originalCurves[channel] =
                [
                    new Point(0, 0),
                    new Point(255, 255)
                ];
                _curves[channel] =
                [
                    new Point(0, 0),
                    new Point(255, 255)
                ];
            }

            _currentPoints = _curves["RGB"];
        }

        private void PopulateChannelComboBox()
        {
            cboRGB.Items.Clear();
            cboRGB.Items.AddRange(["RGB", "R", "G", "B"]);
            cboRGB.SelectedIndex = 0;
        }

        private void FormCurves_Load(object sender, EventArgs e)
        {
            trackBarScaleDark.Value = 0;
            trackBarScaleLight.Value = 255;
            if (Image != null)
            {
                pictureSample.Image = new Bitmap(Image);
            }
            GenerateHistogram();
            DrawCurve();
            UpdatePreview();
            UpdatePointInfo();
        }

        private void GenerateHistogram()
        {
            if (Image == null) return;

            _histogramImage?.Dispose();
            _histogramImage_R?.Dispose();
            _histogramImage_G?.Dispose();
            _histogramImage_B?.Dispose();

            _histogramImage = GenerateHistogramChannel((Bitmap)Image, Color.FromArgb(120, 100, 200, 255));
            _histogramImage_R = GenerateHistogramChannel((Bitmap)Image, Color.FromArgb(120, 255, 0, 0));
            _histogramImage_G = GenerateHistogramChannel((Bitmap)Image, Color.FromArgb(120, 0, 255, 0));
            _histogramImage_B = GenerateHistogramChannel((Bitmap)Image, Color.FromArgb(120, 0, 0, 255));
        }

        private Bitmap GenerateHistogramChannel(Bitmap image, Color color)
        {
            Bitmap histBmp = new(pictureCurves.Width, pictureCurves.Height);
            using Graphics g = Graphics.FromImage(histBmp);
            g.Clear(Color.Transparent);

            int[] histogram = new int[256];

            BitmapData data = image.LockBits(new Rectangle(0, 0, image.Width, image.Height),
                ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

            unsafe
            {
                byte* ptr = (byte*)data.Scan0;
                for (int y = 0; y < image.Height; y++)
                {
                    for (int x = 0; x < image.Width; x++)
                    {
                        int index = y * data.Stride + x * 4;
                        byte b = ptr[index];
                        byte g2 = ptr[index + 1];
                        byte r = ptr[index + 2];

                        int value;
                        if (color == Color.FromArgb(120, 255, 0, 0))
                            value = r;
                        else if (color == Color.FromArgb(120, 0, 255, 0))
                            value = g2;
                        else if (color == Color.FromArgb(120, 0, 0, 255))
                            value = b;
                        else
                            value = (byte)(0.299 * r + 0.587 * g2 + 0.114 * b);

                        histogram[Math.Clamp(value, 0, 255)]++;
                    }
                }
            }
            image.UnlockBits(data);

            int maxCount = histogram.Max();
            if (maxCount > 0)
            {
                float scale = (float)pictureCurves.Height / maxCount;
                int barWidth = Math.Max(1, pictureCurves.Width / 256);

                for (int i = 0; i < 256; i++)
                {
                    int x = (i * pictureCurves.Width) / 256;
                    int barHeight = (int)(histogram[i] * scale);
                    if (barHeight > 0)
                    {
                        using SolidBrush brush = new(color);
                        g.FillRectangle(brush, x, pictureCurves.Height - barHeight, barWidth, barHeight);
                    }
                }
            }

            return histBmp;
        }

        private void UpdateCurvePoints()
        {
            int minInput = Math.Min(_darkScale, _lightScale);
            int maxInput = Math.Max(_darkScale, _lightScale);

            foreach (var kvp in _originalCurves)
            {
                List<Point> scaledPoints = [];
                foreach (Point p in kvp.Value)
                {
                    double normalizedX = p.X / 255.0;
                    int scaledX = (int)(minInput + normalizedX * (maxInput - minInput));
                    scaledX = Math.Clamp(scaledX, 0, 255);
                    scaledPoints.Add(new Point(scaledX, p.Y));
                }
                _curves[kvp.Key] = scaledPoints;
            }

            _currentPoints = _curves[_currentChannel];
        }

        private void UpdatePointInfo()
        {
            if (_selectedPointIndex >= 0 && _selectedPointIndex < _currentPoints.Count)
            {
                Point selectedPoint = _currentPoints[_selectedPointIndex];
                if (selectedPoint.X.ToString() != txtInput.Text)
                {
                    txtInput.Text = selectedPoint.X.ToString();
                }
                if (selectedPoint.Y.ToString() != txtOutput.Text)
                {
                    txtOutput.Text = selectedPoint.Y.ToString();
                }
            }
            else
            {
                txtInput.Text = "";
                txtOutput.Text = "";
            }
        }

        private void SelectPoint(int index)
        {
            if (index >= 0 && index < _currentPoints.Count)
            {
                _selectedPointIndex = index;
                UpdatePointInfo();
                DrawCurve();
            }
        }

        private void TxtInput_TextChanged(object sender, EventArgs e)
        {
            if (_selectedPointIndex < 0 || _selectedPointIndex >= _currentPoints.Count)
                return;

            if (int.TryParse(txtInput.Text, out int newX))
            {
                newX = Math.Clamp(newX, 0, 255);

                if (_selectedPointIndex == 0)
                    newX = 0;
                else if (_selectedPointIndex == _currentPoints.Count - 1)
                    newX = 255;
                else if (_selectedPointIndex > 0 && newX <= _currentPoints[_selectedPointIndex - 1].X)
                    newX = _currentPoints[_selectedPointIndex - 1].X + 1;
                else if (_selectedPointIndex < _currentPoints.Count - 1 && newX >= _currentPoints[_selectedPointIndex + 1].X)
                    newX = _currentPoints[_selectedPointIndex + 1].X - 1;

                Point currentPoint = _currentPoints[_selectedPointIndex];
                if (currentPoint.X != newX)
                {
                    var newPoint = new Point(newX, currentPoint.Y);
                    _currentPoints[_selectedPointIndex] = newPoint;
                    _currentPoints.Sort((a, b) => a.X.CompareTo(b.X));
                    _selectedPointIndex = _currentPoints.IndexOf(newPoint);
                    _originalCurves[_currentChannel] = [.. _currentPoints];
                    UpdateCurvePoints();
                    UpdatePointInfo();
                    DrawCurve();
                    UpdatePreview();
                }
            }
        }

        private void TxtOutput_TextChanged(object sender, EventArgs e)
        {
            if (_selectedPointIndex < 0 || _selectedPointIndex >= _currentPoints.Count)
                return;

            if (int.TryParse(txtOutput.Text, out int newY))
            {
                newY = Math.Clamp(newY, 0, 255);

                Point currentPoint = _currentPoints[_selectedPointIndex];
                if (currentPoint.Y != newY)
                {
                    var newPoint = new Point(currentPoint.X, newY);
                    _currentPoints[_selectedPointIndex] = newPoint;
                    _originalCurves[_currentChannel] = [.. _currentPoints];
                    UpdateCurvePoints();
                    UpdatePointInfo();
                    DrawCurve();
                    UpdatePreview();
                }
            }
        }

        private void TrackBarScaleDark_Scroll(object sender, EventArgs e)
        {
            _darkScale = trackBarScaleDark.Value;
            if (_darkScale > _lightScale)
            {
                _lightScale = _darkScale;
                trackBarScaleLight.Value = _darkScale;
            }
            UpdateCurvePoints();
            DrawCurve();
            UpdatePreview();
        }

        private void TrackBarScaleLight_Scroll(object sender, EventArgs e)
        {
            _lightScale = trackBarScaleLight.Value;
            if (_lightScale < _darkScale)
            {
                _darkScale = _lightScale;
                trackBarScaleDark.Value = _lightScale;
            }
            UpdateCurvePoints();
            DrawCurve();
            UpdatePreview();
        }

        private void BtnReset_Click(object sender, EventArgs e)
        {
            _originalCurves[_currentChannel] =
            [
                new Point(0, 0),
                new Point(255, 255)
            ];
            UpdateCurvePoints();
            _selectedPointIndex = -1;
            UpdatePointInfo();
            DrawCurve();
            UpdatePreview();
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (_selectedPointIndex >= 0 && _currentPoints.Count > 2)
            {
                _currentPoints.RemoveAt(_selectedPointIndex);
                _originalCurves[_currentChannel] = [.. _currentPoints];
                UpdateCurvePoints();
                _selectedPointIndex = -1;
                UpdatePointInfo();
                DrawCurve();
                UpdatePreview();
            }
        }

        private void CboRGB_SelectedIndexChanged(object sender, EventArgs e)
        {
            _currentChannel = cboRGB.Text;
            _currentPoints = _curves[_currentChannel];
            _selectedPointIndex = -1;
            UpdatePointInfo();
            DrawCurve();
            UpdatePreview();
        }

        private void PictureCurves_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                _isDragging = true;
                int clickedIndex = -1;

                for (int i = 0; i < _currentPoints.Count; i++)
                {
                    Point screenPoint = CurvePointToScreen(_currentPoints[i]);
                    if (Math.Abs(e.X - screenPoint.X) < 6 && Math.Abs(e.Y - screenPoint.Y) < 6)
                    {
                        clickedIndex = i;
                        break;
                    }
                }

                if (clickedIndex >= 0)
                {
                    SelectPoint(clickedIndex);
                }
                else
                {
                    Point newPoint = ScreenToCurvePoint(e.X, e.Y);
                    newPoint.X = Math.Clamp(newPoint.X, 0, 255);
                    newPoint.Y = Math.Clamp(newPoint.Y, 0, 255);

                    bool exists = _currentPoints.Any(p => Math.Abs(p.X - newPoint.X) < 3);
                    if (!exists)
                    {
                        _currentPoints.Add(newPoint);
                        _currentPoints.Sort((a, b) => a.X.CompareTo(b.X));
                        int newIndex = _currentPoints.IndexOf(newPoint);
                        _originalCurves[_currentChannel] = [.. _currentPoints];
                        UpdateCurvePoints();
                        SelectPoint(newIndex);
                        UpdatePreview();
                    }
                }

                DrawCurve();
            }
        }

        private void PictureCurves_MouseMove(object sender, MouseEventArgs e)
        {
            bool hoveringOverPoint = false;
            for (int i = 0; i < _currentPoints.Count; i++)
            {
                Point screenPoint = CurvePointToScreen(_currentPoints[i]);
                if (Math.Abs(e.X - screenPoint.X) < 6 && Math.Abs(e.Y - screenPoint.Y) < 6)
                {
                    hoveringOverPoint = true;
                    break;
                }
            }
            pictureCurves.Cursor = hoveringOverPoint ? Cursors.Hand : Cursors.Default;

            if (_isDragging && _selectedPointIndex >= 0)
            {
                Point newPoint = ScreenToCurvePoint(e.X, e.Y);
                newPoint.X = Math.Clamp(newPoint.X, 0, 255);
                newPoint.Y = Math.Clamp(newPoint.Y, 0, 255);

                if (_selectedPointIndex == 0)
                    newPoint.X = 0;
                if (_selectedPointIndex == _currentPoints.Count - 1)
                    newPoint.X = 255;

                if (_selectedPointIndex > 0 && newPoint.X <= _currentPoints[_selectedPointIndex - 1].X)
                    newPoint.X = _currentPoints[_selectedPointIndex - 1].X + 1;
                if (_selectedPointIndex < _currentPoints.Count - 1 && newPoint.X >= _currentPoints[_selectedPointIndex + 1].X)
                    newPoint.X = _currentPoints[_selectedPointIndex + 1].X - 1;

                _currentPoints[_selectedPointIndex] = newPoint;
                _originalCurves[_currentChannel] = [.. _currentPoints];

                UpdatePointInfo();

                DateTime now = DateTime.Now;
                if ((now - _lastDrawTime).TotalMilliseconds >= MinDrawIntervalMs)
                {
                    UpdateCurvePoints();
                    DrawCurve();
                    UpdatePreview();
                    _lastDrawTime = now;
                }
            }
        }

        private void PictureCurves_MouseUp(object sender, MouseEventArgs e)
        {
            _isDragging = false;
            UpdateCurvePoints();
            DrawCurve();
            UpdatePreview();
        }

        private void DrawCurve()
        {
            Bitmap bmp = new(pictureCurves.Width, pictureCurves.Height);
            using Graphics g = Graphics.FromImage(bmp);
            g.Clear(Color.FromArgb(50, 50, 50));

            if (_currentChannel == "RGB" && _histogramImage != null)
            {
                g.DrawImage(_histogramImage, 0, 0);
            }
            else if (_currentChannel == "R" && _histogramImage_R != null)
            {
                g.DrawImage(_histogramImage_R, 0, 0);
            }
            else if (_currentChannel == "G" && _histogramImage_G != null)
            {
                g.DrawImage(_histogramImage_G, 0, 0);
            }
            else if (_currentChannel == "B" && _histogramImage_B != null)
            {
                g.DrawImage(_histogramImage_B, 0, 0);
            }

            using Pen gridPen = new(Color.FromArgb(80, 80, 80), 1);
            for (int i = 0; i <= 255; i += 32)
            {
                int x = (i * pictureCurves.Width) / 256;
                int y = pictureCurves.Height - (i * pictureCurves.Height) / 256;
                g.DrawLine(gridPen, x, 0, x, pictureCurves.Height);
                g.DrawLine(gridPen, 0, y, pictureCurves.Width, y);
            }

            if (_currentPoints.Count >= 2)
            {
                Point[] screenPoints = [.. _currentPoints.Select(p => CurvePointToScreen(p))];

                if (_currentPoints.Count == 2)
                {
                    using Pen curvePen = new(Color.Yellow, 2);
                    g.DrawLine(curvePen, screenPoints[0], screenPoints[1]);
                }
                else
                {
                    List<PointF> smoothPoints = CreateSmoothCurve(screenPoints);
                    using Pen curvePen = new(Color.Yellow, 2);
                    g.DrawLines(curvePen, smoothPoints.ToArray());
                }

                foreach (Point p in screenPoints)
                {
                    g.FillEllipse(Brushes.White, p.X - 4, p.Y - 4, 8, 8);
                    g.DrawEllipse(Pens.Black, p.X - 4, p.Y - 4, 8, 8);
                }

                if (_selectedPointIndex >= 0 && _selectedPointIndex < screenPoints.Length)
                {
                    Point selected = screenPoints[_selectedPointIndex];
                    using Pen redPen = new(Color.Red, 2);
                    g.DrawEllipse(redPen, selected.X - 6, selected.Y - 6, 12, 12);

                    string label = $"({_currentPoints[_selectedPointIndex].X}, {_currentPoints[_selectedPointIndex].Y})";
                    using Font font = new("Arial", 8);
                    SizeF labelSize = g.MeasureString(label, font);
                    float labelX = selected.X - labelSize.Width / 2;
                    float labelY = selected.Y - 20 - labelSize.Height;

                    if (labelY < 0) labelY = selected.Y + 10;
                    if (labelX < 0) labelX = 0;
                    if (labelX + labelSize.Width > pictureCurves.Width)
                        labelX = pictureCurves.Width - labelSize.Width;

                    using SolidBrush bgBrush = new(Color.FromArgb(180, 0, 0, 0));
                    g.FillRectangle(bgBrush, labelX - 2, labelY - 2, labelSize.Width + 4, labelSize.Height + 4);

                    using SolidBrush textBrush = new(Color.White);
                    g.DrawString(label, font, textBrush, labelX, labelY);
                }

                using Pen refPen = new(Color.FromArgb(50, 255, 255, 255), 1);
                g.DrawLine(refPen, 0, pictureCurves.Height, pictureCurves.Width, 0);
            }

            pictureCurves.Image?.Dispose();
            pictureCurves.Image = bmp;
        }

        private static List<PointF> CreateSmoothCurve(Point[] points)
        {
            List<PointF> smoothPoints = [];

            for (int i = 0; i < points.Length - 1; i++)
            {
                PointF p0 = points[Math.Max(0, i - 1)];
                PointF p1 = points[i];
                PointF p2 = points[i + 1];
                PointF p3 = points[Math.Min(points.Length - 1, i + 2)];

                for (float t = 0; t < 1; t += 0.02f)
                {
                    float t2 = t * t;
                    float t3 = t2 * t;

                    float x = 0.5f * ((2 * p1.X) +
                        (-p0.X + p2.X) * t +
                        (2 * p0.X - 5 * p1.X + 4 * p2.X - p3.X) * t2 +
                        (-p0.X + 3 * p1.X - 3 * p2.X + p3.X) * t3);

                    float y = 0.5f * ((2 * p1.Y) +
                        (-p0.Y + p2.Y) * t +
                        (2 * p0.Y - 5 * p1.Y + 4 * p2.Y - p3.Y) * t2 +
                        (-p0.Y + 3 * p1.Y - 3 * p2.Y + p3.Y) * t3);

                    smoothPoints.Add(new PointF(x, y));
                }
            }

            smoothPoints.Add(points[^1]);
            return smoothPoints;
        }

        private Point CurvePointToScreen(Point curvePoint)
        {
            int x = (curvePoint.X * pictureCurves.Width) / 256;
            int y = pictureCurves.Height - (curvePoint.Y * pictureCurves.Height) / 256;
            return new Point(x, y);
        }

        private Point ScreenToCurvePoint(int screenX, int screenY)
        {
            int x = (screenX * 256) / pictureCurves.Width;
            int y = ((pictureCurves.Height - screenY) * 256) / pictureCurves.Height;
            return new Point(x, y);
        }

        private void UpdatePreview()
        {
            if (Image != null)
            {
                Bitmap newImage = ManipulatorLighting.ApplyCurvesToImage((Bitmap)Image, _currentPoints);

                if (pictureSample.Image != null)
                {
                    var oldImage = pictureSample.Image;
                    pictureSample.Image = null;
                    oldImage.Dispose();
                }

                pictureSample.Image = newImage;
            }
        }

        private void BtnOK_Click(object sender, EventArgs e)
        {
            if (Image != null)
            {
                Bitmap result = ManipulatorLighting.ApplyCurvesToImage((Bitmap)Image, _currentPoints);
                Image.Dispose();
                Image = result;
            }
            DialogResult = DialogResult.OK;
            Close();
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            pictureSample?.Dispose();
            pictureCurves?.Dispose();
            DialogResult = DialogResult.Cancel;
            Close();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
            _histogramImage?.Dispose();
            _histogramImage_R?.Dispose();
            _histogramImage_G?.Dispose();
            _histogramImage_B?.Dispose();
            pictureCurves.Image?.Dispose();
            pictureSample.Image?.Dispose();
        }
    }
}