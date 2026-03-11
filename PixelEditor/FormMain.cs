using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;

namespace PixelEditor
{
    public struct IconInfo
    {
        public bool fIcon;
        public int xHotspot;
        public int yHotspot;
        public IntPtr hbmMask;
        public IntPtr hbmColor;
    }

    public partial class FormMain : Form
    {
        private Paint paint;
        private Point lastMousePosition;
        private PointF lazyLocalPos;
        private PointF strokeLastInterpolated;
        private PointF selectionCenter;
        private RectangleF selectionBounds;
        private bool isDragging = false;
        private bool isPainting = false;
        private bool isRotating = false;
        private bool isScaling = false;
        private bool isLassoSelecting = false;
        private bool isRectSelecting = false;
        private bool isDirty = false;
        private float _dashOffset = 0;
        private float startMouseAngle = 0;
        private float rotationAngle = 0;
        private float initialScaleDistance = 0f;
        private float initialScaleFactor = 0f;
        private float scaleFactor = 1.0f;
        private int selectedBrushIndex = 0;
        private string currentFilePath = "";
        private DateTime lastPaintTime = DateTime.MinValue;
        private readonly List<Image> brushes = [];
        private List<PointF> strokePoints = [];
        private readonly List<Point> selectionPoints = [];
        private Bitmap? selectedAreaBitmap = null;
        private readonly Matrix transformMatrix = new();

        private const float ROTATION_HANDLE_SIZE = 20;
        private const float SCALE_HANDLE_SIZE = 15;

        [DllImport("user32.dll")]
        public static extern IntPtr CreateIconIndirect(ref IconInfo icon);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetIconInfo(IntPtr hIcon, ref IconInfo pIconInfo);

        public FormMain()
        {
            InitializeComponent();

            System.Windows.Forms.Timer animationTimer = new()
            {
                Interval = 100 // Roughly 10 frames per second
            };
            animationTimer.Tick += (s, e) =>
            {
                if (selectionPoints.Count > 1)
                {
                    _dashOffset += 1.0f; // Decrement to move forward, increment to move backward

                    if (_dashOffset > 100.0f) _dashOffset = 0; // Reset offset to prevent it from growing into a massive number over time

                    RedrawImage();
                }
            };
            animationTimer.Start();

            layersControl.LayerVisibilityChanged += LayersControl_LayerVisibilityChanged;
            layersControl.SelectedLayerChanged += LayersControl_LayerOrderChanged;
        }

        private void FormMain_Load(object sender, EventArgs e)
        {
            Form1_Resize(sender, e);
            UpdateTitleBar();
            cboBlendMode.Items.Clear();
            cboBlendMode.Items.AddRange(Enum.GetNames<ImageBlending>());
            cboBlendMode.SelectedIndex = 0;
            cboFillBlendMode.Items.Clear();
            cboFillBlendMode.Items.AddRange(Enum.GetNames<ImageBlending>());
            cboFillBlendMode.SelectedIndex = 0;
            cboFIllGradient.SelectedIndex = 0;
            brush_size.Value = 12;
            brush_opacity.Value = 100;
            brush_smoothness.Value = 22;
            brush_hardness.Value = 80;
            Brush_size_Scroll(sender, e);
            Brush_opacity_Scroll(sender, e);
            Brush_smoothness_Scroll(sender, e);
            Brush_hardness_Scroll(sender, e);
            ReloadBrushes();
        }

        private void LayersControl_LayerVisibilityChanged(object? sender, LayerVisibilityChangedEventArgs e)
        {
            Console.WriteLine($"Layer {e.Layer.Name}'s visibility changed from {e.OldValue} to {e.NewValue}");

            HistoryManager.RecordState(new HistoryItem(LayersManipulator.Zoom, LayersManipulator.ImageOffset, layersControl.GetLayers(), layersControl.GetSelectedLayerIndex()));
            RedrawImage();
        }

        private void LayersControl_LayerOrderChanged(object? sender, SelectedLayerChangedEventArgs e)
        {
            Console.WriteLine($"Layer change from {e.OldIndex} to {e.NewIndex}. '{e.NewLayer?.Name}' was picked");

            HistoryManager.RecordState(new HistoryItem(LayersManipulator.Zoom, LayersManipulator.ImageOffset, layersControl.GetLayers(), layersControl.GetSelectedLayerIndex()));
            UpdateControls();

            RedrawImage();
        }

