using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
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
        private Bitmap? selectedAreaBitmap = null;
        private readonly Matrix transformMatrix = new();


        private readonly GroupBox groupBrushDetail = new();
        private readonly Label lblBrushHardness = new();
        private readonly Label lblBrushSmoothness = new();
        private readonly Label lblBrushOpacity = new();
        private readonly Label lblBrushSize = new();
        private readonly Button btnPenColor = new();
        private readonly Panel panel2 = new();
        private readonly TrackBar brush_size = new();
        private readonly TrackBar brush_opacity = new();
        private readonly Label label10 = new();
        private readonly Label label9 = new();
        private readonly Label label11 = new();
        private readonly Label label8 = new();
        private readonly TrackBar brush_hardness = new();
        private readonly TrackBar brush_smoothness = new();

        private readonly GroupBox groupFillDetail = new();
        private readonly ComboBox cboFillBlendMode = new();
        private readonly Label label5 = new();
        private readonly Label label4 = new();
        private readonly Label label3 = new();
        private readonly NumericUpDown fillOpacity = new();
        private readonly Label label7 = new();
        private readonly Button btnFillColor1 = new();
        private readonly Button btnFillColor = new();
        private readonly ComboBox cboFIllGradient = new();
        private readonly ComboBox comboBox1 = new();

        private readonly GroupBox groupMagicWand = new();
        private readonly TrackBar selectionThreshold = new();
        private readonly Label label12 = new();
        private readonly Label label13 = new();
        private readonly ComboBox cboMWSelectionMode = new();

        [DllImport("user32.dll")]
        public static extern IntPtr CreateIconIndirect(ref IconInfo icon);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetIconInfo(IntPtr hIcon, ref IconInfo pIconInfo);

        public FormMain()
        {
            InitializeComponent();
            InitializeComponentGroupFill();
            InitializeComponentGroupBrush();
            InitializeComponentGroupMagicWand();
            InitializeTimer();
            layersControl.LayerVisibilityChanged += LayersControl_LayerVisibilityChanged;
            layersControl.SelectedLayerChanged += LayersControl_LayerOrderChanged;
            layersControl.LayerCountChanged += LayersControl_LayerCountChanged;
        }

        private void InitializeTimer()
        {
            System.Windows.Forms.Timer animationTimer = new()
            {
                Interval = 100
            };
            animationTimer.Tick += (s, e) =>
            {
                if (ImageSelections.ContainsSelection())
                {
                    _dashOffset += 1.0f;

                    if (_dashOffset > 100.0f) _dashOffset = 0;

                    RedrawImage();
                }
            };
            animationTimer.Start();
        }

        private void InitializeComponentGroupFill()
        {
            groupFillDetail.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)fillOpacity).BeginInit();
            SuspendLayout();
            groupFillDetail.Controls.Add(cboFillBlendMode);
            groupFillDetail.Controls.Add(label5);
            groupFillDetail.Controls.Add(label4);
            groupFillDetail.Controls.Add(label3);
            groupFillDetail.Controls.Add(fillOpacity);
            groupFillDetail.Controls.Add(label7);
            groupFillDetail.Controls.Add(btnFillColor1);
            groupFillDetail.Controls.Add(btnFillColor);
            groupFillDetail.Controls.Add(cboFIllGradient);
            groupFillDetail.Controls.Add(comboBox1);
            groupFillDetail.Location = new Point(12, 74);
            groupFillDetail.Name = "groupFillDetail";
            groupFillDetail.Size = new Size(230, 143);
            groupFillDetail.TabIndex = 29;
            groupFillDetail.TabStop = false;
            groupFillDetail.Text = "Fill Detail";
            groupFillDetail.Visible = false;
            cboFillBlendMode.DropDownStyle = ComboBoxStyle.DropDownList;
            cboFillBlendMode.Font = new Font("Segoe UI", 8.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
            cboFillBlendMode.FormattingEnabled = true;
            cboFillBlendMode.Location = new Point(69, 22);
            cboFillBlendMode.Name = "cboFillBlendMode";
            cboFillBlendMode.Size = new Size(150, 21);
            cboFillBlendMode.TabIndex = 31;
            label5.AutoSize = true;
            label5.Font = new Font("Segoe UI", 8.25F);
            label5.Location = new Point(9, 108);
            label5.Name = "label5";
            label5.Size = new Size(47, 13);
            label5.TabIndex = 29;
            label5.Text = "Pattern:";
            label4.AutoSize = true;
            label4.Font = new Font("Segoe UI", 8.25F);
            label4.Location = new Point(8, 82);
            label4.Name = "label4";
            label4.Size = new Size(38, 13);
            label4.TabIndex = 29;
            label4.Text = "Color:";
            label3.AutoSize = true;
            label3.Font = new Font("Segoe UI", 8.25F);
            label3.Location = new Point(7, 52);
            label3.Name = "label3";
            label3.Size = new Size(49, 13);
            label3.TabIndex = 29;
            label3.Text = "Opacity:";
            fillOpacity.Location = new Point(172, 49);
            fillOpacity.Name = "fillOpacity";
            fillOpacity.Size = new Size(47, 23);
            fillOpacity.TabIndex = 26;
            fillOpacity.Value = new decimal([100, 0, 0, 0]);
            label7.AutoSize = true;
            label7.Font = new Font("Segoe UI", 8.25F);
            label7.Location = new Point(7, 27);
            label7.Name = "label7";
            label7.Size = new Size(40, 13);
            label7.TabIndex = 29;
            label7.Text = "Blend:";
            btnFillColor1.BackColor = Color.White;
            btnFillColor1.Enabled = false;
            btnFillColor1.FlatStyle = FlatStyle.Popup;
            btnFillColor1.ForeColor = Color.Black;
            btnFillColor1.Location = new Point(91, 79);
            btnFillColor1.Name = "btnFillColor1";
            btnFillColor1.Size = new Size(20, 20);
            btnFillColor1.TabIndex = 25;
            btnFillColor1.UseVisualStyleBackColor = false;
            btnFillColor1.Click += BtnFillColor1_Click;
            btnFillColor.BackColor = Color.Black;
            btnFillColor.FlatStyle = FlatStyle.Popup;
            btnFillColor.Location = new Point(69, 79);
            btnFillColor.Name = "btnFillColor";
            btnFillColor.Size = new Size(20, 20);
            btnFillColor.TabIndex = 25;
            btnFillColor.UseVisualStyleBackColor = false;
            btnFillColor.Click += BtnFillColor_Click;
            cboFIllGradient.DropDownStyle = ComboBoxStyle.DropDownList;
            cboFIllGradient.Font = new Font("Segoe UI", 8.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
            cboFIllGradient.FormattingEnabled = true;
            cboFIllGradient.Items.AddRange(["No Gradient", "Linear Gradient", "Radial Gradient"]);
            cboFIllGradient.Location = new Point(114, 78);
            cboFIllGradient.Name = "cboFIllGradient";
            cboFIllGradient.Size = new Size(105, 21);
            cboFIllGradient.TabIndex = 20;
            comboBox1.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBox1.Font = new Font("Segoe UI", 8.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
            comboBox1.FormattingEnabled = true;
            comboBox1.Location = new Point(70, 105);
            comboBox1.Name = "comboBox1";
            comboBox1.Size = new Size(150, 21);
            comboBox1.TabIndex = 20;
            Controls.Add(groupFillDetail);
            groupFillDetail.ResumeLayout(false);
            groupFillDetail.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)fillOpacity).EndInit();
            ResumeLayout(false);
        }

        private void InitializeComponentGroupBrush()
        {
            groupBrushDetail.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)brush_size).BeginInit();
            ((System.ComponentModel.ISupportInitialize)brush_opacity).BeginInit();
            ((System.ComponentModel.ISupportInitialize)brush_hardness).BeginInit();
            ((System.ComponentModel.ISupportInitialize)brush_smoothness).BeginInit();
            SuspendLayout();
            groupBrushDetail.Controls.Add(lblBrushHardness);
            groupBrushDetail.Controls.Add(lblBrushSmoothness);
            groupBrushDetail.Controls.Add(lblBrushOpacity);
            groupBrushDetail.Controls.Add(lblBrushSize);
            groupBrushDetail.Controls.Add(btnPenColor);
            groupBrushDetail.Controls.Add(panel2);
            groupBrushDetail.Controls.Add(brush_size);
            groupBrushDetail.Controls.Add(brush_opacity);
            groupBrushDetail.Controls.Add(label10);
            groupBrushDetail.Controls.Add(label9);
            groupBrushDetail.Controls.Add(label11);
            groupBrushDetail.Controls.Add(label8);
            groupBrushDetail.Controls.Add(brush_hardness);
            groupBrushDetail.Controls.Add(brush_smoothness);
            groupBrushDetail.Location = new Point(12, 74);
            groupBrushDetail.Name = "groupBrushDetail";
            groupBrushDetail.Size = new Size(230, 430);
            groupBrushDetail.TabIndex = 28;
            groupBrushDetail.TabStop = false;
            groupBrushDetail.Text = "Brush Detail";
            groupBrushDetail.Visible = false;
            lblBrushHardness.BackColor = Color.White;
            lblBrushHardness.BorderStyle = BorderStyle.Fixed3D;
            lblBrushHardness.FlatStyle = FlatStyle.Flat;
            lblBrushHardness.Font = new Font("Segoe UI", 8.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
            lblBrushHardness.Location = new Point(188, 349);
            lblBrushHardness.Name = "lblBrushHardness";
            lblBrushHardness.Size = new Size(32, 24);
            lblBrushHardness.TabIndex = 30;
            lblBrushHardness.TextAlign = ContentAlignment.MiddleCenter;
            lblBrushSmoothness.BackColor = Color.White;
            lblBrushSmoothness.BorderStyle = BorderStyle.Fixed3D;
            lblBrushSmoothness.FlatStyle = FlatStyle.Flat;
            lblBrushSmoothness.Font = new Font("Segoe UI", 8.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
            lblBrushSmoothness.Location = new Point(188, 298);
            lblBrushSmoothness.Name = "lblBrushSmoothness";
            lblBrushSmoothness.Size = new Size(32, 24);
            lblBrushSmoothness.TabIndex = 30;
            lblBrushSmoothness.TextAlign = ContentAlignment.MiddleCenter;
            lblBrushOpacity.BackColor = Color.White;
            lblBrushOpacity.BorderStyle = BorderStyle.Fixed3D;
            lblBrushOpacity.FlatStyle = FlatStyle.Flat;
            lblBrushOpacity.Font = new Font("Segoe UI", 8.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
            lblBrushOpacity.Location = new Point(188, 247);
            lblBrushOpacity.Name = "lblBrushOpacity";
            lblBrushOpacity.Size = new Size(32, 24);
            lblBrushOpacity.TabIndex = 30;
            lblBrushOpacity.TextAlign = ContentAlignment.MiddleCenter;
            lblBrushSize.BackColor = Color.White;
            lblBrushSize.BorderStyle = BorderStyle.Fixed3D;
            lblBrushSize.FlatStyle = FlatStyle.Flat;
            lblBrushSize.Font = new Font("Segoe UI", 8.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
            lblBrushSize.Location = new Point(188, 196);
            lblBrushSize.Name = "lblBrushSize";
            lblBrushSize.Size = new Size(32, 24);
            lblBrushSize.TabIndex = 30;
            lblBrushSize.TextAlign = ContentAlignment.MiddleCenter;
            btnPenColor.BackColor = Color.Black;
            btnPenColor.FlatStyle = FlatStyle.Popup;
            btnPenColor.Location = new Point(73, 22);
            btnPenColor.Name = "btnPenColor";
            btnPenColor.Size = new Size(20, 20);
            btnPenColor.TabIndex = 24;
            btnPenColor.UseVisualStyleBackColor = false;
            btnPenColor.Click += BtnPenColor_Click;
            panel2.BackColor = Color.White;
            panel2.BorderStyle = BorderStyle.FixedSingle;
            panel2.Location = new Point(12, 51);
            panel2.Name = "panel2";
            panel2.Size = new Size(155, 139);
            panel2.TabIndex = 22;
            brush_size.Location = new Point(76, 196);
            brush_size.Maximum = 100;
            brush_size.Minimum = 1;
            brush_size.Name = "brush_size";
            brush_size.Size = new Size(106, 45);
            brush_size.TabIndex = 23;
            brush_size.TickStyle = TickStyle.None;
            brush_size.Value = 12;
            brush_size.Scroll += Brush_size_Scroll;
            brush_opacity.Location = new Point(76, 247);
            brush_opacity.Maximum = 100;
            brush_opacity.Name = "brush_opacity";
            brush_opacity.Size = new Size(106, 45);
            brush_opacity.TabIndex = 23;
            brush_opacity.TickStyle = TickStyle.None;
            brush_opacity.Value = 100;
            brush_opacity.Scroll += Brush_opacity_Scroll;
            label10.AutoSize = true;
            label10.Font = new Font("Segoe UI", 8.25F);
            label10.Location = new Point(7, 349);
            label10.Name = "label10";
            label10.Size = new Size(58, 13);
            label10.TabIndex = 29;
            label10.Text = "Hardness:";
            label9.AutoSize = true;
            label9.Font = new Font("Segoe UI", 8.25F);
            label9.Location = new Point(6, 298);
            label9.Name = "label9";
            label9.Size = new Size(73, 13);
            label9.TabIndex = 29;
            label9.Text = "Smoothness:";
            label11.AutoSize = true;
            label11.Font = new Font("Segoe UI", 8.25F);
            label11.Location = new Point(7, 196);
            label11.Name = "label11";
            label11.Size = new Size(30, 13);
            label11.TabIndex = 29;
            label11.Text = "Size:";
            label8.AutoSize = true;
            label8.Font = new Font("Segoe UI", 8.25F);
            label8.Location = new Point(7, 247);
            label8.Name = "label8";
            label8.Size = new Size(49, 13);
            label8.TabIndex = 29;
            label8.Text = "Opacity:";
            brush_hardness.Location = new Point(76, 349);
            brush_hardness.Maximum = 100;
            brush_hardness.Minimum = 1;
            brush_hardness.Name = "brush_hardness";
            brush_hardness.Size = new Size(106, 45);
            brush_hardness.TabIndex = 23;
            brush_hardness.TickStyle = TickStyle.None;
            brush_hardness.Value = 80;
            brush_hardness.Scroll += Brush_hardness_Scroll;
            brush_smoothness.Location = new Point(76, 298);
            brush_smoothness.Maximum = 100;
            brush_smoothness.Name = "brush_smoothness";
            brush_smoothness.Size = new Size(106, 45);
            brush_smoothness.TabIndex = 23;
            brush_smoothness.TickStyle = TickStyle.None;
            brush_smoothness.Value = 22;
            brush_smoothness.Scroll += Brush_smoothness_Scroll;
            Controls.Add(groupBrushDetail);
            groupBrushDetail.ResumeLayout(false);
            groupBrushDetail.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)brush_size).EndInit();
            ((System.ComponentModel.ISupportInitialize)brush_opacity).EndInit();
            ((System.ComponentModel.ISupportInitialize)brush_hardness).EndInit();
            ((System.ComponentModel.ISupportInitialize)brush_smoothness).EndInit();
            ResumeLayout(false);
        }

        private void InitializeComponentGroupMagicWand()
        {
            groupMagicWand.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)selectionThreshold).BeginInit();
            SuspendLayout();
            groupMagicWand.Controls.Add(selectionThreshold);
            groupMagicWand.Controls.Add(label12);
            groupMagicWand.Controls.Add(label13);
            groupMagicWand.Controls.Add(cboMWSelectionMode);
            groupMagicWand.Location = new Point(12, 74);
            groupMagicWand.Name = "groupMagicWand";
            groupMagicWand.Size = new Size(230, 116);
            groupMagicWand.TabIndex = 0;
            groupMagicWand.TabStop = false;
            groupMagicWand.Text = "Magic Wand";
            groupMagicWand.Visible = false;
            selectionThreshold.Location = new Point(74, 65);
            selectionThreshold.Name = "selectionThreshold";
            selectionThreshold.Size = new Size(150, 45);
            selectionThreshold.TabIndex = 2;
            selectionThreshold.TickStyle = TickStyle.None;
            selectionThreshold.Minimum = 1;
            selectionThreshold.Maximum = 100;
            selectionThreshold.Value = 5;
            selectionThreshold.Scroll += SelectionThreshold_Scroll;
            label12.AutoSize = true;
            label12.Location = new Point(6, 65);
            label12.Name = "label12";
            label12.Size = new Size(63, 15);
            label12.TabIndex = 1;
            label12.Text = "Threshold:";
            label13.AutoSize = true;
            label13.Location = new Point(6, 25);
            label13.Name = "label13";
            label13.Size = new Size(57, 15);
            label13.TabIndex = 1;
            label13.Text = "Select By:";
            cboMWSelectionMode.DropDownStyle = ComboBoxStyle.DropDownList;
            cboMWSelectionMode.FormattingEnabled = true;
            cboMWSelectionMode.Items.AddRange(["All", "Red", "Green", "Blue", "Alpha"]);
            cboMWSelectionMode.SelectedIndex = 0;
            cboMWSelectionMode.Location = new Point(74, 22);
            cboMWSelectionMode.Name = "cboMWSelectionMode";
            cboMWSelectionMode.Size = new Size(150, 23);
            cboMWSelectionMode.TabIndex = 0;
            Controls.Add(groupMagicWand);
            groupMagicWand.ResumeLayout(false);
            groupMagicWand.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)selectionThreshold).EndInit();
            ResumeLayout(false);
        }

        private void FormMain_Load(object sender, EventArgs e)
        {
            HistoryManager.RecordState(new HistoryItem(LayersManipulator.Zoom, LayersManipulator.ImageOffset, layersControl.GetLayers(), layersControl.GetSelectedLayerIndex()));
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

            btnMoveUp.Enabled = e.NewIndex > 0;
            btnMoveDown.Enabled = e.NewIndex < layersControl.GetLayers().Count - 1 && e.NewIndex >= 0;

            UpdateControls();

            RedrawImage();
        }

        private void LayersControl_LayerCountChanged(object? sender, LayersCountChangedEventArgs e)
        {
            Console.WriteLine($"Layers count changed from {e.OldCount} to {e.NewCount}.");

            btnMoveUp.Enabled = layersControl.GetSelectedLayerIndex() > 0;
            btnMoveDown.Enabled = layersControl.GetSelectedLayerIndex() < layersControl.GetLayers().Count - 1 && layersControl.GetSelectedLayerIndex() >= 0;
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

        private void BtnPenColor_Click(object? sender, EventArgs e)
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

        private void BtnFillColor_Click(object? sender, EventArgs e)
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

        private void BtnFillColor1_Click(object? sender, EventArgs e)
        {
            ColorDialog c = new()
            {
                Color = btnFillColor.BackColor
            };
            if (c.ShowDialog() == DialogResult.OK)
            {
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
                    groupMagicWand.Visible = false;
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
                groupMagicWand.Visible = true;
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
                        else if (sourcePixel.A > 0)
                        {
                            bitmap.SetPixel(x, y, paint.GetFillColor());
                        }
                    }
                }
                canvas.Cursor = GetCursor(new Bitmap(bitmap, 24, 24), 0, 0);
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

                canvas.Cursor = GetCursor(new Bitmap(paint.Brush, cursorWidth, cursorHeight), hotSpotX, hotSpotY);
            }
            else
            {
                canvas.Cursor = Cursors.Default;
            }
        }

        private void UpdateCursor(Point mousePosition)
        {
            if (IsOverRotationHandle(mousePosition, LayersManipulator.Width, LayersManipulator.Height, canvas.Width, canvas.Height, LayersManipulator.Zoom))
            {
                canvas.Cursor = Cursors.Hand;
            }
            else if (IsOverScaleHandle(mousePosition, canvas.Width, canvas.Height, out string handle))
            {
                canvas.Cursor = GetCursor(handle);
            }
            else
            {
            }
        }

        private static Cursor GetCursor(Bitmap bmp, int xHotSpot, int yHotSpot)
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

        private static Cursor GetCursor(string handle)
        {
            return handle switch
            {
                "topLeft" or "bottomRight" => Cursors.SizeNWSE,
                "topRight" or "bottomLeft" => Cursors.SizeNESW,
                _ => Cursors.SizeAll,
            };
        }

        private void Brush_size_Scroll(object? sender, EventArgs e)
        {
            lblBrushSize.Text = $"{brush_size.Value}";
            UpdateCursor();
        }

        private void Brush_opacity_Scroll(object? sender, EventArgs e)
        {
            lblBrushOpacity.Text = $"{brush_opacity.Value}";
        }

        private void Brush_smoothness_Scroll(object? sender, EventArgs e)
        {
            lblBrushSmoothness.Text = $"{brush_smoothness.Value}";
        }

        private void Brush_hardness_Scroll(object? sender, EventArgs e)
        {
            lblBrushHardness.Text = $"{brush_hardness.Value}";
            if (paint.Brush != null)
            {
                paint.Reset(btnPenColor.BackColor, paint.GetRadius() * (brush_hardness.Maximum - brush_hardness.Value) / brush_hardness.Maximum);
                PaintingEngine.SetBrush(paint);
                UpdateCursor();
            }
        }

        private void SelectionThreshold_Scroll(object? sender, EventArgs e)
        {
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            if (WindowState != FormWindowState.Minimized)
            {
                canvas.Size = new Size(ClientSize.Width - canvas.Location.X - 240, ClientSize.Height - canvas.Location.Y - 40);
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

                        var layer = new Layer($"layer {layersControl.GetLayers().Count + 1}", true)
                        {
                            Image = new Bitmap(image, image.Width, image.Height),
                            X = LayersManipulator.Width / 2 - image.Width / 2,
                            Y = LayersManipulator.Height / 2 - image.Height / 2
                        };
                        layersControl.InsertLayer(0, layer);

                        RedrawImage();
                    }
                    image.Dispose();
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
            ofd.Filter = "Paint++ Files (*.xpe)|*.xpe";

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    HistoryManager.Clear();

                    using FileStream fs = File.Open(ofd.FileName, FileMode.Open);

                    XPELoader.Load(fs, out var layers, out int selectedLayerIndex);

                    layersControl.ClearLayers();
                    layersControl.AddRange(layers);
                    layersControl.SetSelectedLayerIndex(selectedLayerIndex);

                    currentFilePath = ofd.FileName;
                    isDirty = false;
                    UpdateTitleBar();
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
                sfd.Filter = "Paint++ Files (*.xpe)|*.xpe";

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
            sfd.Filter = "Paint++ Files (*.xpe)|*.xpe";

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
                XPESaver.Save(fs, layersControl.GetLayers(), layersControl.GetSelectedLayerIndex());
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

            if (displayName.EndsWith(".xpe", StringComparison.OrdinalIgnoreCase) && displayName.ToLower().LastIndexOf(".xpe") > 0)
            {
                displayName = displayName[..displayName.LastIndexOf(".xpe")];
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
                    sfd.Filter = "Paint++ Files (*.xpe)|*.xpe";
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

        private void BthUpdateLayer_Click(object sender, EventArgs e)
        {
            FormLayer frm = new()
            {
                StartPosition = FormStartPosition.CenterParent
            };

            var selectedLayer = layersControl.GetLayer(layersControl.GetSelectedLayerIndex());

            if (selectedLayer != null)
            {
                frm.Layer = selectedLayer;
                if (frm.ShowDialog() == DialogResult.OK)
                {
                    HistoryManager.RecordState(new HistoryItem(LayersManipulator.Zoom, LayersManipulator.ImageOffset, layersControl.GetLayers(), layersControl.GetSelectedLayerIndex()));
                    layersControl.UpdateLayer(layersControl.GetSelectedLayerIndex(), frm.Layer);
                    RedrawImage();
                }
            }
        }

        private void BtnAddLayer_Click(object sender, EventArgs e)
        {
            FormLayer frm = new()
            {
                Layer = new Layer($"layer {layersControl.GetLayers().Count + 1}", true),
                StartPosition = FormStartPosition.CenterParent
            };

            if (frm.ShowDialog(this) == DialogResult.OK)
            {
                HistoryManager.RecordState(new HistoryItem(LayersManipulator.Zoom, LayersManipulator.ImageOffset, layersControl.GetLayers(), layersControl.GetSelectedLayerIndex()));
                layersControl.InsertLayer(0, frm.Layer);
                RedrawImage();
            }
        }

        private void BtnSubtractLayer_Click(object sender, EventArgs e)
        {
            HistoryManager.RecordState(new HistoryItem(LayersManipulator.Zoom, LayersManipulator.ImageOffset, layersControl.GetLayers(), layersControl.GetSelectedLayerIndex()));

            layersControl.RemoveLayerAt(layersControl.GetSelectedLayerIndex());
            RedrawImage();
        }

        private void BtnMoveUp_Click(object sender, EventArgs e)
        {
            int currentIndex = layersControl.GetSelectedLayerIndex();

            var layerToMove = layersControl.GetLayer(currentIndex);

            if (layerToMove != null)
            {
                HistoryManager.RecordState(new HistoryItem(LayersManipulator.Zoom, LayersManipulator.ImageOffset, layersControl.GetLayers(), layersControl.GetSelectedLayerIndex()));

                layersControl.MoveLayerUp();

                RedrawImage();
            }
        }

        private void BtnMoveDown_Click(object sender, EventArgs e)
        {
            int currentIndex = layersControl.GetSelectedLayerIndex();

            var layerToMove = layersControl.GetLayer(currentIndex);

            if (layerToMove != null)
            {
                HistoryManager.RecordState(new HistoryItem(LayersManipulator.Zoom, LayersManipulator.ImageOffset, layersControl.GetLayers(), layersControl.GetSelectedLayerIndex()));

                layersControl.MoveLayerDown();

                RedrawImage();
            }
        }

        private void MoveToTopToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int currentIndex = layersControl.GetSelectedLayerIndex();

            var layerToMove = layersControl.GetLayer(currentIndex);

            if (layerToMove != null)
            {
                HistoryManager.RecordState(new HistoryItem(LayersManipulator.Zoom, LayersManipulator.ImageOffset, layersControl.GetLayers(), layersControl.GetSelectedLayerIndex()));

                layersControl.MoveLayerToTop();

                RedrawImage();
            }
        }

        private void MoveToBottomToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int currentIndex = layersControl.GetSelectedLayerIndex();

            var layerToMove = layersControl.GetLayer(currentIndex);

            if (layerToMove != null)
            {
                HistoryManager.RecordState(new HistoryItem(LayersManipulator.Zoom, LayersManipulator.ImageOffset, layersControl.GetLayers(), layersControl.GetSelectedLayerIndex()));

                layersControl.MoveLayerToBottom();

                RedrawImage();
            }
        }

        private void BtnShowLayer_Click(object sender, EventArgs e)
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

        private void BtnHideLayer_Click(object sender, EventArgs e)
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

        private void BtnDuplicate_Click(object sender, EventArgs e)
        {
            int selectedIndex = layersControl.GetSelectedLayerIndex();

            var layer = layersControl.GetLayer(selectedIndex);
            if (layer != null)
            {
                HistoryManager.RecordState(new HistoryItem(LayersManipulator.Zoom, LayersManipulator.ImageOffset, layersControl.GetLayers(), layersControl.GetSelectedLayerIndex()));

                Layer clone = layer.Clone();
                clone.Name += "_Clone";
                layersControl.InsertLayer(selectedIndex + 1, clone);

                UpdateControls();

                RedrawImage();
            }
        }

        private void BtnMergeDown_Click(object sender, EventArgs e)
        {
            if (layersControl.GetLayers().Count < 2)
                return;

            int selectedIndex = layersControl.GetSelectedLayerIndex();

            if (selectedIndex < 0 || selectedIndex >= layersControl.GetLayers().Count - 1)
                return;

            var top = layersControl.GetLayer(selectedIndex);
            var bottom = layersControl.GetLayer(selectedIndex + 1);
            if (top != null && bottom != null)
            {
                HistoryManager.RecordState(new HistoryItem(LayersManipulator.Zoom, LayersManipulator.ImageOffset, layersControl.GetLayers(), layersControl.GetSelectedLayerIndex()));

                top = LayersManipulator.MergeLayers(top, bottom);
                layersControl.UpdateLayer(selectedIndex, top);
                layersControl.RemoveLayerAt(selectedIndex + 1);

                UpdateControls();

                RedrawImage();
            }
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
            var selectedLayer = layersControl.GetLayer(layersControl.GetSelectedLayerIndex());
            if (selectedLayer != null)
            {
                allToolStripMenuItem.Checked = !allToolStripMenuItem.Checked;
                redToolStripMenuItem.Checked = false;
                greenToolStripMenuItem.Checked = false;
                blueToolStripMenuItem.Checked = false;
                selectedLayer.Channel = LayerChannel.RGB;
                LayersManipulator.DirtyRegions.Add(new(selectedLayer.X, selectedLayer.Y, selectedLayer.Image?.Width ?? 0, selectedLayer.Image?.Height ?? 0));
                RedrawImage();
            }
        }

        private void RedToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var selectedLayer = layersControl.GetLayer(layersControl.GetSelectedLayerIndex());
            if (selectedLayer != null)
            {
                allToolStripMenuItem.Checked = false;
                redToolStripMenuItem.Checked = !redToolStripMenuItem.Checked;
                greenToolStripMenuItem.Checked = false;
                blueToolStripMenuItem.Checked = false;
                selectedLayer.Channel = LayerChannel.Red;
                LayersManipulator.DirtyRegions.Add(new(selectedLayer.X, selectedLayer.Y, selectedLayer.Image?.Width ?? 0, selectedLayer.Image?.Height ?? 0));
                RedrawImage();
            }
        }

        private void GreenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var selectedLayer = layersControl.GetLayer(layersControl.GetSelectedLayerIndex());
            if (selectedLayer != null)
            {
                allToolStripMenuItem.Checked = false;
                redToolStripMenuItem.Checked = false;
                greenToolStripMenuItem.Checked = !greenToolStripMenuItem.Checked;
                blueToolStripMenuItem.Checked = false;
                selectedLayer.Channel = LayerChannel.Green;
                LayersManipulator.DirtyRegions.Add(new(selectedLayer.X, selectedLayer.Y, selectedLayer.Image?.Width ?? 0, selectedLayer.Image?.Height ?? 0));
                RedrawImage();
            }
        }

        private void BlueToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var selectedLayer = layersControl.GetLayer(layersControl.GetSelectedLayerIndex());
            if (selectedLayer != null)
            {
                allToolStripMenuItem.Checked = false;
                redToolStripMenuItem.Checked = false;
                greenToolStripMenuItem.Checked = false;
                blueToolStripMenuItem.Checked = !blueToolStripMenuItem.Checked;
                selectedLayer.Channel = LayerChannel.Blue;
                LayersManipulator.DirtyRegions.Add(new(selectedLayer.X, selectedLayer.Y, selectedLayer.Image?.Width ?? 0, selectedLayer.Image?.Height ?? 0));
                RedrawImage();
            }
        }

        private void RdoAll_CheckedChanged(object sender, EventArgs e)
        {
            if (rdoAll.Checked)
            {
                allToolStripMenuItem.Checked = true;
                redToolStripMenuItem.Checked = false;
                greenToolStripMenuItem.Checked = false;
                blueToolStripMenuItem.Checked = false;
                var selectedLayer = layersControl.GetLayer(layersControl.GetSelectedLayerIndex());
                if (selectedLayer != null)
                {
                    selectedLayer.Channel = LayerChannel.RGB;
                    LayersManipulator.DirtyRegions.Add(new(selectedLayer.X, selectedLayer.Y, selectedLayer.Image?.Width ?? 0, selectedLayer.Image?.Height ?? 0));
                    RedrawImage();
                }
            }
            else if (rdoRed.Checked)
            {
                allToolStripMenuItem.Checked = false;
                redToolStripMenuItem.Checked = true;
                greenToolStripMenuItem.Checked = false;
                blueToolStripMenuItem.Checked = false;
                var selectedLayer = layersControl.GetLayer(layersControl.GetSelectedLayerIndex());
                if (selectedLayer != null)
                {
                    selectedLayer.Channel = LayerChannel.Red;
                    LayersManipulator.DirtyRegions.Add(new(selectedLayer.X, selectedLayer.Y, selectedLayer.Image?.Width ?? 0, selectedLayer.Image?.Height ?? 0));
                    RedrawImage();
                }
            }
            else if (rdoGreen.Checked)
            {
                allToolStripMenuItem.Checked = false;
                redToolStripMenuItem.Checked = false;
                greenToolStripMenuItem.Checked = true;
                blueToolStripMenuItem.Checked = false;
                var selectedLayer = layersControl.GetLayer(layersControl.GetSelectedLayerIndex());
                if (selectedLayer != null)
                {
                    selectedLayer.Channel = LayerChannel.Green;
                    LayersManipulator.DirtyRegions.Add(new(selectedLayer.X, selectedLayer.Y, selectedLayer.Image?.Width ?? 0, selectedLayer.Image?.Height ?? 0));
                    RedrawImage();
                }
            }
            else if (rdoBlue.Checked)
            {
                allToolStripMenuItem.Checked = false;
                redToolStripMenuItem.Checked = false;
                greenToolStripMenuItem.Checked = false;
                blueToolStripMenuItem.Checked = true;
                var selectedLayer = layersControl.GetLayer(layersControl.GetSelectedLayerIndex());
                if (selectedLayer != null)
                {
                    selectedLayer.Channel = LayerChannel.Blue;
                    LayersManipulator.DirtyRegions.Add(new(selectedLayer.X, selectedLayer.Y, selectedLayer.Image?.Width ?? 0, selectedLayer.Image?.Height ?? 0));
                    RedrawImage();
                }
            }
        }

        private void RedToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            redToolStripMenuItem1.Checked = !redToolStripMenuItem1.Checked;
        }

        private void GreenToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            greenToolStripMenuItem1.Checked = !greenToolStripMenuItem1.Checked;
        }

        private void BlueToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            blueToolStripMenuItem1.Checked = !blueToolStripMenuItem1.Checked;
        }

        private void ChkFilter1_CheckedChanged(object sender, EventArgs e)
        {
            if (redToolStripMenuItem1.Checked != chkRed.Checked)
            {
                redToolStripMenuItem1.Checked = chkRed.Checked;
            }
            if (greenToolStripMenuItem1.Checked != chkGreen.Checked)
            {
                greenToolStripMenuItem1.Checked = chkGreen.Checked;
            }
            if (blueToolStripMenuItem1.Checked != chkBlue.Checked)
            {
                blueToolStripMenuItem1.Checked = chkBlue.Checked;
            }
        }

        private void ChkFilter_CheckedChanged(object sender, EventArgs e)
        {
            var selectedLayer = layersControl.GetLayer(layersControl.GetSelectedLayerIndex());
            if (selectedLayer != null)
            {
                selectedLayer.RedFilter = redToolStripMenuItem1.Checked;
                selectedLayer.GreenFilter = greenToolStripMenuItem1.Checked;
                selectedLayer.BlueFilter = blueToolStripMenuItem1.Checked;
                LayersManipulator.DirtyRegions.Add(new(selectedLayer.X, selectedLayer.Y, selectedLayer.Image?.Width ?? 0, selectedLayer.Image?.Height ?? 0));
                RedrawImage();
            }
        }

        private void DeleteImageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var selectedLayer = layersControl.GetLayer(layersControl.GetSelectedLayerIndex());
            if (selectedLayer != null)
            {
                if (ImageSelections.ContainsSelection())
                {
                    HistoryManager.RecordState(new HistoryItem(LayersManipulator.Zoom, LayersManipulator.ImageOffset, layersControl.GetLayers(), layersControl.GetSelectedLayerIndex()));
                    ImageSelections.CalculateSelectionBounds();
                    selectedLayer.Image = CutSelectionFromLayer(selectedLayer);
                    rotationAngle = 0;
                    scaleFactor = 1.0f;
                    ImageSelections.ClearSelections();
                    RedrawImage();
                }
                else
                {
                    HistoryManager.RecordState(new HistoryItem(LayersManipulator.Zoom, LayersManipulator.ImageOffset, layersControl.GetLayers(), layersControl.GetSelectedLayerIndex()));
                    int width = selectedLayer.Image != null ? selectedLayer.Image.Width : LayersManipulator.Width;
                    int height = selectedLayer.Image != null ? selectedLayer.Image.Height : LayersManipulator.Height;
                    selectedLayer.Image?.Dispose();
                    selectedLayer.Image = LayersManipulator.GetImage(Color.Transparent, width, height);
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
                if (ImageSelections.ContainsSelection())
                {
                    HistoryManager.RecordState(new HistoryItem(LayersManipulator.Zoom, LayersManipulator.ImageOffset, layersControl.GetLayers(), layersControl.GetSelectedLayerIndex()));
                    ImageSelections.CalculateSelectionBounds();
                    Image? tempImage = ExtractSelectedArea(selectedLayer);
                    selectedLayer.Image = CutSelectionFromLayer(selectedLayer);
                    if (tempImage != null)
                    {
                        Clipboard.SetImage(tempImage);
                        ImageSelections.ClearSelections();
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
                if (ImageSelections.ContainsSelection())
                {
                    HistoryManager.RecordState(new HistoryItem(LayersManipulator.Zoom, LayersManipulator.ImageOffset, layersControl.GetLayers(), layersControl.GetSelectedLayerIndex()));
                    ImageSelections.CalculateSelectionBounds();
                    Image? tempImage = ExtractSelectedArea(selectedLayer);
                    if (tempImage != null)
                    {
                        Clipboard.SetImage(tempImage);
                        ImageSelections.ClearSelections();
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
                    HistoryManager.RecordState(new HistoryItem(LayersManipulator.Zoom, LayersManipulator.ImageOffset, layersControl.GetLayers(), layersControl.GetSelectedLayerIndex()));

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
                    HistoryManager.RecordState(new HistoryItem(LayersManipulator.Zoom, LayersManipulator.ImageOffset, layersControl.GetLayers(), layersControl.GetSelectedLayerIndex()));

                    int imgW = selectedLayer.Image.Width;
                    int imgH = selectedLayer.Image.Height;
                    int x0 = selectedLayer.X;
                    int y0 = selectedLayer.Y;

                    ImageSelections.AddSelectionPoint(new Point(x0, y0));
                    ImageSelections.AddSelectionPoint(new Point(x0, y0 + imgH));
                    ImageSelections.AddSelectionPoint(new Point(x0 + imgW, y0 + imgH));
                    ImageSelections.AddSelectionPoint(new Point(x0 + imgW, y0));
                    ImageSelections.AddSelectionPoint(new Point(x0, y0));
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
                formSettings.LayerX = selectedLayer.X;
                formSettings.LayerY = selectedLayer.Y;
                if (formSettings.ShowDialog(this) == DialogResult.OK)
                {
                    HistoryManager.RecordState(new HistoryItem(LayersManipulator.Zoom, LayersManipulator.ImageOffset, layersControl.GetLayers(), layersControl.GetSelectedLayerIndex()));
                    selectedLayer.X = formSettings.LayerX;
                    selectedLayer.Y = formSettings.LayerY;
                    selectedLayer.Image = (selectedLayer.Image != null) ?
                        LayersManipulator.CropFromCenter(selectedLayer.Image, formSettings.LayerWidth, formSettings.LayerHeight) :
                        LayersManipulator.GetImage(selectedLayer.FillType == FillType.Transparency ? Color.Transparent : selectedLayer.FillColor, formSettings.LayerWidth, formSettings.LayerHeight);
                    RedrawImage();
                }
            }
        }

        private void DarkToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var selectedLayer = layersControl.GetLayer(layersControl.GetSelectedLayerIndex());
            if (selectedLayer != null)
            {
                HistoryManager.RecordState(new HistoryItem(LayersManipulator.Zoom, LayersManipulator.ImageOffset, layersControl.GetLayers(), layersControl.GetSelectedLayerIndex()));
                bool[,] mask = LayersManipulator.GetDarkPixels(LayersManipulator.Screen, 0.0f, 1.0f);
                ColorGrid grid = LayersManipulator.DarkPixelGrid(mask, LayersManipulator.Width, LayersManipulator.Height);
                selectedLayer.Image = new Bitmap(LayersManipulator.GetImage(grid));
                RedrawImage();
            }
        }

        private void InvertToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var selectedLayer = layersControl.GetLayer(layersControl.GetSelectedLayerIndex());
            if (selectedLayer != null)
            {
                if (selectedLayer.Image != null)
                {
                    HistoryManager.RecordState(new HistoryItem(LayersManipulator.Zoom, LayersManipulator.ImageOffset, layersControl.GetLayers(), layersControl.GetSelectedLayerIndex()));
                    Bitmap inverted = LayersManipulator.InvertColors((Bitmap)selectedLayer.Image);
                    selectedLayer.Image.Dispose();
                    selectedLayer.Image = inverted;
                    RedrawImage();
                }
            }
        }

        private void LightingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var selectedLayer = layersControl.GetLayer(layersControl.GetSelectedLayerIndex());
            if (selectedLayer != null)
            {
                if (selectedLayer.Image != null)
                {
                    FormLighting frm = new()
                    {
                        Image = selectedLayer.Image
                    };
                    if (frm.ShowDialog() == DialogResult.OK && frm.Image != null)
                    {
                        HistoryManager.RecordState(new HistoryItem(LayersManipulator.Zoom, LayersManipulator.ImageOffset, layersControl.GetLayers(), layersControl.GetSelectedLayerIndex()));
                        Bitmap result = new(frm.Image);
                        selectedLayer.Image.Dispose();
                        selectedLayer.Image = result;
                        RedrawImage();
                    }
                }
            }
        }

        private void SaturationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var selectedLayer = layersControl.GetLayer(layersControl.GetSelectedLayerIndex());
            if (selectedLayer != null)
            {
                if (selectedLayer.Image != null)
                {
                    FormColorBalance frm = new()
                    {
                        Image = selectedLayer.Image
                    };
                    if (frm.ShowDialog() == DialogResult.OK && frm.Image != null)
                    {
                        HistoryManager.RecordState(new HistoryItem(LayersManipulator.Zoom, LayersManipulator.ImageOffset, layersControl.GetLayers(), layersControl.GetSelectedLayerIndex()));
                        Bitmap result = new(frm.Image);
                        selectedLayer.Image.Dispose();
                        selectedLayer.Image = result;
                        RedrawImage();
                    }
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
                    FormBlur frm = new()
                    {
                        Image = selectedLayer.Image
                    };
                    if (frm.ShowDialog() == DialogResult.OK && frm.Image != null)
                    {
                        HistoryManager.RecordState(new HistoryItem(LayersManipulator.Zoom, LayersManipulator.ImageOffset, layersControl.GetLayers(), layersControl.GetSelectedLayerIndex()));
                        Bitmap blurred = new(frm.Image);
                        selectedLayer.Image.Dispose();
                        selectedLayer.Image = blurred;
                        RedrawImage();
                    }
                }
            }
        }

        private void SharpnessToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var selectedLayer = layersControl.GetLayer(layersControl.GetSelectedLayerIndex());
            if (selectedLayer != null)
            {
                if (selectedLayer.Image != null)
                {
                    FormSharpness frm = new()
                    {
                        Image = selectedLayer.Image
                    };
                    if (frm.ShowDialog() == DialogResult.OK && frm.Image != null)
                    {
                        HistoryManager.RecordState(new HistoryItem(LayersManipulator.Zoom, LayersManipulator.ImageOffset, layersControl.GetLayers(), layersControl.GetSelectedLayerIndex()));
                        Bitmap blurred = new(frm.Image);
                        selectedLayer.Image.Dispose();
                        selectedLayer.Image = blurred;
                        RedrawImage();
                    }
                }
            }
        }

        private void UpdateTransformMatrix()
        {
            PointF screenCenter = GetSelectionCenterScreen(canvas.Width, canvas.Height);

            transformMatrix.Reset();
            transformMatrix.Translate(-screenCenter.X, -screenCenter.Y, MatrixOrder.Append);
            transformMatrix.Rotate(rotationAngle, MatrixOrder.Append);
            transformMatrix.Scale(scaleFactor, scaleFactor, MatrixOrder.Append);
            transformMatrix.Translate(screenCenter.X, screenCenter.Y, MatrixOrder.Append);
        }

        private static Bitmap? ExtractSelectedArea(Layer selectedLayer)
        {
            if (selectedLayer.Image == null) return null;

            RectangleF worldBounds = ImageSelections.GetSelectionBounds();

            int srcX = (int)worldBounds.X - selectedLayer.X;
            int srcY = (int)worldBounds.Y - selectedLayer.Y;
            int srcW = (int)worldBounds.Width;
            int srcH = (int)worldBounds.Height;

            Bitmap result = new(srcW, srcH);

            using Graphics g = Graphics.FromImage(result);

            var selectionPoints = ImageSelections.GetSelections();
            for (int i = 0; i < selectionPoints.Count; i++)
            {
                if (selectionPoints[i].Points.Count < 3) continue;

                using GraphicsPath path = new();
                PointF[] localPoints = [.. selectionPoints[i].Points.Select(p =>
                    new PointF(p.X - selectedLayer.X - srcX, p.Y - selectedLayer.Y - srcY))];

                path.AddPolygon(localPoints);
                g.SetClip(path);

                g.DrawImage(selectedLayer.Image,
                    new Rectangle(0, 0, srcW, srcH),
                    new Rectangle(srcX, srcY, srcW, srcH),
                    GraphicsUnit.Pixel);
            }

            return result;
        }

        private Bitmap? CutSelectionFromLayer(Layer selectedLayer, bool emptyHole = true)
        {
            if (layersControl.GetSelectedLayerIndex() < 0 || selectedLayer.Image == null) return null;

            int width = selectedLayer.Image.Width;
            int height = selectedLayer.Image.Height;
            Bitmap result = new(selectedLayer.Image);

            byte fillA = emptyHole ? (byte)0 : (byte)255;
            byte fillR = 255; byte fillG = 255; byte fillB = 255;

            var selections = ImageSelections.GetSelections();
            var polygons = selections.Where(s => s.Points.Count >= 3).ToList();

            BitmapData data = result.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadWrite, PixelFormat.Format32bppPArgb);
            int stride = data.Stride;
            IntPtr ptr = data.Scan0;

            try
            {
                Parallel.For(0, height, y =>
                {
                    int canvasY = y + selectedLayer.Y;
                    var cutIntervals = new List<(int Start, int End)>();

                    foreach (var poly in polygons)
                    {
                        bool isHole = false;
                        foreach (var other in polygons)
                        {
                            if (other == poly) continue;
                            if (IsPolygonContained(poly, other))
                            {
                                isHole = !isHole;
                            }
                        }

                        var intervals = GetScanlineIntervals(new List<SelectionPolygon> { poly }, canvasY, selectedLayer.X, width);

                        if (!isHole)
                        {
                            cutIntervals.AddRange(intervals);
                        }
                    }

                    cutIntervals = MergeIntervals(cutIntervals);

                    var holeIntervals = new List<(int Start, int End)>();
                    foreach (var poly in polygons)
                    {
                        bool isHole = false;
                        foreach (var other in polygons)
                        {
                            if (other == poly) continue;
                            if (IsPolygonContained(poly, other))
                            {
                                isHole = !isHole;
                            }
                        }

                        if (isHole)
                        {
                            var intervals = GetScanlineIntervals(new List<SelectionPolygon> { poly }, canvasY, selectedLayer.X, width);
                            holeIntervals.AddRange(intervals);
                        }
                    }

                    holeIntervals = MergeIntervals(holeIntervals);

                    unsafe
                    {
                        byte* row = (byte*)ptr + (y * stride);
                        for (int x = 0; x < width; x++)
                        {
                            bool inCut = IsInIntervals(cutIntervals, x);
                            bool inHole = IsInIntervals(holeIntervals, x);

                            if (inCut && !inHole)
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

        private List<(int Start, int End)> MergeIntervals(List<(int Start, int End)> intervals)
        {
            if (intervals.Count <= 1) return intervals;

            var sorted = intervals.OrderBy(i => i.Start).ToList();
            var merged = new List<(int Start, int End)> { sorted[0] };

            for (int i = 1; i < sorted.Count; i++)
            {
                var last = merged[merged.Count - 1];
                var current = sorted[i];

                if (current.Start <= last.End + 1)
                {
                    merged[merged.Count - 1] = (last.Start, Math.Max(last.End, current.End));
                }
                else
                {
                    merged.Add(current);
                }
            }

            return merged;
        }

        private bool IsPolygonContained(SelectionPolygon inner, SelectionPolygon outer)
        {
            var randomPoint = inner.Points[inner.Points.Count / 2];
            return IsPointInPolygon(randomPoint, outer.Points);
        }

        private bool IsPointInPolygon(Point point, List<Point> polygon)
        {
            bool result = false;
            int j = polygon.Count - 1;
            for (int i = 0; i < polygon.Count; i++)
            {
                if ((polygon[i].Y < point.Y && polygon[j].Y >= point.Y) ||
                    (polygon[j].Y < point.Y && polygon[i].Y >= point.Y))
                {
                    float xIntersection = polygon[i].X +
                        ((float)(point.Y - polygon[i].Y) / (polygon[j].Y - polygon[i].Y)) *
                        (polygon[j].X - polygon[i].X);

                    if (xIntersection < point.X)
                    {
                        result = !result;
                    }
                }
                j = i;
            }
            return result;
        }

        private static List<(int Start, int End)> GetScanlineIntervals(List<SelectionPolygon> polygons, int y, int layerX, int maxW)
        {
            var intervals = new List<(int, int)>();
            foreach (var poly in polygons)
            {
                var xIntersections = new List<int>();
                for (int i = 0; i < poly.Points.Count; i++)
                {
                    Point p1 = poly.Points[i];
                    Point p2 = poly.Points[(i + 1) % poly.Points.Count];

                    if ((p1.Y <= y && p2.Y > y) || (p2.Y <= y && p1.Y > y))
                    {
                        if (p2.Y == p1.Y) continue;
                        float x = p1.X + (float)(y - p1.Y) / (p2.Y - p1.Y) * (p2.X - p1.X);
                        xIntersections.Add((int)x - layerX);
                    }
                }
                xIntersections.Sort();
                for (int i = 0; i < xIntersections.Count - 1; i += 2)
                {
                    int start = Math.Max(0, xIntersections[i]);
                    int end = Math.Min(maxW - 1, xIntersections[i + 1]);
                    if (start <= end)
                    {
                        intervals.Add((start, end));
                    }
                }
            }
            return intervals;
        }

        private static bool IsInIntervals(List<(int Start, int End)> intervals, int x)
        {
            foreach (var interval in intervals)
            {
                if (x >= interval.Start && x <= interval.End) return true;
            }
            return false;
        }

        private Bitmap? MergeSelectionToLayer(Layer selectedLayer)
        {
            if (selectedLayer.Image == null || selectedAreaBitmap == null) return null;

            Bitmap result = new(selectedLayer.Image);
            using (Graphics g = Graphics.FromImage(result))
            {
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.SmoothingMode = SmoothingMode.AntiAlias;

                RectangleF worldBounds = ImageSelections.GetSelectionBounds();

                float localX = worldBounds.X - selectedLayer.X;
                float localY = worldBounds.Y - selectedLayer.Y;

                using Matrix layerMatrix = new();
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
            ImageSelections.ClearSelections();

            return result;
        }

        private float CalculateScaleFactor(Point mousePosition)
        {
            Point worldMouse = LayersManipulator.ScreenToWorld(mousePosition, canvas.Width, canvas.Height);
            float currentDistance = ImageSelections.Distance(worldMouse, ImageSelections.GetSelectionCenter());

            if (initialScaleDistance > 0)
            {
                return (currentDistance / initialScaleDistance) * initialScaleFactor;
            }
            return 1.0f;
        }

        public static bool IsOverRotationHandle(Point screenPoint, int worldWidth, int worldHeight, int canvasWidth, int canvasHeight, float zoom)
        {
            PointF worldHandle = new(
                ImageSelections.GetSelectionCenter().X,
                ImageSelections.GetSelectionBounds().Y - ImageSelections.ROTATION_HANDLE_SIZE / ImageSelections.GetScreenToWorldScale(worldWidth, worldHeight, canvasWidth, canvasHeight, zoom)
            );
            Point screenHandle = LayersManipulator.WorldToScreen(Point.Round(worldHandle), canvasWidth, canvasHeight);
            float distance = ImageSelections.Distance(screenPoint, screenHandle);
            return distance < ImageSelections.ROTATION_HANDLE_SIZE;
        }

        public static bool IsOverScaleHandle(Point screenPoint, int canvasWidth, int canvasHeight, out string handle)
        {
            handle = "";

            var corners = new[]
            {
                new { Name = "topLeft",     World = new PointF(ImageSelections.GetSelectionBounds().X,     ImageSelections.GetSelectionBounds().Y) },
                new { Name = "topRight",    World = new PointF(ImageSelections.GetSelectionBounds().Right, ImageSelections.GetSelectionBounds().Y) },
                new { Name = "bottomLeft",  World = new PointF(ImageSelections.GetSelectionBounds().X,     ImageSelections.GetSelectionBounds().Bottom) },
                new { Name = "bottomRight", World = new PointF(ImageSelections.GetSelectionBounds().Right, ImageSelections.GetSelectionBounds().Bottom) }
            };

            foreach (var corner in corners)
            {
                Point screenCorner = LayersManipulator.WorldToScreen(Point.Round(corner.World), canvasWidth, canvasHeight);
                if (ImageSelections.Distance(screenPoint, screenCorner) < ImageSelections.SCALE_HANDLE_SIZE)
                {
                    handle = corner.Name;
                    return true;
                }
            }

            return false;
        }

        public static RectangleF GetSelectionBoundsScreen(int canvasWidth, int canvasHeight)
        {
            Point topLeft = LayersManipulator.WorldToScreen(new Point((int)ImageSelections.GetSelectionBounds().X, (int)ImageSelections.GetSelectionBounds().Y), canvasWidth, canvasHeight);
            Point bottomRight = LayersManipulator.WorldToScreen(new Point((int)ImageSelections.GetSelectionBounds().Right, (int)ImageSelections.GetSelectionBounds().Bottom), canvasWidth, canvasHeight);
            return new RectangleF(topLeft.X, topLeft.Y, bottomRight.X - topLeft.X, bottomRight.Y - topLeft.Y);
        }

        public static PointF GetSelectionCenterScreen(int canvasWidth, int canvasHeight)
        {
            Point p = LayersManipulator.WorldToScreen(Point.Round(ImageSelections.GetSelectionCenter()), canvasWidth, canvasHeight);
            return p;
        }

        public static float CalculateRotationAngle(Point screenMousePosition, int canvasWidth, int canvasHeight)
        {
            PointF screenCenter = GetSelectionCenterScreen(canvasWidth, canvasHeight);
            float dx = screenMousePosition.X - screenCenter.X;
            float dy = screenMousePosition.Y - screenCenter.Y;
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
                    if (ImageSelections.ContainsSelection())
                    {
                        if (selectedLayer != null)
                        {
                            if (selectedLayer.Image == null) return;

                            if (IsOverRotationHandle(e.Location, LayersManipulator.Width, LayersManipulator.Height, canvas.Width, canvas.Height, LayersManipulator.Zoom))
                            {
                                isRotating = true;
                                Cursor.Current = Cursors.SizeAll;
                                startMouseAngle = CalculateRotationAngle(e.Location, canvas.Width, canvas.Height) - rotationAngle;
                            }
                            else if (IsOverScaleHandle(e.Location, canvas.Width, canvas.Height, out string handle))
                            {
                                isScaling = true;
                                Cursor.Current = GetCursor(handle);
                                Point worldMouse = LayersManipulator.ScreenToWorld(e.Location, canvas.Width, canvas.Height);
                                initialScaleDistance = ImageSelections.Distance(worldMouse, ImageSelections.GetSelectionCenter());
                                initialScaleFactor = scaleFactor;
                            }
                            else if (ImageSelections.IsPointInSelection(LayersManipulator.ScreenToWorld(e.Location, canvas.Width, canvas.Height)) >= 0)
                            {
                                isDragging = true;
                                Cursor.Current = Cursors.SizeAll;
                            }

                            if (selectedAreaBitmap == null)
                            {
                                ImageSelections.CalculateSelectionBounds();
                                selectedAreaBitmap = ExtractSelectedArea(selectedLayer);
                                bool hasTransparentPixels = LayersManipulator.HasTransparentPixels(selectedAreaBitmap);
                                selectedLayer.Image = CutSelectionFromLayer(selectedLayer, hasTransparentPixels);
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
                            Bitmap? bitmap = null;
                            var selectionPolygons = ImageSelections.GetSelections();
                            if (selectionPolygons.Count == 0)
                            {
                                bitmap = LayersManipulator.FillColor(selectedLayer.Image, selectedLayer.X, selectedLayer.Y,
                                    canvas.Width, canvas.Height,
                                    selectedLayer.ScaleWidth, selectedLayer.ScaleHeight,
                                    (ImageBlending)cboFillBlendMode.SelectedIndex, paint.GetFillColor(),
                                    (float)(fillOpacity.Value / fillOpacity.Maximum),
                                    lastMousePosition,
                                    new SelectionPolygon());
                            }
                            else
                            {
                                for (int i = 0; i < selectionPolygons.Count; i++)
                                {
                                    if (ImageSelections.IsPointInPolygon(LayersManipulator.ScreenToWorld(lastMousePosition, canvas.Width, canvas.Height), selectionPolygons[i]))
                                    {
                                        bitmap = LayersManipulator.FillColor(selectedLayer.Image, selectedLayer.X, selectedLayer.Y,
                                        canvas.Width, canvas.Height,
                                        selectedLayer.ScaleWidth, selectedLayer.ScaleHeight,
                                        (ImageBlending)cboFillBlendMode.SelectedIndex, paint.GetFillColor(),
                                        (float)(fillOpacity.Value / fillOpacity.Maximum),
                                        lastMousePosition,
                                        selectionPolygons[i]);
                                    }
                                }
                            }
                            if (bitmap != null)
                            {
                                var old = selectedLayer.Image;
                                selectedLayer.Image = bitmap;
                                old?.Dispose();
                            }
                            PaintingEngine.SetTarget(selectedLayer.Image);
                            RedrawImage();
                        }
                    }
                }
                else if (btnLassoSelect.Checked)
                {
                    isLassoSelecting = true;

                    if (ModifierKeys.HasFlag(Keys.Shift))
                    {
                        ImageSelections.IncreaseSelectionPolygons();
                    }
                    else
                    {
                        ImageSelections.ClearSelections();
                    }

                    ImageSelections.AddSelectionPoint(LayersManipulator.ScreenToWorld(lastMousePosition, canvas.Width, canvas.Height));
                }
                else if (btnRectangleSelect.Checked)
                {
                    isRectSelecting = true;

                    if (ModifierKeys.HasFlag(Keys.Shift))
                    {
                        ImageSelections.IncreaseSelectionPolygons();
                    }
                    else
                    {
                        ImageSelections.ClearSelections();
                    }

                    ImageSelections.AddSelectionPoint(LayersManipulator.ScreenToWorld(lastMousePosition, canvas.Width, canvas.Height));
                }
                else if (btnMagicWand.Checked)
                {
                    if (selectedLayer != null)
                    {
                        if (selectedLayer.Image != null)
                        {
                            Point position = LayersManipulator.ScreenToWorld(lastMousePosition, canvas.Width, canvas.Height);
                            position.X -= selectedLayer.X;
                            position.Y -= selectedLayer.Y;

                            bool[,] mask = LayersManipulator.MagicWandSelect(selectedLayer.Image, position, (float)selectionThreshold.Value / selectionThreshold.Maximum, cboMWSelectionMode.Text);

                            if (ModifierKeys.HasFlag(Keys.Shift))
                            {
                                ImageSelections.IncreaseSelectionPolygons();
                            }
                            else
                            {
                                ImageSelections.ClearSelections();
                            }

                            List<SelectionPolygon> polygons = ImageSelections.GetSelectionPointsFromMask(mask, position);
                            foreach (var polygon in polygons)
                            {
                                ImageSelections.IncreaseSelectionPolygons(polygon.Inwards);
                                foreach (Point point in polygon.Points)
                                {
                                    ImageSelections.AddSelectionPoint(new Point(point.X + selectedLayer.X, point.Y + selectedLayer.Y));
                                }
                            }
                        }
                    }
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
                        if (ImageSelections.ContainsSelection())
                        {
                            float ratio = LayersManipulator.ScreenToWorldRatio(canvas.Width, canvas.Height);
                            float worldDx = dx / ratio;
                            float worldDy = dy / ratio;
                            ImageSelections.MoveSelection(worldDx, worldDy);
                            UpdateTransformMatrix();
                            RedrawImage();
                        }
                        else
                        {
                            float ratio = LayersManipulator.ScreenToWorldRatio(canvas.Width, canvas.Height);
                            selectedLayer.X += (int)Math.Round(dx / ratio);
                            selectedLayer.Y += (int)Math.Round(dy / ratio);

                            int minPaintIntervalMs = 16;
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
                if (ImageSelections.ContainsSelection())
                {
                    rotationAngle = CalculateRotationAngle(e.Location, canvas.Width, canvas.Height) - startMouseAngle;
                    UpdateTransformMatrix();
                }
            }
            else if (isScaling)
            {
                if (ImageSelections.ContainsSelection())
                {
                    scaleFactor = CalculateScaleFactor(e.Location);
                    UpdateTransformMatrix();
                }
            }
            else if (isPainting)
            {
                if (selectedLayer != null)
                {
                    var sw = System.Diagnostics.Stopwatch.StartNew();

                    float lazySmoothing = (float)brush_smoothness.Value / brush_smoothness.Maximum;

                    Point currentWorldPos = LayersManipulator.ScreenToWorld(e.Location, canvas.Width, canvas.Height);
                    Point localCurrentRaw = new(currentWorldPos.X - selectedLayer.X, currentWorldPos.Y - selectedLayer.Y);

                    if (strokePoints.Count == 0)
                    {
                        strokePoints = [localCurrentRaw];
                        lazyLocalPos = localCurrentRaw;
                        strokeLastInterpolated = localCurrentRaw;
                        lastMousePosition = e.Location;

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

                            const int segments = 1;

                            PointF previousPos = strokeLastInterpolated;

                            for (int i = 1; i <= segments; i++)
                            {
                                float t = i / (float)segments;
                                PointF pos = PaintingEngine.CatmullRomPoint(p0, p1, p2, p3, t);

                                Point prevRounded = Point.Round(previousPos);
                                Point currRounded = Point.Round(pos);

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

                    const int minPaintIntervalMs = 32;
                    if ((DateTime.Now - lastPaintTime).TotalMilliseconds >= minPaintIntervalMs)
                    {
                        RedrawImage(layersControl.GetSelectedLayerIndex());
                        LayersManipulator.DirtyRegions.Clear();
                        LayersManipulator.DirtyRegions.Add(dirty);
                        lastPaintTime = DateTime.Now;
                    }
                }
            }
            else if (isLassoSelecting)
            {
                if (e.X != lastMousePosition.X || e.Y != lastMousePosition.Y)
                {
                    ImageSelections.AddSelectionPoint(LayersManipulator.ScreenToWorld(e.Location, canvas.Width, canvas.Height));
                }
            }
            else if (isRectSelecting)
            {
                if (e.X != lastMousePosition.X || e.Y != lastMousePosition.Y)
                {
                    Point worldCurrent = LayersManipulator.ScreenToWorld(e.Location, canvas.Width, canvas.Height);
                    Point worldAnchor = ImageSelections.GetLastSelection().Count > 0
                        ? ImageSelections.GetLastSelection()[0]
                        : worldCurrent;

                    if (ImageSelections.GetLastSelection().Count == 1)
                    {
                        ImageSelections.AddSelectionPoint(new Point(worldCurrent.X, worldAnchor.Y));
                        ImageSelections.AddSelectionPoint(worldCurrent);
                        ImageSelections.AddSelectionPoint(new Point(worldAnchor.X, worldCurrent.Y));
                        ImageSelections.AddSelectionPoint(worldAnchor);
                    }
                    else if (ImageSelections.GetLastSelection().Count == 5)
                    {
                        ImageSelections.UpdateSelectionPoint(1, new Point(worldCurrent.X, worldAnchor.Y));
                        ImageSelections.UpdateSelectionPoint(2, worldCurrent);
                        ImageSelections.UpdateSelectionPoint(3, new Point(worldAnchor.X, worldCurrent.Y));
                    }
                }
            }
            else
            {
                if (ImageSelections.ContainsSelection())
                {
                    //UpdateCursor(e.Location);
                }
            }

            lastMousePosition = e.Location;
        }

        private void PixelImage_MouseUp(object sender, MouseEventArgs e)
        {
            if (isLassoSelecting)
            {
                if (ImageSelections.GetLastSelection().Count > 1)
                {
                    ImageSelections.AddSelectionPoint(ImageSelections.GetLastSelection()[0]);
                }
            }
            isDragging = false;
            isPainting = false;
            isLassoSelecting = false;
            isRectSelecting = false;
            isRotating = false;
            isScaling = false;
            HistoryManager.RecordState(new HistoryItem(LayersManipulator.Zoom, LayersManipulator.ImageOffset, layersControl.GetLayers(), layersControl.GetSelectedLayerIndex()));
            LayersManipulator.UpdateBuffers();
            PaintingEngine.EndStroke();
            ImageSelections.MergeIntersections();
            ImageSelections.CalculateSelectionBounds();
            RedrawImage();
            layersControl.RefreshLayersDisplay();
        }

        private void Canvas_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                if (ImageSelections.ContainsSelection())
                {
                    ImageSelections.ClearSelections();
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
            RedrawSelectedLayerRect();
            RedrawHandles();

            sw.Stop();

            if (_dashOffset == 0)
            {
                Console.WriteLine($"PopulateColorGrid: {sw.ElapsedMilliseconds}ms");
            }
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
                    RectangleF screenBounds = GetSelectionBoundsScreen(canvas.Width, canvas.Height);
                    g.MultiplyTransform(transformMatrix);
                    g.DrawImage(selectedAreaBitmap, screenBounds);
                    g.ResetTransform();
                }
            }

            var old = canvas.Image;
            canvas.Image = bmp;
            old?.Dispose();
        }

        private void RedrawSelectedLayerRect()
        {
            if (canvas.Image == null) return;
            using (Graphics g = Graphics.FromImage(canvas.Image))
            {
                using Pen selectionPen = new(Color.Yellow, 2.0f);
                selectionPen.DashPattern = [2, 2];
                selectionPen.DashOffset = _dashOffset;
                var selectedLayer = layersControl.GetLayer(layersControl.GetSelectedLayerIndex());
                if (selectedLayer != null && selectedLayer.Image != null)
                {
                    float ratio = LayersManipulator.ScreenToWorldRatio(canvas.Width, canvas.Height);
                    Point p = LayersManipulator.WorldToScreen(new Point(selectedLayer.X, selectedLayer.Y), canvas.Width, canvas.Height);
                    g.DrawRectangle(selectionPen, p.X, p.Y, selectedLayer.Image.Width * ratio, selectedLayer.Image.Height * ratio);
                }
            }
            canvas.Invalidate();
        }

        private void RedrawHandles()
        {
            if (!ImageSelections.ContainsSelection() || canvas.Image == null) return;

            using (Graphics g = Graphics.FromImage(canvas.Image))
            {
                var selectionPoints = ImageSelections.GetSelections();
                for (int i = 0; i < selectionPoints.Count; i++)
                {
                    if (selectionPoints[i].Points.Count > 2)
                    {
                        Point[] screenPoints = [.. selectionPoints[i].Points.Select(p =>
                            LayersManipulator.WorldToScreen(p, canvas.Width, canvas.Height))];

                        using Pen selectionPen = new(Color.Blue, 2f);
                        selectionPen.DashPattern = [5, 5];
                        selectionPen.DashOffset = _dashOffset;
                        g.DrawLines(selectionPen, screenPoints);
                    }

                    g.SmoothingMode = SmoothingMode.AntiAlias;

                    using Pen handlePen = new(Color.White, 1.5f);
                    using Brush scaleBrush = new SolidBrush(Color.FromArgb(0, 120, 215));
                    using Brush rotateBrush = new SolidBrush(Color.Gold);

                    RectangleF screenBounds = GetSelectionBoundsScreen(canvas.Width, canvas.Height);

                    PointF[] corners =
                    [
                        new(screenBounds.X, screenBounds.Y),
                        new(screenBounds.Right, screenBounds.Y),
                        new(screenBounds.X, screenBounds.Bottom),
                        new(screenBounds.Right, screenBounds.Bottom)
                    ];

                    foreach (var corner in corners)
                    {
                        RectangleF rect = new(
                            corner.X - ImageSelections.SCALE_HANDLE_SIZE / 2,
                            corner.Y - ImageSelections.SCALE_HANDLE_SIZE / 2,
                            ImageSelections.SCALE_HANDLE_SIZE,
                            ImageSelections.SCALE_HANDLE_SIZE
                        );
                        g.FillRectangle(scaleBrush, rect);
                        g.DrawRectangle(handlePen, rect.X, rect.Y, rect.Width, rect.Height);
                    }

                    float centerX = screenBounds.X + screenBounds.Width / 2;
                    float handleCenterY = screenBounds.Y - ImageSelections.ROTATION_HANDLE_SIZE;

                    g.DrawLine(handlePen, centerX, screenBounds.Y, centerX, handleCenterY);

                    RectangleF rotRect = new(
                        centerX - ImageSelections.ROTATION_HANDLE_SIZE / 2,
                        handleCenterY - ImageSelections.ROTATION_HANDLE_SIZE / 2,
                        ImageSelections.ROTATION_HANDLE_SIZE,
                        ImageSelections.ROTATION_HANDLE_SIZE
                    );

                    g.FillEllipse(rotateBrush, rotRect);
                    g.DrawEllipse(handlePen, rotRect);
                }
            }

            canvas.Invalidate();
        }

        private void FormMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!ConfirmAbandonChanges())
            {
                e.Cancel = true;
            }
        }

        private void ExitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
    }
}