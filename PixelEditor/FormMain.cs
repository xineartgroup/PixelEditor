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
        private PointF imageOffset = new(0, 0);
        private bool isDragging = false;
        private bool isPainting = false;
        private bool isSelecting = false;
        private bool isDirty = false;
        private float zoom = 0.95f;
        private float _dashOffset = 0;
        private int selectedBrushIndex = 0;
        private string currentFilePath = "";
        private DateTime lastPaintTime = DateTime.MinValue;
        private readonly List<Layer> imageLayers = [];
        private readonly List<Image> brushes = [];
        private List<PointF> strokePoints = [];
        private readonly List<Point> selectionPoints = [];
        private PointF lazyLocalPos;
        private PointF strokeLastInterpolated;

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

        private void Pic_Click(object sender, EventArgs e)
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

        private void BtnPointer_Click(object sender, EventArgs e)
        {
            UncheckOthers(btnPointer);
        }

        private void BtnFiller_Click(object sender, EventArgs e)
        {
            UncheckOthers(btnFiller);
        }

        private void BtnBrusher_Click(object sender, EventArgs e)
        {
            UncheckOthers(btnBrusher);
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

        private void UncheckOthers(RadioButton? btn)
        {
            if (btn is not null)
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
            }
        }

        private void BtnPointer_CheckedChanged(object sender, EventArgs e)
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
            else if (btnFreehand.Checked)
            {
                PaintingEngine.SetBrush(paint);
                canvas.Cursor = Cursors.Cross;
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

                // Calculate hot spot relative to the scaled cursor size
                int hotSpotX = cursorWidth / 2;
                int hotSpotY = cursorHeight / 2;

                canvas.Cursor = CreateCursor(new Bitmap(paint.Brush, cursorWidth, cursorHeight), hotSpotX, hotSpotY);
            }
            else
            {
                canvas.Cursor = Cursors.Default;
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

        private void AddLayer(Image? image)
        {
            int count = 1;

            for (int i = 0; i < imageLayers.Count; i++)
            {
                if (imageLayers[i].Name.StartsWith("layer "))
                {
                    string suffix = imageLayers[i].Name[6..];
                    if (int.TryParse(suffix, out int num))
                    {
                        count = Math.Max(count, num + 1);
                    }
                }
            }

            if (chkListLayers.Items.Count != imageLayers.Count)
            {
                MessageBox.Show("Layer count mismatch. Cannot add new layer.");
                return;
            }

            var layer = new Layer($"layer {count}", true);
            layer.OnLayerChanged += (bounds) =>
            {
                if (bounds != Rectangle.Empty)
                    LayerManipulator.DirtyRegions.Add(bounds);
            };

            chkListLayers.Items.Insert(0, layer.Name);
            imageLayers.Insert(0, layer);
            chkListLayers.SelectedIndex = 0;
            imageLayers[chkListLayers.SelectedIndex].Image = image;
            chkListLayers.SetItemChecked(chkListLayers.SelectedIndex, true);
        }

        private void RemoveLayer(int selectedIndex)
        {
            if (chkListLayers.Items.Count != imageLayers.Count)
            {
                MessageBox.Show("Layer count mismatch. Cannot add new layer.");
                return;
            }

            if (selectedIndex < chkListLayers.Items.Count && selectedIndex >= 0)
            {
                imageLayers.RemoveAt(selectedIndex);
                chkListLayers.Items.RemoveAt(selectedIndex);
            }
        }

        private void BtnBrowseImage_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog1 = new();
            if ((openFileDialog1).ShowDialog() == DialogResult.OK)
            {
                Image image = Image.FromFile(openFileDialog1.FileName);
                if (image is not null)
                {
                    DialogResult result = DialogResult.Yes;
                    if (result != DialogResult.Cancel)
                    {
                        zoom = 1.0f;
                        imageOffset = new PointF(0, 0);

                        redToolStripMenuItem1.Checked = false;
                        greenToolStripMenuItem1.Checked = false;
                        blueToolStripMenuItem1.Checked = false;

                        AddLayer(image);

                        RedrawImage();
                    }
                }
            }
        }

        private void NewToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!ConfirmAbandonChanges()) return;

            zoom = 1.0f;
            imageOffset = new PointF(0, 0);

            HistoryManager.Clear();
            imageLayers.Clear();
            chkListLayers.Items.Clear();
            imageLayers.Clear();
            RedrawImage();

            currentFilePath = null;
            isDirty = false;
            UpdateTitleBar();
            this.Invalidate();
        }

        private void OpenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!ConfirmAbandonChanges()) return;

            using OpenFileDialog ofd = new();
            ofd.Filter = "Pixel to Vector Files (*.ptv)|*.ptv";

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    using FileStream fs = File.Open(ofd.FileName, FileMode.Open);

                    StrokePTVLoader.Load(fs, out zoom, out imageOffset, out var layers, out int selectedLayerIndex);

                    imageLayers.Clear();
                    chkListLayers.Items.Clear();

                    imageLayers.AddRange(layers);
                    foreach (var layer in imageLayers)
                    {
                        chkListLayers.Items.Add(layer.Name);
                        chkListLayers.SetItemChecked(chkListLayers.Items.Count - 1, layer.IsVisible);
                    }

                    chkListLayers.SelectedIndex = selectedLayerIndex;

                    currentFilePath = ofd.FileName;
                    isDirty = false;
                    UpdateTitleBar();

                    HistoryManager.Clear();

                    UpdateControls();

                    RedrawImage();
                    this.Invalidate();
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
                sfd.Filter = "Pixel to Vector Files (*.ptv)|*.ptv";

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
            sfd.Filter = "Pixel to Vector Files (*.ptv)|*.ptv";

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
                StrokePTVSaver.Save(fs, zoom, imageOffset, imageLayers, chkListLayers.SelectedIndex);
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
                    sfd.Filter = "Pixel to Vector Files (*.ptv)|*.ptv";
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
            if (chkListLayers.Items.Count == imageLayers.Count && chkListLayers.SelectedIndex < chkListLayers.Items.Count && chkListLayers.SelectedIndex >= 0)
            {
                imageLayers[chkListLayers.SelectedIndex].Opacity = (int)opacity.Value;
                RedrawImage();
            }
        }

        private void CboBlendMode_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (chkListLayers.Items.Count == imageLayers.Count && chkListLayers.SelectedIndex < chkListLayers.Items.Count && chkListLayers.SelectedIndex >= 0)
            {
                imageLayers[chkListLayers.SelectedIndex].BlendMode = Enum.Parse<ImageBlending>(cboBlendMode.Text);
                RedrawImage();
            }
        }

        private void BtnAddVector_Click(object sender, EventArgs e)
        {
            Bitmap bmp = new(LayerManipulator.Width, LayerManipulator.Height);

            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.Clear(Color.White);
            }

            AddLayer(bmp);
            RedrawImage();
            HistoryManager.RecordState(new HistoryItem(zoom, imageOffset, imageLayers, chkListLayers.SelectedIndex));
        }

        private void BtnSubtractVector_Click(object sender, EventArgs e)
        {
            RemoveLayer(chkListLayers.SelectedIndex);
            RedrawImage();
            HistoryManager.RecordState(new HistoryItem(zoom, imageOffset, imageLayers, chkListLayers.SelectedIndex));
        }

        private void BtnMoveUp_Click(object sender, EventArgs e)
        {
            if (chkListLayers.SelectedIndex <= 0) return;

            int currentIndex = chkListLayers.SelectedIndex;
            int newIndex = currentIndex - 1;

            object itemToMove = chkListLayers.Items[currentIndex];
            chkListLayers.Items.RemoveAt(currentIndex);
            chkListLayers.Items.Insert(newIndex, itemToMove);

            var layerToMove = imageLayers[currentIndex];
            imageLayers.RemoveAt(currentIndex);
            imageLayers.Insert(newIndex, layerToMove);

            chkListLayers.SelectedIndex = newIndex;
            chkListLayers.SetItemChecked(newIndex, imageLayers[newIndex].IsVisible);

            RedrawImage();
            HistoryManager.RecordState(new HistoryItem(zoom, imageOffset, imageLayers, chkListLayers.SelectedIndex));
        }

        private void BtnMoveDown_Click(object sender, EventArgs e)
        {
            if (chkListLayers.SelectedIndex < 0 ||
                chkListLayers.SelectedIndex >= chkListLayers.Items.Count - 1) return;

            int currentIndex = chkListLayers.SelectedIndex;
            int newIndex = currentIndex + 1;

            object itemToMove = chkListLayers.Items[currentIndex];
            chkListLayers.Items.RemoveAt(currentIndex);
            chkListLayers.Items.Insert(newIndex, itemToMove);

            var layerToMove = imageLayers[currentIndex];
            imageLayers.RemoveAt(currentIndex);
            imageLayers.Insert(newIndex, layerToMove);

            chkListLayers.SelectedIndex = newIndex;
            chkListLayers.SetItemChecked(newIndex, imageLayers[newIndex].IsVisible);

            RedrawImage();
            HistoryManager.RecordState(new HistoryItem(zoom, imageOffset, imageLayers, chkListLayers.SelectedIndex));
        }

        private void MoveToTopToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (chkListLayers.SelectedIndex < 0 ||
                chkListLayers.SelectedIndex >= chkListLayers.Items.Count - 1) return;

            int currentIndex = chkListLayers.SelectedIndex;

            object itemToMove = chkListLayers.Items[currentIndex];
            chkListLayers.Items.RemoveAt(currentIndex);
            chkListLayers.Items.Add(itemToMove);

            var layerToMove = imageLayers[currentIndex];
            imageLayers.RemoveAt(currentIndex);
            imageLayers.Add(layerToMove);

            chkListLayers.SelectedIndex = 0;

            RedrawImage();
            HistoryManager.RecordState(new HistoryItem(zoom, imageOffset, imageLayers, chkListLayers.SelectedIndex));
        }

        private void MoveToBottomToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (chkListLayers.SelectedIndex < 0 ||
                chkListLayers.SelectedIndex >= chkListLayers.Items.Count - 1) return;

            int currentIndex = chkListLayers.SelectedIndex;
            int newIndex = chkListLayers.Items.Count - 2;

            object itemToMove = chkListLayers.Items[currentIndex];
            chkListLayers.Items.RemoveAt(currentIndex);
            chkListLayers.Items.Insert(newIndex, itemToMove);

            var layerToMove = imageLayers[currentIndex];
            imageLayers.RemoveAt(currentIndex);
            imageLayers.Insert(newIndex, layerToMove);

            chkListLayers.SelectedIndex = newIndex;

            RedrawImage();
            HistoryManager.RecordState(new HistoryItem(zoom, imageOffset, imageLayers, chkListLayers.SelectedIndex));
        }

        private void ChkListLayers_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            BtnEditCaption_Click(sender, e);
        }

        private void ChkListLayers_SelectedIndexChanged(object sender, EventArgs e)
        {
            btnMoveUp.Enabled = chkListLayers.SelectedIndex > 0;
            btnMoveDown.Enabled = chkListLayers.SelectedIndex < chkListLayers.Items.Count - 1
                               && chkListLayers.SelectedIndex >= 0;

            UpdateControls();
        }

        private void ChkListLayers_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            if (chkListLayers.Items.Count == imageLayers.Count && chkListLayers.SelectedIndex < chkListLayers.Items.Count && chkListLayers.SelectedIndex >= 0)
            {
                imageLayers[chkListLayers.SelectedIndex].IsVisible = !chkListLayers.GetItemChecked(chkListLayers.SelectedIndex);
                chkListLayers.Items[chkListLayers.SelectedIndex] = chkListLayers.Text;
                RedrawImage();
            }
        }

        private void BtnShowVector_Click(object sender, EventArgs e)
        {
            if (chkListLayers.Items.Count == imageLayers.Count && chkListLayers.SelectedIndex < chkListLayers.Items.Count && chkListLayers.SelectedIndex >= 0)
            {
                imageLayers[chkListLayers.SelectedIndex].IsVisible = true;
                chkListLayers.Items[chkListLayers.SelectedIndex] = chkListLayers.Text;
                RedrawImage();
            }
        }

        private void BtnHideVector_Click(object sender, EventArgs e)
        {
            if (chkListLayers.Items.Count == imageLayers.Count && chkListLayers.SelectedIndex < chkListLayers.Items.Count && chkListLayers.SelectedIndex >= 0)
            {
                imageLayers[chkListLayers.SelectedIndex].IsVisible = false;
                chkListLayers.Items[chkListLayers.SelectedIndex] = chkListLayers.Text;
                RedrawImage();
            }
        }

        private void BtnEditCaption_Click(object sender, EventArgs e)
        {
            FormName formName = new();
            if (chkListLayers.Items.Count == imageLayers.Count && chkListLayers.SelectedIndex < chkListLayers.Items.Count && chkListLayers.SelectedIndex >= 0)
            {
                formName.StrokeName = imageLayers[chkListLayers.SelectedIndex].Name;
                if (formName.ShowDialog() == DialogResult.OK)
                {
                    HistoryManager.RecordState(new HistoryItem(zoom, imageOffset, imageLayers, chkListLayers.SelectedIndex));
                    imageLayers[chkListLayers.SelectedIndex].Name = formName.StrokeName;
                    chkListLayers.Items[chkListLayers.SelectedIndex] = formName.StrokeName;
                }
            }
        }

        private void BtnMergeDown_Click(object sender, EventArgs e)
        {
            if (imageLayers.Count < 2)
                return;

            int selectedIndex = chkListLayers.SelectedIndex;

            if (selectedIndex < 0 || selectedIndex >= imageLayers.Count - 1)
                return;

            ListBoxVectors_SelectedIndexChanged(sender, e);
        }

        private void UpdateControls()
        {
            if (chkListLayers.Items.Count == imageLayers.Count && chkListLayers.Items.Count > chkListLayers.SelectedIndex && chkListLayers.SelectedIndex >= 0)
            {
                var layer = imageLayers[chkListLayers.SelectedIndex];
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
            btnMoveUp.Enabled = chkListLayers.SelectedIndex > 0;
            btnMoveDown.Enabled = chkListLayers.SelectedIndex < chkListLayers.Items.Count - 1
                               && chkListLayers.SelectedIndex >= 0;

            UpdateControls();
        }

        private void ListBoxVectors_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            BtnEditCaption_Click(sender, e);
        }

        private void ZoomInToolStripMenuItem_Click(object sender, EventArgs e)
        {
            HistoryManager.RecordState(new HistoryItem(zoom, imageOffset, imageLayers, chkListLayers.SelectedIndex));
            float zoomDelta = 0.1f;
            zoom = Math.Max(0.1f, Math.Min(10.0f, zoom + zoomDelta));
            RedrawImage();
        }

        private void ZoomOutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            HistoryManager.RecordState(new HistoryItem(zoom, imageOffset, imageLayers, chkListLayers.SelectedIndex));
            float zoomDelta = 0.1f;
            zoom = Math.Max(0.1f, Math.Min(10.0f, zoom - zoomDelta));
            RedrawImage();
        }

        private void BtnResetZoom_Click(object sender, EventArgs e)
        {
            HistoryManager.RecordState(new HistoryItem(zoom, imageOffset, imageLayers, chkListLayers.SelectedIndex));
            zoom = 1.0f;
            imageOffset = new(0, 0);
            RedrawImage();
        }

        private void AllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (chkListLayers.Items.Count == imageLayers.Count &&
                chkListLayers.Items.Count > chkListLayers.SelectedIndex &&
                chkListLayers.SelectedIndex >= 0)
            {
                redToolStripMenuItem.Checked = false;
                greenToolStripMenuItem.Checked = false;
                blueToolStripMenuItem.Checked = false;
                allToolStripMenuItem.Checked = !allToolStripMenuItem.Checked;
                imageLayers[chkListLayers.SelectedIndex].Channel = LayerChannel.RGB;
                RedrawImage();
            }
        }

        private void RedToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (chkListLayers.Items.Count == imageLayers.Count &&
                chkListLayers.Items.Count > chkListLayers.SelectedIndex &&
                chkListLayers.SelectedIndex >= 0)
            {
                redToolStripMenuItem.Checked = !redToolStripMenuItem.Checked;
                greenToolStripMenuItem.Checked = false;
                blueToolStripMenuItem.Checked = false;
                allToolStripMenuItem.Checked = !redToolStripMenuItem.Checked;
                imageLayers[chkListLayers.SelectedIndex].Channel = LayerChannel.Red;
                RedrawImage();
            }
        }

        private void GreenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (chkListLayers.Items.Count == imageLayers.Count &&
                chkListLayers.Items.Count > chkListLayers.SelectedIndex &&
                chkListLayers.SelectedIndex >= 0)
            {
                redToolStripMenuItem.Checked = false;
                greenToolStripMenuItem.Checked = !greenToolStripMenuItem.Checked;
                blueToolStripMenuItem.Checked = false;
                allToolStripMenuItem.Checked = !greenToolStripMenuItem.Checked;
                imageLayers[chkListLayers.SelectedIndex].Channel = LayerChannel.Green;
                RedrawImage();
            }
        }

        private void BlueToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (chkListLayers.Items.Count == imageLayers.Count &&
                chkListLayers.Items.Count > chkListLayers.SelectedIndex &&
                chkListLayers.SelectedIndex >= 0)
            {
                redToolStripMenuItem.Checked = false;
                greenToolStripMenuItem.Checked = false;
                blueToolStripMenuItem.Checked = !blueToolStripMenuItem.Checked;
                allToolStripMenuItem.Checked = !blueToolStripMenuItem.Checked;
                imageLayers[chkListLayers.SelectedIndex].Channel = LayerChannel.Blue;
                RedrawImage();
            }
        }

        private void ChkFilter_CheckedChanged(object sender, EventArgs e)
        {
            if (chkListLayers.Items.Count == imageLayers.Count && chkListLayers.Items.Count > chkListLayers.SelectedIndex && chkListLayers.SelectedIndex >= 0)
            {
                if (imageLayers[chkListLayers.SelectedIndex].Image is not null)
                {
                    if (sender.GetType() == typeof(ToolStripMenuItem))
                    {
                        ToolStripMenuItem item = (ToolStripMenuItem)sender;
                        item.Checked = !item.Checked;
                    }

                    imageLayers[chkListLayers.SelectedIndex].RedFilter = redToolStripMenuItem1.Checked;
                    imageLayers[chkListLayers.SelectedIndex].GreenFilter = greenToolStripMenuItem1.Checked;
                    imageLayers[chkListLayers.SelectedIndex].BlueFilter = blueToolStripMenuItem1.Checked;
                    RedrawImage();
                }
            }
        }

        private void DarkToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (chkListLayers.Items.Count == imageLayers.Count && chkListLayers.Items.Count > chkListLayers.SelectedIndex && chkListLayers.SelectedIndex >= 0)
            {
                if (imageLayers[chkListLayers.SelectedIndex].Image is not null)
                {
                    bool[,] mask = LayerManipulator.GetDarkPixels(LayerManipulator.Screen, 0.0f, 1.0f);
                    ColorGrid grid = LayerManipulator.DarkPixelGrid(mask, LayerManipulator.Screen.Width, LayerManipulator.Screen.Height);
                    imageLayers[chkListLayers.SelectedIndex].Image = LayerManipulator.GetImage(grid, new Rectangle(0, 0, grid.Width, grid.Height));
                    RedrawImage();
                }
            }
        }

        private void DeleteImageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (chkListLayers.Items.Count == imageLayers.Count && chkListLayers.Items.Count > chkListLayers.SelectedIndex && chkListLayers.SelectedIndex >= 0)
            {
                HistoryManager.RecordState(new HistoryItem(zoom, imageOffset, imageLayers, chkListLayers.SelectedIndex));
                canvas.Image?.Dispose();
                canvas.Image = null;
                imageLayers[chkListLayers.SelectedIndex].Image?.Dispose();
                imageLayers[chkListLayers.SelectedIndex].Image = null;
            }
        }

        private void CutImageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (chkListLayers.Items.Count == imageLayers.Count
                && chkListLayers.Items.Count > chkListLayers.SelectedIndex
                && chkListLayers.SelectedIndex >= 0)
            {
                Image? tempImage = imageLayers[chkListLayers.SelectedIndex].Image;
                if (tempImage != null)
                {
                    Clipboard.SetImage(tempImage);
                    DeleteImageToolStripMenuItem_Click(sender, e);
                }
            }
        }

        private void CopyImageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (chkListLayers.Items.Count == imageLayers.Count
                && chkListLayers.Items.Count > chkListLayers.SelectedIndex
                && chkListLayers.SelectedIndex >= 0)
            {
                Image? tempImage = imageLayers[chkListLayers.SelectedIndex].Image;
                if (tempImage != null)
                {
                    Clipboard.SetImage(tempImage);
                }
            }
        }

        private void PasteImageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Image? clipboardImage = Clipboard.GetImage();
            if (clipboardImage != null)
            {
                if (chkListLayers.Items.Count == imageLayers.Count && chkListLayers.Items.Count > chkListLayers.SelectedIndex && chkListLayers.SelectedIndex >= 0)
                {
                    imageLayers[chkListLayers.SelectedIndex].Image = new Bitmap(clipboardImage);

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
                    imageLayers.Clear();
                    chkListLayers.Items.Clear();
                    imageLayers.AddRange(history.Layers);
                    zoom = history.Zoom;
                    imageOffset = history.Offset;
                    foreach (var layer in imageLayers)
                    {
                        chkListLayers.Items.Add(layer.Name);
                        chkListLayers.SetItemChecked(chkListLayers.Items.Count - 1, layer.IsVisible);
                    }
                    chkListLayers.SelectedIndex = history.SelectedLayerIndex;
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
                    imageLayers.Clear();
                    chkListLayers.Items.Clear();
                    imageLayers.AddRange(history.Layers);
                    zoom = history.Zoom;
                    imageOffset = history.Offset;
                    foreach (var layer in imageLayers)
                    {
                        chkListLayers.Items.Add(layer.Name);
                        chkListLayers.SetItemChecked(chkListLayers.Items.Count - 1, layer.IsVisible);
                    }
                    chkListLayers.SelectedIndex = history.SelectedLayerIndex;
                }

                Refresh();
                RedrawImage();
            }
        }

        private void GeneralSettingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FormSettings formSettings = new();
            if (chkListLayers.Items.Count == imageLayers.Count && chkListLayers.Items.Count > chkListLayers.SelectedIndex && chkListLayers.SelectedIndex >= 0)
            {
                formSettings.LayerHeight = imageLayers[chkListLayers.SelectedIndex].Image?.Height ?? 2;
                formSettings.LayerWidth = imageLayers[chkListLayers.SelectedIndex].Image?.Width ?? 2;
            }
            if (formSettings.ShowDialog(this) == DialogResult.OK)
            {
                imageLayers[chkListLayers.SelectedIndex].ResizeContainer(formSettings.LayerWidth, formSettings.LayerHeight);
                RedrawImage();
            }
        }

        private void InvertToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (chkListLayers.Items.Count == imageLayers.Count
                && chkListLayers.SelectedIndex >= 0
                && chkListLayers.SelectedIndex < imageLayers.Count)
            {
                var selectedLayer = imageLayers[chkListLayers.SelectedIndex];
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
            if (chkListLayers.Items.Count == imageLayers.Count
                && chkListLayers.SelectedIndex >= 0
                && chkListLayers.SelectedIndex < imageLayers.Count)
            {
                var selectedLayer = imageLayers[chkListLayers.SelectedIndex];
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
            if (chkListLayers.Items.Count == imageLayers.Count
                && chkListLayers.SelectedIndex >= 0
                && chkListLayers.SelectedIndex < imageLayers.Count)
            {
                var selectedLayer = imageLayers[chkListLayers.SelectedIndex];
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

        private void PixelImage_MouseDown(object sender, MouseEventArgs e)
        {
            lastMousePosition = e.Location;

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
                isDragging = true;
            }
            else if (btnFiller.Checked)
            {
                if (chkListLayers.Items.Count == imageLayers.Count
                    && chkListLayers.SelectedIndex >= 0
                    && chkListLayers.SelectedIndex < imageLayers.Count)
                {
                    var selectedLayer = imageLayers[chkListLayers.SelectedIndex];
                    if (selectedLayer.Image != null)
                    {
                        Bitmap bitmap = ImageManipulator.FillColor(selectedLayer.Image, 
                            (ImageBlending)cboFillBlendMode.SelectedIndex, paint.GetFillColor(), 
                            (float)(fillOpacity.Value / fillOpacity.Maximum),
                            lastMousePosition,
                            selectionPoints,
                            canvas,
                            zoom,
                            imageOffset);
                        var old = selectedLayer.Image;
                        selectedLayer.Image = bitmap;
                        old?.Dispose();
                        PaintingEngine.SetTarget(selectedLayer.Image);
                        RedrawImage();
                    }
                }
            }
            else if (btnFreehand.Checked)
            {
                isSelecting = true;
                selectionPoints.Clear();
                selectionPoints.Add(lastMousePosition);
            }
        }

        private void PixelImage_MouseMove(object sender, MouseEventArgs e)
        {
            labelMousePosition.Text = $"({e.X}, {e.Y})";
            labelDocStatus.Text = $"Zoom: {zoom * 100:F1}% Offset {imageOffset}";

            if (isDragging)
            {
                int dx = e.X - lastMousePosition.X;
                int dy = e.Y - lastMousePosition.Y;

                if (ModifierKeys.HasFlag(Keys.Control))
                {
                    float zoomDelta = dy * 0.01f;
                    zoom = Math.Max(0.1f, Math.Min(10.0f, zoom - zoomDelta));
                    LayerManipulator.InvalidateCompositeBuffers();
                    RedrawImage(selectedLayerIndex: -1, repopulateImage: false);
                }
                else if (ModifierKeys.HasFlag(Keys.Shift))
                {
                    imageOffset.X += dx;
                    imageOffset.Y += dy;
                    LayerManipulator.InvalidateCompositeBuffers();
                    RedrawImage(selectedLayerIndex: -1, repopulateImage: false);
                }
                else
                {
                    if (chkListLayers.Items.Count == imageLayers.Count
                        && chkListLayers.SelectedIndex >= 0
                        && chkListLayers.SelectedIndex < imageLayers.Count)
                    {
                        var layer = imageLayers[chkListLayers.SelectedIndex];

                        float ratio = GetCanvasToWorldRatio();
                        layer.X += (int)Math.Round(dx / ratio);
                        layer.Y += (int)Math.Round(dy / ratio);

                        int minPaintIntervalMs = 16; // 60 fps
                        if ((DateTime.Now - lastPaintTime).TotalMilliseconds >= minPaintIntervalMs)
                        {
                            RedrawImage(chkListLayers.SelectedIndex);
                            lastPaintTime = DateTime.Now;
                        }
                    }
                }
            }
            else if (isPainting)
            {
                if (chkListLayers.Items.Count == imageLayers.Count
                    && chkListLayers.SelectedIndex >= 0
                    && chkListLayers.SelectedIndex < imageLayers.Count)
                {
                    var sw = System.Diagnostics.Stopwatch.StartNew();

                    float lazySmoothing = (float)brush_smoothness.Value / brush_smoothness.Maximum; //0.22f

                    var selectedLayer = imageLayers[chkListLayers.SelectedIndex];

                    Point currentWorldPos = ScreenToWorld(e.Location, LayerManipulator.Width, LayerManipulator.Height);
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

                    float aspectRatio = (float)LayerManipulator.Screen.Width / LayerManipulator.Screen.Height;
                    float containerAspectRatio = (float)canvas.Width / canvas.Height;
                    float scale = 1.0f;
                    if (aspectRatio > containerAspectRatio)
                        scale = (float)LayerManipulator.Screen.Width / canvas.Width;
                    else if (aspectRatio < containerAspectRatio)
                        scale = (float)LayerManipulator.Screen.Height / canvas.Height;

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

                        dirty.Intersect(new Rectangle(0, 0, LayerManipulator.Width, LayerManipulator.Height));

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
                        LayerManipulator.DirtyRegions.Add(dirty);

                    lastMousePosition = e.Location;

                    sw.Stop();
                    Console.WriteLine($"painting: {sw.ElapsedMilliseconds}ms");

                    const int minPaintIntervalMs = 32; // 30 fps
                    if ((DateTime.Now - lastPaintTime).TotalMilliseconds >= minPaintIntervalMs)
                    {
                        RedrawImage(chkListLayers.SelectedIndex);
                        LayerManipulator.DirtyRegions.Clear();
                        LayerManipulator.DirtyRegions.Add(dirty); // Keep the last dirty region for the next paint
                        lastPaintTime = DateTime.Now;
                    }
                }
            }
            else if (isSelecting)
            {
                if (e.X != lastMousePosition.X || e.Y != lastMousePosition.Y)
                {
                    selectionPoints.Add(e.Location); // You don't need to call RedrawImage() here since the selection is drawn with the timer for selectionPoints.Count > 1
                }
            }

            lastMousePosition = e.Location;
        }

        private void PixelImage_MouseUp(object sender, MouseEventArgs e)
        {
            if (isSelecting)
            {
                if (selectionPoints.Count > 2)
                {
                    selectionPoints.Add(selectionPoints[0]);
                }
            }
            isDragging = false;
            isPainting = false;
            isSelecting = false;
            LayerManipulator.UpdateBuffers();
            PaintingEngine.EndStroke();
            RedrawImage();
            HistoryManager.RecordState(new HistoryItem(zoom, imageOffset, imageLayers, chkListLayers.SelectedIndex));
        }

        private float GetCanvasToWorldRatio()
        {
            float aspectRatio = (float)LayerManipulator.Width / LayerManipulator.Height;
            float containerAspectRatio = (float)canvas.Width / canvas.Height;

            float scaledWidth = aspectRatio > containerAspectRatio
                ? canvas.Width * zoom
                : canvas.Height * zoom * aspectRatio;

            return scaledWidth / LayerManipulator.Width;
        }

        private Point ScreenToWorld(Point screenPt, int canvasW, int canvasH)
        {
            float aspectRatio = (float)canvasW / canvasH;
            float containerAspectRatio = (float)canvas.Width / canvas.Height;

            float scaledWidth, scaledHeight;
            if (aspectRatio > containerAspectRatio)
            {
                scaledWidth = canvas.Width * zoom;
                scaledHeight = scaledWidth / aspectRatio;
            }
            else
            {
                scaledHeight = canvas.Height * zoom;
                scaledWidth = scaledHeight * aspectRatio;
            }

            float centerX = (canvas.Width - scaledWidth) / 2;
            float centerY = (canvas.Height - scaledHeight) / 2;

            // Use the ratio of Screen Pixels to Canvas Pixels
            float ratio = scaledWidth / canvasW;

            int worldX = (int)((screenPt.X - (centerX + imageOffset.X)) / ratio);
            int worldY = (int)((screenPt.Y - (centerY + imageOffset.Y)) / ratio);

            return new Point(worldX, worldY);
        }

        private void RedrawImage(int selectedLayerIndex = -1, bool repopulateImage = true)
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();

            if (repopulateImage)
            {
                LayerManipulator.PopulateColorGrid(imageLayers, selectedLayerIndex);
            }

            var rect = repopulateImage ?
                new Rectangle(0, 0, LayerManipulator.Width, LayerManipulator.Height) :
                new Rectangle(0, 0, 0, 0);

            RedrawRasterImage(LayerManipulator.GetImage(LayerManipulator.Screen, rect));

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
                    scaledWidth = canvas.Width * zoom;
                    scaledHeight = scaledWidth / aspectRatio;
                }
                else
                {
                    scaledHeight = canvas.Height * zoom;
                    scaledWidth = scaledHeight * aspectRatio;
                }

                float centerX = (canvas.Width - scaledWidth) / 2;
                float centerY = (canvas.Height - scaledHeight) / 2;

                RectangleF destRect = new(centerX + imageOffset.X, centerY + imageOffset.Y, scaledWidth, scaledHeight);

                g.InterpolationMode = (zoom < 0.5f || zoom > 2.0f) ? InterpolationMode.HighQualityBicubic : InterpolationMode.NearestNeighbor;
                g.PixelOffsetMode = PixelOffsetMode.Half;

                g.DrawImage(image, destRect);

                if (selectionPoints.Count > 1)
                {
                    using Pen selectionPen = new(Color.Blue, 2f);
                    selectionPen.DashPattern = [5, 5];
                    selectionPen.DashOffset = _dashOffset;

                    g.DrawLines(selectionPen, selectionPoints.ToArray());
                }
            }

            var old = canvas.Image;
            canvas.Image = bmp;
            old?.Dispose();
        }

        private void ExitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
    }
}