        private void ReloadBrushes()
        {
            try
            {
                string[] files = Directory.GetFiles(Application.StartupPath + @"\Brushes\");
                int x = 0;
                int y = 0;
                int index = 0;
                foreach (string file in files)
                {
                    PictureBox pic = new()
                    {
                        Image = new Bitmap(file),
                        Size = new Size(24, 24),
                        Location = new Point(x, y),
                        SizeMode = PictureBoxSizeMode.StretchImage,
                        BorderStyle = BorderStyle.None,
                        Tag = index++
                    };
                    pic.Click += Pic_Click;
                    panel2.Controls.Add(pic);
                    brushes.Add(new Bitmap(pic.Image));
                    x += 24;
                    if (x > 128)
                    {
                        x = 0; y += 24;
                    }
                }
            }
            catch
            {
                MessageBox.Show("Couldn't load Brushes");
            }
        }

        private void Pic_Click(object? sender, EventArgs e)
        {
            try
            {
                if (sender is not null && sender is PictureBox brush)
                {
                    if (brush.Image is not null)
                    {
                        selectedBrushIndex = brush.Tag != null ? (int)brush.Tag : -1;
                        paint.Brush = new Bitmap(brush.Image);
                        if (paint.Brush != null)
                        {
                            paint.Reset(btnPenColor.BackColor, paint.GetRadius() * (brush_hardness.Maximum - brush_hardness.Value) / brush_hardness.Maximum);
                            PaintingEngine.SetBrush(paint);
                        }
                        UpdateCursor();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private void BtnPenColor_Click(object sender, EventArgs e)
        {
            ColorDialog c = new()
            {
                Color = btnPenColor.BackColor
            };
            if (c.ShowDialog() == DialogResult.OK)
            {
                btnPenColor.BackColor = c.Color;
                paint.Reset(btnPenColor.BackColor, paint.GetRadius() * (brush_hardness.Maximum - brush_hardness.Value) / brush_hardness.Maximum);
                if (paint.Brush != null)
                {
                    PaintingEngine.SetBrush(paint);
                    UpdateCursor();
                }
            }
        }

        private void BtnFillColor_Click(object sender, EventArgs e)
        {
            ColorDialog c = new()
            {
                Color = btnFillColor.BackColor
            };
            if (c.ShowDialog() == DialogResult.OK)
            {
                btnFillColor.BackColor = c.Color;
                paint.SetFillColor(btnFillColor.BackColor);
                PaintingEngine.SetBrush(paint);
                UpdateCursor(btnFiller.Image);
            }
        }

        private void BtnFillColor1_Click(object sender, EventArgs e)
        {
            ColorDialog c = new()
            {
                Color = btnFillColor.BackColor
            };
            if (c.ShowDialog() == DialogResult.OK)
            {
                //btnFillColor.BackColor = c.Color;
                //paint.SetFillColor(btnFillColor.BackColor);
                //PaintingEngine.SetBrush(paint);
                //UpdateCursor(btnFiller.Image);
            }
        }

        private void BtnTools_Click(object sender, EventArgs e)
        {
            if (sender is not null && sender is RadioButton btn)
            {
                if (btn != btnPointer)
                {
                    btnPointer.Checked = false;
                }
                if (btn != btnFiller)
                {
                    btnFiller.Checked = false;
                    groupFillDetail.Visible = false;
                }
                if (btn != btnBrusher)
                {
                    btnBrusher.Checked = false;
                    groupBrushDetail.Visible = false;
                }
                if (btn != btnLassoSelect)
                {
                    btnLassoSelect.Checked = false;
                }
                if (btn != btnRectangleSelect)
                {
                    btnRectangleSelect.Checked = false;
                }
                if (btn != btnMagicWand)
                {
                    btnMagicWand.Checked = false;
                    //groupMagicWand.Visible = false;
                }
            }
        }

        private void BtnTools_CheckedChanged(object sender, EventArgs e)
        {
            if (btnPointer.Checked)
            {
                PaintingEngine.SetBrush(paint);
                canvas.Cursor = Cursors.SizeAll;
            }
            else if (btnFiller.Checked)
            {
                paint.SetFillColor(btnFillColor.BackColor);
                PaintingEngine.SetBrush(paint);
                groupFillDetail.Visible = true;
                groupFillDetail.TabIndex = 0;
                UpdateCursor(btnFiller.Image);
            }
            else if (btnBrusher.Checked)
            {
                paint.Brush = brushes.Count > selectedBrushIndex && selectedBrushIndex >= 0 ? new Bitmap(brushes[selectedBrushIndex]) : null;
                paint.Reset(btnPenColor.BackColor, paint.GetRadius() * (brush_hardness.Maximum - brush_hardness.Value) / brush_hardness.Maximum);
                PaintingEngine.SetBrush(paint);
                groupBrushDetail.Visible = true;
                UpdateCursor();
            }
            else if (btnLassoSelect.Checked)
            {
                PaintingEngine.SetBrush(paint);
                canvas.Cursor = Cursors.Cross;
            }
            else if (btnRectangleSelect.Checked)
            {
                PaintingEngine.SetBrush(paint);
                canvas.Cursor = Cursors.Cross;
            }
            else if (btnMagicWand.Checked)
            {
                PaintingEngine.SetBrush(paint);
                UpdateCursor(btnMagicWand.Image);
            }
            else
            {
                canvas.Cursor = Cursors.Default;
            }
        }

        private void UpdateCursor(Image? image)
        {
            if (image != null)
            {
                canvas.Cursor.Dispose();
                Bitmap bitmap = new(image, 24, 24);
                bitmap.MakeTransparent(Color.White);
                for (int i = 128; i < 256; i++)
                {
                    bitmap.MakeTransparent(Color.FromArgb(i, i, i)); //Make all gray-scale pixels that are nearly white transparent
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
                        else if (sourcePixel.A > 0)
                        {
                            bitmap.SetPixel(x, y, paint.GetFillColor());
                        }
                    }
                }
                canvas.Cursor = CreateCursor(new Bitmap(bitmap, 24, 24), 0, 0);
            }
        }

        private void UpdateCursor()
        {
            if (paint.Brush != null)
            {
                canvas.Cursor.Dispose();

                int cursorWidth = 2 * paint.Brush.Width * brush_size.Value / brush_size.Maximum;
                int cursorHeight = 2 * paint.Brush.Height * brush_size.Value / brush_size.Maximum;

                int hotSpotX = cursorWidth / 2;
                int hotSpotY = cursorHeight / 2;

                canvas.Cursor = CreateCursor(new Bitmap(paint.Brush, cursorWidth, cursorHeight), hotSpotX, hotSpotY);
            }
            else
            {
                canvas.Cursor = Cursors.Default;
            }
        }

        private void UpdateCursor(Point mousePosition)
        {
            if (IsOverRotationHandle(mousePosition))
            {
                Cursor.Current = Cursors.Hand;
            }
            else if (IsOverScaleHandle(mousePosition, out string handle))
            {
                Cursor.Current = GetScaleCursor(handle);
            }
            else if (IsPointInSelection(mousePosition))
            {
                //Cursor.Current = Cursors.SizeAll;
            }
            else
            {
                //Cursor.Current = Cursors.Default;
            }
        }

        private static Cursor GetScaleCursor(string handle)
        {
            switch (handle)
            {
                case "topLeft":
                case "bottomRight":
                    return Cursors.SizeNWSE;
                case "topRight":
                case "bottomLeft":
                    return Cursors.SizeNESW;
                default:
                    return Cursors.SizeAll;
            }
        }

        public static Cursor CreateCursor(Bitmap bmp, int xHotSpot, int yHotSpot)
        {
            IntPtr ptr = bmp.GetHicon();
            var tmp = new IconInfo();
            GetIconInfo(ptr, ref tmp);
            tmp.xHotspot = xHotSpot;
            tmp.yHotspot = yHotSpot;
            tmp.fIcon = false;
            ptr = CreateIconIndirect(ref tmp);
            return new Cursor(ptr);
        }

        private void Brush_size_Scroll(object sender, EventArgs e)
        {
            lblBrushSize.Text = $"{brush_size.Value}";
            UpdateCursor();
        }

        private void Brush_opacity_Scroll(object sender, EventArgs e)
        {
            lblBrushOpacity.Text = $"{brush_opacity.Value}";
        }

        private void Brush_smoothness_Scroll(object sender, EventArgs e)
        {
            lblBrushSmoothness.Text = $"{brush_smoothness.Value}";
        }

        private void Brush_hardness_Scroll(object sender, EventArgs e)
        {
            lblBrushHardness.Text = $"{brush_hardness.Value}";
            if (paint.Brush != null)
            {
                paint.Reset(btnPenColor.BackColor, paint.GetRadius() * (brush_hardness.Maximum - brush_hardness.Value) / brush_hardness.Maximum);
                PaintingEngine.SetBrush(paint);
                UpdateCursor();
            }
        }

        private void FormMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!ConfirmAbandonChanges())
            {
                e.Cancel = true;
            }
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            if (WindowState != FormWindowState.Minimized)
            {
                canvas.Size = new Size(ClientSize.Width - canvas.Location.X - 220,
                    ClientSize.Height - canvas.Location.Y - 40);
                RedrawImage();
            }
        }

        private void BtnBrowseImage_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog1 = new()
            {
                Filter = "Image Files|*.bmp;*.jpg;*.jpeg;*.png;*.gif;*.tiff|All Files|*.*"
            };
            if ((openFileDialog1).ShowDialog() == DialogResult.OK)
            {
                Image image = Image.FromFile(openFileDialog1.FileName);
                if (image is not null)
                {
                    DialogResult result = DialogResult.Yes;
                    if (result != DialogResult.Cancel)
                    {
                        LayersManipulator.Zoom = 0.95f;
                        LayersManipulator.ImageOffset = new PointF(0, 0);

                        redToolStripMenuItem1.Checked = false;
                        greenToolStripMenuItem1.Checked = false;
                        blueToolStripMenuItem1.Checked = false;

                        //AddLayer(image);
                        var layer = new Layer($"layer {layersControl.GetLayers().Count + 1}", true)
                        {
                            Image = image
                        };
                        layersControl.InsertLayer(0, layer);

                        RedrawImage();
                    }
                }
            }
        }

        private void PNGPictureToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog1 = new()
            {
                Filter = "PNG Image|*.png"
            };
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    LayersManipulator.PopulateColorGrid(layersControl.GetLayers(), -1, includeBackground: false);

                    var rect = new Rectangle(0, 0, LayersManipulator.Width, LayersManipulator.Height);

                    Bitmap bitmap = LayersManipulator.GetImage(LayersManipulator.Screen, rect);
                    bitmap.Save(saveFileDialog1.FileName, System.Drawing.Imaging.ImageFormat.Png);
                    labelStatus.Text = "Image saved successfully!";
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error saving image: {ex.Message}");
                }
            }
        }

        private void JPEGPictureToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog1 = new()
            {
                Filter = "JPEG Image|*.jpg;*.jpeg"
            };
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    LayersManipulator.PopulateColorGrid(layersControl.GetLayers(), -1, includeBackground: false);

                    var rect = new Rectangle(0, 0, LayersManipulator.Width, LayersManipulator.Height);

                    Bitmap bitmap = LayersManipulator.GetImage(LayersManipulator.Screen, rect);
                    bitmap.Save(saveFileDialog1.FileName, System.Drawing.Imaging.ImageFormat.Jpeg);
                    labelStatus.Text = "Image saved successfully!";
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error saving image: {ex.Message}");
                }
            }
        }

        private void BMPPictureToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog1 = new()
            {
                Filter = "BMP Image|*.bmp"
            };
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    LayersManipulator.PopulateColorGrid(layersControl.GetLayers(), -1, includeBackground: false);

                    var rect = new Rectangle(0, 0, LayersManipulator.Width, LayersManipulator.Height);

                    Bitmap bitmap = LayersManipulator.GetImage(LayersManipulator.Screen, rect);
                    bitmap.Save(saveFileDialog1.FileName, System.Drawing.Imaging.ImageFormat.Bmp);
                    labelStatus.Text = "Image saved successfully!";
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error saving image: {ex.Message}");
                }
            }
        }

        private void GIFPictureToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog1 = new()
            {
                Filter = "GIF Image|*.gif"
            };
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    LayersManipulator.PopulateColorGrid(layersControl.GetLayers(), -1, includeBackground: false);

                    var rect = new Rectangle(0, 0, LayersManipulator.Width, LayersManipulator.Height);

                    Bitmap bitmap = LayersManipulator.GetImage(LayersManipulator.Screen, rect);
                    bitmap.Save(saveFileDialog1.FileName, System.Drawing.Imaging.ImageFormat.Gif);
                    labelStatus.Text = "Image saved successfully!";
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error saving image: {ex.Message}");
                }
            }
        }

        private void NewToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!ConfirmAbandonChanges()) return;

            LayersManipulator.Zoom = 0.95f;
            LayersManipulator.ImageOffset = new PointF(0, 0);

            HistoryManager.Clear();
            layersControl.ClearLayers();
            RedrawImage();

            currentFilePath = "";
            isDirty = false;
            UpdateTitleBar();
            this.Invalidate();
        }

        private void OpenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!ConfirmAbandonChanges()) return;

            using OpenFileDialog ofd = new();
            ofd.Filter = "Paint++ Files (*.ptv)|*.ptv";

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    using FileStream fs = File.Open(ofd.FileName, FileMode.Open);

                    float zoom;
                    PointF imageOffset;
                    StrokePTVLoader.Load(fs, out zoom, out imageOffset, out var layers, out int selectedLayerIndex);
                    LayersManipulator.Zoom = zoom;
                    LayersManipulator.ImageOffset = imageOffset;

                    layersControl.ClearLayers();
                    layersControl.AddRange(layers);
                    layersControl.SetSelectedLayerIndex(selectedLayerIndex);

                    currentFilePath = ofd.FileName;
                    isDirty = false;
                    UpdateTitleBar();

                    HistoryManager.Clear();

                    UpdateControls();

                    RedrawImage();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Load Error: {ex.Message}");
                }
            }
        }

        private void SaveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(currentFilePath))
            {
                using SaveFileDialog sfd = new();
                sfd.Filter = "Paint++ Files (*.ptv)|*.ptv";

                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    PerformSave(sfd.FileName);
                }
            }
            else
            {
                PerformSave(currentFilePath);
            }
        }

        private void SaveAsProjectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using SaveFileDialog sfd = new();
            sfd.Filter = "Paint++ Files (*.ptv)|*.ptv";

            if (sfd.ShowDialog() == DialogResult.OK)
            {
                PerformSave(sfd.FileName);
            }
        }

        private void CloseToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void PerformSave(string fileName)
        {
            try
            {
                using FileStream fs = File.Open(fileName, FileMode.Create);
                StrokePTVSaver.Save(fs, LayersManipulator.Zoom, LayersManipulator.ImageOffset, layersControl.GetLayers(), layersControl.GetSelectedLayerIndex());
                currentFilePath = fileName;
                isDirty = false;
                UpdateTitleBar();

                labelStatus.Text += " Saved Successfully!";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Save Error: {ex.Message}");
            }
        }

        private void UpdateTitleBar()
        {
            string displayName = string.IsNullOrEmpty(currentFilePath)
                ? "Untitled"
                : System.IO.Path.GetFileName(currentFilePath);

            string dirtyMarker = isDirty && HistoryManager.CanUndo ? " *" : "";

            if (displayName.EndsWith(".ptv", StringComparison.OrdinalIgnoreCase) && displayName.ToLower().LastIndexOf(".ptv") > 0)
            {
                displayName = displayName[..displayName.LastIndexOf(".ptv")];
            }

            Text = $"{displayName}{dirtyMarker}";
        }

        private bool ConfirmAbandonChanges()
        {
            if (!isDirty) return true;

            var result = MessageBox.Show(
                "You have unsaved changes. Would you like to save them now?",
                "Unsaved Changes",
                MessageBoxButtons.YesNoCancel,
                MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                if (string.IsNullOrEmpty(currentFilePath))
                {
                    using SaveFileDialog sfd = new();
                    sfd.Filter = "Paint++ Files (*.ptv)|*.ptv";
                    if (sfd.ShowDialog() == DialogResult.OK)
                    {
                        PerformSave(sfd.FileName);
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    PerformSave(currentFilePath);
                }
            }

            if (result == DialogResult.No)
            {
                return true;
            }

            return false;
        }

        private void Opacity_Leave(object sender, EventArgs e)
        {
            Layer? layer = layersControl.GetLayer(layersControl.GetSelectedLayerIndex());
            if (layer != null)
            {
                layer.Opacity = (int)opacity.Value;
                layersControl.UpdateLayer(layersControl.GetSelectedLayerIndex(), layer);
                RedrawImage();
            }
        }

        private void CboBlendMode_SelectedIndexChanged(object sender, EventArgs e)
        {
            Layer? layer = layersControl.GetLayer(layersControl.GetSelectedLayerIndex());
            if (layer != null)
            {
                if (layer.BlendMode != Enum.Parse<ImageBlending>(cboBlendMode.Text))
                {
                    layer.BlendMode = Enum.Parse<ImageBlending>(cboBlendMode.Text);
                    layersControl.UpdateLayer(layersControl.GetSelectedLayerIndex(), layer);
                    RedrawImage();
                }
            }
        }

        private void BtnAddVector_Click(object sender, EventArgs e)
        {
            HistoryManager.RecordState(new HistoryItem(LayersManipulator.Zoom, LayersManipulator.ImageOffset, layersControl.GetLayers(), layersControl.GetSelectedLayerIndex()));

            Bitmap bmp = new(LayersManipulator.Width, LayersManipulator.Height);

            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.Clear(Color.White);
            }

            var layer = new Layer($"layer {layersControl.GetLayers().Count + 1}", true)
            {
                Image = bmp
            };
            layersControl.InsertLayer(0, layer);
            RedrawImage();
        }

        private void BtnSubtractVector_Click(object sender, EventArgs e)
        {
            HistoryManager.RecordState(new HistoryItem(LayersManipulator.Zoom, LayersManipulator.ImageOffset, layersControl.GetLayers(), layersControl.GetSelectedLayerIndex()));

            layersControl.RemoveLayerAt(layersControl.GetSelectedLayerIndex());
            RedrawImage();
        }

        private void BtnMoveUp_Click(object sender, EventArgs e)
        {
            if (layersControl.GetSelectedLayerIndex() <= 0) return;

            int currentIndex = layersControl.GetSelectedLayerIndex();
            int newIndex = currentIndex - 1;

            var layerToMove = layersControl.GetLayer(currentIndex);

            if (layerToMove != null)
            {
                HistoryManager.RecordState(new HistoryItem(LayersManipulator.Zoom, LayersManipulator.ImageOffset, layersControl.GetLayers(), layersControl.GetSelectedLayerIndex()));

                layersControl.RemoveLayerAt(currentIndex);
                layersControl.InsertLayer(newIndex, layerToMove);

                layersControl.SetSelectedLayerIndex(newIndex);

                RedrawImage();
            }
        }

        private void BtnMoveDown_Click(object sender, EventArgs e)
        {
            if (layersControl.GetSelectedLayerIndex() < 0 ||
                layersControl.GetSelectedLayerIndex() >= layersControl.GetLayers().Count - 1) return;

            int currentIndex = layersControl.GetSelectedLayerIndex();
            int newIndex = currentIndex + 1;

            var layerToMove = layersControl.GetLayer(currentIndex);

            if (layerToMove != null)
            {
                HistoryManager.RecordState(new HistoryItem(LayersManipulator.Zoom, LayersManipulator.ImageOffset, layersControl.GetLayers(), layersControl.GetSelectedLayerIndex()));

                layersControl.RemoveLayerAt(currentIndex);
                layersControl.InsertLayer(newIndex, layerToMove);

                layersControl.SetSelectedLayerIndex(newIndex);

                RedrawImage();
            }
        }

        private void MoveToTopToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (layersControl.GetSelectedLayerIndex() < 0 ||
                layersControl.GetSelectedLayerIndex() >= layersControl.GetLayers().Count - 1) return;

            int currentIndex = layersControl.GetSelectedLayerIndex();
            int newIndex = 0;

            var layerToMove = layersControl.GetLayer(currentIndex);

            if (layerToMove != null)
            {
                HistoryManager.RecordState(new HistoryItem(LayersManipulator.Zoom, LayersManipulator.ImageOffset, layersControl.GetLayers(), layersControl.GetSelectedLayerIndex()));

                layersControl.RemoveLayerAt(currentIndex);
                layersControl.InsertLayer(newIndex, layerToMove);

                layersControl.SetSelectedLayerIndex(newIndex);

                RedrawImage();
            }
        }

        private void MoveToBottomToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (layersControl.GetSelectedLayerIndex() < 0 ||
                layersControl.GetSelectedLayerIndex() >= layersControl.GetLayers().Count - 1) return;

            int currentIndex = layersControl.GetSelectedLayerIndex();
            int newIndex = layersControl.GetLayers().Count - 2;

            var layerToMove = layersControl.GetLayer(currentIndex);

            if (layerToMove != null)
            {
                HistoryManager.RecordState(new HistoryItem(LayersManipulator.Zoom, LayersManipulator.ImageOffset, layersControl.GetLayers(), layersControl.GetSelectedLayerIndex()));

                layersControl.RemoveLayerAt(currentIndex);
                layersControl.InsertLayer(newIndex, layerToMove);

                layersControl.SetSelectedLayerIndex(newIndex);

                RedrawImage();
            }
        }

        private void ChkListLayers_SelectedIndexChanged(object sender, EventArgs e)
        {
            btnMoveUp.Enabled = layersControl.GetSelectedLayerIndex() > 0;
            btnMoveDown.Enabled = layersControl.GetSelectedLayerIndex() < layersControl.GetLayers().Count - 1
                               && layersControl.GetSelectedLayerIndex() >= 0;

            UpdateControls();
        }

        private void BtnShowVector_Click(object sender, EventArgs e)
        {
            var layer = layersControl.GetLayer(layersControl.GetSelectedLayerIndex());

            if (layer != null)
            {
                HistoryManager.RecordState(new HistoryItem(LayersManipulator.Zoom, LayersManipulator.ImageOffset, layersControl.GetLayers(), layersControl.GetSelectedLayerIndex()));
                layer.IsVisible = true;
                layersControl.UpdateLayer(layersControl.GetSelectedLayerIndex(), layer);
                RedrawImage();
            }
        }

        private void BtnHideVector_Click(object sender, EventArgs e)
        {
            var layer = layersControl.GetLayer(layersControl.GetSelectedLayerIndex());

            if (layer != null)
            {
                HistoryManager.RecordState(new HistoryItem(LayersManipulator.Zoom, LayersManipulator.ImageOffset, layersControl.GetLayers(), layersControl.GetSelectedLayerIndex()));
                layer.IsVisible = false;
                layersControl.UpdateLayer(layersControl.GetSelectedLayerIndex(), layer);
                RedrawImage();
            }
        }

        private void BtnMergeDown_Click(object sender, EventArgs e)
        {
            //if (imageLayers.Count < 2)
            //    return;

            //int selectedIndex = layersControl.GetSelectedLayerIndex();

            //if (selectedIndex < 0 || selectedIndex >= imageLayers.Count - 1)
            //    return;

            //ListBoxVectors_SelectedIndexChanged(sender, e);
        }

        private void UpdateControls()
        {
            var layer = layersControl.GetLayer(layersControl.GetSelectedLayerIndex());
            if (layer != null)
            {
                opacity.Value = layer.Opacity;
                cboBlendMode.SelectedItem = layer.BlendMode.ToString();
                redToolStripMenuItem.Checked = layer.Channel == LayerChannel.Red;
                greenToolStripMenuItem.Checked = layer.Channel == LayerChannel.Green;
                blueToolStripMenuItem.Checked = layer.Channel == LayerChannel.Blue;
                allToolStripMenuItem.Checked = layer.Channel == LayerChannel.RGB;
                redToolStripMenuItem1.Checked = layer.RedFilter;
                greenToolStripMenuItem1.Checked = layer.GreenFilter;
                blueToolStripMenuItem1.Checked = layer.BlueFilter;
            }
        }

        private void ListBoxVectors_SelectedIndexChanged(object sender, EventArgs e)
        {
            btnMoveUp.Enabled = layersControl.GetSelectedLayerIndex() > 0;
            btnMoveDown.Enabled = layersControl.GetSelectedLayerIndex() < layersControl.GetLayers().Count - 1
                               && layersControl.GetSelectedLayerIndex() >= 0;

            UpdateControls();
        }

        private void ZoomInToolStripMenuItem_Click(object sender, EventArgs e)
        {
            HistoryManager.RecordState(new HistoryItem(LayersManipulator.Zoom, LayersManipulator.ImageOffset, layersControl.GetLayers(), layersControl.GetSelectedLayerIndex()));
            float zoomDelta = 0.1f;
            LayersManipulator.Zoom = Math.Max(0.1f, Math.Min(10.0f, LayersManipulator.Zoom + zoomDelta));
            RedrawImage();
        }

        private void ZoomOutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            HistoryManager.RecordState(new HistoryItem(LayersManipulator.Zoom, LayersManipulator.ImageOffset, layersControl.GetLayers(), layersControl.GetSelectedLayerIndex()));
            float zoomDelta = 0.1f;
            LayersManipulator.Zoom = Math.Max(0.1f, Math.Min(10.0f, LayersManipulator.Zoom - zoomDelta));
            RedrawImage();
        }

        private void BtnResetZoom_Click(object sender, EventArgs e)
        {
            HistoryManager.RecordState(new HistoryItem(LayersManipulator.Zoom, LayersManipulator.ImageOffset, layersControl.GetLayers(), layersControl.GetSelectedLayerIndex()));
            LayersManipulator.Zoom = 0.95f;
            LayersManipulator.ImageOffset = new(0, 0);
            RedrawImage();
        }

        private void AllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //if (layersControl.GetLayers().Count == imageLayers.Count &&
            //    layersControl.GetLayers().Count > layersControl.GetSelectedLayerIndex() &&
            //    layersControl.GetSelectedLayerIndex() >= 0)
            //{
            //    redToolStripMenuItem.Checked = false;
            //    greenToolStripMenuItem.Checked = false;
            //    blueToolStripMenuItem.Checked = false;
            //    allToolStripMenuItem.Checked = !allToolStripMenuItem.Checked;
            //    imageLayers[layersControl.GetSelectedLayerIndex()].Channel = LayerChannel.RGB;
            //    RedrawImage();
            //}
        }

        private void RedToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //if (layersControl.GetLayers().Count == imageLayers.Count &&
            //    layersControl.GetLayers().Count > layersControl.GetSelectedLayerIndex() &&
            //    layersControl.GetSelectedLayerIndex() >= 0)
            //{
            //    redToolStripMenuItem.Checked = !redToolStripMenuItem.Checked;
            //    greenToolStripMenuItem.Checked = false;
            //    blueToolStripMenuItem.Checked = false;
            //    allToolStripMenuItem.Checked = !redToolStripMenuItem.Checked;
            //    imageLayers[layersControl.GetSelectedLayerIndex()].Channel = LayerChannel.Red;
            //    RedrawImage();
            //}
        }

        private void GreenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //if (layersControl.GetLayers().Count == imageLayers.Count &&
            //    layersControl.GetLayers().Count > layersControl.GetSelectedLayerIndex() &&
            //    layersControl.GetSelectedLayerIndex() >= 0)
            //{
            //    redToolStripMenuItem.Checked = false;
            //    greenToolStripMenuItem.Checked = !greenToolStripMenuItem.Checked;
            //    blueToolStripMenuItem.Checked = false;
            //    allToolStripMenuItem.Checked = !greenToolStripMenuItem.Checked;
            //    imageLayers[layersControl.GetSelectedLayerIndex()].Channel = LayerChannel.Green;
            //    RedrawImage();
            //}
        }

        private void BlueToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //if (layersControl.GetLayers().Count == imageLayers.Count &&
            //    layersControl.GetLayers().Count > layersControl.GetSelectedLayerIndex() &&
            //    layersControl.GetSelectedLayerIndex() >= 0)
            //{
            //    redToolStripMenuItem.Checked = false;
            //    greenToolStripMenuItem.Checked = false;
            //    blueToolStripMenuItem.Checked = !blueToolStripMenuItem.Checked;
            //    allToolStripMenuItem.Checked = !blueToolStripMenuItem.Checked;
            //    imageLayers[layersControl.GetSelectedLayerIndex()].Channel = LayerChannel.Blue;
            //    RedrawImage();
            //}
        }

        private void ChkFilter_CheckedChanged(object sender, EventArgs e)
        {
            //if (layersControl.GetLayers().Count == imageLayers.Count && layersControl.GetLayers().Count > layersControl.GetSelectedLayerIndex() && layersControl.GetSelectedLayerIndex() >= 0)
            //{
            //    if (imageLayers[layersControl.GetSelectedLayerIndex()].Image is not null)
            //    {
            //        if (sender.GetType() == typeof(ToolStripMenuItem))
            //        {
            //            ToolStripMenuItem item = (ToolStripMenuItem)sender;
            //            item.Checked = !item.Checked;
            //        }

            //        imageLayers[layersControl.GetSelectedLayerIndex()].RedFilter = redToolStripMenuItem1.Checked;
            //        imageLayers[layersControl.GetSelectedLayerIndex()].GreenFilter = greenToolStripMenuItem1.Checked;
            //        imageLayers[layersControl.GetSelectedLayerIndex()].BlueFilter = blueToolStripMenuItem1.Checked;
            //        RedrawImage();
            //    }
            //}
        }

        private void DarkToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //if (layersControl.GetLayers().Count == imageLayers.Count && layersControl.GetLayers().Count > layersControl.GetSelectedLayerIndex() && layersControl.GetSelectedLayerIndex() >= 0)
            //{
            //    if (imageLayers[layersControl.GetSelectedLayerIndex()].Image is not null)
            //    {
            //        bool[,] mask = LayerManipulator.GetDarkPixels(LayerManipulator.Screen, 0.0f, 1.0f);
            //        ColorGrid grid = LayerManipulator.DarkPixelGrid(mask, LayerManipulator.Screen.Width, LayerManipulator.Screen.Height);
            //        imageLayers[layersControl.GetSelectedLayerIndex()].Image = LayerManipulator.GetImage(grid, new Rectangle(0, 0, grid.Width, grid.Height));
            //        RedrawImage();
            //    }
            //}
        }

        private void DeleteImageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var selectedLayer = layersControl.GetLayer(layersControl.GetSelectedLayerIndex());
            if (selectedLayer != null)
            {
                if (selectionPoints.Count > 0)
                {
                    HistoryManager.RecordState(new HistoryItem(LayersManipulator.Zoom, LayersManipulator.ImageOffset, layersControl.GetLayers(), layersControl.GetSelectedLayerIndex()));
                    CalculateSelectionBounds();
                    selectedLayer.Image = CutSelectionFromLayer(selectedLayer, true);
                    rotationAngle = 0;
                    scaleFactor = 1.0f;
                    selectionPoints.Clear();
                    RedrawImage();
                }
                else
                {
                    HistoryManager.RecordState(new HistoryItem(LayersManipulator.Zoom, LayersManipulator.ImageOffset, layersControl.GetLayers(), layersControl.GetSelectedLayerIndex()));
                    selectedLayer.Image?.Dispose();
                    selectedLayer.Image = null;
                    rotationAngle = 0;
                    scaleFactor = 1.0f;
                    RedrawImage();
                }
            }
        }

        private void CutImageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var selectedLayer = layersControl.GetLayer(layersControl.GetSelectedLayerIndex());
            if (selectedLayer != null)
            {
                if (selectionPoints.Count > 0)
                {
                    HistoryManager.RecordState(new HistoryItem(LayersManipulator.Zoom, LayersManipulator.ImageOffset, layersControl.GetLayers(), layersControl.GetSelectedLayerIndex()));
                    CalculateSelectionBounds();
                    Image? tempImage = ExtractSelectedArea(selectedLayer);
                    selectedLayer.Image = CutSelectionFromLayer(selectedLayer, true);
                    if (tempImage != null)
                    {
                        Clipboard.SetImage(tempImage);
                        selectionPoints.Clear();
                        RedrawImage();
                    }
                }
                else
                {
                    Image? tempImage = selectedLayer.Image;
                    if (tempImage != null)
                    {
                        HistoryManager.RecordState(new HistoryItem(LayersManipulator.Zoom, LayersManipulator.ImageOffset, layersControl.GetLayers(), layersControl.GetSelectedLayerIndex()));
                        Clipboard.SetImage(tempImage);
                        DeleteImageToolStripMenuItem_Click(sender, e);
                        RedrawImage();
                    }
                }
            }
        }

        private void CopyImageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var selectedLayer = layersControl.GetLayer(layersControl.GetSelectedLayerIndex());
            if (selectedLayer != null)
            {
                if (selectionPoints.Count > 0)
                {
                    HistoryManager.RecordState(new HistoryItem(LayersManipulator.Zoom, LayersManipulator.ImageOffset, layersControl.GetLayers(), layersControl.GetSelectedLayerIndex()));
                    CalculateSelectionBounds();
                    Image? tempImage = ExtractSelectedArea(selectedLayer);
                    if (tempImage != null)
                    {
                        Clipboard.SetImage(tempImage);
                        selectionPoints.Clear();
                        RedrawImage();
                    }
                }
                else
                {
                    Image? tempImage = selectedLayer.Image;
                    if (tempImage != null)
                    {
                        HistoryManager.RecordState(new HistoryItem(LayersManipulator.Zoom, LayersManipulator.ImageOffset, layersControl.GetLayers(), layersControl.GetSelectedLayerIndex()));
                        Clipboard.SetImage(tempImage);
                        RedrawImage();
                    }
                }
            }
        }

        private void PasteImageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Image? clipboardImage = Clipboard.GetImage();
            if (clipboardImage != null)
            {
                var selectedLayer = layersControl.GetLayer(layersControl.GetSelectedLayerIndex());
                if (selectedLayer != null)
                {
                    selectedLayer.Image = new Bitmap(clipboardImage);

                    RedrawImage();
                }
            }
        }

        private void UndoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (HistoryManager.CanUndo)
            {
                HistoryItem? history = HistoryManager.Undo();

                if (history != null)
                {
                    layersControl.ClearLayers();
                    layersControl.AddRange(history.Layers);
                    LayersManipulator.Zoom = history.Zoom;
                    LayersManipulator.ImageOffset = history.Offset;
                    layersControl.SetSelectedLayerIndex(history.SelectedLayerIndex);
                }

                Refresh();
                RedrawImage();
            }
        }

        private void RedoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (HistoryManager.CanRedo)
            {
                HistoryItem? history = HistoryManager.Redo();

                if (history != null)
                {
                    layersControl.ClearLayers();
                    layersControl.AddRange(history.Layers);
                    LayersManipulator.Zoom = history.Zoom;
                    LayersManipulator.ImageOffset = history.Offset;
                    layersControl.SetSelectedLayerIndex(history.SelectedLayerIndex);
                }

                Refresh();
                RedrawImage();
            }
        }

        private void SelectAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var selectedLayer = layersControl.GetLayer(layersControl.GetSelectedLayerIndex());
            if (selectedLayer != null)
            {
                if (selectedLayer.Image != null)
                {
                    float aspectRatio = (float)LayersManipulator.Width / LayersManipulator.Height;
                    float containerAspectRatio = (float)canvas.Width / canvas.Height;
                    float imageAspectRatio = (float)selectedLayer.Image.Width / selectedLayer.Image.Height;
                    float scaledWidth, scaledHeight;
                    float scaledImageWidth, scaledImageHeight;

                    if (aspectRatio > containerAspectRatio)
                    {
                        scaledImageWidth = selectedLayer.Image.Width * (float)canvas.Width / (float)LayersManipulator.Width * LayersManipulator.Zoom;
                        scaledImageHeight = scaledImageWidth / imageAspectRatio;
                        scaledWidth = canvas.Width * LayersManipulator.Zoom;
                        scaledHeight = scaledWidth / aspectRatio;
                    }
                    else
                    {
                        scaledImageHeight = selectedLayer.Image.Height * (float)canvas.Height / (float)LayersManipulator.Height * LayersManipulator.Zoom;
                        scaledImageWidth = scaledImageHeight * imageAspectRatio;
                        scaledHeight = canvas.Height * LayersManipulator.Zoom;
                        scaledWidth = scaledHeight * aspectRatio;
                    }

                    float centerX = (canvas.Width - scaledWidth) / 2;
                    float centerY = (canvas.Height - scaledHeight) / 2;

                    RectangleF destRect = new(centerX + LayersManipulator.ImageOffset.X, centerY + LayersManipulator.ImageOffset.Y, scaledImageWidth, scaledImageHeight);

                    selectionPoints.Add(new Point((int)destRect.X, (int)destRect.Y));
                    selectionPoints.Add(new Point((int)destRect.X, (int)(destRect.Y + destRect.Height)));
                    selectionPoints.Add(new Point((int)(destRect.X + destRect.Width), (int)(destRect.Y + destRect.Height)));
                    selectionPoints.Add(new Point((int)(destRect.X + destRect.Width), (int)destRect.Y));
                    selectionPoints.Add(selectionPoints[0]);
                }
            }
        }

        private void GeneralSettingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FormSettings formSettings = new();
            var selectedLayer = layersControl.GetLayer(layersControl.GetSelectedLayerIndex());
            if (selectedLayer != null)
            {
                formSettings.LayerHeight = selectedLayer.Image?.Height ?? 2;
                formSettings.LayerWidth = selectedLayer.Image?.Width ?? 2;
                if (formSettings.ShowDialog(this) == DialogResult.OK)
                {
                    selectedLayer.ResizeContainer(formSettings.LayerWidth, formSettings.LayerHeight);
                    RedrawImage();
                }
            }
        }

        private void InvertToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var selectedLayer = layersControl.GetLayer(layersControl.GetSelectedLayerIndex());
            if (selectedLayer != null)
            {
                if (selectedLayer.Image != null)
                {
                    Bitmap inverted = ImageManipulator.InvertColors((Bitmap)selectedLayer.Image);
                    selectedLayer.Image.Dispose();
                    selectedLayer.Image = inverted;
                    RedrawImage();
                }
            }
        }

        private void GrayscaleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var selectedLayer = layersControl.GetLayer(layersControl.GetSelectedLayerIndex());
            if (selectedLayer != null)
            {
                if (selectedLayer.Image != null)
                {
                    Bitmap greyed = ImageManipulator.Grayscale((Bitmap)selectedLayer.Image);
                    selectedLayer.Image.Dispose();
                    selectedLayer.Image = greyed;
                    RedrawImage();
                }
            }
        }

        private void BlurImageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var selectedLayer = layersControl.GetLayer(layersControl.GetSelectedLayerIndex());
            if (selectedLayer != null)
            {
                if (selectedLayer.Image != null)
                {
                    Bitmap blurred = ImageManipulator.GaussianBlur((Bitmap)selectedLayer.Image, radius: 16);
                    selectedLayer.Image.Dispose();
                    selectedLayer.Image = blurred;
                    RedrawImage();
                }
            }
        }

        private void ContrastToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void BrightnessToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void CalculateSelectionBounds()
        {
            if (selectionPoints.Count == 0) return;

            float minX = float.MaxValue, minY = float.MaxValue;
            float maxX = float.MinValue, maxY = float.MinValue;

            foreach (var point in selectionPoints)
            {
                minX = Math.Min(minX, point.X);
                minY = Math.Min(minY, point.Y);
                maxX = Math.Max(maxX, point.X);
                maxY = Math.Max(maxY, point.Y);
            }

            selectionBounds = new RectangleF(minX, minY, maxX - minX, maxY - minY);
            selectionCenter = new PointF(
                selectionBounds.X + selectionBounds.Width / 2,
                selectionBounds.Y + selectionBounds.Height / 2
            );
        }

        private void UpdateTransformMatrix()
        {
            transformMatrix.Reset();
            transformMatrix.Translate(-selectionCenter.X, -selectionCenter.Y, MatrixOrder.Append);
            transformMatrix.Rotate(rotationAngle, MatrixOrder.Append);
            transformMatrix.Scale(scaleFactor, scaleFactor, MatrixOrder.Append);
            transformMatrix.Translate(selectionCenter.X, selectionCenter.Y, MatrixOrder.Append);
        }

        private void MoveSelectionContent(float dx, float dy)
        {
            selectionCenter = new PointF(selectionCenter.X + dx, selectionCenter.Y + dy);

            UpdateTransformMatrix();
        }

        private Bitmap? ExtractSelectedArea(Layer selectedLayer)
        {
            if (selectedLayer.Image == null || selectionPoints.Count < 3) return null;

            Point layerTopLeft = ScreenToWorld(new Point((int)selectionBounds.X, (int)selectionBounds.Y), LayersManipulator.Width, LayersManipulator.Height);
            layerTopLeft.X -= selectedLayer.X;
            layerTopLeft.Y -= selectedLayer.Y;

            float ratio = GetCanvasToWorldRatio();
            int srcW = (int)(selectionBounds.Width / ratio);
            int srcH = (int)(selectionBounds.Height / ratio);

            Bitmap result = new(srcW, srcH);

            using Graphics g = Graphics.FromImage(result);

            using GraphicsPath path = new();
            PointF[] sourcePoints = [.. selectionPoints.Select(p => {
                Point worldP = ScreenToWorld(p, LayersManipulator.Width, LayersManipulator.Height);
                return new PointF(worldP.X - selectedLayer.X - layerTopLeft.X,
                                  worldP.Y - selectedLayer.Y - layerTopLeft.Y);
            })];

            path.AddPolygon(sourcePoints);
            g.SetClip(path);

            g.DrawImage(selectedLayer.Image,
                new Rectangle(0, 0, srcW, srcH),
                new Rectangle(layerTopLeft.X, layerTopLeft.Y, srcW, srcH),
                GraphicsUnit.Pixel);

            return result;
        }

        private Bitmap? CutSelectionFromLayer(Layer selectedLayer, bool emptyHole = false)
        {
            if (layersControl.GetSelectedLayerIndex() < 0) return null;
            if (selectedLayer.Image == null || selectionPoints.Count < 3) return null;

            Bitmap result = new(selectedLayer.Image);
            using (Graphics g = Graphics.FromImage(result))
            {
                PointF[] layerPoints = [.. selectionPoints.Select(p =>
                {
                    Point worldP = ScreenToWorld(p, LayersManipulator.Width, LayersManipulator.Height);
                    return new PointF(worldP.X - selectedLayer.X, worldP.Y - selectedLayer.Y);
                })];

                Color fillColor = emptyHole ? Color.Transparent : Color.White;

                using GraphicsPath path = new();
                path.AddPolygon(layerPoints);
                g.CompositingMode = CompositingMode.SourceCopy;
                using Brush transparentBrush = new SolidBrush(fillColor);
                g.FillPath(transparentBrush, path);
            }
            return result;
        }

        private Bitmap? MergeSelectionToLayer(Layer selectedLayer)
        {
            if (selectedLayer.Image == null || selectedAreaBitmap == null) return null;

            Bitmap result = new(selectedLayer.Image);
            using (Graphics g = Graphics.FromImage(result))
            {
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.SmoothingMode = SmoothingMode.AntiAlias;

                float ratio = GetCanvasToWorldRatio();
                Point worldPos = ScreenToWorld(new Point((int)selectionBounds.X, (int)selectionBounds.Y),
                                                LayersManipulator.Width, LayersManipulator.Height);

                float localX = worldPos.X - selectedLayer.X;
                float localY = worldPos.Y - selectedLayer.Y;

                using Matrix layerMatrix = transformMatrix.Clone();
                float[] elements = layerMatrix.Elements;

                float worldOffsetX = (selectionCenter.X - lastMousePosition.X) / ratio;
                float worldOffsetY = (selectionCenter.Y - lastMousePosition.Y) / ratio;

                layerMatrix.Reset();
                layerMatrix.Translate(localX + (selectedAreaBitmap.Width / 2f),
                                     localY + (selectedAreaBitmap.Height / 2f));
                layerMatrix.Rotate(rotationAngle);
                layerMatrix.Scale(scaleFactor, scaleFactor);
                layerMatrix.Translate(-(selectedAreaBitmap.Width / 2f),
                                     -(selectedAreaBitmap.Height / 2f));

                g.Transform = layerMatrix;
                g.DrawImage(selectedAreaBitmap, 0, 0);
            }

            selectedAreaBitmap?.Dispose();
            selectedAreaBitmap = null;
            selectionPoints.Clear();

            return result;
        }

        private bool IsPointInSelection(Point point)
        {
            if (selectionPoints.Count < 3) return false;

            using GraphicsPath path = new();
            path.AddPolygon(selectionPoints.ToArray());
            return path.IsVisible(point);
        }

        private bool IsOverRotationHandle(Point point)
        {
            PointF rotationHandle = new(
                selectionCenter.X,
                selectionBounds.Y - ROTATION_HANDLE_SIZE
            );

            float distance = Distance(point, rotationHandle);
            return distance < ROTATION_HANDLE_SIZE;
        }

        private bool IsOverScaleHandle(Point point, out string handle)
        {
            handle = "";

            var corners = new[]
            {
                new { Name = "topLeft", Point = new PointF(selectionBounds.X, selectionBounds.Y) },
                new { Name = "topRight", Point = new PointF(selectionBounds.Right, selectionBounds.Y) },
                new { Name = "bottomLeft", Point = new PointF(selectionBounds.X, selectionBounds.Bottom) },
                new { Name = "bottomRight", Point = new PointF(selectionBounds.Right, selectionBounds.Bottom) }
            };

            foreach (var corner in corners)
            {
                if (Distance(point, corner.Point) < SCALE_HANDLE_SIZE)
                {
                    handle = corner.Name;
                    return true;
                }
            }

            return false;
        }

        private float CalculateScaleFactor(Point mousePosition)
        {
            float currentDistance = Distance(Point.Round(selectionCenter), mousePosition);

            if (initialScaleDistance > 0)
            {
                return (currentDistance / initialScaleDistance) * initialScaleFactor;
            }
            return 1.0f;
        }

        private void MoveSelection(float dx, float dy)
        {
            for (int i = 0; i < selectionPoints.Count; i++)
            {
                selectionPoints[i] = new Point(
                    (int)(selectionPoints[i].X + dx),
                    (int)(selectionPoints[i].Y + dy)
                );
            }

            CalculateSelectionBounds();
        }

        private static float Distance(Point p1, PointF p2)
        {
            float dx = p1.X - p2.X;
            float dy = p1.Y - p2.Y;
            return (float)Math.Sqrt(dx * dx + dy * dy);
        }

        private float CalculateRotationAngle(Point mousePosition)
        {
            float dx = mousePosition.X - selectionCenter.X;
            float dy = mousePosition.Y - selectionCenter.Y;
            return (float)(Math.Atan2(dy, dx) * 180 / Math.PI);
        }

        private void PixelImage_MouseDown(object sender, MouseEventArgs e)
        {
            lastMousePosition = e.Location;

            if (e.Button == MouseButtons.Left)
            {
                var selectedLayer = layersControl.GetLayer(layersControl.GetSelectedLayerIndex());

                if (btnBrusher.Checked)
                {
                    isPainting = true;
                    strokePoints = [];
                    lazyLocalPos = PointF.Empty;
                    strokeLastInterpolated = PointF.Empty;
                    PaintingEngine.BeginStroke();
                }
                else if (btnPointer.Checked)
                {
                    if (selectionPoints.Count > 0)
                    {
                        if (selectedLayer != null)
                        {
                            if (selectedLayer.Image == null || selectionPoints.Count == 0) return;

                            if (IsOverRotationHandle(e.Location))
                            {
                                isRotating = true;
                                Cursor.Current = Cursors.SizeAll;
                                startMouseAngle = CalculateRotationAngle(e.Location) - rotationAngle;
                            }
                            else if (IsOverScaleHandle(e.Location, out string handle))
                            {
                                isScaling = true;
                                Cursor.Current = GetScaleCursor(handle);
                                initialScaleDistance = Distance(Point.Round(selectionCenter), e.Location);
                                initialScaleFactor = scaleFactor;
                            }
                            else if (IsPointInSelection(e.Location))
                            {
                                isDragging = true;
                                Cursor.Current = Cursors.SizeAll;
                            }

                            if (selectedAreaBitmap == null)
                            {
                                CalculateSelectionBounds();
                                selectedAreaBitmap = ExtractSelectedArea(selectedLayer);
                                selectedLayer.Image = CutSelectionFromLayer(selectedLayer);
                            }
                        }
                    }
                    else
                    {
                        isDragging = true;
                    }
                }
                else if (btnFiller.Checked)
                {
                    if (selectedLayer != null)
                    {
                        if (selectedLayer.Image != null)
                        {
                            Bitmap bitmap = ImageManipulator.FillColor(selectedLayer.Image,
                                (ImageBlending)cboFillBlendMode.SelectedIndex, paint.GetFillColor(),
                                (float)(fillOpacity.Value / fillOpacity.Maximum),
                                lastMousePosition,
                                selectionPoints,
                                canvas,
                                LayersManipulator.Zoom,
                                LayersManipulator.ImageOffset);
                            var old = selectedLayer.Image;
                            selectedLayer.Image = bitmap;
                            old?.Dispose();
                            PaintingEngine.SetTarget(selectedLayer.Image);
                            RedrawImage();
                        }
                    }
                }
                else if (btnLassoSelect.Checked)
                {
                    isLassoSelecting = true;
                    selectionPoints.Clear();
                    selectionPoints.Add(lastMousePosition);
                }
                else if (btnRectangleSelect.Checked)
                {
                    isRectSelecting = true;
                    selectionPoints.Clear();
                    selectionPoints.Add(lastMousePosition);
                }
            }
        }

        private void PixelImage_MouseMove(object sender, MouseEventArgs e)
        {
            labelMousePosition.Text = $"({e.X}, {e.Y})";
            labelDocStatus.Text = $"Zoom: {LayersManipulator.Zoom * 100:F1}% Offset {LayersManipulator.ImageOffset}";

            var selectedLayer = layersControl.GetLayer(layersControl.GetSelectedLayerIndex());

            if (isDragging)
            {
                int dx = e.X - lastMousePosition.X;
                int dy = e.Y - lastMousePosition.Y;

                if (ModifierKeys.HasFlag(Keys.Control))
                {
                    float zoomDelta = dy * 0.01f;
                    LayersManipulator.Zoom = Math.Max(0.1f, Math.Min(10.0f, LayersManipulator.Zoom - zoomDelta));
                    LayersManipulator.InvalidateCompositeBuffers();
                    RedrawImage(selectedLayerIndex: -1, repopulateImage: false);
                }
                else if (ModifierKeys.HasFlag(Keys.Shift))
                {
                    float x = LayersManipulator.ImageOffset.X + dx;
                    float y = LayersManipulator.ImageOffset.Y + dy;
                    LayersManipulator.ImageOffset = new PointF(x, y);
                    LayersManipulator.InvalidateCompositeBuffers();
                    RedrawImage(selectedLayerIndex: -1, repopulateImage: false);
                }
                else
                {
                    if (selectedLayer != null)
                    {
                        if (selectionPoints.Count > 0)
                        {
                            MoveSelection(dx, dy);
                            MoveSelectionContent(dx, dy);
                            RedrawImage(); //selectedLayerIndex: layersControl.GetSelectedLayerIndex()
                        }
                        else
                        {
                            float ratio = GetCanvasToWorldRatio();
                            selectedLayer.X += (int)Math.Round(dx / ratio);
                            selectedLayer.Y += (int)Math.Round(dy / ratio);

                            int minPaintIntervalMs = 16; // 60 fps
                            if ((DateTime.Now - lastPaintTime).TotalMilliseconds >= minPaintIntervalMs)
                            {
                                RedrawImage(layersControl.GetSelectedLayerIndex());
                                lastPaintTime = DateTime.Now;
                            }
                        }
                    }
                }
            }
            else if (isRotating)
            {
                if (selectionPoints.Count > 0)
                {
                    rotationAngle = CalculateRotationAngle(e.Location) - startMouseAngle;
                    UpdateTransformMatrix();
                }
                else
                {
                }
            }
            else if (isScaling)
            {
                if (selectionPoints.Count > 0)
                {
                    scaleFactor = CalculateScaleFactor(e.Location);
                    UpdateTransformMatrix();
                }
                else
                {
                }
            }
            else if (isPainting)
            {
                if (selectedLayer != null)
                {
                    var sw = System.Diagnostics.Stopwatch.StartNew();

                    float lazySmoothing = (float)brush_smoothness.Value / brush_smoothness.Maximum; //0.22f

                    Point currentWorldPos = ScreenToWorld(e.Location, LayersManipulator.Width, LayersManipulator.Height);
                    Point localCurrentRaw = new(currentWorldPos.X - selectedLayer.X, currentWorldPos.Y - selectedLayer.Y);

                    if (strokePoints.Count == 0)
                    {
                        strokePoints = [localCurrentRaw];
                        lazyLocalPos = localCurrentRaw;
                        strokeLastInterpolated = localCurrentRaw;
                        lastMousePosition = e.Location;

                        // Start the stroke once at the beginning
                        PaintingEngine.SetTarget(selectedLayer.Image);
                        PaintingEngine.BeginStroke();
                        return;
                    }

                    PointF delta = new(localCurrentRaw.X - lazyLocalPos.X, localCurrentRaw.Y - lazyLocalPos.Y);

                    lazyLocalPos.X += delta.X * lazySmoothing;
                    lazyLocalPos.Y += delta.Y * lazySmoothing;

                    strokePoints.Add(lazyLocalPos);

                    float aspectRatio = (float)LayersManipulator.Screen.Width / LayersManipulator.Screen.Height;
                    float containerAspectRatio = (float)canvas.Width / canvas.Height;
                    float scale = 1.0f;
                    if (aspectRatio > containerAspectRatio)
                        scale = (float)LayersManipulator.Screen.Width / canvas.Width;
                    else if (aspectRatio < containerAspectRatio)
                        scale = (float)LayersManipulator.Screen.Height / canvas.Height;

                    float brushPixelSize = 2 * scale * (float)brush_size.Value / brush_size.Maximum;
                    float currentOpacity = (float)brush_opacity.Value / brush_opacity.Maximum;

                    float radius = paint.Brush != null ? (int)(paint.Brush.Width * brushPixelSize / 2) : 0;
                    Rectangle dirty = new(0, 0, 0, 0);

                    if (strokePoints.Count >= 2)
                    {
                        int minX = (int)(Math.Min(strokePoints[^2].X, strokePoints[^1].X) - radius);
                        int minY = (int)(Math.Min(strokePoints[^2].Y, strokePoints[^1].Y) - radius);
                        int maxX = (int)(Math.Max(strokePoints[^2].X, strokePoints[^1].X) + radius);
                        int maxY = (int)(Math.Max(strokePoints[^2].Y, strokePoints[^1].Y) + radius);

                        dirty = new(minX + selectedLayer.X, minY + selectedLayer.Y, maxX - minX, maxY - minY);

                        dirty.Intersect(new Rectangle(0, 0, LayersManipulator.Width, LayersManipulator.Height));

                        if (strokePoints.Count == 2)
                        {
                            Point start = Point.Round(strokePoints[0]);
                            Point end = Point.Round(strokePoints[1]);
                            PaintingEngine.PaintStroke(start, end, brushPixelSize, currentOpacity);
                        }
                        else
                        {
                            int n = strokePoints.Count;
                            PointF p0 = (n >= 3) ? strokePoints[n - 3] : strokePoints[n - 2];
                            PointF p1 = strokePoints[n - 2];
                            PointF p2 = strokePoints[n - 1];
                            PointF p3 = p2;

                            const int segments = 1; // Increased for smoother curves, allegedly (intitialy set to 8 - too slow)

                            PointF previousPos = strokeLastInterpolated;

                            for (int i = 1; i <= segments; i++)
                            {
                                float t = i / (float)segments;
                                PointF pos = PaintingEngine.CatmullRomPoint(p0, p1, p2, p3, t);

                                Point prevRounded = Point.Round(previousPos);
                                Point currRounded = Point.Round(pos);

                                // Only paint if we've moved at least 1 pixel
                                if (prevRounded != currRounded)
                                {
                                    PaintingEngine.PaintStroke(prevRounded, currRounded, brushPixelSize, currentOpacity);
                                }

                                previousPos = pos;
                            }

                            strokeLastInterpolated = previousPos;
                        }
                    }

                    if (!dirty.IsEmpty)
                        LayersManipulator.DirtyRegions.Add(dirty);

                    lastMousePosition = e.Location;

                    sw.Stop();
                    Console.WriteLine($"painting: {sw.ElapsedMilliseconds}ms");

                    const int minPaintIntervalMs = 32; // 30 fps
                    if ((DateTime.Now - lastPaintTime).TotalMilliseconds >= minPaintIntervalMs)
                    {
                        RedrawImage(layersControl.GetSelectedLayerIndex());
                        LayersManipulator.DirtyRegions.Clear();
                        LayersManipulator.DirtyRegions.Add(dirty); // Keep the last dirty region for the next paint
                        lastPaintTime = DateTime.Now;
                    }
                }
            }
            else if (isLassoSelecting)
            {
                if (e.X != lastMousePosition.X || e.Y != lastMousePosition.Y)
                {
                    selectionPoints.Add(e.Location); // You don't need to call RedrawImage() here since the selection is drawn with the timer for selectionPoints.Count > 1
                }
            }
            else if (isRectSelecting)
            {
                if (e.X != lastMousePosition.X || e.Y != lastMousePosition.Y)
                {
                    if (selectionPoints.Count == 1)
                    {
                        selectionPoints.Add(new Point(e.X, selectionPoints[0].Y));
                        selectionPoints.Add(e.Location);
                        selectionPoints.Add(new Point(selectionPoints[0].X, e.Y));
                        selectionPoints.Add(selectionPoints[0]);
                    }
                    else if (selectionPoints.Count == 5)
                    {
                        selectionPoints[1] = new Point(e.X, selectionPoints[0].Y);
                        selectionPoints[2] = e.Location;
                        selectionPoints[3] = new Point(selectionPoints[0].X, e.Y);
                    }
                }
            }
            else
            {
                if (selectionPoints.Count > 0)
                {
                    UpdateCursor(e.Location);
                }
            }

            lastMousePosition = e.Location;
        }

        private void PixelImage_MouseUp(object sender, MouseEventArgs e)
        {
            HistoryManager.RecordState(new HistoryItem(LayersManipulator.Zoom, LayersManipulator.ImageOffset, layersControl.GetLayers(), layersControl.GetSelectedLayerIndex()));
            if (isLassoSelecting)
            {
                if (selectionPoints.Count > 2)
                {
                    selectionPoints.Add(selectionPoints[0]);
                }
            }
            isDragging = false;
            isPainting = false;
            isLassoSelecting = false;
            isRectSelecting = false;
            isRotating = false;
            isScaling = false;
            Cursor.Current = Cursors.Default;
            LayersManipulator.UpdateBuffers();
            PaintingEngine.EndStroke();
            RedrawImage();
            layersControl.RefreshLayersDisplay();
        }

        private void Canvas_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                if (selectionPoints.Count > 0)
                {
                    selectionPoints.Clear();
                }

                var selectedLayer = layersControl.GetLayer(layersControl.GetSelectedLayerIndex());

                if (selectedAreaBitmap != null)
                {
                    selectedLayer?.Image = MergeSelectionToLayer(selectedLayer);
                }

                rotationAngle = 0;
                scaleFactor = 1.0f;
            }
        }

        private float GetCanvasToWorldRatio()
        {
            float aspectRatio = (float)LayersManipulator.Width / LayersManipulator.Height;
            float containerAspectRatio = (float)canvas.Width / canvas.Height;

            float scaledWidth = aspectRatio > containerAspectRatio
                ? canvas.Width * LayersManipulator.Zoom
                : canvas.Height * LayersManipulator.Zoom * aspectRatio;

            return scaledWidth / LayersManipulator.Width;
        }

        private Point ScreenToWorld(Point screenPt, int canvasW, int canvasH)
        {
            float aspectRatio = (float)canvasW / canvasH;
            float containerAspectRatio = (float)canvas.Width / canvas.Height;

            float scaledWidth, scaledHeight;
            if (aspectRatio > containerAspectRatio)
            {
                scaledWidth = canvas.Width * LayersManipulator.Zoom;
                scaledHeight = scaledWidth / aspectRatio;
            }
            else
            {
                scaledHeight = canvas.Height * LayersManipulator.Zoom;
                scaledWidth = scaledHeight * aspectRatio;
            }

            float centerX = (canvas.Width - scaledWidth) / 2;
            float centerY = (canvas.Height - scaledHeight) / 2;

            float ratio = scaledWidth / canvasW;

            int worldX = (int)((screenPt.X - (centerX + LayersManipulator.ImageOffset.X)) / ratio);
            int worldY = (int)((screenPt.Y - (centerY + LayersManipulator.ImageOffset.Y)) / ratio);

            return new Point(worldX, worldY);
        }

        private void RedrawImage(int selectedLayerIndex = -1, bool repopulateImage = true)
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();

            if (repopulateImage)
            {
                LayersManipulator.PopulateColorGrid(layersControl.GetLayers(), selectedLayerIndex);
            }

            var rect = repopulateImage ?
                new Rectangle(0, 0, LayersManipulator.Width, LayersManipulator.Height) :
                new Rectangle(0, 0, 0, 0);

            RedrawRasterImage(LayersManipulator.GetImage(LayersManipulator.Screen, rect));
            RedrawHandles();

            sw.Stop();
            Console.WriteLine($"PopulateColorGrid: {sw.ElapsedMilliseconds}ms");
        }

        private void RedrawRasterImage(Image? image)
        {
            if (image is null)
            {
                return;
            }

            Bitmap bmp = new(canvas.Width, canvas.Height);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.Clear(Color.DimGray);

                float aspectRatio = (float)image.Width / image.Height;
                float containerAspectRatio = (float)canvas.Width / canvas.Height;
                float scaledWidth, scaledHeight;

                if (aspectRatio > containerAspectRatio)
                {
                    scaledWidth = canvas.Width * LayersManipulator.Zoom;
                    scaledHeight = scaledWidth / aspectRatio;
                }
                else
                {
                    scaledHeight = canvas.Height * LayersManipulator.Zoom;
                    scaledWidth = scaledHeight * aspectRatio;
                }

                float centerX = (canvas.Width - scaledWidth) / 2;
                float centerY = (canvas.Height - scaledHeight) / 2;

                RectangleF destRect = new(centerX + LayersManipulator.ImageOffset.X, centerY + LayersManipulator.ImageOffset.Y, scaledWidth, scaledHeight);

                g.InterpolationMode = (LayersManipulator.Zoom < 0.5f || LayersManipulator.Zoom > 2.0f) ? InterpolationMode.HighQualityBicubic : InterpolationMode.NearestNeighbor;
                g.PixelOffsetMode = PixelOffsetMode.Half;
                g.DrawImage(image, destRect);

                if (selectedAreaBitmap != null)
                {
                    g.MultiplyTransform(transformMatrix);
                    g.DrawImage(selectedAreaBitmap, selectionBounds);
                    g.ResetTransform();
                }
            }

            var old = canvas.Image;
            canvas.Image = bmp;
            old?.Dispose();
        }

        private void RedrawHandles()
        {
            if (selectionPoints.Count == 0 || canvas.Image == null) return;

            using (Graphics g = Graphics.FromImage(canvas.Image))
            {
                if (selectionPoints.Count > 1)
                {
                    using Pen selectionPen = new(Color.Blue, 2f);
                    selectionPen.DashPattern = [5, 5];
                    selectionPen.DashOffset = _dashOffset;
                    g.DrawLines(selectionPen, selectionPoints.ToArray());
                }

                g.SmoothingMode = SmoothingMode.AntiAlias;

                using Pen handlePen = new(Color.White, 1.5f);
                using Brush scaleBrush = new SolidBrush(Color.FromArgb(0, 120, 215));
                using Brush rotateBrush = new SolidBrush(Color.Gold);

                PointF[] corners =
                [
                    new(selectionBounds.X, selectionBounds.Y),
                    new(selectionBounds.Right, selectionBounds.Y),
                    new(selectionBounds.X, selectionBounds.Bottom),
                    new(selectionBounds.Right, selectionBounds.Bottom)
                ];

                foreach (var corner in corners)
                {
                    RectangleF rect = new(
                        corner.X - SCALE_HANDLE_SIZE / 2,
                        corner.Y - SCALE_HANDLE_SIZE / 2,
                        SCALE_HANDLE_SIZE,
                        SCALE_HANDLE_SIZE
                    );
                    g.FillRectangle(scaleBrush, rect);
                    g.DrawRectangle(handlePen, rect.X, rect.Y, rect.Width, rect.Height);
                }

                float centerX = selectionBounds.X + selectionBounds.Width / 2;
                float handleCenterY = selectionBounds.Y - ROTATION_HANDLE_SIZE;

                g.DrawLine(handlePen, centerX, selectionBounds.Y, centerX, handleCenterY);

                RectangleF rotRect = new(
                    centerX - ROTATION_HANDLE_SIZE / 2,
                    handleCenterY - ROTATION_HANDLE_SIZE / 2,
                    ROTATION_HANDLE_SIZE,
                    ROTATION_HANDLE_SIZE
                );

                g.FillEllipse(rotateBrush, rotRect);
                g.DrawEllipse(handlePen, rotRect);
            }

            canvas.Invalidate();
        }

        private void ExitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
    }
}
