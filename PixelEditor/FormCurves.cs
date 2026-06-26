using System.Drawing.Imaging;

namespace PixelEditor
{
    public partial class FormCurves : Form
    {
        public Image? Image = null;
        private readonly Dictionary<string, List<Point>> _curves = [];
        private List<Point> _currentPoints = [];
        private string _currentChannel = "RGB";
        private bool _isDragging = false;
        private int _selectedPointIndex = -1;
        private int _darkScale = 0;
        private int _lightScale = 255;
        private DateTime _lastDrawTime = DateTime.MinValue;
        private const int MinDrawIntervalMs = 32;

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
            DrawCurve();
            UpdatePreview();
        }

        private void BtnOK_Click(object sender, EventArgs e)
        {
            if (Image != null)
            {
                Bitmap result = ManipulatorLighting.ApplyCurvesToImage((Bitmap)Image, _curves);
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

        private void TrackBarScaleDark_Scroll(object sender, EventArgs e)
        {
            _darkScale = trackBarScaleDark.Value;
            DrawCurve();
            UpdatePreview();
        }

        private void TrackBarScaleLight_Scroll(object sender, EventArgs e)
        {
            _lightScale = trackBarScaleLight.Value;
            DrawCurve();
            UpdatePreview();
        }

        private void BtnReset_Click(object sender, EventArgs e)
        {
            _curves[_currentChannel] =
            [
                new Point(0, 0),
                new Point(255, 255)
            ];
            _currentPoints = _curves[_currentChannel];
            _selectedPointIndex = -1;
            DrawCurve();
            UpdatePreview();
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (_selectedPointIndex >= 0 && _currentPoints.Count > 2)
            {
                _currentPoints.RemoveAt(_selectedPointIndex);
                _selectedPointIndex = -1;
                DrawCurve();
                UpdatePreview();
            }
        }

        private void CboRGB_SelectedIndexChanged(object sender, EventArgs e)
        {
            _currentChannel = cboRGB.Text;
            _currentPoints = _curves[_currentChannel];
            _selectedPointIndex = -1;
            DrawCurve();
            UpdatePreview();
        }

        private void PictureCurves_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                _isDragging = true;
                _selectedPointIndex = -1;

                for (int i = 0; i < _currentPoints.Count; i++)
                {
                    Point screenPoint = CurvePointToScreen(_currentPoints[i]);
                    if (Math.Abs(e.X - screenPoint.X) < 6 && Math.Abs(e.Y - screenPoint.Y) < 6)
                    {
                        _selectedPointIndex = i;
                        break;
                    }
                }

                if (_selectedPointIndex == -1)
                {
                    Point newPoint = ScreenToCurvePoint(e.X, e.Y);
                    newPoint.X = Math.Clamp(newPoint.X, 0, 255);
                    newPoint.Y = Math.Clamp(newPoint.Y, 0, 255);

                    bool exists = _currentPoints.Any(p => Math.Abs(p.X - newPoint.X) < 3);
                    if (!exists)
                    {
                        _currentPoints.Add(newPoint);
                        _currentPoints.Sort((a, b) => a.X.CompareTo(b.X));
                        _selectedPointIndex = _currentPoints.IndexOf(newPoint);
                        DrawCurve();
                        UpdatePreview();
                    }
                }

                DrawCurve();
            }
        }

        private void PictureCurves_MouseMove(object sender, MouseEventArgs e)
        {
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

                DateTime now = DateTime.Now;
                if ((now - _lastDrawTime).TotalMilliseconds >= MinDrawIntervalMs)
                {
                    DrawCurve();
                    UpdatePreview();
                    _lastDrawTime = now;
                }
            }
        }

        private void PictureCurves_MouseUp(object sender, MouseEventArgs e)
        {
            _isDragging = false;
            DrawCurve();
            UpdatePreview();
        }

        private void DrawCurve()
        {
            Bitmap bmp = new(pictureCurves.Width, pictureCurves.Height);
            using Graphics g = Graphics.FromImage(bmp);
            g.Clear(Color.FromArgb(50, 50, 50));

            using Pen gridPen = new(Color.FromArgb(80, 80, 80), 1);
            for (int i = 0; i <= 255; i += 32)
            {
                int x = (i * pictureCurves.Width) / 256;
                int y = pictureCurves.Height - (i * pictureCurves.Height) / 256;
                g.DrawLine(gridPen, x, 0, x, pictureCurves.Height);
                g.DrawLine(gridPen, 0, y, pictureCurves.Width, y);
            }

            List<Point> scaledPoints = GetScaledCurvePoints();
            if (scaledPoints.Count >= 2)
            {
                Point[] screenPoints = [.. scaledPoints.Select(p => CurvePointToScreen(p))];

                if (scaledPoints.Count == 2)
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

        private List<Point> GetScaledCurvePoints()
        {
            List<Point> scaledPoints = [];

            foreach (Point p in _currentPoints)
            {
                double normalizedOutput = p.Y / 255.0;
                int scaledOutput = (int)(_darkScale + normalizedOutput * (_lightScale - _darkScale));
                scaledOutput = Math.Clamp(scaledOutput, 0, 255);
                scaledPoints.Add(new Point(p.X, scaledOutput));
            }

            return scaledPoints;
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
                pictureSample.Image?.Dispose();
                pictureSample.Image = ManipulatorLighting.ApplyCurvesToImage((Bitmap)Image, _curves);
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
            pictureCurves.Image?.Dispose();
            pictureSample.Image?.Dispose();
        }
    }
}