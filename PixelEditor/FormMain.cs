using PixelEditor.Vector;
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
        private PointF startPoint;
        private PointF dragOffset = PointF.Empty;
        private bool isDragging = false;
        private bool isPainting = false;
        private bool isErasing = false;
        private bool isWarping = false;
        private bool isRotating = false;
        private bool isScaling = false;
        private bool isLassoSelecting = false;
        private bool isRectSelecting = false;
        private bool isDrawing = false;
        private bool isCropping = false;
        private bool isDirty = false;
        private bool isDrawingShape = false;
        private bool isDraggingShape = false;
        private bool isUpdatingPolygonShape = false;
        private bool isResizingShape = false;
        private int activeHandleIndex = -1; // 0: Top-Left, 1: Top-Right, 2: Bottom-Left, 3: Bottom-Right
        private float dashOffset = 0;
        private float startMouseAngle = 0;
        private float rotationAngle = 0;
        private int selectedBrushIndex = 0;
        private int selectedEraserIndex = 0;
        private string currentFilePath = "";
        private DateTime lastPaintTime = DateTime.MinValue;
        private readonly List<Image> brushes = [];
        private List<PointF> strokePoints = [];
        private Bitmap? selectedAreaBitmap = null;
        private readonly Matrix transformMatrix = new();
        private float brushPixelSize = 0.0f;
        private float currentOpacity = 1.0f;

        private float scaleFactorX = 1.0f;
        private float scaleFactorY = 1.0f;
        private float initialScaleDistanceX = 0f;
        private float initialScaleDistanceY = 0f;
        private PointF scaleAnchorWorld = PointF.Empty;
        private Point scaleStartMouseScreen = Point.Empty;
        private string activeScaleHandle = "";

        private readonly GroupBox groupBrushDetail = new();
        private readonly Label lblBrushHardness = new();
        private readonly Label lblBrushSmoothness = new();
        private readonly Label lblBrushOpacity = new();
        private readonly Label lblBrushSize = new();
        private readonly Button btnPenColor = new();
        private readonly Panel panelBrush = new();
        private readonly TrackBar brush_size = new();
        private readonly TrackBar brush_opacity = new();
        private readonly Label label10 = new();
        private readonly Label label9 = new();
        private readonly Label label11 = new();
        private readonly Label label8 = new();
        private readonly TrackBar brush_hardness = new();
        private readonly TrackBar brush_smoothness = new();

        private readonly GroupBox groupEraserDetail = new();
        private readonly Button btnEraserColor = new();
        private readonly Panel panelEraser = new();
        private readonly TrackBar eraser_size = new();
        private readonly TrackBar eraser_opacity = new();
        private readonly TrackBar eraser_hardness = new();
        private readonly TrackBar eraser_smoothness = new();
        private readonly Label lblEraserSize = new();
        private readonly Label lblEraserOpacity = new();
        private readonly Label lblEraserHardness = new();
        private readonly Label lblEraserSmoothness = new();
        private readonly Label labelEraserSize = new();
        private readonly Label labelEraserOpacity = new();
        private readonly Label labelEraserHardness = new();
        private readonly Label labelEraserSmoothness = new();

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

        private readonly GroupBox groupWarpDetail = new();
        private readonly TrackBar warpBrushSize = new();
        private readonly Label lblWarpBrushSize = new();
        private readonly Label labelWarpBrushSize = new();

        private readonly GroupBox groupCropDetail = new();
        private readonly Button btnCropAction = new();

        private readonly GroupBox groupRectShapeDetail = new();
        private readonly Label labelRectLineSize = new();
        private readonly TrackBar rectLineSizeTrack = new();
        private readonly Label lblRectLineSizeValue = new();
        private readonly Label labelRectLineOpacity = new();
        private readonly NumericUpDown rectLineOpacityNum = new();
        private readonly Label labelRectLineColor = new();
        private readonly Button btnRectLineColor = new();
        private readonly Label labelRectLinePattern = new();
        private readonly ComboBox cboRectLinePattern = new();
        private readonly Label labelRectFillOpacity = new();
        private readonly NumericUpDown fillRectOpacityNum = new();
        private readonly Label labelRectFillColor = new();
        private readonly Button btnRectFillColorShape = new();

        private readonly GroupBox groupEllipseShapeDetail = new();
        private readonly Label labelEllipseLineSize = new();
        private readonly TrackBar ellipseLineSizeTrack = new();
        private readonly Label lblEllipseLineSizeValue = new();
        private readonly Label labelEllipseLineOpacity = new();
        private readonly NumericUpDown ellipseLineOpacityNum = new();
        private readonly Label labelEllipseLineColor = new();
        private readonly Button btnEllipseLineColor = new();
        private readonly Label labelEllipseLinePattern = new();
        private readonly ComboBox cboEllipseLinePattern = new();
        private readonly Label labelEllipseFillOpacity = new();
        private readonly NumericUpDown fillEllipseOpacityNum = new();
        private readonly Label labelEllipseFillColor = new();
        private readonly Button btnEllipseFillColorShape = new();

        private readonly GroupBox groupPolygonShapeDetail = new();
        private readonly Label labelPolygonLineSize = new();
        private readonly TrackBar polygonLineSizeTrack = new();
        private readonly Label lblPolygonLineSizeValue = new();
        private readonly Label labelPolygonLineOpacity = new();
        private readonly NumericUpDown polygonLineOpacityNum = new();
        private readonly Label labelPolygonLineColor = new();
        private readonly Button btnPolygonLineColor = new();
        private readonly Label labelPolygonLinePattern = new();
        private readonly ComboBox cboPolygonLinePattern = new();
        private readonly Label labelPolygonFillOpacity = new();
        private readonly NumericUpDown fillPolygonOpacityNum = new();
        private readonly Label labelPolygonFillColor = new();
        private readonly Button btnPolygonFillColorShape = new();
        private readonly CheckBox chkClosed = new();

        private readonly GroupBox groupTextShapeDetail = new();
        private readonly Label lblTextLineText = new();
        private readonly TextBox txtTextLineText = new();
        private readonly Label lblTextFont = new();
        private readonly Button btnTextFont = new();
        private readonly Label lblTextFillOpacity = new();
        private readonly NumericUpDown numTextFillOpacity = new();
        private readonly Label lblTextFillColor = new();
        private readonly Button btnTextFillColor = new();

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
            InitializeComponentGroupEraser();
            InitializeComponentGroupMagicWand();
            InitializeComponentGroupWarp();
            InitializeComponentGroupCrop();
            InitializeComponentGroupRectShape();
            InitializeComponentGroupEllipseShape();
            InitializeComponentGroupPolygonShape();
            InitializeComponentGroupTextShape();
            InitializeTimer();
            layersControl.LayerChanged += LayersControl_LayerChanged;
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
                    dashOffset += 1.0f;

                    if (dashOffset > 100.0f) dashOffset = 0;

                    RedrawImage();
                }
                if (isDrawingShape)
                {
                    RedrawImage(layersControl.GetSelectedLayerIndex());

                    isDrawingShape = false;
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
            btnFillColor1.ForeColor = btnRectLineColor.BackColor;
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
            groupBrushDetail.Controls.Add(panelBrush);
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
            panelBrush.BackColor = Color.White;
            panelBrush.BorderStyle = BorderStyle.FixedSingle;
            panelBrush.Location = new Point(12, 51);
            panelBrush.Name = "panel2";
            panelBrush.Size = new Size(155, 139);
            panelBrush.TabIndex = 22;
            brush_size.Location = new Point(76, 196);
            brush_size.Maximum = 100;
            brush_size.Minimum = 1;
            brush_size.Name = "brush_size";
            brush_size.Size = new Size(106, 45);
            brush_size.TabIndex = 23;
            brush_size.TickStyle = TickStyle.None;
            brush_size.Value = 6;
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

        private void InitializeComponentGroupEraser()
        {
            groupEraserDetail.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)eraser_size).BeginInit();
            ((System.ComponentModel.ISupportInitialize)eraser_opacity).BeginInit();
            ((System.ComponentModel.ISupportInitialize)eraser_hardness).BeginInit();
            ((System.ComponentModel.ISupportInitialize)eraser_smoothness).BeginInit();
            SuspendLayout();

            groupEraserDetail.Controls.Add(lblEraserHardness);
            groupEraserDetail.Controls.Add(lblEraserSmoothness);
            groupEraserDetail.Controls.Add(lblEraserOpacity);
            groupEraserDetail.Controls.Add(lblEraserSize);
            groupEraserDetail.Controls.Add(btnEraserColor);
            groupEraserDetail.Controls.Add(panelEraser);
            groupEraserDetail.Controls.Add(eraser_size);
            groupEraserDetail.Controls.Add(eraser_opacity);
            groupEraserDetail.Controls.Add(labelEraserHardness);
            groupEraserDetail.Controls.Add(labelEraserSmoothness);
            groupEraserDetail.Controls.Add(labelEraserSize);
            groupEraserDetail.Controls.Add(labelEraserOpacity);
            groupEraserDetail.Controls.Add(eraser_hardness);
            groupEraserDetail.Controls.Add(eraser_smoothness);

            groupEraserDetail.Location = new Point(12, 74);
            groupEraserDetail.Name = "groupEraserDetail";
            groupEraserDetail.Size = new Size(230, 430);
            groupEraserDetail.TabIndex = 29;
            groupEraserDetail.TabStop = false;
            groupEraserDetail.Text = "Eraser Detail";
            groupEraserDetail.Visible = false;

            // Labels for displaying values
            lblEraserHardness.BackColor = Color.White;
            lblEraserHardness.BorderStyle = BorderStyle.Fixed3D;
            lblEraserHardness.FlatStyle = FlatStyle.Flat;
            lblEraserHardness.Font = new Font("Segoe UI", 8.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
            lblEraserHardness.Location = new Point(188, 349);
            lblEraserHardness.Name = "lblEraserHardness";
            lblEraserHardness.Size = new Size(32, 24);
            lblEraserHardness.TabIndex = 30;
            lblEraserHardness.TextAlign = ContentAlignment.MiddleCenter;

            lblEraserSmoothness.BackColor = Color.White;
            lblEraserSmoothness.BorderStyle = BorderStyle.Fixed3D;
            lblEraserSmoothness.FlatStyle = FlatStyle.Flat;
            lblEraserSmoothness.Font = new Font("Segoe UI", 8.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
            lblEraserSmoothness.Location = new Point(188, 298);
            lblEraserSmoothness.Name = "lblEraserSmoothness";
            lblEraserSmoothness.Size = new Size(32, 24);
            lblEraserSmoothness.TabIndex = 30;
            lblEraserSmoothness.TextAlign = ContentAlignment.MiddleCenter;

            lblEraserOpacity.BackColor = Color.White;
            lblEraserOpacity.BorderStyle = BorderStyle.Fixed3D;
            lblEraserOpacity.FlatStyle = FlatStyle.Flat;
            lblEraserOpacity.Font = new Font("Segoe UI", 8.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
            lblEraserOpacity.Location = new Point(188, 247);
            lblEraserOpacity.Name = "lblEraserOpacity";
            lblEraserOpacity.Size = new Size(32, 24);
            lblEraserOpacity.TabIndex = 30;
            lblEraserOpacity.TextAlign = ContentAlignment.MiddleCenter;

            lblEraserSize.BackColor = Color.White;
            lblEraserSize.BorderStyle = BorderStyle.Fixed3D;
            lblEraserSize.FlatStyle = FlatStyle.Flat;
            lblEraserSize.Font = new Font("Segoe UI", 8.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
            lblEraserSize.Location = new Point(188, 196);
            lblEraserSize.Name = "lblEraserSize";
            lblEraserSize.Size = new Size(32, 24);
            lblEraserSize.TabIndex = 30;
            lblEraserSize.TextAlign = ContentAlignment.MiddleCenter;

            // Eraser Color Button
            btnEraserColor.BackColor = Color.White;
            btnEraserColor.FlatStyle = FlatStyle.Popup;
            btnEraserColor.Location = new Point(73, 22);
            btnEraserColor.Name = "btnEraserColor";
            btnEraserColor.Size = new Size(20, 20);
            btnEraserColor.TabIndex = 24;
            btnEraserColor.UseVisualStyleBackColor = false;
            btnEraserColor.Click += BtnEraserColor_Click;

            // Preview panel
            panelEraser.BackColor = Color.White;
            panelEraser.BorderStyle = BorderStyle.FixedSingle;
            panelEraser.Location = new Point(12, 51);
            panelEraser.Name = "panelEraser";
            panelEraser.Size = new Size(155, 139);
            panelEraser.TabIndex = 22;

            // Size control
            eraser_size.Location = new Point(76, 196);
            eraser_size.Maximum = 100;
            eraser_size.Minimum = 1;
            eraser_size.Name = "eraser_size";
            eraser_size.Size = new Size(106, 45);
            eraser_size.TabIndex = 23;
            eraser_size.TickStyle = TickStyle.None;
            eraser_size.Value = 12;
            eraser_size.Scroll += Eraser_size_Scroll;

            // Opacity control
            eraser_opacity.Location = new Point(76, 247);
            eraser_opacity.Maximum = 100;
            eraser_opacity.Name = "eraser_opacity";
            eraser_opacity.Size = new Size(106, 45);
            eraser_opacity.TabIndex = 23;
            eraser_opacity.TickStyle = TickStyle.None;
            eraser_opacity.Value = 100;
            eraser_opacity.Scroll += Eraser_opacity_Scroll;

            // Labels
            labelEraserHardness.AutoSize = true;
            labelEraserHardness.Font = new Font("Segoe UI", 8.25F);
            labelEraserHardness.Location = new Point(7, 349);
            labelEraserHardness.Name = "labelEraserHardness";
            labelEraserHardness.Size = new Size(58, 13);
            labelEraserHardness.TabIndex = 29;
            labelEraserHardness.Text = "Hardness:";

            labelEraserSmoothness.AutoSize = true;
            labelEraserSmoothness.Font = new Font("Segoe UI", 8.25F);
            labelEraserSmoothness.Location = new Point(6, 298);
            labelEraserSmoothness.Name = "labelEraserSmoothness";
            labelEraserSmoothness.Size = new Size(73, 13);
            labelEraserSmoothness.TabIndex = 29;
            labelEraserSmoothness.Text = "Smoothness:";

            labelEraserSize.AutoSize = true;
            labelEraserSize.Font = new Font("Segoe UI", 8.25F);
            labelEraserSize.Location = new Point(7, 196);
            labelEraserSize.Name = "labelEraserSize";
            labelEraserSize.Size = new Size(30, 13);
            labelEraserSize.TabIndex = 29;
            labelEraserSize.Text = "Size:";

            labelEraserOpacity.AutoSize = true;
            labelEraserOpacity.Font = new Font("Segoe UI", 8.25F);
            labelEraserOpacity.Location = new Point(7, 247);
            labelEraserOpacity.Name = "labelEraserOpacity";
            labelEraserOpacity.Size = new Size(49, 13);
            labelEraserOpacity.TabIndex = 29;
            labelEraserOpacity.Text = "Opacity:";

            // Hardness control
            eraser_hardness.Location = new Point(76, 349);
            eraser_hardness.Maximum = 100;
            eraser_hardness.Minimum = 1;
            eraser_hardness.Name = "eraser_hardness";
            eraser_hardness.Size = new Size(106, 45);
            eraser_hardness.TabIndex = 23;
            eraser_hardness.TickStyle = TickStyle.None;
            eraser_hardness.Value = 80;
            eraser_hardness.Scroll += Eraser_hardness_Scroll;

            // Smoothness control
            eraser_smoothness.Location = new Point(76, 298);
            eraser_smoothness.Maximum = 100;
            eraser_smoothness.Name = "eraser_smoothness";
            eraser_smoothness.Size = new Size(106, 45);
            eraser_smoothness.TabIndex = 23;
            eraser_smoothness.TickStyle = TickStyle.None;
            eraser_smoothness.Value = 22;
            eraser_smoothness.Scroll += Eraser_smoothness_Scroll;

            Controls.Add(groupEraserDetail);

            groupEraserDetail.ResumeLayout(false);
            groupEraserDetail.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)eraser_size).EndInit();
            ((System.ComponentModel.ISupportInitialize)eraser_opacity).EndInit();
            ((System.ComponentModel.ISupportInitialize)eraser_hardness).EndInit();
            ((System.ComponentModel.ISupportInitialize)eraser_smoothness).EndInit();
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
            cboMWSelectionMode.Items.AddRange(["All", "Red", "Green", "Blue", "Alpha", "Color"]);
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

        private void InitializeComponentGroupWarp()
        {
            groupWarpDetail.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)warpBrushSize).BeginInit();
            SuspendLayout();

            groupWarpDetail.Controls.Add(lblWarpBrushSize);
            groupWarpDetail.Controls.Add(labelWarpBrushSize);
            groupWarpDetail.Controls.Add(warpBrushSize);

            groupWarpDetail.Location = new Point(12, 74);
            groupWarpDetail.Name = "groupWarpDetail";
            groupWarpDetail.Size = new Size(230, 145);
            groupWarpDetail.TabIndex = 30;
            groupWarpDetail.TabStop = false;
            groupWarpDetail.Text = "Warp Detail";
            groupWarpDetail.Visible = false;

            // Label for displaying the current brush size value
            lblWarpBrushSize.BackColor = Color.White;
            lblWarpBrushSize.BorderStyle = BorderStyle.Fixed3D;
            lblWarpBrushSize.FlatStyle = FlatStyle.Flat;
            lblWarpBrushSize.Font = new Font("Segoe UI", 8.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
            lblWarpBrushSize.Location = new Point(188, 40);
            lblWarpBrushSize.Name = "lblWarpBrushSize";
            lblWarpBrushSize.Size = new Size(32, 24);
            lblWarpBrushSize.TabIndex = 30;
            lblWarpBrushSize.TextAlign = ContentAlignment.MiddleCenter;

            // Text label
            labelWarpBrushSize.AutoSize = true;
            labelWarpBrushSize.Font = new Font("Segoe UI", 8.25F);
            labelWarpBrushSize.Location = new Point(7, 40);
            labelWarpBrushSize.Name = "labelWarpBrushSize";
            labelWarpBrushSize.Size = new Size(30, 13);
            labelWarpBrushSize.TabIndex = 29;
            labelWarpBrushSize.Text = "Size:";

            // Brush size TrackBar
            warpBrushSize.Location = new Point(76, 40);
            warpBrushSize.Maximum = 100;
            warpBrushSize.Minimum = 1;
            warpBrushSize.Name = "warpBrushSize";
            warpBrushSize.Size = new Size(106, 45);
            warpBrushSize.TabIndex = 23;
            warpBrushSize.TickStyle = TickStyle.None;
            warpBrushSize.Value = 12;
            warpBrushSize.Scroll += WarpBrushSize_Scroll;

            Controls.Add(groupWarpDetail);

            groupWarpDetail.ResumeLayout(false);
            groupWarpDetail.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)warpBrushSize).EndInit();
            ResumeLayout(false);
        }

        private void InitializeComponentGroupCrop()
        {
            groupCropDetail.SuspendLayout();
            SuspendLayout();

            groupCropDetail.Controls.Add(btnCropAction);

            groupCropDetail.Location = new Point(12, 74);
            groupCropDetail.Name = "groupCropDetail";
            groupCropDetail.Size = new Size(230, 430);
            groupCropDetail.TabIndex = 31;
            groupCropDetail.TabStop = false;
            groupCropDetail.Text = "Crop Detail";
            groupCropDetail.Visible = false;

            // Crop Button
            btnCropAction.BackColor = Color.LightGray;
            btnCropAction.FlatStyle = FlatStyle.Popup;
            btnCropAction.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btnCropAction.Location = new Point(12, 22);
            btnCropAction.Name = "btnCropAction";
            btnCropAction.Size = new Size(206, 40);
            btnCropAction.TabIndex = 24;
            btnCropAction.Text = "Crop";
            btnCropAction.UseVisualStyleBackColor = false;
            btnCropAction.Click += BtnCrop_Click;

            Controls.Add(groupCropDetail);

            groupCropDetail.ResumeLayout(false);
            ResumeLayout(false);
        }

        private void InitializeComponentGroupRectShape()
        {
            groupRectShapeDetail.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)rectLineSizeTrack).BeginInit();
            ((System.ComponentModel.ISupportInitialize)rectLineOpacityNum).BeginInit();
            ((System.ComponentModel.ISupportInitialize)fillRectOpacityNum).BeginInit();
            SuspendLayout();

            // Add Controls to Group
            groupRectShapeDetail.Controls.Add(lblRectLineSizeValue);
            groupRectShapeDetail.Controls.Add(rectLineSizeTrack);
            groupRectShapeDetail.Controls.Add(rectLineOpacityNum);
            groupRectShapeDetail.Controls.Add(btnRectLineColor);
            groupRectShapeDetail.Controls.Add(cboRectLinePattern);
            groupRectShapeDetail.Controls.Add(fillRectOpacityNum);
            groupRectShapeDetail.Controls.Add(btnRectFillColorShape);
            groupRectShapeDetail.Controls.Add(labelRectLineSize);
            groupRectShapeDetail.Controls.Add(labelRectLineOpacity);
            groupRectShapeDetail.Controls.Add(labelRectLineColor);
            groupRectShapeDetail.Controls.Add(labelRectLinePattern);
            groupRectShapeDetail.Controls.Add(labelRectFillOpacity);
            groupRectShapeDetail.Controls.Add(labelRectFillColor);

            // GroupBox Settings
            groupRectShapeDetail.Location = new Point(12, 74);
            groupRectShapeDetail.Name = "groupShapeDetail";
            groupRectShapeDetail.Size = new Size(230, 260);
            groupRectShapeDetail.TabIndex = 30;
            groupRectShapeDetail.TabStop = false;
            groupRectShapeDetail.Text = "Rectangle Detail";
            groupRectShapeDetail.Visible = false;

            // --- LINE SECTION ---

            // Line Size
            labelRectLineSize.AutoSize = true;
            labelRectLineSize.Location = new Point(10, 25);
            labelRectLineSize.Text = "Line Size:";

            rectLineSizeTrack.Location = new Point(75, 22);
            rectLineSizeTrack.Size = new Size(110, 45);
            rectLineSizeTrack.Maximum = 200;
            rectLineSizeTrack.Minimum = 1;
            rectLineSizeTrack.TickStyle = TickStyle.None;
            rectLineSizeTrack.Value = 2;
            rectLineSizeTrack.Scroll += RectLineSize_Scroll;

            lblRectLineSizeValue.BackColor = Color.White;
            lblRectLineSizeValue.BorderStyle = BorderStyle.Fixed3D;
            lblRectLineSizeValue.Location = new Point(188, 22);
            lblRectLineSizeValue.Size = new Size(32, 22);
            lblRectLineSizeValue.TextAlign = ContentAlignment.MiddleCenter;
            lblRectLineSizeValue.Text = "2";

            labelRectLineOpacity.AutoSize = true;
            labelRectLineOpacity.Location = new Point(10, 73);
            labelRectLineOpacity.Text = "Line Opacity:";

            rectLineOpacityNum.Location = new Point(125, 71);
            rectLineOpacityNum.Size = new Size(60, 23);
            rectLineOpacityNum.Value = new decimal([100, 0, 0, 0]);
            rectLineOpacityNum.ValueChanged += OnRectLineOpacityValueChanged;

            labelRectLineColor.AutoSize = true;
            labelRectLineColor.Location = new Point(10, 103);
            labelRectLineColor.Text = "Line Color:";

            btnRectLineColor.BackColor = Color.Black;
            btnRectLineColor.FlatStyle = FlatStyle.Popup;
            btnRectLineColor.Location = new Point(125, 100);
            btnRectLineColor.Size = new Size(20, 20);
            btnRectLineColor.Click += BtnRectLineColor_Click;

            labelRectLinePattern.AutoSize = true;
            labelRectLinePattern.Location = new Point(10, 133);
            labelRectLinePattern.Text = "Pattern:";

            cboRectLinePattern.DropDownStyle = ComboBoxStyle.DropDownList;
            cboRectLinePattern.Items.AddRange(["Solid", "Dash", "Dot", "DashDot", "DashDotDot", "Custom"]);
            cboRectLinePattern.Location = new Point(125, 130);
            cboRectLinePattern.Size = new Size(100, 21);
            cboRectLinePattern.SelectedIndexChanged += CboRectLinePattern_SelectedIndexChanged;

            // --- FILL SECTION (All shifted down 8px) ---

            // Fill Opacity
            labelRectFillOpacity.AutoSize = true;
            labelRectFillOpacity.Location = new Point(10, 173);
            labelRectFillOpacity.Text = "Fill Opacity:";

            fillRectOpacityNum.Location = new Point(125, 171);
            fillRectOpacityNum.Size = new Size(60, 23);
            fillRectOpacityNum.Value = new decimal([100, 0, 0, 0]);
            fillRectOpacityNum.ValueChanged += FillRectOpacityNum_ValueChanged;

            // Fill Color
            labelRectFillColor.AutoSize = true;
            labelRectFillColor.Location = new Point(10, 203);
            labelRectFillColor.Text = "Fill Color:";

            btnRectFillColorShape.BackColor = Color.White;
            btnRectFillColorShape.FlatStyle = FlatStyle.Popup;
            btnRectFillColorShape.Location = new Point(125, 200);
            btnRectFillColorShape.Size = new Size(20, 20);
            btnRectFillColorShape.Click += BtnFillRectColorShape_Click;

            // Finalize
            Controls.Add(groupRectShapeDetail);
            groupRectShapeDetail.ResumeLayout(false);
            groupRectShapeDetail.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)rectLineSizeTrack).EndInit();
            ((System.ComponentModel.ISupportInitialize)rectLineOpacityNum).EndInit();
            ((System.ComponentModel.ISupportInitialize)fillRectOpacityNum).EndInit();
            ResumeLayout(false);
        }

        private void InitializeComponentGroupEllipseShape()
        {
            groupEllipseShapeDetail.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)ellipseLineSizeTrack).BeginInit();
            ((System.ComponentModel.ISupportInitialize)ellipseLineOpacityNum).BeginInit();
            ((System.ComponentModel.ISupportInitialize)fillEllipseOpacityNum).BeginInit();
            SuspendLayout();

            // Add Controls to Group
            groupEllipseShapeDetail.Controls.Add(lblEllipseLineSizeValue);
            groupEllipseShapeDetail.Controls.Add(ellipseLineSizeTrack);
            groupEllipseShapeDetail.Controls.Add(ellipseLineOpacityNum);
            groupEllipseShapeDetail.Controls.Add(btnEllipseLineColor);
            groupEllipseShapeDetail.Controls.Add(cboEllipseLinePattern);
            groupEllipseShapeDetail.Controls.Add(fillEllipseOpacityNum);
            groupEllipseShapeDetail.Controls.Add(btnEllipseFillColorShape);
            groupEllipseShapeDetail.Controls.Add(labelEllipseLineSize);
            groupEllipseShapeDetail.Controls.Add(labelEllipseLineOpacity);
            groupEllipseShapeDetail.Controls.Add(labelEllipseLineColor);
            groupEllipseShapeDetail.Controls.Add(labelEllipseLinePattern);
            groupEllipseShapeDetail.Controls.Add(labelEllipseFillOpacity);
            groupEllipseShapeDetail.Controls.Add(labelEllipseFillColor);

            // GroupBox Settings
            groupEllipseShapeDetail.Location = new Point(12, 74);
            groupEllipseShapeDetail.Name = "groupShapeDetail";
            groupEllipseShapeDetail.Size = new Size(230, 260);
            groupEllipseShapeDetail.TabIndex = 30;
            groupEllipseShapeDetail.TabStop = false;
            groupEllipseShapeDetail.Text = "Ellipse Detail";
            groupEllipseShapeDetail.Visible = false;

            // --- LINE SECTION ---

            // Line Size
            labelEllipseLineSize.AutoSize = true;
            labelEllipseLineSize.Location = new Point(10, 25);
            labelEllipseLineSize.Text = "Line Size:";

            ellipseLineSizeTrack.Location = new Point(75, 22);
            ellipseLineSizeTrack.Size = new Size(110, 45);
            ellipseLineSizeTrack.Maximum = 200;
            ellipseLineSizeTrack.Minimum = 1;
            ellipseLineSizeTrack.TickStyle = TickStyle.None;
            ellipseLineSizeTrack.Value = 2;
            ellipseLineSizeTrack.Scroll += EllipseLineSize_Scroll;

            lblEllipseLineSizeValue.BackColor = Color.White;
            lblEllipseLineSizeValue.BorderStyle = BorderStyle.Fixed3D;
            lblEllipseLineSizeValue.Location = new Point(188, 22);
            lblEllipseLineSizeValue.Size = new Size(32, 22);
            lblEllipseLineSizeValue.TextAlign = ContentAlignment.MiddleCenter;
            lblEllipseLineSizeValue.Text = "2";

            labelEllipseLineOpacity.AutoSize = true;
            labelEllipseLineOpacity.Location = new Point(10, 73);
            labelEllipseLineOpacity.Text = "Line Opacity:";

            ellipseLineOpacityNum.Location = new Point(125, 71);
            ellipseLineOpacityNum.Size = new Size(60, 23);
            ellipseLineOpacityNum.Value = new decimal([100, 0, 0, 0]);
            ellipseLineOpacityNum.ValueChanged += OnEllipseLineOpacityValueChanged;

            labelEllipseLineColor.AutoSize = true;
            labelEllipseLineColor.Location = new Point(10, 103);
            labelEllipseLineColor.Text = "Line Color:";

            btnEllipseLineColor.BackColor = Color.Black;
            btnEllipseLineColor.FlatStyle = FlatStyle.Popup;
            btnEllipseLineColor.Location = new Point(125, 100);
            btnEllipseLineColor.Size = new Size(20, 20);
            btnEllipseLineColor.Click += BtnEllipseLineColor_Click;

            labelEllipseLinePattern.AutoSize = true;
            labelEllipseLinePattern.Location = new Point(10, 133);
            labelEllipseLinePattern.Text = "Pattern:";

            cboEllipseLinePattern.DropDownStyle = ComboBoxStyle.DropDownList;
            cboEllipseLinePattern.Items.AddRange(["Solid", "Dash", "Dot", "DashDot", "DashDotDot", "Custom"]);
            cboEllipseLinePattern.Location = new Point(125, 130);
            cboEllipseLinePattern.Size = new Size(100, 21);
            cboEllipseLinePattern.SelectedIndexChanged += CboEllipseLinePattern_SelectedIndexChanged;

            // --- FILL SECTION (All shifted down 8px) ---

            // Fill Opacity
            labelEllipseFillOpacity.AutoSize = true;
            labelEllipseFillOpacity.Location = new Point(10, 173);
            labelEllipseFillOpacity.Text = "Fill Opacity:";

            fillEllipseOpacityNum.Location = new Point(125, 171);
            fillEllipseOpacityNum.Size = new Size(60, 23);
            fillEllipseOpacityNum.Value = new decimal([100, 0, 0, 0]);
            fillEllipseOpacityNum.ValueChanged += FillEllipseOpacityNum_ValueChanged;

            // Fill Color
            labelEllipseFillColor.AutoSize = true;
            labelEllipseFillColor.Location = new Point(10, 203);
            labelEllipseFillColor.Text = "Fill Color:";

            btnEllipseFillColorShape.BackColor = Color.White;
            btnEllipseFillColorShape.FlatStyle = FlatStyle.Popup;
            btnEllipseFillColorShape.Location = new Point(125, 200);
            btnEllipseFillColorShape.Size = new Size(20, 20);
            btnEllipseFillColorShape.Click += BtnFillEllipseColorShape_Click;

            // Finalize
            Controls.Add(groupEllipseShapeDetail);
            groupEllipseShapeDetail.ResumeLayout(false);
            groupEllipseShapeDetail.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)ellipseLineSizeTrack).EndInit();
            ((System.ComponentModel.ISupportInitialize)ellipseLineOpacityNum).EndInit();
            ((System.ComponentModel.ISupportInitialize)fillEllipseOpacityNum).EndInit();
            ResumeLayout(false);
        }

        private void InitializeComponentGroupPolygonShape()
        {
            groupPolygonShapeDetail.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)polygonLineSizeTrack).BeginInit();
            ((System.ComponentModel.ISupportInitialize)polygonLineOpacityNum).BeginInit();
            ((System.ComponentModel.ISupportInitialize)fillPolygonOpacityNum).BeginInit();
            SuspendLayout();

            // Add Controls to Group
            groupPolygonShapeDetail.Controls.Add(lblPolygonLineSizeValue);
            groupPolygonShapeDetail.Controls.Add(polygonLineSizeTrack);
            groupPolygonShapeDetail.Controls.Add(polygonLineOpacityNum);
            groupPolygonShapeDetail.Controls.Add(btnPolygonLineColor);
            groupPolygonShapeDetail.Controls.Add(cboPolygonLinePattern);
            groupPolygonShapeDetail.Controls.Add(fillPolygonOpacityNum);
            groupPolygonShapeDetail.Controls.Add(btnPolygonFillColorShape);
            groupPolygonShapeDetail.Controls.Add(labelPolygonLineSize);
            groupPolygonShapeDetail.Controls.Add(labelPolygonLineOpacity);
            groupPolygonShapeDetail.Controls.Add(labelPolygonLineColor);
            groupPolygonShapeDetail.Controls.Add(labelPolygonLinePattern);
            groupPolygonShapeDetail.Controls.Add(labelPolygonFillOpacity);
            groupPolygonShapeDetail.Controls.Add(labelPolygonFillColor);
            groupPolygonShapeDetail.Controls.Add(chkClosed);

            // GroupBox Settings
            groupPolygonShapeDetail.Location = new Point(12, 74);
            groupPolygonShapeDetail.Name = "groupShapeDetail";
            groupPolygonShapeDetail.Size = new Size(230, 260);
            groupPolygonShapeDetail.TabIndex = 30;
            groupPolygonShapeDetail.TabStop = false;
            groupPolygonShapeDetail.Text = "Polygon Detail";
            groupPolygonShapeDetail.Visible = false;

            // --- LINE SECTION ---

            // Line Size
            labelPolygonLineSize.AutoSize = true;
            labelPolygonLineSize.Location = new Point(10, 25);
            labelPolygonLineSize.Text = "Line Size:";

            polygonLineSizeTrack.Location = new Point(75, 22);
            polygonLineSizeTrack.Size = new Size(110, 45);
            polygonLineSizeTrack.Maximum = 200;
            polygonLineSizeTrack.Minimum = 1;
            polygonLineSizeTrack.TickStyle = TickStyle.None;
            polygonLineSizeTrack.Value = 2;
            polygonLineSizeTrack.Scroll += PolygonLineSize_Scroll;

            lblPolygonLineSizeValue.BackColor = Color.White;
            lblPolygonLineSizeValue.BorderStyle = BorderStyle.Fixed3D;
            lblPolygonLineSizeValue.Location = new Point(188, 22);
            lblPolygonLineSizeValue.Size = new Size(32, 22);
            lblPolygonLineSizeValue.TextAlign = ContentAlignment.MiddleCenter;
            lblPolygonLineSizeValue.Text = "2";

            labelPolygonLineOpacity.AutoSize = true;
            labelPolygonLineOpacity.Location = new Point(10, 73);
            labelPolygonLineOpacity.Text = "Line Opacity:";

            polygonLineOpacityNum.Location = new Point(125, 71);
            polygonLineOpacityNum.Size = new Size(60, 23);
            polygonLineOpacityNum.Value = new decimal([100, 0, 0, 0]);
            polygonLineOpacityNum.ValueChanged += OnPolygonLineOpacityValueChanged;

            labelPolygonLineColor.AutoSize = true;
            labelPolygonLineColor.Location = new Point(10, 103);
            labelPolygonLineColor.Text = "Line Color:";

            btnPolygonLineColor.BackColor = Color.Black;
            btnPolygonLineColor.FlatStyle = FlatStyle.Popup;
            btnPolygonLineColor.Location = new Point(125, 100);
            btnPolygonLineColor.Size = new Size(20, 20);
            btnPolygonLineColor.Click += BtnPolygonLineColor_Click;

            labelPolygonLinePattern.AutoSize = true;
            labelPolygonLinePattern.Location = new Point(10, 133);
            labelPolygonLinePattern.Text = "Pattern:";

            cboPolygonLinePattern.DropDownStyle = ComboBoxStyle.DropDownList;
            cboPolygonLinePattern.Items.AddRange(["Solid", "Dash", "Dot", "DashDot", "DashDotDot", "Custom"]);
            cboPolygonLinePattern.Location = new Point(125, 130);
            cboPolygonLinePattern.Size = new Size(100, 21);
            cboPolygonLinePattern.SelectedIndexChanged += CboPolygonLinePattern_SelectedIndexChanged;

            // --- FILL SECTION (All shifted down 8px) ---

            // Fill Opacity
            labelPolygonFillOpacity.AutoSize = true;
            labelPolygonFillOpacity.Location = new Point(10, 173);
            labelPolygonFillOpacity.Text = "Fill Opacity:";

            fillPolygonOpacityNum.Location = new Point(125, 171);
            fillPolygonOpacityNum.Size = new Size(60, 23);
            fillPolygonOpacityNum.Value = new decimal([100, 0, 0, 0]);
            fillPolygonOpacityNum.ValueChanged += FillPolygonOpacityNum_ValueChanged;

            // Fill Color
            labelPolygonFillColor.AutoSize = true;
            labelPolygonFillColor.Location = new Point(10, 203);
            labelPolygonFillColor.Text = "Fill Color:";

            btnPolygonFillColorShape.BackColor = Color.White;
            btnPolygonFillColorShape.FlatStyle = FlatStyle.Popup;
            btnPolygonFillColorShape.Location = new Point(125, 200);
            btnPolygonFillColorShape.Size = new Size(20, 20);
            btnPolygonFillColorShape.Click += BtnFillPolygonColorShape_Click;

            // Closed Checkbox
            chkClosed.AutoSize = true;
            chkClosed.Location = new Point(10, 233);
            chkClosed.Size = new Size(62, 19);
            chkClosed.TabIndex = 31;
            chkClosed.Text = "Closed";
            chkClosed.UseVisualStyleBackColor = true;
            chkClosed.CheckedChanged += ChkClosed_CheckedChanged;

            // Finalize
            Controls.Add(groupPolygonShapeDetail);
            groupPolygonShapeDetail.ResumeLayout(false);
            groupPolygonShapeDetail.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)polygonLineSizeTrack).EndInit();
            ((System.ComponentModel.ISupportInitialize)polygonLineOpacityNum).EndInit();
            ((System.ComponentModel.ISupportInitialize)fillPolygonOpacityNum).EndInit();
            ResumeLayout(false);
        }

        private void InitializeComponentGroupTextShape()
        {
            groupTextShapeDetail.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numTextFillOpacity).BeginInit();
            SuspendLayout();

            groupTextShapeDetail.Controls.Add(txtTextLineText);
            groupTextShapeDetail.Controls.Add(btnTextFont);
            groupTextShapeDetail.Controls.Add(numTextFillOpacity);
            groupTextShapeDetail.Controls.Add(btnTextFillColor);
            groupTextShapeDetail.Controls.Add(lblTextLineText);
            groupTextShapeDetail.Controls.Add(lblTextFont);
            groupTextShapeDetail.Controls.Add(lblTextFillOpacity);
            groupTextShapeDetail.Controls.Add(lblTextFillColor);

            groupTextShapeDetail.Location = new Point(12, 74);
            groupTextShapeDetail.Name = "groupTextShapeDetail";
            groupTextShapeDetail.Size = new Size(230, 260);
            groupTextShapeDetail.TabIndex = 30;
            groupTextShapeDetail.TabStop = false;
            groupTextShapeDetail.Text = "Text Detail";
            groupTextShapeDetail.Visible = false;

            lblTextLineText.AutoSize = true;
            lblTextLineText.Location = new Point(10, 27);
            lblTextLineText.Text = "Text";

            txtTextLineText.Location = new Point(125, 24);
            txtTextLineText.Size = new Size(60, 23);
            txtTextLineText.Text = "Text";
            txtTextLineText.TextAlign = HorizontalAlignment.Center;
            txtTextLineText.TextChanged += TxtTextLineSize_TextChanged;

            lblTextFont.AutoSize = true;
            lblTextFont.Location = new Point(10, 73);
            lblTextFont.Text = "Font:";

            btnTextFont.BackColor = Color.LightGray;
            btnTextFont.FlatStyle = FlatStyle.Popup;
            btnTextFont.Location = new Point(125, 68);
            btnTextFont.Size = new Size(60, 23);
            btnTextFont.Text = "";
            btnTextFont.UseVisualStyleBackColor = false;
            btnTextFont.Click += BtnTextFont_Click;

            lblTextFillOpacity.AutoSize = true;
            lblTextFillOpacity.Location = new Point(10, 123);
            lblTextFillOpacity.Text = "Fill Opacity:";

            numTextFillOpacity.Location = new Point(125, 121);
            numTextFillOpacity.Size = new Size(60, 23);
            numTextFillOpacity.Value = new decimal([100, 0, 0, 0]);
            numTextFillOpacity.ValueChanged += NumTextFillOpacity_ValueChanged;

            lblTextFillColor.AutoSize = true;
            lblTextFillColor.Location = new Point(10, 153);
            lblTextFillColor.Text = "Fill Color:";

            btnTextFillColor.BackColor = Color.Black;
            btnTextFillColor.FlatStyle = FlatStyle.Popup;
            btnTextFillColor.Location = new Point(125, 150);
            btnTextFillColor.Size = new Size(20, 20);
            btnTextFillColor.Click += BtnTextFillColor_Click;

            Controls.Add(groupTextShapeDetail);
            groupTextShapeDetail.ResumeLayout(false);
            groupTextShapeDetail.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)numTextFillOpacity).EndInit();
            ResumeLayout(false);
        }

        private void TxtTextLineSize_TextChanged(object? sender, EventArgs e)
        {
            if (layersControl.GetLayer(layersControl.GetSelectedLayerIndex())?.CurrentShape is ShapeText textShape)
            {
                textShape.Content = txtTextLineText.Text;
                RedrawImage();
            }
        }

        private void BtnTextFont_Click(object? sender, EventArgs e)
        {
            using FontDialog fontDialog = new();
            if (fontDialog.ShowDialog() == DialogResult.OK)
            {
                btnTextFont.Font = fontDialog.Font;
                if (layersControl.GetLayer(layersControl.GetSelectedLayerIndex())?.CurrentShape is ShapeText textShape)
                {
                    textShape.FontFamily = btnTextFont.Font.Name;
                    textShape.IsBold = btnTextFont.Font.Bold;
                    textShape.IsItalic = btnTextFont.Font.Italic;
                    textShape.FontSize = btnTextFont.Font.Size;
                }
                NumTextFillOpacity_ValueChanged(sender, e);
            }
        }

        private void NumTextFillOpacity_ValueChanged(object? sender, EventArgs e)
        {
            if (layersControl.GetLayer(layersControl.GetSelectedLayerIndex())?.CurrentShape is ShapeText shape)
            {
                shape.FillColor = Color.FromArgb((int)(255 * (numTextFillOpacity.Value / numTextFillOpacity.Maximum)), btnTextFillColor.BackColor);

                RedrawImage();
            }
        }

        private void BtnTextFillColor_Click(object? sender, EventArgs e)
        {
            using ColorDialog cd = new();
            cd.Color = btnTextFillColor.BackColor;
            if (cd.ShowDialog() == DialogResult.OK)
            {
                btnTextFillColor.BackColor = cd.Color;
                FillTextOpacityNum_ValueChanged(sender, e);
            }
        }

        private void CboRectLinePattern_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (layersControl.GetLayer(layersControl.GetSelectedLayerIndex())?.CurrentShape is ShapeRect shape)
            {
                shape.DashStyle = ManipulatorGeneral.GetDashStyle(cboRectLinePattern.Text);
                RedrawImage();
            }
        }

        private void CboEllipseLinePattern_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (layersControl.GetLayer(layersControl.GetSelectedLayerIndex())?.CurrentShape is ShapeEllipse shape)
            {
                shape.DashStyle = ManipulatorGeneral.GetDashStyle(cboEllipseLinePattern.Text);
                RedrawImage();
            }
        }

        private void CboPolygonLinePattern_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (layersControl.GetLayer(layersControl.GetSelectedLayerIndex())?.CurrentShape is ShapePolygon shape)
            {
                shape.DashStyle = ManipulatorGeneral.GetDashStyle(cboPolygonLinePattern.Text);
                RedrawImage();
            }
        }

        private void FillRectOpacityNum_ValueChanged(object? sender, EventArgs e)
        {
            if (layersControl.GetLayer(layersControl.GetSelectedLayerIndex())?.CurrentShape is ShapeRect shape)
            {
                shape.FillColor = Color.FromArgb((int)(255 * (fillRectOpacityNum.Value / fillRectOpacityNum.Maximum)), btnRectFillColorShape.BackColor);
                RedrawImage();
            }
        }

        private void FillEllipseOpacityNum_ValueChanged(object? sender, EventArgs e)
        {
            if (layersControl.GetLayer(layersControl.GetSelectedLayerIndex())?.CurrentShape is ShapeEllipse shape)
            {
                shape.FillColor = Color.FromArgb((int)(255 * (fillEllipseOpacityNum.Value / fillEllipseOpacityNum.Maximum)), btnEllipseFillColorShape.BackColor);
                RedrawImage();
            }
        }

        private void FillPolygonOpacityNum_ValueChanged(object? sender, EventArgs e)
        {
            if (layersControl.GetLayer(layersControl.GetSelectedLayerIndex())?.CurrentShape is ShapePolygon shape)
            {
                shape.FillColor = Color.FromArgb((int)(255 * (fillPolygonOpacityNum.Value / fillPolygonOpacityNum.Maximum)), btnPolygonFillColorShape.BackColor);
                RedrawImage();
            }
        }

        private void FillTextOpacityNum_ValueChanged(object? sender, EventArgs e)
        {
            if (layersControl.GetLayer(layersControl.GetSelectedLayerIndex())?.CurrentShape is ShapeText shape)
            {
                shape.FillColor = Color.FromArgb((int)(255 * (numTextFillOpacity.Value / numTextFillOpacity.Maximum)), btnTextFillColor.BackColor);
                RedrawImage();
            }
        }

        private void OnRectLineOpacityValueChanged(object? sender, EventArgs e)
        {
            if (layersControl.GetLayer(layersControl.GetSelectedLayerIndex())?.CurrentShape is ShapeRect shape)
            {
                shape.LineColor = Color.FromArgb((int)(255 * (rectLineOpacityNum.Value / rectLineOpacityNum.Maximum)), btnRectLineColor.BackColor);
                RedrawImage();
            }
        }

        private void OnEllipseLineOpacityValueChanged(object? sender, EventArgs e)
        {
            if (layersControl.GetLayer(layersControl.GetSelectedLayerIndex())?.CurrentShape is ShapeEllipse shape)
            {
                shape.LineColor = Color.FromArgb((int)(255 * (ellipseLineOpacityNum.Value / ellipseLineOpacityNum.Maximum)), btnEllipseLineColor.BackColor);
                RedrawImage();
            }
        }

        private void OnPolygonLineOpacityValueChanged(object? sender, EventArgs e)
        {
            if (layersControl.GetLayer(layersControl.GetSelectedLayerIndex())?.CurrentShape is ShapePolygon shape)
            {
                shape.LineColor = Color.FromArgb((int)(255 * (polygonLineOpacityNum.Value / polygonLineOpacityNum.Maximum)), btnPolygonLineColor.BackColor);
                RedrawImage();
            }
        }

        private void BtnFillRectColorShape_Click(object? sender, EventArgs e)
        {
            using ColorDialog cd = new();
            cd.Color = btnRectFillColorShape.BackColor;
            if (cd.ShowDialog() == DialogResult.OK)
            {
                btnRectFillColorShape.BackColor = cd.Color;
                FillRectOpacityNum_ValueChanged(sender, e);
            }
        }

        private void BtnFillEllipseColorShape_Click(object? sender, EventArgs e)
        {
            using ColorDialog cd = new();
            cd.Color = btnEllipseFillColorShape.BackColor;
            if (cd.ShowDialog() == DialogResult.OK)
            {
                btnEllipseFillColorShape.BackColor = cd.Color;
                FillEllipseOpacityNum_ValueChanged(sender, e);
            }
        }

        private void BtnFillPolygonColorShape_Click(object? sender, EventArgs e)
        {
            using ColorDialog cd = new();
            cd.Color = btnPolygonFillColorShape.BackColor;
            if (cd.ShowDialog() == DialogResult.OK)
            {
                btnPolygonFillColorShape.BackColor = cd.Color;
                FillPolygonOpacityNum_ValueChanged(sender, e);
            }
        }

        private void BtnRectLineColor_Click(object? sender, EventArgs e)
        {
            using ColorDialog cd = new();
            cd.Color = btnRectLineColor.BackColor;
            if (cd.ShowDialog() == DialogResult.OK)
            {
                btnRectLineColor.BackColor = cd.Color;
                OnRectLineOpacityValueChanged(sender, e);
            }
        }

        private void BtnEllipseLineColor_Click(object? sender, EventArgs e)
        {
            using ColorDialog cd = new();
            cd.Color = btnEllipseLineColor.BackColor;
            if (cd.ShowDialog() == DialogResult.OK)
            {
                btnEllipseLineColor.BackColor = cd.Color;
                OnEllipseLineOpacityValueChanged(sender, e);
            }
        }

        private void BtnPolygonLineColor_Click(object? sender, EventArgs e)
        {
            using ColorDialog cd = new();
            cd.Color = btnPolygonLineColor.BackColor;
            if (cd.ShowDialog() == DialogResult.OK)
            {
                btnPolygonLineColor.BackColor = cd.Color;
                OnPolygonLineOpacityValueChanged(sender, e);
            }
        }

        private void RectLineSize_Scroll(object? sender, EventArgs e)
        {
            lblRectLineSizeValue.Text = rectLineSizeTrack.Value.ToString();

            if (layersControl.GetLayer(layersControl.GetSelectedLayerIndex())?.CurrentShape is ShapeRect shape)
            {
                shape.LineWidth = rectLineSizeTrack.Value;
                RedrawImage();
            }
        }

        private void EllipseLineSize_Scroll(object? sender, EventArgs e)
        {
            lblEllipseLineSizeValue.Text = ellipseLineSizeTrack.Value.ToString();

            if (layersControl.GetLayer(layersControl.GetSelectedLayerIndex())?.CurrentShape is ShapeEllipse shape)
            {
                shape.LineWidth = ellipseLineSizeTrack.Value;
                RedrawImage();
            }
        }

        private void PolygonLineSize_Scroll(object? sender, EventArgs e)
        {
            lblPolygonLineSizeValue.Text = polygonLineSizeTrack.Value.ToString();

            if (layersControl.GetLayer(layersControl.GetSelectedLayerIndex())?.CurrentShape is ShapePolygon shape)
            {
                shape.LineWidth = polygonLineSizeTrack.Value;
                RedrawImage();
            }
        }

        private void ChkClosed_CheckedChanged(object? sender, EventArgs e)
        {
            if (layersControl.GetLayer(layersControl.GetSelectedLayerIndex())?.CurrentShape is ShapePolygon shape)
            {
                shape.IsClosed = chkClosed.Checked;
                RedrawImage();
            }
        }

        private void FormMain_Load(object? sender, EventArgs e)
        {
            LoadNewDocument(true);
            HistoryManager.RecordState(new HistoryItem(layersControl.GetLayers(), layersControl.GetSelectedLayerIndex()));
            Form1_Resize(sender, e);
            UpdateTitleBar();
            cboBlendMode.Items.Clear();
            cboBlendMode.Items.AddRange(Enum.GetNames<ImageBlending>());
            cboBlendMode.SelectedIndex = 0;
            cboFillBlendMode.Items.Clear();
            cboFillBlendMode.Items.AddRange(Enum.GetNames<ImageBlending>());
            cboFillBlendMode.SelectedIndex = 0;
            cboFIllGradient.SelectedIndex = 0;
            Brush_size_Scroll(sender, e);
            Brush_opacity_Scroll(sender, e);
            Brush_smoothness_Scroll(sender, e);
            Brush_hardness_Scroll(sender, e);
            Eraser_size_Scroll(sender, e);
            Eraser_opacity_Scroll(sender, e);
            Eraser_smoothness_Scroll(sender, e);
            Eraser_hardness_Scroll(sender, e);
            ReloadBrushes();
            HistoryManager.CurrentState(new HistoryItem(layersControl.GetLayers(), layersControl.GetSelectedLayerIndex()));
        }

        private void LayersControl_LayerChanged(object? sender, LayerChangedEventArgs e)
        {
            Console.WriteLine($"Layer {e.Layer.Name}'s properties changed");

            if (e.LayerTypeChanged)
            {
                Console.WriteLine($"Layer {e.Layer.Name}'s type changed to {e.Layer.LayerType}.");
            }

            HistoryManager.RecordState(new HistoryItem(layersControl.GetLayers(), layersControl.GetSelectedLayerIndex()));
            RedrawImage();
            UpdateControls();
            HistoryManager.CurrentState(new HistoryItem(layersControl.GetLayers(), layersControl.GetSelectedLayerIndex()));
        }

        private void LayersControl_LayerVisibilityChanged(object? sender, LayerVisibilityChangedEventArgs e)
        {
            Console.WriteLine($"Layer {e.Layer.Name}'s visibility changed from {e.OldValue} to {e.NewValue}");

            HistoryManager.RecordState(new HistoryItem(layersControl.GetLayers(), layersControl.GetSelectedLayerIndex()));
            RedrawImage();
            HistoryManager.CurrentState(new HistoryItem(layersControl.GetLayers(), layersControl.GetSelectedLayerIndex()));
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
                    PictureBox picBrush = new()
                    {
                        Image = new Bitmap(file),
                        Size = new Size(24, 24),
                        Location = new Point(x, y),
                        SizeMode = PictureBoxSizeMode.StretchImage,
                        BorderStyle = BorderStyle.None,
                        Tag = index++
                    };
                    picBrush.Click += PicBrush_Click;
                    panelBrush.Controls.Add(picBrush);

                    PictureBox picEraser = new()
                    {
                        Image = new Bitmap(file),
                        Size = new Size(24, 24),
                        Location = new Point(x, y),
                        SizeMode = PictureBoxSizeMode.StretchImage,
                        BorderStyle = BorderStyle.None,
                        Tag = index++
                    };
                    picEraser.Click += PicEraser_Click;
                    panelEraser.Controls.Add(picEraser);

                    brushes.Add(new Bitmap(picBrush.Image));
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

        private void PicBrush_Click(object? sender, EventArgs e)
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

        private void PicEraser_Click(object? sender, EventArgs e)
        {
            try
            {
                if (sender is not null && sender is PictureBox brush)
                {
                    if (brush.Image is not null)
                    {
                        selectedEraserIndex = brush.Tag != null ? (int)brush.Tag : -1;
                        paint.Brush = new Bitmap(brush.Image);
                        if (paint.Brush != null)
                        {
                            paint.Reset(btnEraserColor.BackColor, paint.GetRadius() * (eraser_hardness.Maximum - eraser_hardness.Value) / eraser_hardness.Maximum);
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
                Color = btnPenColor.BackColor,
                FullOpen = true
            };
            if (c.ShowDialog() == DialogResult.OK)
            {
                btnPenColor.BackColor = c.Color;
                paint.Reset(btnPenColor.BackColor, paint.GetRadius() * (brush_hardness.Maximum - brush_hardness.Value) / brush_hardness.Maximum);
                if (paint.Brush != null)
                {
                    PaintingEngine.SetBrush(paint);
                    int cursorWidth = 2 * paint.Brush.Width * brush_size.Value / brush_size.Maximum;
                    int cursorHeight = 2 * paint.Brush.Height * brush_size.Value / brush_size.Maximum;
                    UpdateCursor(cursorWidth, cursorHeight);
                }
            }
        }

        private void BtnEraserColor_Click(object? sender, EventArgs e)
        {
            ColorDialog c = new()
            {
                Color = btnEraserColor.BackColor,
                FullOpen = true
            };
            if (c.ShowDialog() == DialogResult.OK)
            {
                btnEraserColor.BackColor = c.Color;
                paint.Reset(btnEraserColor.BackColor, paint.GetRadius() * (brush_hardness.Maximum - eraser_hardness.Value) / eraser_hardness.Maximum);
                if (paint.Brush != null)
                {
                    PaintingEngine.SetBrush(paint);
                    int cursorWidth = 2 * paint.Brush.Width * eraser_size.Value / eraser_size.Maximum;
                    int cursorHeight = 2 * paint.Brush.Height * eraser_size.Value / eraser_size.Maximum;
                    UpdateCursor(cursorWidth, cursorHeight);
                }
            }
        }

        private void BtnFillColor_Click(object? sender, EventArgs e)
        {
            ColorDialog c = new()
            {
                Color = btnFillColor.BackColor,
                FullOpen = true
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
                Color = btnFillColor.BackColor,
                FullOpen = true
            };
            if (c.ShowDialog() == DialogResult.OK)
            {
            }
        }

        private void BtnTools_Click(object? sender, EventArgs e)
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
                if (btn != btnEraser)
                {
                    btnEraser.Checked = false;
                    groupEraserDetail.Visible = false;
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
                if (btn != btnWarp)
                {
                    btnWarp.Checked = false;
                    groupWarpDetail.Visible = false;
                }
                if (btn != btnCrop)
                {
                    btnCrop.Checked = false;
                    groupCropDetail.Visible = false;
                }
                if (btn != btnShapeRect)
                {
                    btnShapeRect.Checked = false;
                    groupRectShapeDetail.Visible = false;
                }
                if (btn != btnShapeEllipse)
                {
                    btnShapeEllipse.Checked = false;
                    groupEllipseShapeDetail.Visible = false;
                }
                if (btn != btnShapePolygon)
                {
                    btnShapePolygon.Checked = false;
                    groupPolygonShapeDetail.Visible = false;
                }
                if (btn != btnShapeText)
                {
                    btnShapeText.Checked = false;
                    groupTextShapeDetail.Visible = false;
                }
            }
        }

        private void BtnTools_CheckedChanged(object? sender, EventArgs e)
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
                if (paint.Brush != null)
                {
                    int cursorWidth = 2 * paint.Brush.Width * brush_size.Value / brush_size.Maximum;
                    int cursorHeight = 2 * paint.Brush.Height * brush_size.Value / brush_size.Maximum;
                    UpdateCursor(cursorWidth, cursorHeight);
                }
            }
            else if (btnEraser.Checked)
            {
                paint.Brush = brushes.Count > selectedEraserIndex && selectedEraserIndex >= 0 ? new Bitmap(brushes[selectedEraserIndex]) : null;
                paint.Reset(btnEraserColor.BackColor, paint.GetRadius() * (eraser_hardness.Maximum - eraser_hardness.Value) / eraser_hardness.Maximum);
                PaintingEngine.SetBrush(paint);
                groupEraserDetail.Visible = true;
                if (paint.Brush != null)
                {
                    int cursorWidth = 2 * paint.Brush.Width * eraser_size.Value / eraser_size.Maximum;
                    int cursorHeight = 2 * paint.Brush.Height * eraser_size.Value / eraser_size.Maximum;
                    UpdateCursor(cursorWidth, cursorHeight);
                }
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
            else if (btnWarp.Checked)
            {
                PaintingEngine.SetBrush(paint);
                groupWarpDetail.Visible = true;
                UpdateCursor(btnWarp.Image);
            }
            else if (btnCrop.Checked)
            {
                PaintingEngine.SetBrush(paint);
                groupCropDetail.Visible = true;
                UpdateCursor(btnCrop.Image);
            }
            else if (btnShapeRect.Checked)
            {
                PaintingEngine.SetBrush(paint);
                groupRectShapeDetail.Visible = true;
                canvas.Cursor = Cursors.Default;
            }
            else if (btnShapeEllipse.Checked)
            {
                PaintingEngine.SetBrush(paint);
                groupEllipseShapeDetail.Visible = true;
                canvas.Cursor = Cursors.Default;
            }
            else if (btnShapePolygon.Checked)
            {
                PaintingEngine.SetBrush(paint);
                groupPolygonShapeDetail.Visible = true;
                canvas.Cursor = Cursors.Default;
            }
            else if (btnShapeText.Checked)
            {
                PaintingEngine.SetBrush(paint);
                groupTextShapeDetail.Visible = true;
                canvas.Cursor = Cursors.Default;
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
            canvas.Cursor = Cursors.Default;
        }

        private void UpdateCursor(int cursorWidth, int cursorHeight)
        {
            if (paint.Brush != null)
            {
                canvas.Cursor.Dispose();

                int hotSpotX = cursorWidth / 2;
                int hotSpotY = cursorHeight / 2;

                canvas.Cursor = GetCursor(new Bitmap(paint.Brush, cursorWidth, cursorHeight), hotSpotX, hotSpotY);
            }
            else
            {
                canvas.Cursor = Cursors.Default;
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
                "topMid" or "bottomMid" => Cursors.SizeNS,
                "leftMid" or "rightMid" => Cursors.SizeWE,
                _ => Cursors.SizeAll,
            };
        }

        private void Brush_size_Scroll(object? sender, EventArgs e)
        {
            lblBrushSize.Text = $"{brush_size.Value}";
            if (paint.Brush != null)
            {
                int cursorWidth = 2 * paint.Brush.Width * brush_size.Value / brush_size.Maximum;
                int cursorHeight = 2 * paint.Brush.Height * brush_size.Value / brush_size.Maximum;
                UpdateCursor(cursorWidth, cursorHeight);
            }
        }

        private void Eraser_size_Scroll(object? sender, EventArgs e)
        {
            lblEraserSize.Text = $"{eraser_size.Value}";
            if (paint.Brush != null)
            {
                int cursorWidth = 2 * paint.Brush.Width * eraser_size.Value / eraser_size.Maximum;
                int cursorHeight = 2 * paint.Brush.Height * eraser_size.Value / eraser_size.Maximum;
                UpdateCursor(cursorWidth, cursorHeight);
            }
        }

        private void Brush_opacity_Scroll(object? sender, EventArgs e)
        {
            lblBrushOpacity.Text = $"{brush_opacity.Value}";
        }

        private void Eraser_opacity_Scroll(object? sender, EventArgs e)
        {
            lblEraserOpacity.Text = $"{eraser_opacity.Value}";
        }

        private void Brush_smoothness_Scroll(object? sender, EventArgs e)
        {
            lblBrushSmoothness.Text = $"{brush_smoothness.Value}";
        }

        private void Eraser_smoothness_Scroll(object? sender, EventArgs e)
        {
            lblEraserSmoothness.Text = $"{eraser_smoothness.Value}";
        }

        private void Brush_hardness_Scroll(object? sender, EventArgs e)
        {
            lblBrushHardness.Text = $"{brush_hardness.Value}";
            if (paint.Brush != null)
            {
                paint.Reset(btnPenColor.BackColor, paint.GetRadius() * (brush_hardness.Maximum - brush_hardness.Value) / brush_hardness.Maximum);
                PaintingEngine.SetBrush(paint);
                int cursorWidth = 2 * paint.Brush.Width * brush_size.Value / brush_size.Maximum;
                int cursorHeight = 2 * paint.Brush.Height * brush_size.Value / brush_size.Maximum;
                UpdateCursor(cursorWidth, cursorHeight);
            }
        }

        private void Eraser_hardness_Scroll(object? sender, EventArgs e)
        {
            lblEraserHardness.Text = $"{eraser_hardness.Value}";
            if (paint.Brush != null)
            {
                paint.Reset(btnEraserColor.BackColor, paint.GetRadius() * (eraser_hardness.Maximum - eraser_hardness.Value) / eraser_hardness.Maximum);
                PaintingEngine.SetBrush(paint);
                int cursorWidth = 2 * paint.Brush.Width * eraser_size.Value / eraser_size.Maximum;
                int cursorHeight = 2 * paint.Brush.Height * eraser_size.Value / eraser_size.Maximum;
                UpdateCursor(cursorWidth, cursorHeight);
            }
        }

        private void SelectionThreshold_Scroll(object? sender, EventArgs e)
        {
        }

        private void WarpBrushSize_Scroll(object? sender, EventArgs e)
        {
            lblWarpBrushSize.Text = warpBrushSize.Value.ToString();
        }

        private void BtnCrop_Click(object? sender, EventArgs e)
        {
            if (!ImageSelections.ContainsSelection()) return;

            ImageSelections.CalculateSelectionBounds();
            RectangleF cropRect = ImageSelections.GetSelectionBounds();

            if (cropRect.Width <= 0 || cropRect.Height <= 0) return;

            foreach (var layer in layersControl.GetLayers())
            {
                if (layer.Image is Bitmap oldBmp)
                {
                    RectangleF intersection = RectangleF.Intersect(new Rectangle(layer.X, layer.Y, oldBmp.Width, oldBmp.Height), cropRect);

                    if (intersection.Width > 0 && intersection.Height > 0)
                    {
                        Bitmap croppedBmp = new((int)intersection.Width, (int)intersection.Height);
                        using (Graphics g = Graphics.FromImage(croppedBmp))
                        {
                            g.DrawImage(oldBmp,
                                new Rectangle(0, 0, (int)intersection.Width, (int)intersection.Height),
                                new Rectangle((int)(intersection.X - layer.X), (int)(intersection.Y - layer.Y), (int)intersection.Width, (int)intersection.Height),
                                GraphicsUnit.Pixel);
                        }

                        layer.Image = croppedBmp;
                        layer.X = (int)(intersection.X - cropRect.X);
                        layer.Y = (int)(intersection.Y - cropRect.Y);
                        oldBmp.Dispose();
                    }
                    else
                    {
                        layer.Image = new Bitmap(1, 1);
                    }
                }
            }

            Document.Width = (int)cropRect.Width;
            Document.Height = (int)cropRect.Height;

            HistoryManager.RecordState(new HistoryItem(layersControl.GetLayers(), layersControl.GetSelectedLayerIndex()));

            ImageSelections.ClearSelections();
            ManipulatorGeneral.InvalidateCompositeBuffers();
            RedrawImage();

            labelStatus.Text = "Image Cropped.";
        }

        private void Form1_Resize(object? sender, EventArgs e)
        {
            if (WindowState != FormWindowState.Minimized)
            {
                canvas.Size = new Size(ClientSize.Width - canvas.Location.X - 240, ClientSize.Height - canvas.Location.Y - 40);
                RedrawImage();
            }
        }

        private void BtnBrowseImage_Click(object? sender, EventArgs e)
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
                        redToolStripMenuItem1.Checked = false;
                        greenToolStripMenuItem1.Checked = false;
                        blueToolStripMenuItem1.Checked = false;

                        var layer = new Layer($"layer {layersControl.GetLayers().Count + 1}", true)
                        {
                            Image = new Bitmap(image, image.Width, image.Height),
                            X = Document.Width / 2 - image.Width / 2,
                            Y = Document.Height / 2 - image.Height / 2,
                            FillType = FillType.Transparency
                        };
                        layersControl.InsertLayer(0, layer);

                        RedrawImage();
                    }
                    image.Dispose();
                }
            }
        }

        private void PNGPictureToolStripMenuItem_Click(object? sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog1 = new()
            {
                Filter = "PNG Image|*.png"
            };
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    ManipulatorGeneral.PopulateColorGrid(layersControl.GetLayers(), -1, includeBackground: false);

                    var rect = new Rectangle(0, 0, Document.Width, Document.Height);

                    Bitmap bitmap = ManipulatorGeneral.GetImage(ManipulatorGeneral.Screen, rect);
                    bitmap.Save(saveFileDialog1.FileName, System.Drawing.Imaging.ImageFormat.Png);
                    labelStatus.Text = "Image saved successfully!";
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error saving image: {ex.Message}");
                }
            }
        }

        private void JPEGPictureToolStripMenuItem_Click(object? sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog1 = new()
            {
                Filter = "JPEG Image|*.jpg;*.jpeg"
            };
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    ManipulatorGeneral.PopulateColorGrid(layersControl.GetLayers(), -1, includeBackground: false);

                    var rect = new Rectangle(0, 0, Document.Width, Document.Height);

                    Bitmap bitmap = ManipulatorGeneral.GetImage(ManipulatorGeneral.Screen, rect);
                    bitmap.Save(saveFileDialog1.FileName, System.Drawing.Imaging.ImageFormat.Jpeg);
                    labelStatus.Text = "Image saved successfully!";
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error saving image: {ex.Message}");
                }
            }
        }

        private void BMPPictureToolStripMenuItem_Click(object? sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog1 = new()
            {
                Filter = "BMP Image|*.bmp"
            };
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    ManipulatorGeneral.PopulateColorGrid(layersControl.GetLayers(), -1, includeBackground: false);

                    var rect = new Rectangle(0, 0, Document.Width, Document.Height);

                    Bitmap bitmap = ManipulatorGeneral.GetImage(ManipulatorGeneral.Screen, rect);
                    bitmap.Save(saveFileDialog1.FileName, System.Drawing.Imaging.ImageFormat.Bmp);
                    labelStatus.Text = "Image saved successfully!";
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error saving image: {ex.Message}");
                }
            }
        }

        private void GIFPictureToolStripMenuItem_Click(object? sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog1 = new()
            {
                Filter = "GIF Image|*.gif"
            };
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    ManipulatorGeneral.PopulateColorGrid(layersControl.GetLayers(), -1, includeBackground: false);

                    var rect = new Rectangle(0, 0, Document.Width, Document.Height);

                    Bitmap bitmap = ManipulatorGeneral.GetImage(ManipulatorGeneral.Screen, rect);
                    bitmap.Save(saveFileDialog1.FileName, System.Drawing.Imaging.ImageFormat.Gif);
                    labelStatus.Text = "Image saved successfully!";
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error saving image: {ex.Message}");
                }
            }
        }

        private void NewToolStripMenuItem_Click(object? sender, EventArgs e)
        {
            LoadNewDocument(false);
        }

        private void LoadNewDocument(bool appLaunch)
        {
            FormSettings frm = new()
            {
                StartPosition = FormStartPosition.CenterParent,
                LayerHeight = Document.Height,
                LayerWidth = Document.Width
            };
            if (frm.ShowDialog(this) == DialogResult.OK)
            {
                if (!ConfirmAbandonChanges()) return;

                Document.Zoom = 0.95f;
                Document.ImageOffset = new PointF(0, 0);

                HistoryManager.Clear();
                layersControl.ClearLayers();

                Layer selectedLayer = new($"layer {layersControl.GetLayers().Count + 1}", true)
                {
                    X = frm.LayerX,
                    Y = frm.LayerY,
                    FillType = FillType.Transparency
                };

                if (selectedLayer.Image == null)
                {
                    selectedLayer.Image = ManipulatorGeneral.GetImage(selectedLayer.FillType == FillType.Transparency ? Color.Transparent : selectedLayer.FillColor, frm.LayerWidth, frm.LayerHeight);
                }
                else
                {
                    if (frm.ResizeImage)
                    {
                        selectedLayer.Image = ManipulatorLighting.ResizeImage(selectedLayer.Image, frm.LayerWidth, frm.LayerHeight);
                    }
                    else
                    {
                        selectedLayer.Image = ManipulatorLighting.CropFromCenter(selectedLayer.Image, frm.LayerWidth, frm.LayerHeight);
                    }
                }

                layersControl.InsertLayer(0, selectedLayer);
                layersControl.RefreshLayersDisplay();

                currentFilePath = "";
                isDirty = false;
                UpdateTitleBar();
                RedrawImage();
            }
            else
            {
                if (appLaunch)
                {
                    Application.Exit();
                }
            }
        }

        private void OpenToolStripMenuItem_Click(object? sender, EventArgs e)
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

        private void SaveToolStripMenuItem_Click(object? sender, EventArgs e)
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

        private void SaveAsProjectToolStripMenuItem_Click(object? sender, EventArgs e)
        {
            using SaveFileDialog sfd = new();
            sfd.Filter = "Paint++ Files (*.xpe)|*.xpe";

            if (sfd.ShowDialog() == DialogResult.OK)
            {
                PerformSave(sfd.FileName);
            }
        }

        private void CloseToolStripMenuItem_Click(object? sender, EventArgs e)
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

                labelStatus.Text = " Saved Successfully!";
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

        private void Opacity_Leave(object? sender, EventArgs e)
        {
            Layer? layer = layersControl.GetLayer(layersControl.GetSelectedLayerIndex());
            if (layer != null)
            {
                layer.Opacity = (int)opacity.Value;
                layersControl.UpdateLayer(layersControl.GetSelectedLayerIndex(), layer);
                RedrawImage();
            }
        }

        private void CboBlendMode_SelectedIndexChanged(object? sender, EventArgs e)
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

        private void BthUpdateLayer_Click(object? sender, EventArgs e)
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
                    HistoryManager.RecordState(new HistoryItem(layersControl.GetLayers(), layersControl.GetSelectedLayerIndex()));
                    layersControl.UpdateLayer(layersControl.GetSelectedLayerIndex(), frm.Layer);
                    UpdateControls();
                    RedrawImage();
                    HistoryManager.CurrentState(new HistoryItem(layersControl.GetLayers(), layersControl.GetSelectedLayerIndex()));
                }
            }
        }

        private void AboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FormAbout frm = new()
            {
            };

            frm.ShowDialog(this);
        }

        private void ToolStripMenuFlipHorizontal_Click(object sender, EventArgs e)
        {
            var selectedLayer = layersControl.GetLayer(layersControl.GetSelectedLayerIndex());

            if (selectedLayer != null)
            {
                if (selectedLayer.LayerType == LayerType.Image && selectedLayer.Image != null)
                {
                    HistoryManager.RecordState(new HistoryItem(layersControl.GetLayers(), layersControl.GetSelectedLayerIndex()));

                    Bitmap flippedBitmap = new(selectedLayer.Image.Width, selectedLayer.Image.Height);
                    using (Graphics g = Graphics.FromImage(flippedBitmap))
                    {
                        g.DrawImage(selectedLayer.Image,
                            new Rectangle(0, 0, selectedLayer.Image.Width, selectedLayer.Image.Height),
                            new Rectangle(selectedLayer.Image.Width, 0, -selectedLayer.Image.Width, selectedLayer.Image.Height),
                            GraphicsUnit.Pixel);
                    }
                    selectedLayer.Image.Dispose();
                    selectedLayer.Image = flippedBitmap;

                    RedrawImage();

                    HistoryManager.CurrentState(new HistoryItem(layersControl.GetLayers(), layersControl.GetSelectedLayerIndex()));
                }
            }
        }

        private void ToolStripMenuFlipVertical_Click(object sender, EventArgs e)
        {
            var selectedLayer = layersControl.GetLayer(layersControl.GetSelectedLayerIndex());

            if (selectedLayer != null)
            {
                if (selectedLayer.LayerType == LayerType.Image && selectedLayer.Image != null)
                {
                    HistoryManager.RecordState(new HistoryItem(layersControl.GetLayers(), layersControl.GetSelectedLayerIndex()));

                    Bitmap flippedBitmap = new(selectedLayer.Image.Width, selectedLayer.Image.Height);
                    using (Graphics g = Graphics.FromImage(flippedBitmap))
                    {
                        g.DrawImage(selectedLayer.Image,
                            new Rectangle(0, 0, selectedLayer.Image.Width, selectedLayer.Image.Height),
                            new Rectangle(0, selectedLayer.Image.Height, selectedLayer.Image.Width, -selectedLayer.Image.Height),
                            GraphicsUnit.Pixel);
                    }
                    selectedLayer.Image.Dispose();
                    selectedLayer.Image = flippedBitmap;

                    RedrawImage();

                    HistoryManager.CurrentState(new HistoryItem(layersControl.GetLayers(), layersControl.GetSelectedLayerIndex()));
                }
            }
        }

        private void ToolStripMenuRotate90CW_Click(object sender, EventArgs e)
        {
            HistoryManager.RecordState(new HistoryItem(layersControl.GetLayers(), layersControl.GetSelectedLayerIndex()));

            RotateSelectedLayerImage(rotationAngle + 90);

            HistoryManager.CurrentState(new HistoryItem(layersControl.GetLayers(), layersControl.GetSelectedLayerIndex()));
        }

        private void ToolStripMenuRotate90CWW_Click(object sender, EventArgs e)
        {
            HistoryManager.RecordState(new HistoryItem(layersControl.GetLayers(), layersControl.GetSelectedLayerIndex()));

            RotateSelectedLayerImage(rotationAngle - 90);

            HistoryManager.CurrentState(new HistoryItem(layersControl.GetLayers(), layersControl.GetSelectedLayerIndex()));
        }

        private void ToolStripMenuRotate180_Click(object sender, EventArgs e)
        {
            HistoryManager.RecordState(new HistoryItem(layersControl.GetLayers(), layersControl.GetSelectedLayerIndex()));

            RotateSelectedLayerImage(rotationAngle + 180);

            HistoryManager.CurrentState(new HistoryItem(layersControl.GetLayers(), layersControl.GetSelectedLayerIndex()));
        }

        private void ToolStripMenuRotate_Click(object sender, EventArgs e)
        {
            HistoryManager.RecordState(new HistoryItem(layersControl.GetLayers(), layersControl.GetSelectedLayerIndex()));

            using (FormRotate frm = new())
            {
                frm.StartPosition = FormStartPosition.CenterParent;
                frm.RotationAngle = rotationAngle;
                if (frm.ShowDialog() == DialogResult.OK)
                {
                    RotateSelectedLayerImage(rotationAngle + frm.RotationAngle);
                }
            }

            HistoryManager.CurrentState(new HistoryItem(layersControl.GetLayers(), layersControl.GetSelectedLayerIndex()));
        }

        private void RotateSelectedLayerImage(float angle)
        {
            var selectedLayer = layersControl.GetLayer(layersControl.GetSelectedLayerIndex());

            if (selectedLayer != null)
            {
                if (selectedLayer.LayerType == LayerType.Image && selectedLayer.Image != null)
                {
                    if (ImageSelections.ContainsSelection() && selectedAreaBitmap == null)
                    {
                        ImageSelections.CalculateSelectionBounds();
                        selectedAreaBitmap = ManipulatorGeneral.ExtractSelectedArea(selectedLayer);
                        selectedLayer.Image = ManipulatorGeneral.CutSelectionFromLayer(selectedLayer) ?? new Bitmap(selectedLayer.Image.Width, selectedLayer.Image.Height);
                    }

                    if (selectedAreaBitmap != null)
                    {
                        ImageSelections.RotateSelections(-rotationAngle);

                        scaleAnchorWorld = ImageSelections.GetSelectionCenter();
                        rotationAngle = angle;
                        UpdateTransformMatrix();

                        ImageSelections.RotateSelections(angle);

                        RedrawImage();
                    }
                    else
                    {
                        Bitmap rotatedBitmap = new(selectedLayer.Image.Height, selectedLayer.Image.Width);

                        using (Graphics g = Graphics.FromImage(rotatedBitmap))
                        {
                            g.TranslateTransform(rotatedBitmap.Width / 2f, rotatedBitmap.Height / 2f);
                            g.RotateTransform(angle);
                            g.TranslateTransform(-selectedLayer.Image.Width / 2f, -selectedLayer.Image.Height / 2f);
                            g.DrawImage(selectedLayer.Image, Point.Empty);
                        }

                        selectedLayer.Image.Dispose();
                        selectedLayer.Image = rotatedBitmap;

                        RedrawImage();
                    }
                }
            }
        }

        private void ConvertToVectorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var selectedLayer = layersControl.GetLayer(layersControl.GetSelectedLayerIndex());

            if (selectedLayer != null)
            {
                if (selectedLayer.LayerType == LayerType.Image)
                {
                    HistoryManager.RecordState(new HistoryItem(layersControl.GetLayers(), layersControl.GetSelectedLayerIndex()));

                    List<ShapePolygon> polygons = StrokeRegionTracer.TraceStrokes(ManipulatorGeneral.RasterizeImage(selectedLayer.Image));
                    selectedLayer.Image?.Dispose();
                    selectedLayer.Image = new Bitmap(Document.Width, Document.Height);

                    foreach (var polygon in polygons)
                    {
                        for (int i = 0; i < polygon.Points.Count; i++)
                        {
                            polygon.Points[i] = new PointF(polygon.Points[i].X + selectedLayer.X, polygon.Points[i].Y + selectedLayer.Y);
                        }
                        selectedLayer.Shapes.Add(polygon);
                    }

                    selectedLayer.LayerType = LayerType.Vector;
                    selectedLayer.X = 0;
                    selectedLayer.Y = 0;
                    UpdateControls();
                    RedrawImage();

                    HistoryManager.CurrentState(new HistoryItem(layersControl.GetLayers(), layersControl.GetSelectedLayerIndex()));
                }
            }
        }

        private void ConvertToRasterToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var selectedLayer = layersControl.GetLayer(layersControl.GetSelectedLayerIndex());

            if (selectedLayer != null)
            {
                if (selectedLayer.LayerType == LayerType.Vector)
                {
                    HistoryManager.RecordState(new HistoryItem(layersControl.GetLayers(), layersControl.GetSelectedLayerIndex()));

                    Bitmap? image = new(Document.Width, Document.Height);
                    using (Graphics g = Graphics.FromImage(image))
                    {
                        foreach (BaseShape shape in selectedLayer.Shapes)
                        {
                            Layer.DrawShape(shape, g);
                        }
                    }

                    selectedLayer.Image = image;
                    selectedLayer.LayerType = LayerType.Image;
                    selectedLayer.Shapes.Clear();
                    selectedLayer.CurrentShape = null;
                    UpdateControls();
                    ManipulatorGeneral.InvalidateCompositeBuffers();
                    RedrawImage();

                    HistoryManager.CurrentState(new HistoryItem(layersControl.GetLayers(), layersControl.GetSelectedLayerIndex()));
                }
            }
        }

        private void ConvertToPathToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var selectedLayer = layersControl.GetLayer(layersControl.GetSelectedLayerIndex());

            if (selectedLayer != null)
            {
                if (selectedLayer.LayerType == LayerType.Vector)
                {
                    HistoryManager.RecordState(new HistoryItem(layersControl.GetLayers(), layersControl.GetSelectedLayerIndex()));

                    int selectedShapeIndex = selectedLayer.CurrentShape != null ? selectedLayer.Shapes.IndexOf(selectedLayer.CurrentShape) : -1;

                    if (selectedLayer.CurrentShape != null && selectedShapeIndex >= 0 && selectedShapeIndex < selectedLayer.Shapes.Count)
                    {
                        selectedLayer.Shapes[selectedShapeIndex] = selectedLayer.CurrentShape = ShapeToPathConverter.Convert(selectedLayer.CurrentShape);
                    }

                    UpdateControls();
                    ManipulatorGeneral.InvalidateCompositeBuffers();
                    RedrawImage();

                    HistoryManager.CurrentState(new HistoryItem(layersControl.GetLayers(), layersControl.GetSelectedLayerIndex()));
                }
            }
        }

        private void BtnAddLayer_Click(object? sender, EventArgs e)
        {
            FormLayer frm = new()
            {
                StartPosition = FormStartPosition.CenterParent,
                Layer = new($"layer {layersControl.GetLayers().Count + 1}", true)
                {
                    FillType = FillType.Transparency
                }
            };

            if (frm.ShowDialog(this) == DialogResult.OK)
            {
                layersControl.InsertLayer(0, frm.Layer);
                RedrawImage();
            }
        }

        private void BtnSubtractLayer_Click(object? sender, EventArgs e)
        {
            layersControl.RemoveLayerAt(layersControl.GetSelectedLayerIndex());
            RedrawImage();
        }

        private void BtnMoveUp_Click(object? sender, EventArgs e)
        {
            int currentIndex = layersControl.GetSelectedLayerIndex();

            var layerToMove = layersControl.GetLayer(currentIndex);

            if (layerToMove != null)
            {
                layersControl.MoveLayerUp();
                RedrawImage();
            }
        }

        private void BtnMoveDown_Click(object? sender, EventArgs e)
        {
            int currentIndex = layersControl.GetSelectedLayerIndex();

            var layerToMove = layersControl.GetLayer(currentIndex);

            if (layerToMove != null)
            {
                layersControl.MoveLayerDown();
                RedrawImage();
            }
        }

        private void MoveToTopToolStripMenuItem_Click(object? sender, EventArgs e)
        {
            int currentIndex = layersControl.GetSelectedLayerIndex();

            var layerToMove = layersControl.GetLayer(currentIndex);

            if (layerToMove != null)
            {
                layersControl.MoveLayerToTop();
                RedrawImage();
            }
        }

        private void MoveToBottomToolStripMenuItem_Click(object? sender, EventArgs e)
        {
            int currentIndex = layersControl.GetSelectedLayerIndex();

            var layerToMove = layersControl.GetLayer(currentIndex);

            if (layerToMove != null)
            {
                layersControl.MoveLayerToBottom();
                RedrawImage();
            }
        }

        private void BtnShowLayer_Click(object? sender, EventArgs e)
        {
            var layer = layersControl.GetLayer(layersControl.GetSelectedLayerIndex());

            if (layer != null)
            {
                HistoryManager.RecordState(new HistoryItem(layersControl.GetLayers(), layersControl.GetSelectedLayerIndex()));
                layer.IsVisible = true;
                layersControl.UpdateLayer(layersControl.GetSelectedLayerIndex(), layer);
                RedrawImage();
                HistoryManager.CurrentState(new HistoryItem(layersControl.GetLayers(), layersControl.GetSelectedLayerIndex()));
            }
        }

        private void BtnHideLayer_Click(object? sender, EventArgs e)
        {
            var layer = layersControl.GetLayer(layersControl.GetSelectedLayerIndex());

            if (layer != null)
            {
                HistoryManager.RecordState(new HistoryItem(layersControl.GetLayers(), layersControl.GetSelectedLayerIndex()));
                layer.IsVisible = false;
                layersControl.UpdateLayer(layersControl.GetSelectedLayerIndex(), layer);
                RedrawImage();
                HistoryManager.CurrentState(new HistoryItem(layersControl.GetLayers(), layersControl.GetSelectedLayerIndex()));
            }
        }

        private void BtnDuplicate_Click(object? sender, EventArgs e)
        {
            int selectedIndex = layersControl.GetSelectedLayerIndex();

            var layer = layersControl.GetLayer(selectedIndex);
            if (layer != null)
            {
                Layer clone = layer.Clone();
                clone.Name += "_Clone";
                layersControl.InsertLayer(selectedIndex + 1, clone);

                UpdateControls();

                RedrawImage();
            }
        }

        private void BtnMergeDown_Click(object? sender, EventArgs e)
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
                top = ManipulatorGeneral.MergeLayers(top, bottom);
                layersControl.UpdateLayer(selectedIndex, top);
                layersControl.RemoveLayerAt(selectedIndex + 1);

                UpdateControls();

                RedrawImage();
            }
        }

        private void UpdateControls()
        {
            var selectedLayer = layersControl.GetLayer(layersControl.GetSelectedLayerIndex());
            if (selectedLayer != null)
            {
                opacity.Value = selectedLayer.Opacity;
                cboBlendMode.SelectedItem = selectedLayer.BlendMode.ToString();
                redToolStripMenuItem.Checked = selectedLayer.Channel == LayerChannel.Red;
                greenToolStripMenuItem.Checked = selectedLayer.Channel == LayerChannel.Green;
                blueToolStripMenuItem.Checked = selectedLayer.Channel == LayerChannel.Blue;
                allToolStripMenuItem.Checked = selectedLayer.Channel == LayerChannel.RGB;
                redToolStripMenuItem1.Checked = selectedLayer.RedFilter;
                greenToolStripMenuItem1.Checked = selectedLayer.GreenFilter;
                blueToolStripMenuItem1.Checked = selectedLayer.BlueFilter;

                if (selectedLayer.LayerType == LayerType.Vector)
                {
                    btnPointer.Checked = true;

                    btnFiller.Visible = false;
                    btnBrusher.Visible = false;
                    btnEraser.Visible = false;
                    btnWarp.Visible = false;
                    btnCrop.Visible = false;

                    btnShapeRect.Visible = true;
                    btnShapeEllipse.Visible = true;
                    btnShapePolygon.Visible = true;
                    btnShapeText.Visible = true;
                }
                else
                {
                    btnPointer.Checked = true;

                    btnFiller.Visible = true;
                    btnBrusher.Visible = true;
                    btnEraser.Visible = true;
                    btnWarp.Visible = true;
                    btnCrop.Visible = true;

                    btnShapeRect.Visible = false;
                    btnShapeEllipse.Visible = false;
                    btnShapePolygon.Visible = false;
                    btnShapeText.Visible = false;
                }

                if (btnPointer.Checked)
                {
                    if (selectedLayer.CurrentShape != null)
                    {
                        if (selectedLayer.CurrentShape is ShapeRect r)
                        {
                            groupRectShapeDetail.Visible = true;
                            groupEllipseShapeDetail.Visible = false;
                            groupPolygonShapeDetail.Visible = false;
                            groupTextShapeDetail.Visible = false;

                            btnRectLineColor.BackColor = Color.FromArgb(255, r.LineColor.R, r.LineColor.G, r.LineColor.B);
                            rectLineOpacityNum.Value = (decimal)((r.LineColor.A / 255.0) * (double)rectLineOpacityNum.Maximum);

                            btnRectFillColorShape.BackColor = Color.FromArgb(255, r.FillColor.R, r.FillColor.G, r.FillColor.B);
                            fillRectOpacityNum.Value = (decimal)((r.FillColor.A / 255.0) * (double)fillRectOpacityNum.Maximum);

                            rectLineSizeTrack.Value = (int)r.LineWidth;
                            cboRectLinePattern.SelectedItem = r.DashStyle.ToString();
                        }
                        else if (selectedLayer.CurrentShape is ShapeEllipse el)
                        {
                            groupRectShapeDetail.Visible = false;
                            groupEllipseShapeDetail.Visible = true;
                            groupPolygonShapeDetail.Visible = false;
                            groupTextShapeDetail.Visible = false;

                            btnEllipseLineColor.BackColor = Color.FromArgb(255, el.LineColor.R, el.LineColor.G, el.LineColor.B);
                            ellipseLineOpacityNum.Value = (decimal)((el.LineColor.A / 255.0) * (double)ellipseLineOpacityNum.Maximum);

                            btnEllipseFillColorShape.BackColor = Color.FromArgb(255, el.FillColor.R, el.FillColor.G, el.FillColor.B);
                            fillEllipseOpacityNum.Value = (decimal)((el.FillColor.A / 255.0) * (double)fillEllipseOpacityNum.Maximum);

                            ellipseLineSizeTrack.Value = (int)el.LineWidth;
                            cboEllipseLinePattern.SelectedItem = el.DashStyle.ToString();
                        }
                        else if (selectedLayer.CurrentShape is ShapePolygon pg)
                        {
                            groupRectShapeDetail.Visible = false;
                            groupEllipseShapeDetail.Visible = false;
                            groupPolygonShapeDetail.Visible = true;
                            groupTextShapeDetail.Visible = false;

                            btnPolygonLineColor.BackColor = Color.FromArgb(255, pg.LineColor.R, pg.LineColor.G, pg.LineColor.B);
                            polygonLineOpacityNum.Value = (decimal)((pg.LineColor.A / 255.0) * (double)polygonLineOpacityNum.Maximum);

                            btnPolygonFillColorShape.BackColor = Color.FromArgb(255, pg.FillColor.R, pg.FillColor.G, pg.FillColor.B);
                            fillPolygonOpacityNum.Value = (decimal)((pg.FillColor.A / 255.0) * (double)fillPolygonOpacityNum.Maximum);

                            polygonLineSizeTrack.Value = (int)pg.LineWidth;
                            cboPolygonLinePattern.SelectedItem = pg.DashStyle.ToString();
                        }
                        else if (selectedLayer.CurrentShape is ShapeText t)
                        {
                            groupRectShapeDetail.Visible = false;
                            groupEllipseShapeDetail.Visible = false;
                            groupPolygonShapeDetail.Visible = false;
                            groupTextShapeDetail.Visible = true;

                            txtTextLineText.Text = t.Content;
                            btnTextFillColor.BackColor = Color.FromArgb(255, t.FillColor.R, t.FillColor.G, t.FillColor.B);
                            numTextFillOpacity.Value = (decimal)((t.FillColor.A / 255.0) * (double)numTextFillOpacity.Maximum);

                            FontStyle style = FontStyle.Regular;
                            if (t.IsBold) style |= FontStyle.Bold;
                            if (t.IsItalic) style |= FontStyle.Italic;
                            btnTextFont.Font = new Font(t.FontFamily, (float)t.FontSize, style);
                        }
                    }
                }
            }
        }

        private void ZoomInToolStripMenuItem_Click(object? sender, EventArgs e)
        {
            HistoryManager.RecordState(new HistoryItem(layersControl.GetLayers(), layersControl.GetSelectedLayerIndex()));
            float zoomDelta = 0.1f;
            Document.Zoom = Math.Max(0.1f, Math.Min(10.0f, Document.Zoom + zoomDelta));
            RedrawImage();
            HistoryManager.CurrentState(new HistoryItem(layersControl.GetLayers(), layersControl.GetSelectedLayerIndex()));
        }

        private void ZoomOutToolStripMenuItem_Click(object? sender, EventArgs e)
        {
            HistoryManager.RecordState(new HistoryItem(layersControl.GetLayers(), layersControl.GetSelectedLayerIndex()));
            float zoomDelta = 0.1f;
            Document.Zoom = Math.Max(0.1f, Math.Min(10.0f, Document.Zoom - zoomDelta));
            RedrawImage();
            HistoryManager.CurrentState(new HistoryItem(layersControl.GetLayers(), layersControl.GetSelectedLayerIndex()));
        }

        private void BtnResetZoom_Click(object? sender, EventArgs e)
        {
            HistoryManager.RecordState(new HistoryItem(layersControl.GetLayers(), layersControl.GetSelectedLayerIndex()));
            Document.Zoom = 0.95f;
            Document.ImageOffset = new(0, 0);
            RedrawImage();
            HistoryManager.CurrentState(new HistoryItem(layersControl.GetLayers(), layersControl.GetSelectedLayerIndex()));
        }

        private void AllToolStripMenuItem_Click(object? sender, EventArgs e)
        {
            var selectedLayer = layersControl.GetLayer(layersControl.GetSelectedLayerIndex());
            if (selectedLayer != null)
            {
                allToolStripMenuItem.Checked = !allToolStripMenuItem.Checked;
                redToolStripMenuItem.Checked = false;
                greenToolStripMenuItem.Checked = false;
                blueToolStripMenuItem.Checked = false;
                selectedLayer.Channel = LayerChannel.RGB;
                ManipulatorGeneral.DirtyRegions.Add(new(selectedLayer.X, selectedLayer.Y, selectedLayer.Image?.Width ?? 0, selectedLayer.Image?.Height ?? 0));
                RedrawImage();
            }
        }

        private void RedToolStripMenuItem_Click(object? sender, EventArgs e)
        {
            var selectedLayer = layersControl.GetLayer(layersControl.GetSelectedLayerIndex());
            if (selectedLayer != null)
            {
                allToolStripMenuItem.Checked = false;
                redToolStripMenuItem.Checked = !redToolStripMenuItem.Checked;
                greenToolStripMenuItem.Checked = false;
                blueToolStripMenuItem.Checked = false;
                selectedLayer.Channel = LayerChannel.Red;
                ManipulatorGeneral.DirtyRegions.Add(new(selectedLayer.X, selectedLayer.Y, selectedLayer.Image?.Width ?? 0, selectedLayer.Image?.Height ?? 0));
                RedrawImage();
            }
        }

        private void GreenToolStripMenuItem_Click(object? sender, EventArgs e)
        {
            var selectedLayer = layersControl.GetLayer(layersControl.GetSelectedLayerIndex());
            if (selectedLayer != null)
            {
                allToolStripMenuItem.Checked = false;
                redToolStripMenuItem.Checked = false;
                greenToolStripMenuItem.Checked = !greenToolStripMenuItem.Checked;
                blueToolStripMenuItem.Checked = false;
                selectedLayer.Channel = LayerChannel.Green;
                ManipulatorGeneral.DirtyRegions.Add(new(selectedLayer.X, selectedLayer.Y, selectedLayer.Image?.Width ?? 0, selectedLayer.Image?.Height ?? 0));
                RedrawImage();
            }
        }

        private void BlueToolStripMenuItem_Click(object? sender, EventArgs e)
        {
            var selectedLayer = layersControl.GetLayer(layersControl.GetSelectedLayerIndex());
            if (selectedLayer != null)
            {
                allToolStripMenuItem.Checked = false;
                redToolStripMenuItem.Checked = false;
                greenToolStripMenuItem.Checked = false;
                blueToolStripMenuItem.Checked = !blueToolStripMenuItem.Checked;
                selectedLayer.Channel = LayerChannel.Blue;
                ManipulatorGeneral.DirtyRegions.Add(new(selectedLayer.X, selectedLayer.Y, selectedLayer.Image?.Width ?? 0, selectedLayer.Image?.Height ?? 0));
                RedrawImage();
            }
        }

        private void RdoAll_CheckedChanged(object? sender, EventArgs e)
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
                    ManipulatorGeneral.DirtyRegions.Add(new(selectedLayer.X, selectedLayer.Y, selectedLayer.Image?.Width ?? 0, selectedLayer.Image?.Height ?? 0));
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
                    ManipulatorGeneral.DirtyRegions.Add(new(selectedLayer.X, selectedLayer.Y, selectedLayer.Image?.Width ?? 0, selectedLayer.Image?.Height ?? 0));
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
                    ManipulatorGeneral.DirtyRegions.Add(new(selectedLayer.X, selectedLayer.Y, selectedLayer.Image?.Width ?? 0, selectedLayer.Image?.Height ?? 0));
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
                    ManipulatorGeneral.DirtyRegions.Add(new(selectedLayer.X, selectedLayer.Y, selectedLayer.Image?.Width ?? 0, selectedLayer.Image?.Height ?? 0));
                    RedrawImage();
                }
            }
        }

        private void RedToolStripMenuItem1_Click(object? sender, EventArgs e)
        {
            redToolStripMenuItem1.Checked = !redToolStripMenuItem1.Checked;
        }

        private void GreenToolStripMenuItem1_Click(object? sender, EventArgs e)
        {
            greenToolStripMenuItem1.Checked = !greenToolStripMenuItem1.Checked;
        }

        private void BlueToolStripMenuItem1_Click(object? sender, EventArgs e)
        {
            blueToolStripMenuItem1.Checked = !blueToolStripMenuItem1.Checked;
        }

        private void ChkFilter1_CheckedChanged(object? sender, EventArgs e)
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

        private void ChkFilter_CheckedChanged(object? sender, EventArgs e)
        {
            var selectedLayer = layersControl.GetLayer(layersControl.GetSelectedLayerIndex());
            if (selectedLayer != null)
            {
                selectedLayer.RedFilter = redToolStripMenuItem1.Checked;
                selectedLayer.GreenFilter = greenToolStripMenuItem1.Checked;
                selectedLayer.BlueFilter = blueToolStripMenuItem1.Checked;
                ManipulatorGeneral.DirtyRegions.Add(new(selectedLayer.X, selectedLayer.Y, selectedLayer.Image?.Width ?? 0, selectedLayer.Image?.Height ?? 0));
                RedrawImage();
            }
        }

        private void DeleteImageToolStripMenuItem_Click(object? sender, EventArgs e)
        {
            var selectedLayer = layersControl.GetLayer(layersControl.GetSelectedLayerIndex());
            if (selectedLayer != null)
            {
                if (ImageSelections.ContainsSelection())
                {
                    HistoryManager.RecordState(new HistoryItem(layersControl.GetLayers(), layersControl.GetSelectedLayerIndex()));
                    ImageSelections.CalculateSelectionBounds();
                    selectedLayer.Image = ManipulatorGeneral.CutSelectionFromLayer(selectedLayer);
                    rotationAngle = 0;
                    ImageSelections.ClearSelections();
                    RedrawImage();
                    HistoryManager.CurrentState(new HistoryItem(layersControl.GetLayers(), layersControl.GetSelectedLayerIndex()));
                }
                else if (selectedLayer.Image != null && selectedLayer.Shapes.Count == 0)
                {
                    HistoryManager.RecordState(new HistoryItem(layersControl.GetLayers(), layersControl.GetSelectedLayerIndex()));
                    int width = selectedLayer.Image.Width;
                    int height = selectedLayer.Image.Height;
                    selectedLayer.Image.Dispose();
                    selectedLayer.Image = ManipulatorGeneral.GetImage(Color.Transparent, width, height);
                    rotationAngle = 0;
                    RedrawImage();
                    HistoryManager.CurrentState(new HistoryItem(layersControl.GetLayers(), layersControl.GetSelectedLayerIndex()));
                }
                else if (selectedLayer.CurrentShape != null)
                {
                    HistoryManager.RecordState(new HistoryItem(layersControl.GetLayers(), layersControl.GetSelectedLayerIndex()));
                    selectedLayer.Shapes.Remove(selectedLayer.CurrentShape);
                    selectedLayer.CurrentShape = null;
                    RedrawImage();
                    HistoryManager.CurrentState(new HistoryItem(layersControl.GetLayers(), layersControl.GetSelectedLayerIndex()));
                }
            }
        }

        private void CutImageToolStripMenuItem_Click(object? sender, EventArgs e)
        {
            var selectedLayer = layersControl.GetLayer(layersControl.GetSelectedLayerIndex());
            if (selectedLayer != null)
            {
                if (ImageSelections.ContainsSelection())
                {
                    HistoryManager.RecordState(new HistoryItem(layersControl.GetLayers(), layersControl.GetSelectedLayerIndex()));
                    ImageSelections.CalculateSelectionBounds();
                    Image? tempImage = ManipulatorGeneral.ExtractSelectedArea(selectedLayer);
                    selectedLayer.Image = ManipulatorGeneral.CutSelectionFromLayer(selectedLayer);
                    if (tempImage != null)
                    {
                        Clipboard.SetImage(tempImage);
                        ImageSelections.ClearSelections();
                        RedrawImage();
                    }
                    HistoryManager.CurrentState(new HistoryItem(layersControl.GetLayers(), layersControl.GetSelectedLayerIndex()));
                }
                else
                {
                    Image? tempImage = selectedLayer.Image;
                    if (tempImage != null)
                    {
                        HistoryManager.RecordState(new HistoryItem(layersControl.GetLayers(), layersControl.GetSelectedLayerIndex()));
                        Clipboard.SetImage(tempImage);
                        DeleteImageToolStripMenuItem_Click(sender, e);
                        RedrawImage();
                        HistoryManager.CurrentState(new HistoryItem(layersControl.GetLayers(), layersControl.GetSelectedLayerIndex()));
                    }
                }
            }
        }

        private void CopyImageToolStripMenuItem_Click(object? sender, EventArgs e)
        {
            var selectedLayer = layersControl.GetLayer(layersControl.GetSelectedLayerIndex());
            if (selectedLayer != null)
            {
                if (ImageSelections.ContainsSelection())
                {
                    HistoryManager.RecordState(new HistoryItem(layersControl.GetLayers(), layersControl.GetSelectedLayerIndex()));
                    ImageSelections.CalculateSelectionBounds();
                    Image? tempImage = ManipulatorGeneral.ExtractSelectedArea(selectedLayer);
                    if (tempImage != null)
                    {
                        Clipboard.SetImage(tempImage);
                        ImageSelections.ClearSelections();
                        RedrawImage();
                    }
                    HistoryManager.CurrentState(new HistoryItem(layersControl.GetLayers(), layersControl.GetSelectedLayerIndex()));
                }
                else
                {
                    Image? tempImage = selectedLayer.Image;
                    if (tempImage != null)
                    {
                        HistoryManager.RecordState(new HistoryItem(layersControl.GetLayers(), layersControl.GetSelectedLayerIndex()));
                        Clipboard.SetImage(tempImage);
                        RedrawImage();
                        HistoryManager.CurrentState(new HistoryItem(layersControl.GetLayers(), layersControl.GetSelectedLayerIndex()));
                    }
                }
            }
        }

        private void PasteImageToolStripMenuItem_Click(object? sender, EventArgs e)
        {
            Image? clipboardImage = Clipboard.GetImage();
            if (clipboardImage != null)
            {
                var selectedLayer = layersControl.GetLayer(layersControl.GetSelectedLayerIndex());
                if (selectedLayer != null)
                {
                    HistoryManager.RecordState(new HistoryItem(layersControl.GetLayers(), layersControl.GetSelectedLayerIndex()));

                    selectedLayer.Image = new Bitmap(clipboardImage);
                    RedrawImage();

                    HistoryManager.CurrentState(new HistoryItem(layersControl.GetLayers(), layersControl.GetSelectedLayerIndex()));
                }
            }
        }

        private void UndoToolStripMenuItem_Click(object? sender, EventArgs e)
        {
            if (HistoryManager.CanUndo)
            {
                HistoryItem? history = HistoryManager.Undo();

                if (history != null)
                {
                    layersControl.ClearLayers();
                    layersControl.AddRange(history.Layers);
                    Document.Zoom = history.Zoom;
                    Document.ImageOffset = history.Offset;
                    layersControl.SetSelectedLayerIndex(history.SelectedLayerIndex);
                }

                Refresh();
                RedrawImage();
            }
        }

        private void RedoToolStripMenuItem_Click(object? sender, EventArgs e)
        {
            if (HistoryManager.CanRedo)
            {
                HistoryItem? history = HistoryManager.Redo();

                if (history != null)
                {
                    layersControl.ClearLayers();
                    layersControl.AddRange(history.Layers);
                    Document.Zoom = history.Zoom;
                    Document.ImageOffset = history.Offset;
                    layersControl.SetSelectedLayerIndex(history.SelectedLayerIndex);
                }

                Refresh();
                RedrawImage();
            }
        }

        private void SelectAllToolStripMenuItem_Click(object? sender, EventArgs e)
        {
            var selectedLayer = layersControl.GetLayer(layersControl.GetSelectedLayerIndex());
            if (selectedLayer != null)
            {
                if (selectedLayer.Image != null)
                {
                    HistoryManager.RecordState(new HistoryItem(layersControl.GetLayers(), layersControl.GetSelectedLayerIndex()));

                    int imgW = selectedLayer.Image.Width;
                    int imgH = selectedLayer.Image.Height;
                    int x0 = selectedLayer.X;
                    int y0 = selectedLayer.Y;

                    ImageSelections.AddSelectionPoint(new Point(x0, y0));
                    ImageSelections.AddSelectionPoint(new Point(x0, y0 + imgH));
                    ImageSelections.AddSelectionPoint(new Point(x0 + imgW, y0 + imgH));
                    ImageSelections.AddSelectionPoint(new Point(x0 + imgW, y0));
                    ImageSelections.AddSelectionPoint(new Point(x0, y0));

                    RedrawImage();

                    HistoryManager.CurrentState(new HistoryItem(layersControl.GetLayers(), layersControl.GetSelectedLayerIndex()));
                }
            }
        }

        private void NoneToolStripMenuItem_Click(object? sender, EventArgs e)
        {
            HistoryManager.RecordState(new HistoryItem(layersControl.GetLayers(), layersControl.GetSelectedLayerIndex()));
            ImageSelections.ClearSelections();
            RedrawImage();
            HistoryManager.CurrentState(new HistoryItem(layersControl.GetLayers(), layersControl.GetSelectedLayerIndex()));
        }

        private void InvertToolStripMenuItem1_Click(object? sender, EventArgs e)
        {
            var selectedLayer = layersControl.GetLayer(layersControl.GetSelectedLayerIndex());
            if (selectedLayer != null)
            {
                if (selectedLayer.Image != null)
                {
                    HistoryManager.RecordState(new HistoryItem(layersControl.GetLayers(), layersControl.GetSelectedLayerIndex()));
                    ImageSelections.InvertSelections(selectedLayer.X, selectedLayer.Y, selectedLayer.Image.Width, selectedLayer.Image.Height);
                    RedrawImage();
                    HistoryManager.CurrentState(new HistoryItem(layersControl.GetLayers(), layersControl.GetSelectedLayerIndex()));
                }
            }
        }

        private void GrowToolStripMenuItem_Click(object? sender, EventArgs e)
        {
            FormSelections frm = new();
            if (frm.ShowDialog() == DialogResult.OK)
            {
                var selectedLayer = layersControl.GetLayer(layersControl.GetSelectedLayerIndex());
                if (selectedLayer != null)
                {
                    if (selectedLayer.Image != null)
                    {
                        HistoryManager.RecordState(new HistoryItem(layersControl.GetLayers(), layersControl.GetSelectedLayerIndex()));
                        ImageSelections.GrowSelections(selectedLayer.X, selectedLayer.Y, selectedLayer.Image.Width, selectedLayer.Image.Height, frm.PixelAmount);
                        RedrawImage();
                        HistoryManager.CurrentState(new HistoryItem(layersControl.GetLayers(), layersControl.GetSelectedLayerIndex()));
                    }
                }
            }
        }

        private void ShrinkToolStripMenuItem_Click(object? sender, EventArgs e)
        {
            FormSelections frm = new();
            if (frm.ShowDialog() == DialogResult.OK)
            {
                var selectedLayer = layersControl.GetLayer(layersControl.GetSelectedLayerIndex());
                if (selectedLayer != null)
                {
                    if (selectedLayer.Image != null)
                    {
                        HistoryManager.RecordState(new HistoryItem(layersControl.GetLayers(), layersControl.GetSelectedLayerIndex()));
                        ImageSelections.ShrinkSelections(selectedLayer.X, selectedLayer.Y, selectedLayer.Image.Width, selectedLayer.Image.Height, frm.PixelAmount);
                        RedrawImage();
                        HistoryManager.CurrentState(new HistoryItem(layersControl.GetLayers(), layersControl.GetSelectedLayerIndex()));
                    }
                }
            }
        }

        private void GeneralSettingsToolStripMenuItem_Click(object? sender, EventArgs e)
        {
            FormSettings frm = new()
            {
                StartPosition = FormStartPosition.CenterParent
            };
            var selectedLayer = layersControl.GetLayer(layersControl.GetSelectedLayerIndex());
            if (selectedLayer != null)
            {
                frm.LayerHeight = selectedLayer.Image?.Height ?? 2;
                frm.LayerWidth = selectedLayer.Image?.Width ?? 2;
                frm.LayerX = selectedLayer.X;
                frm.LayerY = selectedLayer.Y;
                if (frm.ShowDialog(this) == DialogResult.OK)
                {
                    HistoryManager.RecordState(new HistoryItem(layersControl.GetLayers(), layersControl.GetSelectedLayerIndex()));
                    selectedLayer.X = frm.LayerX;
                    selectedLayer.Y = frm.LayerY;

                    if (selectedLayer.Image == null)
                    {
                        selectedLayer.Image = ManipulatorGeneral.GetImage(selectedLayer.FillType == FillType.Transparency ? Color.Transparent : selectedLayer.FillColor, frm.LayerWidth, frm.LayerHeight);
                    }
                    else
                    {
                        if (frm.ResizeImage)
                        {
                            selectedLayer.Image = ManipulatorLighting.ResizeImage(selectedLayer.Image, frm.LayerWidth, frm.LayerHeight);
                        }
                        else
                        {
                            selectedLayer.Image = ManipulatorLighting.CropFromCenter(selectedLayer.Image, frm.LayerWidth, frm.LayerHeight);
                        }
                    }

                    layersControl.RefreshLayersDisplay();
                    RedrawImage();
                    HistoryManager.CurrentState(new HistoryItem(layersControl.GetLayers(), layersControl.GetSelectedLayerIndex()));
                }
            }
        }

        private void DarkToolStripMenuItem_Click(object? sender, EventArgs e)
        {
            var selectedLayer = layersControl.GetLayer(layersControl.GetSelectedLayerIndex());
            if (selectedLayer != null)
            {
                if (selectedLayer.Image != null)
                {
                    HistoryManager.RecordState(new HistoryItem(layersControl.GetLayers(), layersControl.GetSelectedLayerIndex()));
                    Bitmap inverted = ManipulatorLighting.DarkImage((Bitmap)selectedLayer.Image);
                    selectedLayer.Image.Dispose();
                    selectedLayer.Image = inverted;
                    RedrawImage();
                    HistoryManager.CurrentState(new HistoryItem(layersControl.GetLayers(), layersControl.GetSelectedLayerIndex()));
                }
            }
        }

        private void InvertToolStripMenuItem_Click(object? sender, EventArgs e)
        {
            var selectedLayer = layersControl.GetLayer(layersControl.GetSelectedLayerIndex());
            if (selectedLayer != null)
            {
                if (selectedLayer.Image != null)
                {
                    HistoryManager.RecordState(new HistoryItem(layersControl.GetLayers(), layersControl.GetSelectedLayerIndex()));
                    Bitmap inverted = ManipulatorLighting.InvertColors((Bitmap)selectedLayer.Image);
                    selectedLayer.Image.Dispose();
                    selectedLayer.Image = inverted;
                    RedrawImage();
                    HistoryManager.CurrentState(new HistoryItem(layersControl.GetLayers(), layersControl.GetSelectedLayerIndex()));
                }
            }
        }

        private void LightingToolStripMenuItem_Click(object? sender, EventArgs e)
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
                        HistoryManager.RecordState(new HistoryItem(layersControl.GetLayers(), layersControl.GetSelectedLayerIndex()));
                        Bitmap result = new(frm.Image);
                        selectedLayer.Image.Dispose();
                        selectedLayer.Image = result;
                        RedrawImage();
                        HistoryManager.CurrentState(new HistoryItem(layersControl.GetLayers(), layersControl.GetSelectedLayerIndex()));
                    }
                }
            }
        }

        private void SaturationToolStripMenuItem_Click(object? sender, EventArgs e)
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
                        HistoryManager.RecordState(new HistoryItem(layersControl.GetLayers(), layersControl.GetSelectedLayerIndex()));
                        Bitmap result = new(frm.Image);
                        selectedLayer.Image.Dispose();
                        selectedLayer.Image = result;
                        RedrawImage();
                        HistoryManager.CurrentState(new HistoryItem(layersControl.GetLayers(), layersControl.GetSelectedLayerIndex()));
                    }
                }
            }
        }

        private void BlurImageToolStripMenuItem_Click(object? sender, EventArgs e)
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
                        HistoryManager.RecordState(new HistoryItem(layersControl.GetLayers(), layersControl.GetSelectedLayerIndex()));
                        Bitmap blurred = new(frm.Image);
                        selectedLayer.Image.Dispose();
                        selectedLayer.Image = blurred;
                        RedrawImage();
                        HistoryManager.CurrentState(new HistoryItem(layersControl.GetLayers(), layersControl.GetSelectedLayerIndex()));
                    }
                }
            }
        }

        private void SharpnessToolStripMenuItem_Click(object? sender, EventArgs e)
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
                        HistoryManager.RecordState(new HistoryItem(layersControl.GetLayers(), layersControl.GetSelectedLayerIndex()));
                        Bitmap blurred = new(frm.Image);
                        selectedLayer.Image.Dispose();
                        selectedLayer.Image = blurred;
                        RedrawImage();
                        HistoryManager.CurrentState(new HistoryItem(layersControl.GetLayers(), layersControl.GetSelectedLayerIndex()));
                    }
                }
            }
        }

        private void UpdateTransformMatrix()
        {
            Point anchorScreen = ManipulatorGeneral.WorldToScreen(Point.Round(scaleAnchorWorld), canvas.Width, canvas.Height);

            transformMatrix.Reset();
            transformMatrix.Translate(-anchorScreen.X, -anchorScreen.Y, MatrixOrder.Append);
            transformMatrix.Rotate(rotationAngle, MatrixOrder.Append);
            transformMatrix.Scale(scaleFactorX, scaleFactorY, MatrixOrder.Append);
            transformMatrix.Translate(anchorScreen.X, anchorScreen.Y, MatrixOrder.Append);
        }

        private Bitmap? MergeSelectionToLayer(Layer selectedLayer)
        {
            if (selectedLayer.Image == null || selectedAreaBitmap == null) return null;

            if (scaleFactorX != 1.0f || scaleFactorY != 1.0f || rotationAngle != 0f)
                BakeTransformIntoSelectedArea(selectedLayer);

            Bitmap result = new(selectedLayer.Image);
            using (Graphics g = Graphics.FromImage(result))
            {
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.SmoothingMode = SmoothingMode.AntiAlias;

                RectangleF worldBounds = ImageSelections.GetSelectionBounds();
                float localX = worldBounds.X - selectedLayer.X;
                float localY = worldBounds.Y - selectedLayer.Y;

                g.DrawImage(selectedAreaBitmap, localX, localY);
            }

            selectedAreaBitmap?.Dispose();
            selectedAreaBitmap = null;
            ImageSelections.ClearSelections();

            return result;
        }

        private (float newX, float newY) CalculateScaleFactors(Point mouseScreenPos, string handle)
        {
            (bool affectsX, bool affectsY) = ManipulatorGeneral.GetHandleAxes(handle);

            Point anchorScreen = ManipulatorGeneral.WorldToScreen(Point.Round(scaleAnchorWorld), canvas.Width, canvas.Height);

            float newX = scaleFactorX;
            float newY = scaleFactorY;

            if (affectsX && initialScaleDistanceX > 0)
            {
                float startDX = Math.Abs(scaleStartMouseScreen.X - anchorScreen.X);
                float currentDX = Math.Abs(mouseScreenPos.X - anchorScreen.X);
                float ratio = startDX > 0 ? currentDX / startDX : 1f;
                newX = Math.Max(0.01f, ratio);
            }

            if (affectsY && initialScaleDistanceY > 0)
            {
                float startDY = Math.Abs(scaleStartMouseScreen.Y - anchorScreen.Y);
                float currentDY = Math.Abs(mouseScreenPos.Y - anchorScreen.Y);
                float ratio = startDY > 0 ? currentDY / startDY : 1f;
                newY = Math.Max(0.01f, ratio);
            }

            return (newX, newY);
        }

        private void BakeTransformIntoSelectedArea(Layer selectedLayer)
        {
            if (selectedAreaBitmap == null) return;

            RectangleF worldBounds = ImageSelections.GetSelectionBounds();

            PointF center = new(
                scaleAnchorWorld.X,
                scaleAnchorWorld.Y);

            PointF[] corners =
            [
                new(worldBounds.Left,  worldBounds.Top),
                new(worldBounds.Right, worldBounds.Top),
                new(worldBounds.Right, worldBounds.Bottom),
                new(worldBounds.Left,  worldBounds.Bottom),
            ];

            using Matrix worldTransform = new();
            worldTransform.Translate(center.X, center.Y);
            worldTransform.Rotate(rotationAngle);
            worldTransform.Scale(scaleFactorX, scaleFactorY);
            worldTransform.Translate(-center.X, -center.Y);
            worldTransform.TransformPoints(corners);

            float newLeft = corners.Min(p => p.X);
            float newTop = corners.Min(p => p.Y);
            float newRight = corners.Max(p => p.X);
            float newBottom = corners.Max(p => p.Y);

            int newWidth = Math.Max(1, (int)(newRight - newLeft));
            int newHeight = Math.Max(1, (int)(newBottom - newTop));

            Bitmap baked = new(newWidth, newHeight, System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
            using (Graphics g = Graphics.FromImage(baked))
            {
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.SmoothingMode = SmoothingMode.AntiAlias;

                float localX = worldBounds.X - selectedLayer.X;
                float localY = worldBounds.Y - selectedLayer.Y;
                float localAnchorX = scaleAnchorWorld.X - selectedLayer.X;
                float localAnchorY = scaleAnchorWorld.Y - selectedLayer.Y;
                float localNewLeft = newLeft - selectedLayer.X;
                float localNewTop = newTop - selectedLayer.Y;

                using Matrix m = new();
                m.Translate(localAnchorX - localNewLeft, localAnchorY - localNewTop);
                m.Rotate(rotationAngle);
                m.Scale(scaleFactorX, scaleFactorY);
                m.Translate(-localAnchorX, -localAnchorY);
                m.Translate(localX, localY);

                g.Transform = m;
                g.DrawImage(selectedAreaBitmap, 0, 0);
            }

            selectedAreaBitmap.Dispose();
            selectedAreaBitmap = baked;

            ImageSelections.ClearSelections();
            ImageSelections.AddSelectionPoint(new Point((int)newLeft, (int)newTop));
            ImageSelections.AddSelectionPoint(new Point((int)newRight, (int)newTop));
            ImageSelections.AddSelectionPoint(new Point((int)newRight, (int)newBottom));
            ImageSelections.AddSelectionPoint(new Point((int)newLeft, (int)newBottom));
            ImageSelections.AddSelectionPoint(new Point((int)newLeft, (int)newTop));
            ImageSelections.CalculateSelectionBounds();

            scaleFactorX = 1.0f;
            scaleFactorY = 1.0f;
            rotationAngle = 0f;
            scaleAnchorWorld = new PointF(
                (newLeft + newRight) / 2f,
                (newTop + newBottom) / 2f);
        }

        private void PixelImage_MouseDown(object? sender, MouseEventArgs e)
        {
            lastMousePosition = e.Location;

            if (e.Button == MouseButtons.Left)
            {
                var selectedLayer = layersControl.GetLayer(layersControl.GetSelectedLayerIndex());

                if (selectedLayer != null)
                {
                    if (btnShapeRect.Checked)
                    {
                        startPoint = ManipulatorGeneral.ScreenToWorld(e.Location, canvas.Width, canvas.Height);
                        isDrawing = true;
                        Color lineColor = Color.FromArgb((int)(255 * (rectLineOpacityNum.Value / rectLineOpacityNum.Maximum)), btnRectLineColor.BackColor);
                        Color fillColor = Color.FromArgb((int)(255 * (fillRectOpacityNum.Value / fillRectOpacityNum.Maximum)), btnRectFillColorShape.BackColor);
                        DashStyle dashStyle = ManipulatorGeneral.GetDashStyle(cboRectLinePattern.SelectedItem?.ToString() ?? "Solid");
                        selectedLayer.CurrentShape = new ShapeRect
                        {
                            X = startPoint.X,
                            Y = startPoint.Y,
                            LineColor = lineColor,
                            FillColor = fillColor,
                            LineWidth = rectLineSizeTrack.Value,
                            DashStyle = dashStyle
                        };
                    }
                    else if (btnShapeEllipse.Checked)
                    {
                        startPoint = ManipulatorGeneral.ScreenToWorld(e.Location, canvas.Width, canvas.Height);
                        isDrawing = true;
                        Color lineColor = Color.FromArgb((int)(255 * (ellipseLineOpacityNum.Value / ellipseLineOpacityNum.Maximum)), btnEllipseLineColor.BackColor);
                        Color fillColor = Color.FromArgb((int)(255 * (fillEllipseOpacityNum.Value / fillEllipseOpacityNum.Maximum)), btnEllipseFillColorShape.BackColor);
                        DashStyle dashStyle = ManipulatorGeneral.GetDashStyle(cboEllipseLinePattern.SelectedItem?.ToString() ?? "Solid");
                        selectedLayer.CurrentShape = new ShapeEllipse
                        {
                            X = startPoint.X,
                            Y = startPoint.Y,
                            LineColor = lineColor,
                            FillColor = fillColor,
                            LineWidth = ellipseLineSizeTrack.Value,
                            DashStyle = dashStyle
                        };
                    }
                    else if (btnShapePolygon.Checked)
                    {
                        startPoint = ManipulatorGeneral.ScreenToWorld(e.Location, canvas.Width, canvas.Height);
                        isDrawing = true;
                        Color lineColor = Color.FromArgb((int)(255 * (polygonLineOpacityNum.Value / polygonLineOpacityNum.Maximum)), btnPolygonLineColor.BackColor);
                        Color fillColor = Color.FromArgb((int)(255 * (fillPolygonOpacityNum.Value / fillPolygonOpacityNum.Maximum)), btnPolygonFillColorShape.BackColor);
                        DashStyle dashStyle = ManipulatorGeneral.GetDashStyle(cboPolygonLinePattern.SelectedItem?.ToString() ?? "Solid");
                        ShapePolygon? polygon = (selectedLayer.CurrentShape is not null && selectedLayer.CurrentShape is ShapePolygon && isUpdatingPolygonShape)
                            ? selectedLayer.CurrentShape as ShapePolygon
                            : new ShapePolygon()
                            {
                                LineColor = lineColor,
                                FillColor = fillColor,
                                LineWidth = polygonLineSizeTrack.Value,
                                DashStyle = dashStyle
                            };
                        polygon?.Points.Add(startPoint);
                        selectedLayer.CurrentShape = polygon;
                        isUpdatingPolygonShape = true;
                    }
                    else if (btnShapeText.Checked)
                    {
                        startPoint = ManipulatorGeneral.ScreenToWorld(e.Location, canvas.Width, canvas.Height);
                        isDrawing = true;
                        Color fillColor = Color.FromArgb((int)(255 * (numTextFillOpacity.Value / numTextFillOpacity.Maximum)), btnTextFillColor.BackColor);
                        DashStyle dashStyle = ManipulatorGeneral.GetDashStyle(cboRectLinePattern.SelectedItem?.ToString() ?? "Solid");
                        ShapeText shapeText = new()
                        {
                            X = startPoint.X,
                            Y = startPoint.Y,
                            Content = txtTextLineText.Text,
                            FontFamily = btnTextFont.Font.Name,
                            IsBold = btnTextFont.Font.Bold,
                            IsItalic = btnTextFont.Font.Italic,
                            FontSize = btnTextFont.Font.Size,
                            FillColor = fillColor,
                            DashStyle = dashStyle
                        };
                        selectedLayer.CurrentShape = shapeText;
                    }
                    else if (btnPointer.Checked)
                    {
                        if (ImageSelections.ContainsSelection())
                        {
                            if (selectedLayer.Image == null) return;

                            if (ManipulatorGeneral.IsOverRotationHandle(e.Location, Document.Width, Document.Height, canvas.Width, canvas.Height, Document.Zoom))
                            {
                                if (selectedAreaBitmap != null)
                                    BakeTransformIntoSelectedArea(selectedLayer);

                                isRotating = true;
                                startMouseAngle = ManipulatorGeneral.CalculateRotationAngle(e.Location, canvas.Width, canvas.Height) - rotationAngle;
                            }
                            else if (ManipulatorGeneral.IsOverScaleHandle(e.Location, canvas.Width, canvas.Height, out string handle))
                            {
                                if (selectedAreaBitmap != null)
                                    BakeTransformIntoSelectedArea(selectedLayer);

                                isScaling = true;
                                activeScaleHandle = handle;
                                scaleFactorX = 1.0f;
                                scaleFactorY = 1.0f;
                                scaleAnchorWorld = ManipulatorGeneral.GetOppositeAnchor(handle);
                                scaleStartMouseScreen = e.Location;

                                Point anchorScreen = ManipulatorGeneral.WorldToScreen(
                                    Point.Round(scaleAnchorWorld), canvas.Width, canvas.Height);

                                (bool affectsX, bool affectsY) = ManipulatorGeneral.GetHandleAxes(handle);

                                initialScaleDistanceX = affectsX
                                    ? Math.Max(1f, Math.Abs(e.Location.X - anchorScreen.X))
                                    : 1f;

                                initialScaleDistanceY = affectsY
                                    ? Math.Max(1f, Math.Abs(e.Location.Y - anchorScreen.Y))
                                    : 1f;
                            }
                            else if (ImageSelections.IsPointInSelection(ManipulatorGeneral.ScreenToWorld(e.Location, canvas.Width, canvas.Height)) >= 0)
                            {
                                isDragging = true;
                                Cursor.Current = Cursors.SizeAll;
                            }

                            if (selectedAreaBitmap == null)
                            {
                                ImageSelections.CalculateSelectionBounds();
                                selectedAreaBitmap = ManipulatorGeneral.ExtractSelectedArea(selectedLayer);
                                selectedLayer.Image = ManipulatorGeneral.CutSelectionFromLayer(selectedLayer);
                            }
                        }
                        else
                        {
                            Point worldPos = ManipulatorGeneral.ScreenToWorld(e.Location, canvas.Width, canvas.Height);
                            Point localPos = new(worldPos.X - selectedLayer.X, worldPos.Y - selectedLayer.Y);

                            if (selectedLayer.CurrentShape != null)
                            {
                                var shape = selectedLayer.CurrentShape;
                                float sx = 0, sy = 0, sw = 0, sh = 0;
                                bool canResize = false;

                                if (shape is ShapeRect r) { sx = r.X; sy = r.Y; sw = r.Width; sh = r.Height; canResize = true; }
                                else if (shape is ShapeEllipse el) { sx = el.X; sy = el.Y; sw = el.Width; sh = el.Height; canResize = true; }
                                else if (shape is ShapeText t) { sx = t.X; sy = t.Y; sw = t.Width; sh = t.Height; canResize = true; }
                                else if (shape is ShapePolygon polygon)
                                {
                                    int size = 10;
                                    int offset = size / 2;

                                    for (int i = 0; i < polygon.Points.Count; i++)
                                    {
                                        var p = polygon.Points[i];
                                        RectangleF handle = new(p.X - offset, p.Y - offset, size, size);

                                        if (handle.Contains(localPos.X, localPos.Y))
                                        {
                                            isResizingShape = true;
                                            activeHandleIndex = i;
                                            break;
                                        }
                                    }
                                }
                                else if (shape is ShapePath path)
                                {
                                    int size = 10; // Keeping the size 10 consistent with your DrawShape method
                                    int offset = size / 2;
                                    int pointIndex = 0;

                                    foreach (var segment in path.PathSegments)
                                    {
                                        for (int i = 0; i < segment.InputPoints.Count; i++)
                                        {
                                            var p = segment.InputPoints[i];
                                            RectangleF handle = new(p.X - offset, p.Y - offset, size, size);

                                            if (handle.Contains(localPos.X, localPos.Y))
                                            {
                                                isResizingShape = true;
                                                activeHandleIndex = pointIndex; // Store the flattened absolute index
                                                break;
                                            }
                                            pointIndex++;
                                        }
                                        if (isResizingShape) break;
                                    }
                                }

                                if (canResize)
                                {
                                    int size = 10;
                                    int offset = size / 2;

                                    RectangleF[] handles =
                                    [
                                        new (sx - offset, sy - offset, size, size),
                                        new (sx + sw - offset, sy - offset, size, size),
                                        new (sx - offset, sy + sh - offset, size, size),
                                        new (sx + sw - offset, sy + sh - offset, size, size)
                                    ];

                                    for (int hIndex = 0; hIndex < handles.Length; hIndex++)
                                    {
                                        if (handles[hIndex].Contains(localPos.X, localPos.Y))
                                        {
                                            isResizingShape = true;
                                            activeHandleIndex = hIndex;
                                            startPoint = localPos; // store original click spot
                                            break;
                                        }
                                    }
                                }
                            }

                            if (!isResizingShape)
                            {
                                bool found = false;
                                for (int i = selectedLayer.Shapes.Count - 1; i >= 0; i--)
                                {
                                    var shape = selectedLayer.Shapes[i];
                                    if (ManipulatorGeneral.IsPointInShape(localPos, shape))
                                    {
                                        selectedLayer.CurrentShape = shape;
                                        isDraggingShape = true;

                                        if (shape is ShapeRect r)
                                            dragOffset = new PointF(localPos.X - r.X, localPos.Y - r.Y);
                                        else if (shape is ShapeEllipse el)
                                            dragOffset = new PointF(localPos.X - el.X, localPos.Y - el.Y);
                                        else if (shape is ShapeText t)
                                            dragOffset = new PointF(localPos.X - t.X, localPos.Y - t.Y);
                                        else if (shape is ShapePolygon pg && pg.Points.Count > 0)
                                            dragOffset = new PointF(localPos.X - pg.Points[0].X, localPos.Y - pg.Points[0].Y);
                                        else if (shape is ShapePath path)
                                        {
                                            PointF anchor = PointF.Empty;
                                            foreach (var seg in path.PathSegments)
                                            {
                                                if (seg.InputPoints.Count > 0)
                                                {
                                                    anchor = seg.InputPoints[0];
                                                    break;
                                                }
                                            }
                                            dragOffset = new PointF(localPos.X - anchor.X, localPos.Y - anchor.Y);
                                        }

                                        found = true;

                                        break;
                                    }
                                }

                                if (!found)
                                    selectedLayer.CurrentShape = null;
                            }

                            UpdateControls();
                        }
                    }
                    else if (btnBrusher.Checked)
                    {
                        isPainting = true;
                        strokePoints = [];
                        lazyLocalPos = PointF.Empty;
                        strokeLastInterpolated = PointF.Empty;

                        float aspectRatio = (float)ManipulatorGeneral.Screen.Width / ManipulatorGeneral.Screen.Height;
                        float containerAspectRatio = (float)canvas.Width / canvas.Height;
                        float scale = 1.0f;
                        if (aspectRatio > containerAspectRatio)
                            scale = (float)ManipulatorGeneral.Screen.Width / canvas.Width;
                        else if (aspectRatio < containerAspectRatio)
                            scale = (float)ManipulatorGeneral.Screen.Height / canvas.Height;

                        brushPixelSize = 2 * scale * (float)brush_size.Value / brush_size.Maximum;
                        currentOpacity = (float)brush_opacity.Value / brush_opacity.Maximum;

                        PaintingEngine.BeginStroke();
                    }
                    else if (btnEraser.Checked)
                    {
                        isErasing = true;
                        strokePoints = [];
                        lazyLocalPos = PointF.Empty;
                        strokeLastInterpolated = PointF.Empty;

                        float aspectRatio = (float)ManipulatorGeneral.Screen.Width / ManipulatorGeneral.Screen.Height;
                        float containerAspectRatio = (float)canvas.Width / canvas.Height;
                        float scale = 1.0f;
                        if (aspectRatio > containerAspectRatio)
                            scale = (float)ManipulatorGeneral.Screen.Width / canvas.Width;
                        else if (aspectRatio < containerAspectRatio)
                            scale = (float)ManipulatorGeneral.Screen.Height / canvas.Height;

                        brushPixelSize = 2 * scale * (float)eraser_size.Value / eraser_size.Maximum;
                        currentOpacity = (float)eraser_opacity.Value / eraser_opacity.Maximum;

                        PaintingEngine.BeginStroke();
                    }
                    else if (btnWarp.Checked)
                    {
                        if (selectedLayer.GetBasicImage() is Bitmap bmp)
                        {
                            isWarping = true;
                            WarpEngine.WarpSnapshot?.Dispose();
                            WarpEngine.WarpSnapshot = (Bitmap)bmp.Clone();

                            float aspectRatio = (float)ManipulatorGeneral.Screen.Width / ManipulatorGeneral.Screen.Height;
                            float containerAspectRatio = (float)canvas.Width / canvas.Height;
                            float scale = 1.0f;
                            if (aspectRatio > containerAspectRatio)
                                scale = (float)ManipulatorGeneral.Screen.Width / canvas.Width;
                            else if (aspectRatio < containerAspectRatio)
                                scale = (float)ManipulatorGeneral.Screen.Height / canvas.Height;

                            brushPixelSize = warpBrushSize.Value * scale;
                        }
                    }
                    else if (btnCrop.Checked)
                    {
                        isCropping = true;
                        ImageSelections.ClearSelections();
                        Point worldPos = ManipulatorGeneral.ScreenToWorld(e.Location, canvas.Width, canvas.Height);
                        ImageSelections.AddSelectionPoint(worldPos);
                    }
                    else if (btnFiller.Checked)
                    {
                        if (selectedLayer.Image != null)
                        {
                            Bitmap? bitmap = null;
                            var selectionPolygons = ImageSelections.GetSelections();
                            if (selectionPolygons.Count == 0)
                            {
                                bitmap = ManipulatorGeneral.FillColor(selectedLayer,
                                    canvas.Width, canvas.Height,
                                    (ImageBlending)cboFillBlendMode.SelectedIndex, paint.GetFillColor(),
                                    (float)(fillOpacity.Value / fillOpacity.Maximum),
                                    lastMousePosition,
                                    new SelectionPolygon());
                            }
                            else
                            {
                                bool found = false;
                                for (int i = 0; i < selectionPolygons.Count; i++)
                                {
                                    if (ImageSelections.IsPointInPolygon(ManipulatorGeneral.ScreenToWorld(lastMousePosition, canvas.Width, canvas.Height), selectionPolygons[i]))
                                    {
                                        bitmap = ManipulatorGeneral.FillColor(selectedLayer,
                                            canvas.Width, canvas.Height,
                                            (ImageBlending)cboFillBlendMode.SelectedIndex, paint.GetFillColor(),
                                            (float)(fillOpacity.Value / fillOpacity.Maximum),
                                            lastMousePosition,
                                            selectionPolygons[i]);
                                        found = true;
                                    }
                                }

                                if (!found)
                                {
                                    for (int i = 0; i < selectionPolygons.Count; i++)
                                    {
                                        bool[,]? mask = selectionPolygons[i].Mask;

                                        if (mask != null)
                                        {
                                            Point position = ManipulatorGeneral.ScreenToWorld(lastMousePosition, canvas.Width, canvas.Height);
                                            if (position.X >= 0 && position.Y >= 0 &&
                                                mask.GetLength(0) > position.X && mask.GetLength(1) > position.Y &&
                                                mask[position.X, position.Y]) // make sure the mouse location is within the region where the mask is true
                                            {
                                                bitmap = ManipulatorGeneral.FillColor(selectedLayer,
                                                    (ImageBlending)cboFillBlendMode.SelectedIndex, paint.GetFillColor(),
                                                    (float)(fillOpacity.Value / fillOpacity.Maximum),
                                                    mask);
                                                break;
                                            }
                                        }
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
                    else if (btnLassoSelect.Checked)
                    {
                        isLassoSelecting = true;

                        if (ModifierKeys.HasFlag(Keys.Shift))
                        {
                            ImageSelections.IncreaseSelectionPolygons(null, Point.Empty);
                        }
                        else if (ModifierKeys.HasFlag(Keys.Alt))
                        {
                            ImageSelections.IncreaseSelectionPolygons(null, Point.Empty, false);
                        }
                        else
                        {
                            ImageSelections.ClearSelections();
                        }

                        ImageSelections.AddSelectionPoint(ManipulatorGeneral.ScreenToWorld(lastMousePosition, canvas.Width, canvas.Height));
                    }
                    else if (btnRectangleSelect.Checked)
                    {
                        isRectSelecting = true;

                        if (ModifierKeys.HasFlag(Keys.Shift))
                        {
                            ImageSelections.IncreaseSelectionPolygons(null, Point.Empty);
                        }
                        else if (ModifierKeys.HasFlag(Keys.Alt))
                        {
                            ImageSelections.IncreaseSelectionPolygons(null, Point.Empty, false);
                        }
                        else
                        {
                            ImageSelections.ClearSelections();
                        }

                        ImageSelections.AddSelectionPoint(ManipulatorGeneral.ScreenToWorld(lastMousePosition, canvas.Width, canvas.Height));
                    }
                    else if (btnMagicWand.Checked)
                    {
                        bool adding = true;
                        if (ModifierKeys.HasFlag(Keys.Alt))
                        {
                            adding = false;
                        }
                        if (selectedLayer != null)
                        {
                            if (selectedLayer.Image != null)
                            {
                                Point position = ManipulatorGeneral.ScreenToWorld(lastMousePosition, canvas.Width, canvas.Height);
                                position.X -= selectedLayer.X;
                                position.Y -= selectedLayer.Y;

                                bool[,] mask;

                                if (cboMWSelectionMode.Text == "Color")
                                {
                                    mask = ManipulatorLighting.ByColorSelect(selectedLayer.Image, position, (float)selectionThreshold.Value / selectionThreshold.Maximum);
                                }
                                else
                                {
                                    mask = ManipulatorLighting.MagicWandSelect(selectedLayer.Image, position, (float)selectionThreshold.Value / selectionThreshold.Maximum, cboMWSelectionMode.Text);
                                }

                                if (!ModifierKeys.HasFlag(Keys.Shift) && !ModifierKeys.HasFlag(Keys.Alt))
                                {
                                    ImageSelections.ClearSelections();
                                }

                                List<SelectionPolygon> polygons = ImageSelections.GetSelectionPointsFromMask(mask, position, adding);

                                foreach (var polygon in polygons)
                                {
                                    ImageSelections.IncreaseSelectionPolygons(polygon.Mask, polygon.SelectionPoint, adding);
                                    var transformedPoints = polygon.Points.Select(p => new Point(p.X + selectedLayer.X, p.Y + selectedLayer.Y));
                                    ImageSelections.AddSelectionPoints(transformedPoints);
                                }
                            }
                        }
                    }
                }
            }
        }

        private void PixelImage_MouseMove(object? sender, MouseEventArgs e)
        {
            labelMousePosition.Text = $"({e.X}, {e.Y})";
            labelDocStatus.Text = $"Zoom: {Document.Zoom * 100:F1}% Offset {Document.ImageOffset}";

            var selectedLayer = layersControl.GetLayer(layersControl.GetSelectedLayerIndex());

            if (isDrawing)
            {
                if (selectedLayer != null)
                {
                    Point currentWorldPos = ManipulatorGeneral.ScreenToWorld(e.Location, canvas.Width, canvas.Height);
                    Point localCurrentRaw = new(currentWorldPos.X - selectedLayer.X, currentWorldPos.Y - selectedLayer.Y);

                    if (selectedLayer.CurrentShape is ShapeRect rect)
                    {
                        rect.X = Math.Min(startPoint.X, localCurrentRaw.X);
                        rect.Y = Math.Min(startPoint.Y, localCurrentRaw.Y);
                        rect.Width = Math.Abs(startPoint.X - localCurrentRaw.X);
                        rect.Height = Math.Abs(startPoint.Y - localCurrentRaw.Y);
                    }
                    else if (selectedLayer.CurrentShape is ShapeEllipse ellipse)
                    {
                        ellipse.X = Math.Min(startPoint.X, localCurrentRaw.X);
                        ellipse.Y = Math.Min(startPoint.Y, localCurrentRaw.Y);
                        ellipse.Width = Math.Abs(startPoint.X - localCurrentRaw.X);
                        ellipse.Height = Math.Abs(startPoint.Y - localCurrentRaw.Y);
                    }
                    else if (selectedLayer.CurrentShape is ShapeText text)
                    {
                        text.X = Math.Min(startPoint.X, localCurrentRaw.X);
                        text.Y = Math.Min(startPoint.Y, localCurrentRaw.Y);
                        text.Width = Math.Abs(startPoint.X - localCurrentRaw.X);
                        text.Height = Math.Abs(startPoint.Y - localCurrentRaw.Y);
                    }

                    isDrawingShape = true; // This flag is used to trigger redraw of shapes in the rendering loop
                }
            }
            else if (isDragging)
            {
                int dx = e.X - lastMousePosition.X;
                int dy = e.Y - lastMousePosition.Y;

                if (ModifierKeys.HasFlag(Keys.Control))
                {
                    float zoomDelta = dy * 0.01f;
                    Document.Zoom = Math.Max(0.1f, Math.Min(10.0f, Document.Zoom - zoomDelta));
                    ManipulatorGeneral.InvalidateCompositeBuffers();
                    RedrawImage(selectedLayerIndex: -1, repopulateImage: false);
                }
                else if (ModifierKeys.HasFlag(Keys.Shift))
                {
                    float x = Document.ImageOffset.X + dx;
                    float y = Document.ImageOffset.Y + dy;
                    Document.ImageOffset = new PointF(x, y);
                    ManipulatorGeneral.InvalidateCompositeBuffers();
                    RedrawImage(selectedLayerIndex: -1, repopulateImage: false);
                }
                else
                {
                    if (selectedLayer != null)
                    {
                        if (ImageSelections.ContainsSelection())
                        {
                            float ratio = ManipulatorGeneral.ScreenToWorldRatio(canvas.Width, canvas.Height);
                            float worldDx = dx / ratio;
                            float worldDy = dy / ratio;
                            ImageSelections.MoveSelection(worldDx, worldDy);
                            UpdateTransformMatrix();
                            RedrawImage();
                        }
                        else
                        {
                            float ratio = ManipulatorGeneral.ScreenToWorldRatio(canvas.Width, canvas.Height);
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
            else if (isResizingShape)
            {
                if (selectedLayer != null && selectedLayer.CurrentShape != null)
                {
                    Point currentWorldPos = ManipulatorGeneral.ScreenToWorld(e.Location, canvas.Width, canvas.Height);
                    Point localPos = new(currentWorldPos.X - selectedLayer.X, currentWorldPos.Y - selectedLayer.Y);

                    float nX = 0, nY = 0, nW = 0, nH = 0;
                    if (selectedLayer.CurrentShape is ShapeRect r) { nX = r.X; nY = r.Y; nW = r.Width; nH = r.Height; }
                    else if (selectedLayer.CurrentShape is ShapeEllipse el) { nX = el.X; nY = el.Y; nW = el.Width; nH = el.Height; }
                    else if (selectedLayer.CurrentShape is ShapeText t) { nX = t.X; nY = t.Y; nW = t.Width; nH = t.Height; }
                    else if (selectedLayer.CurrentShape is ShapePolygon polygon)
                    {
                        if (activeHandleIndex >= 0 && activeHandleIndex < polygon.Points.Count)
                        {
                            polygon.Points[activeHandleIndex] = new PointF(localPos.X, localPos.Y);
                        }
                    }
                    else if (selectedLayer.CurrentShape is ShapePath path)
                    {
                        int pointIndex = 0;
                        foreach (var segment in path.PathSegments)
                        {
                            for (int i = 0; i < segment.InputPoints.Count; i++)
                            {
                                if (pointIndex == activeHandleIndex)
                                {
                                    segment.InputPoints[i] = new PointF(localPos.X, localPos.Y);
                                    pointIndex++;
                                    break;
                                }
                                pointIndex++;
                            }
                        }
                    }

                    float right = nX + nW;
                    float bottom = nY + nH;

                    // 0: Top-Left, 1: Top-Right, 2: Bottom-Left, 3: Bottom-Right
                    if (activeHandleIndex == 0)
                    {
                        nX = Math.Min(localPos.X, right - 5);
                        nY = Math.Min(localPos.Y, bottom - 5);
                        nW = right - nX;
                        nH = bottom - nY;
                    }
                    else if (activeHandleIndex == 1)
                    {
                        nY = Math.Min(localPos.Y, bottom - 5);
                        nW = Math.Max(5, localPos.X - nX);
                        nH = bottom - nY;
                    }
                    else if (activeHandleIndex == 2)
                    {
                        nX = Math.Min(localPos.X, right - 5);
                        nW = right - nX;
                        nH = Math.Max(5, localPos.Y - nY);
                    }
                    else if (activeHandleIndex == 3)
                    {
                        nW = Math.Max(5, localPos.X - nX);
                        nH = Math.Max(5, localPos.Y - nY);
                    }

                    if (selectedLayer.CurrentShape is ShapeRect rect) { rect.X = nX; rect.Y = nY; rect.Width = nW; rect.Height = nH; }
                    else if (selectedLayer.CurrentShape is ShapeEllipse ellipse) { ellipse.X = nX; ellipse.Y = nY; ellipse.Width = nW; ellipse.Height = nH; }
                    else if (selectedLayer.CurrentShape is ShapeText text) { text.X = nX; text.Y = nY; text.Width = nW; text.Height = nH; }

                    isDrawingShape = true;
                }
            }
            else if (isDraggingShape)
            {
                if (selectedLayer != null && selectedLayer.CurrentShape != null)
                {
                    Point currentWorldPos = ManipulatorGeneral.ScreenToWorld(e.Location, canvas.Width, canvas.Height);
                    Point localPos = new(currentWorldPos.X - selectedLayer.X, currentWorldPos.Y - selectedLayer.Y);

                    float newX = localPos.X - dragOffset.X;
                    float newY = localPos.Y - dragOffset.Y;

                    if (selectedLayer.CurrentShape is ShapeRect rect)
                    {
                        rect.X = newX;
                        rect.Y = newY;
                    }
                    else if (selectedLayer.CurrentShape is ShapeEllipse ellipse)
                    {
                        ellipse.X = newX;
                        ellipse.Y = newY;
                    }
                    else if (selectedLayer.CurrentShape is ShapeText text)
                    {
                        text.X = newX;
                        text.Y = newY;
                    }
                    else if (selectedLayer.CurrentShape is ShapePolygon polygon)
                    {
                        float dx = (newX - polygon.Points[0].X);
                        float dy = (newY - polygon.Points[0].Y);
                        for (int i = 0; i < polygon.Points.Count; i++)
                        {
                            polygon.Points[i] = new PointF(polygon.Points[i].X + dx, polygon.Points[i].Y + dy);
                        }
                    }
                    else if (selectedLayer.CurrentShape is ShapePath path)
                    {
                        PointF anchor = PointF.Empty;
                        foreach (var seg in path.PathSegments)
                        {
                            if (seg.InputPoints.Count > 0)
                            {
                                anchor = seg.InputPoints[0];
                                break;
                            }
                        }

                        float dx = newX - anchor.X;
                        float dy = newY - anchor.Y;

                        foreach (var segment in path.PathSegments)
                        {
                            for (int i = 0; i < segment.InputPoints.Count; i++)
                            {
                                segment.InputPoints[i] = new PointF(segment.InputPoints[i].X + dx, segment.InputPoints[i].Y + dy);
                            }
                        }
                    }

                    isDrawingShape = true; // This flag is used to trigger redraw of shapes in the rendering loop
                }
            }
            else if (isRotating)
            {
                if (ImageSelections.ContainsSelection())
                {
                    float angle = ManipulatorGeneral.CalculateRotationAngle(e.Location, canvas.Width, canvas.Height) - startMouseAngle;
                    RotateSelectedLayerImage(angle);
                }
            }
            else if (isScaling)
            {
                if (ImageSelections.ContainsSelection())
                {
                    (scaleFactorX, scaleFactorY) = CalculateScaleFactors(e.Location, activeScaleHandle);
                    UpdateTransformMatrix();
                }
            }
            else if (isWarping)
            {
                if (selectedLayer != null && selectedLayer.GetBasicImage() is Bitmap bmp && WarpEngine.WarpSnapshot != null)
                {
                    Point worldCurrent = ManipulatorGeneral.ScreenToWorld(e.Location, canvas.Width, canvas.Height);
                    Point worldLast = ManipulatorGeneral.ScreenToWorld(lastMousePosition, canvas.Width, canvas.Height);
                    PointF localCenter = new(worldCurrent.X - selectedLayer.X, worldCurrent.Y - selectedLayer.Y);
                    float dx = worldCurrent.X - worldLast.X;
                    float dy = worldCurrent.Y - worldLast.Y;
                    if (Math.Abs(dx) > 0.01f || Math.Abs(dy) > 0.01f)
                    {
                        WarpEngine.ApplyForwardWarp(WarpEngine.WarpSnapshot, bmp, localCenter, dx, dy, brushPixelSize);
                        WarpEngine.WarpSnapshot.Dispose();
                        WarpEngine.WarpSnapshot = (Bitmap)bmp.Clone();
                        ManipulatorGeneral.LayerCache.Remove(selectedLayer.Name);
                        ManipulatorGeneral.DirtyRegions.Add(new Rectangle(
                            selectedLayer.X, selectedLayer.Y,
                            bmp.Width, bmp.Height));
                        const int minWarpIntervalMs = 32;
                        if ((DateTime.Now - lastPaintTime).TotalMilliseconds >= minWarpIntervalMs)
                        {
                            RedrawImage(layersControl.GetSelectedLayerIndex());
                            lastPaintTime = DateTime.Now;
                        }
                    }
                }
                lastMousePosition = e.Location;
            }
            else if (isPainting || isErasing)
            {
                if (selectedLayer != null)
                {
                    var sw = System.Diagnostics.Stopwatch.StartNew();

                    float lazySmoothing = (float)brush_smoothness.Value / brush_smoothness.Maximum;

                    Point currentWorldPos = ManipulatorGeneral.ScreenToWorld(e.Location, canvas.Width, canvas.Height);
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

                    float radius = paint.Brush != null ? (int)(paint.Brush.Width * brushPixelSize / 2) : 0;
                    Rectangle dirty = new(0, 0, 0, 0);

                    if (strokePoints.Count > 1)
                    {
                        int minX = (int)(Math.Min(strokePoints[^2].X, strokePoints[^1].X) - radius);
                        int minY = (int)(Math.Min(strokePoints[^2].Y, strokePoints[^1].Y) - radius);
                        int maxX = (int)(Math.Max(strokePoints[^2].X, strokePoints[^1].X) + radius);
                        int maxY = (int)(Math.Max(strokePoints[^2].Y, strokePoints[^1].Y) + radius);

                        dirty = new(minX + selectedLayer.X, minY + selectedLayer.Y, maxX - minX, maxY - minY);

                        dirty.Intersect(new Rectangle(0, 0, Document.Width, Document.Height));

                        var selectionPolygons = ImageSelections.GetSelections();

                        if (strokePoints.Count == 2)
                        {
                            Point start = Point.Round(strokePoints[0]);
                            Point end = Point.Round(strokePoints[1]);

                            PaintingEngine.PaintStroke(start, end, brushPixelSize, currentOpacity, selectionPolygons.Count > 0 ? selectionPolygons[0].Mask : null);
                        }
                        else
                        {
                            int n = strokePoints.Count;
                            PointF p0 = (n > 2) ? strokePoints[n - 3] : strokePoints[n - 2];
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
                                    PaintingEngine.PaintStroke(prevRounded, currRounded, brushPixelSize, currentOpacity, selectionPolygons.Count > 0 ? selectionPolygons[0].Mask : null);
                                }

                                previousPos = pos;
                            }

                            strokeLastInterpolated = previousPos;
                        }
                    }

                    if (!dirty.IsEmpty)
                        ManipulatorGeneral.DirtyRegions.Add(dirty);

                    lastMousePosition = e.Location;

                    sw.Stop();
                    Console.WriteLine($"painting: {sw.ElapsedMilliseconds}ms");

                    const int minPaintIntervalMs = 32;
                    if ((DateTime.Now - lastPaintTime).TotalMilliseconds >= minPaintIntervalMs)
                    {
                        RedrawImage(layersControl.GetSelectedLayerIndex());
                        ManipulatorGeneral.DirtyRegions.Clear();
                        ManipulatorGeneral.DirtyRegions.Add(dirty);
                        lastPaintTime = DateTime.Now;
                    }
                }
            }
            else if (isLassoSelecting)
            {
                if (e.X != lastMousePosition.X || e.Y != lastMousePosition.Y)
                {
                    ImageSelections.AddSelectionPoint(ManipulatorGeneral.ScreenToWorld(e.Location, canvas.Width, canvas.Height));
                }
            }
            else if (isRectSelecting)
            {
                if (e.X != lastMousePosition.X || e.Y != lastMousePosition.Y)
                {
                    Point worldCurrent = ManipulatorGeneral.ScreenToWorld(e.Location, canvas.Width, canvas.Height);
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
            else if (isCropping)
            {
                if (e.X != lastMousePosition.X || e.Y != lastMousePosition.Y)
                {
                    Point worldCurrent = ManipulatorGeneral.ScreenToWorld(e.Location, canvas.Width, canvas.Height);
                    var lastSelection = ImageSelections.GetLastSelection();

                    if (lastSelection.Count > 0)
                    {
                        Point worldAnchor = lastSelection[0];

                        if (lastSelection.Count == 1)
                        {
                            ImageSelections.AddSelectionPoint(new Point(worldCurrent.X, worldAnchor.Y));
                            ImageSelections.AddSelectionPoint(worldCurrent);
                            ImageSelections.AddSelectionPoint(new Point(worldAnchor.X, worldCurrent.Y));
                            ImageSelections.AddSelectionPoint(worldAnchor);
                        }
                        else if (lastSelection.Count == 5)
                        {
                            ImageSelections.UpdateSelectionPoint(1, new Point(worldCurrent.X, worldAnchor.Y));
                            ImageSelections.UpdateSelectionPoint(2, worldCurrent);
                            ImageSelections.UpdateSelectionPoint(3, new Point(worldAnchor.X, worldCurrent.Y));
                        }

                        RedrawImage();
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

        private void PixelImage_MouseUp(object? sender, MouseEventArgs e)
        {
            var selectedLayer = layersControl.GetLayer(layersControl.GetSelectedLayerIndex());
            if (isLassoSelecting)
            {
                if (ImageSelections.GetLastSelection().Count > 1)
                {
                    ImageSelections.AddSelectionPoint(ImageSelections.GetLastSelection()[0]);
                }
            }
            if (selectedLayer != null)
            {
                if (selectedLayer.Image != null)
                {
                    HistoryManager.RecordState(new HistoryItem(layersControl.GetLayers(), layersControl.GetSelectedLayerIndex()));
                    ManipulatorGeneral.UpdateBuffers();
                    PaintingEngine.EndStroke();
                    ImageSelections.MasAndMergeSelections(selectedLayer.X, selectedLayer.Y, selectedLayer.Image.Width, selectedLayer.Image.Height);
                    ImageSelections.CalculateSelectionBounds();
                    layersControl.RefreshLayersDisplay();
                    RedrawImage();
                    HistoryManager.CurrentState(new HistoryItem(layersControl.GetLayers(), layersControl.GetSelectedLayerIndex()));
                }
                if (selectedLayer.CurrentShape != null && selectedLayer.CurrentShape is not ShapePolygon)
                {
                    HistoryManager.RecordState(new HistoryItem(layersControl.GetLayers(), layersControl.GetSelectedLayerIndex()));
                    if (selectedLayer.IsNewShape(selectedLayer.CurrentShape))
                    {
                        selectedLayer.Shapes.Add(selectedLayer.CurrentShape);
                    }
                    RedrawImage();
                    HistoryManager.CurrentState(new HistoryItem(layersControl.GetLayers(), layersControl.GetSelectedLayerIndex()));
                }
            }
            WarpEngine.WarpSnapshot?.Dispose();
            WarpEngine.WarpSnapshot = null;
            isWarping = false;
            isDragging = false;
            isPainting = false;
            isErasing = false;
            isLassoSelecting = false;
            isRectSelecting = false;
            isCropping = false;
            isRotating = false;
            isScaling = false;
            isDrawing = false;
            isDraggingShape = false;
            isResizingShape = false;
            activeHandleIndex = -1;
        }

        private void Canvas_MouseDoubleClick(object? sender, MouseEventArgs e)
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

                if (selectedLayer != null && selectedLayer.CurrentShape != null && selectedLayer.CurrentShape is ShapePolygon)
                {
                    HistoryManager.RecordState(new HistoryItem(layersControl.GetLayers(), layersControl.GetSelectedLayerIndex()));
                    if (selectedLayer.IsNewShape(selectedLayer.CurrentShape))
                    {
                        selectedLayer.Shapes.Add(selectedLayer.CurrentShape);
                    }
                    RedrawImage();
                    isDraggingShape = false;
                    isUpdatingPolygonShape = false;
                    HistoryManager.CurrentState(new HistoryItem(layersControl.GetLayers(), layersControl.GetSelectedLayerIndex()));
                }

                rotationAngle = 0;
            }
        }

        private void RedrawImage(int selectedLayerIndex = -1, bool repopulateImage = true)
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();

            if (repopulateImage)
            {
                ManipulatorGeneral.PopulateColorGrid(layersControl.GetLayers(), selectedLayerIndex);
            }

            var rect = repopulateImage ?
                new Rectangle(0, 0, Document.Width, Document.Height) :
                new Rectangle(0, 0, 0, 0);

            RedrawRasterImage(ManipulatorGeneral.GetImage(ManipulatorGeneral.Screen, rect));
            RedrawSelectedLayerRect();
            RedrawHandles();

            sw.Stop();

            if (dashOffset == 0)
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
                    scaledWidth = canvas.Width * Document.Zoom;
                    scaledHeight = scaledWidth / aspectRatio;
                }
                else
                {
                    scaledHeight = canvas.Height * Document.Zoom;
                    scaledWidth = scaledHeight * aspectRatio;
                }

                float centerX = (canvas.Width - scaledWidth) / 2;
                float centerY = (canvas.Height - scaledHeight) / 2;

                RectangleF destRect = new(centerX + Document.ImageOffset.X, centerY + Document.ImageOffset.Y, scaledWidth, scaledHeight);

                g.InterpolationMode = (Document.Zoom < 0.5f || Document.Zoom > 2.0f) ? InterpolationMode.HighQualityBicubic : InterpolationMode.NearestNeighbor;
                g.PixelOffsetMode = PixelOffsetMode.Half;
                g.DrawImage(image, destRect);

                if (selectedAreaBitmap != null)
                {
                    RectangleF screenBounds = ManipulatorGeneral.GetSelectionBoundsScreen(canvas.Width, canvas.Height);
                    g.MultiplyTransform(transformMatrix);
                    g.DrawImage(selectedAreaBitmap, screenBounds.X, screenBounds.Y, screenBounds.Width, screenBounds.Height);
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
                selectionPen.DashOffset = dashOffset;
                var selectedLayer = layersControl.GetLayer(layersControl.GetSelectedLayerIndex());
                if (selectedLayer != null && selectedLayer.Image != null)
                {
                    float ratio = ManipulatorGeneral.ScreenToWorldRatio(canvas.Width, canvas.Height);
                    Point p = ManipulatorGeneral.WorldToScreen(new Point(selectedLayer.X, selectedLayer.Y), canvas.Width, canvas.Height);
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
                            ManipulatorGeneral.WorldToScreen(p, canvas.Width, canvas.Height))];

                        using Pen selectionPen = new(Color.Blue, 2f);
                        selectionPen.DashPattern = [5, 5];
                        selectionPen.DashOffset = dashOffset;
                        g.DrawLines(selectionPen, screenPoints);
                    }

                    g.SmoothingMode = SmoothingMode.AntiAlias;

                    using Pen handlePen = new(Color.White, 1.5f);
                    using Brush scaleBrush = new SolidBrush(Color.FromArgb(0, 120, 215));
                    using Brush rotateBrush = new SolidBrush(Color.Gold);

                    RectangleF screenBounds = ManipulatorGeneral.GetSelectionBoundsScreen(canvas.Width, canvas.Height);

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

                    // After drawing corner handles, add inside RedrawHandles():

                    // Mid-point handles
                    float hSize = ImageSelections.SCALE_HANDLE_SIZE;
                    PointF[] midPoints =
                    [
                        new(screenBounds.X + screenBounds.Width / 2f, screenBounds.Y),          // topMid
                        new(screenBounds.X + screenBounds.Width / 2f, screenBounds.Bottom),     // bottomMid
                        new(screenBounds.X,                            screenBounds.Y + screenBounds.Height / 2f), // leftMid
                        new(screenBounds.Right,                        screenBounds.Y + screenBounds.Height / 2f), // rightMid
                    ];

                    foreach (var mp in midPoints)
                    {
                        RectangleF rect = new(mp.X - hSize / 2, mp.Y - hSize / 2, hSize, hSize);
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

        private void FormMain_FormClosing(object? sender, FormClosingEventArgs e)
        {
            if (!ConfirmAbandonChanges())
            {
                e.Cancel = true;
            }
        }

        private void ExitToolStripMenuItem_Click(object? sender, EventArgs e)
        {
            Application.Exit();
        }
    }
}