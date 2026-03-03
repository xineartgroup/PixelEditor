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
        private Bitmap? tempImage = null;
        private Paint paint;
        private Point lastMousePosition;
        private PointF imageOffset = new(0, 0);
        private bool isDragging = false;
        private bool isPainting = false;
        private bool isDirty = false;
        private float zoom = 1.0f;
        private string? currentFilePath = null;
        private readonly PaintingEngine painter = new();
        private readonly List<Layer> imageLayers = [];
        private Rectangle _lastRenderedDragBounds = Rectangle.Empty;
        private Rectangle _accumulatedDragBounds = Rectangle.Empty;
        private DateTime lastPaintTime = DateTime.MinValue;

        private List<PointF>? strokePoints;
        private PointF lazyMousePos;
        private PointF lazyLocalPos;
        private PointF strokeLastPainted;
        private PointF strokeLastInterpolated;

        public FormMain()
        {
            InitializeComponent();
        }

        private void FormMain_Load(object sender, EventArgs e)
        {
            Bitmap bmp = new(ImageManipulator.Width, ImageManipulator.Height);

            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.Clear(Color.White);
            }

            Form1_Resize(sender, e);
            UpdateTitleBar();
            cboBlendMode.Items.Clear();
            cboBlendMode.Items.AddRange(Enum.GetNames<LayerBlending>());
            cboBlendMode.SelectedIndex = 0;
            AddLayer(bmp);
            ReloadBrushes();
        }

        private void ReloadBrushes()
        {
            try
            {
                string[] files = Directory.GetFiles(Application.StartupPath + @"\Brushes\");
                int x = 0;
                int y = 0;
                foreach (string file in files)
                {
                    PictureBox pic = new()
                    {
                        Image = new Bitmap(file),
                        Size = new Size(24, 24),
                        Location = new Point(x, y),
                        SizeMode = PictureBoxSizeMode.StretchImage,
                        BorderStyle = BorderStyle.None
                    };
                    pic.Click += Pic_Click;
                    panel2.Controls.Add(pic);
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
                        paint.Brush = new Bitmap(brush.Image);
                        if (paint.Brush != null)
                        {
                            paint.SetColor(btnPenColor.BackColor);
                            painter.SetBrush(paint);
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
                if (paint.Brush != null)
                {
                    paint.SetColor(btnPenColor.BackColor);
                    painter.SetBrush(paint);
                    UpdateCursor();
                }
            }
        }

        private void BtnPointer_Click(object sender, EventArgs e)
        {
            paint.Brush = null;
            painter.SetBrush(paint);
            UpdateCursor();
        }

        private void UpdateCursor()
        {
            if (paint.Brush != null)
            {
                canvas.Cursor.Dispose();
                canvas.Cursor = null;

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

        [DllImport("user32.dll")]
        public static extern IntPtr CreateIconIndirect(ref IconInfo icon);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetIconInfo(IntPtr hIcon, ref IconInfo pIconInfo);

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
            labelStatus.Text = $"Size {brush_size.Value}";
            UpdateCursor();
        }

        private void Brush_opacity_Scroll(object sender, EventArgs e)
        {

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
                canvas.Size = new Size(ClientSize.Width - canvas.Location.X - 200,
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
                    ImageManipulator.DirtyRegions.Add(bounds);
            };

            chkListLayers.Items.Insert(0, layer.Name);
            imageLayers.Insert(0, layer);
            chkListLayers.SelectedIndex = 0;
            imageLayers[chkListLayers.SelectedIndex].image = image;
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

                    //StrokePTVLoader.Load(fs, out zoom, out imageOffset, out image, out var layers);

                    imageLayers.Clear();
                    chkListLayers.Items.Clear();

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
                //StrokePTVSaver.Save(fs, zoom, imageOffset, image, strokeLayers, includeImage: true);
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
                imageLayers[chkListLayers.SelectedIndex].BlendMode = Enum.Parse<LayerBlending>(cboBlendMode.Text);
                RedrawImage();
            }
        }

        private void BtnAddVector_Click(object sender, EventArgs e)
        {
            Bitmap bmp = new(ImageManipulator.Width, ImageManipulator.Height);

            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.Clear(Color.White);
            }

            AddLayer(bmp);
            RedrawImage();
        }

        private void BtnSubtractVector_Click(object sender, EventArgs e)
        {
            RemoveLayer(chkListLayers.SelectedIndex);
            RedrawImage();
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

            RedrawImage();
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

            RedrawImage();
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
                    HistoryManager.RecordState(new HistoryItem(zoom, imageOffset, imageLayers));
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
            HistoryManager.RecordState(new HistoryItem(zoom, imageOffset, imageLayers));
            float zoomDelta = 0.1f;
            zoom = Math.Max(0.1f, Math.Min(10.0f, zoom + zoomDelta));
            RedrawImage();
        }

        private void ZoomOutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            HistoryManager.RecordState(new HistoryItem(zoom, imageOffset, imageLayers));
            float zoomDelta = 0.1f;
            zoom = Math.Max(0.1f, Math.Min(10.0f, zoom - zoomDelta));
            RedrawImage();
        }

        private void BtnResetZoom_Click(object sender, EventArgs e)
        {
            HistoryManager.RecordState(new HistoryItem(zoom, imageOffset, imageLayers));
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
                if (imageLayers[chkListLayers.SelectedIndex].image is not null)
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
                if (imageLayers[chkListLayers.SelectedIndex].image is not null)
                {
                    bool[,] mask = ImageManipulator.GetDarkPixels(ImageManipulator.Screen, 0.0f, 1.0f);
                    ColorGrid grid = ImageManipulator.DarkPixelGrid(mask, ImageManipulator.Screen.Width, ImageManipulator.Screen.Height);
                    imageLayers[chkListLayers.SelectedIndex].image = ImageManipulator.GetImage(grid);
                    RedrawImage();
                }
            }
        }

        private void DeleteImageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (chkListLayers.Items.Count == imageLayers.Count && chkListLayers.Items.Count > chkListLayers.SelectedIndex && chkListLayers.SelectedIndex >= 0)
            {
                HistoryManager.RecordState(new HistoryItem(zoom, imageOffset, imageLayers));
                canvas.Image?.Dispose();
                canvas.Image = null;
                imageLayers[chkListLayers.SelectedIndex].image?.Dispose();
                imageLayers[chkListLayers.SelectedIndex].image = null;
                tempImage?.Dispose();
                tempImage = null;
            }
        }

        private void CutImageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (tempImage != null)
            {
                Clipboard.SetImage(tempImage);
                DeleteImageToolStripMenuItem_Click(sender, e);
            }
        }

        private void CopyImageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (tempImage != null)
            {
                Clipboard.SetImage(tempImage);
            }
        }

        private void PasteImageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Image? clipboardImage = Clipboard.GetImage();
            if (clipboardImage != null)
            {
                if (chkListLayers.Items.Count == imageLayers.Count && chkListLayers.Items.Count > chkListLayers.SelectedIndex && chkListLayers.SelectedIndex >= 0)
                {
                    imageLayers[chkListLayers.SelectedIndex].image = new Bitmap(clipboardImage);

                    RedrawImage();
                }
            }
        }

        private void UndoToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void RedoToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void GeneralSettingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FormSettings formSettings = new();
            if (chkListLayers.Items.Count == imageLayers.Count && chkListLayers.Items.Count > chkListLayers.SelectedIndex && chkListLayers.SelectedIndex >= 0)
            {
                formSettings.LayerHeight = imageLayers[chkListLayers.SelectedIndex].image?.Height ?? 2;
                formSettings.LayerWidth = imageLayers[chkListLayers.SelectedIndex].image?.Width ?? 2;
            }
            if (formSettings.ShowDialog(this) == DialogResult.OK)
            {
                imageLayers[chkListLayers.SelectedIndex].ResizeContainer(formSettings.LayerWidth, formSettings.LayerHeight);
                RedrawImage();
            }
        }

        private void PixelImage_MouseDown(object sender, MouseEventArgs e)
        {
            lastMousePosition = e.Location;
            lazyMousePos = e.Location;

            if (paint.Brush != null)
            {
                isPainting = true;
                strokePoints = null;
                lazyLocalPos = PointF.Empty;
                strokeLastPainted = PointF.Empty;
                strokeLastInterpolated = PointF.Empty;
                painter.BeginStroke();
            }
            else
            {
                isDragging = true;
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
                    ImageManipulator.InvalidateCompositeBuffers();
                    RedrawImage();
                }
                else if (ModifierKeys.HasFlag(Keys.Shift))
                {
                    imageOffset.X += dx;
                    imageOffset.Y += dy;
                    ImageManipulator.InvalidateCompositeBuffers();
                    RedrawImage();
                }
                else
                {
                    if (chkListLayers.Items.Count == imageLayers.Count
                        && chkListLayers.SelectedIndex >= 0
                        && chkListLayers.SelectedIndex < imageLayers.Count)
                    {
                        var layer = imageLayers[chkListLayers.SelectedIndex];
                        int dw = layer.image?.Width * layer.ScaleWidth ?? 0;
                        int dh = layer.image?.Height * layer.ScaleHeight ?? 0;

                        Rectangle currentBounds = new(layer.X, layer.Y, dw, dh);
                        _accumulatedDragBounds = _accumulatedDragBounds == Rectangle.Empty
                            ? currentBounds
                            : Rectangle.Union(_accumulatedDragBounds, currentBounds);

                        float ratio = GetCanvasToWorldRatio();
                        layer.X += (int)Math.Round(dx / ratio);
                        layer.Y += (int)Math.Round(dy / ratio);

                        int minPaintIntervalMs = 16;
                        if ((DateTime.Now - lastPaintTime).TotalMilliseconds >= minPaintIntervalMs)
                        {
                            RedrawImage(chkListLayers.SelectedIndex, _accumulatedDragBounds);
                            _accumulatedDragBounds = Rectangle.Empty;
                            lastPaintTime = DateTime.Now;
                        }
                    }
                }
            }
            else if (isPainting)
            {
                float lazySmoothing = 0.22f;

                var selectedLayer = imageLayers[chkListLayers.SelectedIndex];

                Point currentWorldPos = ScreenToWorld(e.Location, ImageManipulator.Width, ImageManipulator.Height);
                Point localCurrentRaw = new(currentWorldPos.X - selectedLayer.X, currentWorldPos.Y - selectedLayer.Y);

                if (strokePoints == null)
                {
                    strokePoints = new List<PointF> { localCurrentRaw };
                    lazyLocalPos = localCurrentRaw;
                    strokeLastPainted = localCurrentRaw;
                    strokeLastInterpolated = localCurrentRaw;
                    lastMousePosition = e.Location;

                    // Start the stroke once at the beginning
                    painter.SetTarget(selectedLayer.image);
                    painter.BeginStroke();
                    return;
                }

                PointF delta = new PointF(
                    localCurrentRaw.X - lazyLocalPos.X,
                    localCurrentRaw.Y - lazyLocalPos.Y);

                lazyLocalPos.X += delta.X * lazySmoothing;
                lazyLocalPos.Y += delta.Y * lazySmoothing;

                strokePoints.Add(lazyLocalPos);

                float aspectRatio = (float)ImageManipulator.Screen.Width / ImageManipulator.Screen.Height;
                float containerAspectRatio = (float)canvas.Width / canvas.Height;
                float scale = 1.0f;
                if (aspectRatio > containerAspectRatio)
                    scale = (float)ImageManipulator.Screen.Width / canvas.Width;
                else if (aspectRatio < containerAspectRatio)
                    scale = (float)ImageManipulator.Screen.Height / canvas.Height;

                float brushPixelSize = 2 * scale * (float)brush_size.Value / brush_size.Maximum;
                float currentOpacity = (float)brush_opacity.Value / brush_opacity.Maximum;

                if (strokePoints.Count >= 2)
                {
                    if (strokePoints.Count == 2)
                    {
                        Point start = Point.Round(strokePoints[0]);
                        Point end = Point.Round(strokePoints[1]);
                        painter.PaintStroke(start, end, brushPixelSize, currentOpacity);
                    }
                    else
                    {
                        int n = strokePoints.Count;
                        PointF p0 = (n >= 3) ? strokePoints[n - 3] : strokePoints[n - 2];
                        PointF p1 = strokePoints[n - 2];
                        PointF p2 = strokePoints[n - 1];
                        PointF p3 = p2;

                        const int segments = 8; // Increased for smoother curves

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
                                painter.PaintStroke(prevRounded, currRounded, brushPixelSize, currentOpacity);
                            }

                            previousPos = pos;
                        }

                        strokeLastInterpolated = previousPos;
                    }
                }

                // Calculate dirty region for redraw
                float radius = brushPixelSize * 1.5f;
                int minX = (int)(Math.Min(strokePoints[^2].X, strokePoints[^1].X) - radius);
                int minY = (int)(Math.Min(strokePoints[^2].Y, strokePoints[^1].Y) - radius);
                int maxX = (int)(Math.Max(strokePoints[^2].X, strokePoints[^1].X) + radius);
                int maxY = (int)(Math.Max(strokePoints[^2].Y, strokePoints[^1].Y) + radius);

                Rectangle dirty = new(minX, minY, maxX - minX, maxY - minY);
                dirty.Intersect(new Rectangle(0, 0, selectedLayer.image?.Width ?? 0, selectedLayer.image?.Height ?? 0));

                if (!dirty.IsEmpty)
                    ImageManipulator.DirtyRegions.Add(dirty);

                lastMousePosition = e.Location;

                // Throttle redraws but DON'T restart the stroke
                const int minPaintIntervalMs = 16;
                if ((DateTime.Now - lastPaintTime).TotalMilliseconds >= minPaintIntervalMs)
                {
                    RedrawImage();
                    lastPaintTime = DateTime.Now;
                }
            }

            lastMousePosition = e.Location;
        }

        private void PixelImage_MouseUp(object sender, MouseEventArgs e)
        {
            isDragging = false;
            isPainting = false;
            _accumulatedDragBounds = Rectangle.Empty;
            painter.EndStroke();
            RedrawImage();
        }

        private float GetCanvasToWorldRatio()
        {
            float aspectRatio = (float)ImageManipulator.Width / ImageManipulator.Height;
            float containerAspectRatio = (float)canvas.Width / canvas.Height;

            float scaledWidth = aspectRatio > containerAspectRatio
                ? canvas.Width * zoom
                : canvas.Height * zoom * aspectRatio;

            return scaledWidth / ImageManipulator.Width;
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

        private void RedrawImage(int dragLayerIndex = -1, Rectangle previousLayerBounds = default)
        {
            //var sw = System.Diagnostics.Stopwatch.StartNew();
            ImageManipulator.PopulateColorGrid(imageLayers, dragLayerIndex, previousLayerBounds);
            //sw.Stop();
            //Console.WriteLine($"PopulateColorGrid: {sw.ElapsedMilliseconds}ms");

            Rectangle dirty;
            if (dragLayerIndex >= 0 && dragLayerIndex < imageLayers.Count)
            {
                var layer = imageLayers[dragLayerIndex];
                int dw = layer.image?.Width * layer.ScaleWidth ?? 0;
                int dh = layer.image?.Height * layer.ScaleHeight ?? 0;
                Rectangle currentBounds = new(layer.X, layer.Y, dw, dh);
                dirty = Rectangle.Union(previousLayerBounds, currentBounds);
                dirty.Intersect(new Rectangle(0, 0, ImageManipulator.Width, ImageManipulator.Height));
            }
            else
            {
                dirty = new Rectangle(0, 0, ImageManipulator.Width, ImageManipulator.Height);
            }

            Bitmap image = ImageManipulator.GetImage(ImageManipulator.Screen, dirty);
            RedrawRasterImage(image);
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
                g.Clear(Color.DimGray); // Use a neutral background for transparency

                // Reuse the logic: Calculate Destination Rectangle
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

                // Quality settings based on zoom
                g.InterpolationMode = (zoom < 0.5f || zoom > 2.0f) ? InterpolationMode.HighQualityBicubic : InterpolationMode.NearestNeighbor;
                g.PixelOffsetMode = PixelOffsetMode.Half; // Prevents "bleeding" edges at pixel level

                g.DrawImage(image, destRect);
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
