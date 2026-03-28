namespace PixelEditor
{
    partial class FormSettings
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            groupBox1 = new GroupBox();
            chkCanvasRatio = new CheckBox();
            btnResetCanvas = new Button();
            btnAutoResizeCanvas = new Button();
            canvasHeight = new NumericUpDown();
            canvasWidth = new NumericUpDown();
            groupBox2 = new GroupBox();
            chkLayerRatio = new CheckBox();
            chkCenterLayer = new CheckBox();
            chkResizeImage = new CheckBox();
            btnResetLayer = new Button();
            btnResizeHeight = new Button();
            btnResizeWidth = new Button();
            btnAutoResizeLayer = new Button();
            layerHeight = new NumericUpDown();
            layerWidth = new NumericUpDown();
            btnOK = new Button();
            btnCancel = new Button();
            groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)canvasHeight).BeginInit();
            ((System.ComponentModel.ISupportInitialize)canvasWidth).BeginInit();
            groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)layerHeight).BeginInit();
            ((System.ComponentModel.ISupportInitialize)layerWidth).BeginInit();
            SuspendLayout();
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(chkCanvasRatio);
            groupBox1.Controls.Add(btnResetCanvas);
            groupBox1.Controls.Add(btnAutoResizeCanvas);
            groupBox1.Controls.Add(canvasHeight);
            groupBox1.Controls.Add(canvasWidth);
            groupBox1.Location = new Point(12, 12);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new Size(360, 60);
            groupBox1.TabIndex = 8;
            groupBox1.TabStop = false;
            groupBox1.Text = "Canvas Size:";
            // 
            // chkCanvasRatio
            // 
            chkCanvasRatio.Appearance = Appearance.Button;
            chkCanvasRatio.Location = new Point(72, 22);
            chkCanvasRatio.Name = "chkCanvasRatio";
            chkCanvasRatio.Size = new Size(22, 22);
            chkCanvasRatio.TabIndex = 9;
            chkCanvasRatio.Text = "x";
            chkCanvasRatio.TextAlign = ContentAlignment.MiddleCenter;
            chkCanvasRatio.UseVisualStyleBackColor = true;
            // 
            // btnResetCanvas
            // 
            btnResetCanvas.FlatStyle = FlatStyle.Popup;
            btnResetCanvas.Location = new Point(296, 22);
            btnResetCanvas.Name = "btnResetCanvas";
            btnResetCanvas.Size = new Size(58, 23);
            btnResetCanvas.TabIndex = 6;
            btnResetCanvas.TabStop = false;
            btnResetCanvas.Text = "Reset";
            btnResetCanvas.UseVisualStyleBackColor = true;
            btnResetCanvas.Click += BtnResetCanvas_Click;
            // 
            // btnAutoResizeCanvas
            // 
            btnAutoResizeCanvas.FlatStyle = FlatStyle.Popup;
            btnAutoResizeCanvas.Location = new Point(169, 22);
            btnAutoResizeCanvas.Name = "btnAutoResizeCanvas";
            btnAutoResizeCanvas.Size = new Size(117, 23);
            btnAutoResizeCanvas.TabIndex = 6;
            btnAutoResizeCanvas.TabStop = false;
            btnAutoResizeCanvas.Text = "Resize to Layer";
            btnAutoResizeCanvas.UseVisualStyleBackColor = true;
            btnAutoResizeCanvas.Click += BtnAutoResizeCanvas_Click;
            // 
            // canvasHeight
            // 
            canvasHeight.Location = new Point(100, 22);
            canvasHeight.Maximum = new decimal(new int[] { 4096, 0, 0, 0 });
            canvasHeight.Minimum = new decimal(new int[] { 2, 0, 0, 0 });
            canvasHeight.Name = "canvasHeight";
            canvasHeight.Size = new Size(60, 23);
            canvasHeight.TabIndex = 1;
            canvasHeight.TextAlign = HorizontalAlignment.Right;
            canvasHeight.Value = new decimal(new int[] { 2, 0, 0, 0 });
            canvasHeight.ValueChanged += CanvasHeight_ValueChanged;
            // 
            // canvasWidth
            // 
            canvasWidth.Location = new Point(6, 22);
            canvasWidth.Maximum = new decimal(new int[] { 4096, 0, 0, 0 });
            canvasWidth.Minimum = new decimal(new int[] { 2, 0, 0, 0 });
            canvasWidth.Name = "canvasWidth";
            canvasWidth.Size = new Size(60, 23);
            canvasWidth.TabIndex = 0;
            canvasWidth.TextAlign = HorizontalAlignment.Right;
            canvasWidth.Value = new decimal(new int[] { 2, 0, 0, 0 });
            canvasWidth.ValueChanged += CanvasWidth_ValueChanged;
            // 
            // groupBox2
            // 
            groupBox2.Controls.Add(chkLayerRatio);
            groupBox2.Controls.Add(chkCenterLayer);
            groupBox2.Controls.Add(chkResizeImage);
            groupBox2.Controls.Add(btnResetLayer);
            groupBox2.Controls.Add(btnResizeHeight);
            groupBox2.Controls.Add(btnResizeWidth);
            groupBox2.Controls.Add(btnAutoResizeLayer);
            groupBox2.Controls.Add(layerHeight);
            groupBox2.Controls.Add(layerWidth);
            groupBox2.Location = new Point(12, 78);
            groupBox2.Name = "groupBox2";
            groupBox2.Size = new Size(360, 164);
            groupBox2.TabIndex = 9;
            groupBox2.TabStop = false;
            groupBox2.Text = "Layer Size:";
            // 
            // chkLayerRatio
            // 
            chkLayerRatio.Appearance = Appearance.Button;
            chkLayerRatio.Location = new Point(72, 22);
            chkLayerRatio.Name = "chkLayerRatio";
            chkLayerRatio.Size = new Size(22, 22);
            chkLayerRatio.TabIndex = 9;
            chkLayerRatio.Text = "x";
            chkLayerRatio.TextAlign = ContentAlignment.MiddleCenter;
            chkLayerRatio.UseVisualStyleBackColor = true;
            // 
            // chkCenterLayer
            // 
            chkCenterLayer.AutoSize = true;
            chkCenterLayer.Location = new Point(6, 137);
            chkCenterLayer.Name = "chkCenterLayer";
            chkCenterLayer.Size = new Size(92, 19);
            chkCenterLayer.TabIndex = 8;
            chkCenterLayer.Text = "Center Layer";
            chkCenterLayer.UseVisualStyleBackColor = true;
            // 
            // chkResizeImage
            // 
            chkResizeImage.AutoSize = true;
            chkResizeImage.Location = new Point(6, 112);
            chkResizeImage.Name = "chkResizeImage";
            chkResizeImage.Size = new Size(125, 19);
            chkResizeImage.TabIndex = 8;
            chkResizeImage.Text = "Resize Layer Image";
            chkResizeImage.UseVisualStyleBackColor = true;
            // 
            // btnResetLayer
            // 
            btnResetLayer.FlatStyle = FlatStyle.Popup;
            btnResetLayer.Location = new Point(296, 22);
            btnResetLayer.Name = "btnResetLayer";
            btnResetLayer.Size = new Size(58, 23);
            btnResetLayer.TabIndex = 7;
            btnResetLayer.TabStop = false;
            btnResetLayer.Text = "Reset";
            btnResetLayer.UseVisualStyleBackColor = true;
            btnResetLayer.Click += BtnResetLayer_Click;
            // 
            // btnResizeHeight
            // 
            btnResizeHeight.FlatStyle = FlatStyle.Popup;
            btnResizeHeight.Location = new Point(169, 80);
            btnResizeHeight.Name = "btnResizeHeight";
            btnResizeHeight.Size = new Size(117, 23);
            btnResizeHeight.TabIndex = 7;
            btnResizeHeight.TabStop = false;
            btnResizeHeight.Text = "Resize Height";
            btnResizeHeight.UseVisualStyleBackColor = true;
            btnResizeHeight.Click += BtnResizeHeight_Click;
            // 
            // btnResizeWidth
            // 
            btnResizeWidth.FlatStyle = FlatStyle.Popup;
            btnResizeWidth.Location = new Point(169, 51);
            btnResizeWidth.Name = "btnResizeWidth";
            btnResizeWidth.Size = new Size(117, 23);
            btnResizeWidth.TabIndex = 7;
            btnResizeWidth.TabStop = false;
            btnResizeWidth.Text = "Resize Width";
            btnResizeWidth.UseVisualStyleBackColor = true;
            btnResizeWidth.Click += BtnResizeWidth_Click;
            // 
            // btnAutoResizeLayer
            // 
            btnAutoResizeLayer.FlatStyle = FlatStyle.Popup;
            btnAutoResizeLayer.Location = new Point(169, 22);
            btnAutoResizeLayer.Name = "btnAutoResizeLayer";
            btnAutoResizeLayer.Size = new Size(117, 23);
            btnAutoResizeLayer.TabIndex = 7;
            btnAutoResizeLayer.TabStop = false;
            btnAutoResizeLayer.Text = "Resize to Canvas";
            btnAutoResizeLayer.UseVisualStyleBackColor = true;
            btnAutoResizeLayer.Click += BtnAutoResizeLayer_Click;
            // 
            // layerHeight
            // 
            layerHeight.Location = new Point(100, 22);
            layerHeight.Maximum = new decimal(new int[] { 4096, 0, 0, 0 });
            layerHeight.Minimum = new decimal(new int[] { 2, 0, 0, 0 });
            layerHeight.Name = "layerHeight";
            layerHeight.Size = new Size(60, 23);
            layerHeight.TabIndex = 3;
            layerHeight.TextAlign = HorizontalAlignment.Right;
            layerHeight.Value = new decimal(new int[] { 2, 0, 0, 0 });
            layerHeight.ValueChanged += LayerHeight_ValueChanged;
            // 
            // layerWidth
            // 
            layerWidth.Location = new Point(6, 22);
            layerWidth.Maximum = new decimal(new int[] { 4096, 0, 0, 0 });
            layerWidth.Minimum = new decimal(new int[] { 2, 0, 0, 0 });
            layerWidth.Name = "layerWidth";
            layerWidth.Size = new Size(60, 23);
            layerWidth.TabIndex = 2;
            layerWidth.TextAlign = HorizontalAlignment.Right;
            layerWidth.Value = new decimal(new int[] { 2, 0, 0, 0 });
            layerWidth.ValueChanged += LayerWidth_ValueChanged;
            // 
            // btnOK
            // 
            btnOK.Location = new Point(73, 426);
            btnOK.Name = "btnOK";
            btnOK.Size = new Size(102, 23);
            btnOK.TabIndex = 4;
            btnOK.Text = "OK";
            btnOK.UseVisualStyleBackColor = true;
            btnOK.Click += BtnOK_Click;
            // 
            // btnCancel
            // 
            btnCancel.Location = new Point(181, 426);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(102, 23);
            btnCancel.TabIndex = 5;
            btnCancel.Text = "Cancel";
            btnCancel.UseVisualStyleBackColor = true;
            btnCancel.Click += BtnCancel_Click;
            // 
            // FormSettings
            // 
            AcceptButton = btnOK;
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = btnCancel;
            ClientSize = new Size(384, 461);
            Controls.Add(btnCancel);
            Controls.Add(btnOK);
            Controls.Add(groupBox2);
            Controls.Add(groupBox1);
            FormBorderStyle = FormBorderStyle.FixedToolWindow;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "FormSettings";
            Text = "Settings";
            Load += FormSettings_Load;
            groupBox1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)canvasHeight).EndInit();
            ((System.ComponentModel.ISupportInitialize)canvasWidth).EndInit();
            groupBox2.ResumeLayout(false);
            groupBox2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)layerHeight).EndInit();
            ((System.ComponentModel.ISupportInitialize)layerWidth).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private GroupBox groupBox1;
        private NumericUpDown canvasHeight;
        private NumericUpDown canvasWidth;
        private GroupBox groupBox2;
        private NumericUpDown layerHeight;
        private NumericUpDown layerWidth;
        private Button btnOK;
        private Button btnCancel;
        private Button btnAutoResizeCanvas;
        private Button btnAutoResizeLayer;
        private Button btnResetCanvas;
        private Button btnResetLayer;
        private CheckBox chkCenterLayer;
        private CheckBox chkResizeImage;
        private Button btnResizeHeight;
        private Button btnResizeWidth;
        private CheckBox chkLayerRatio;
        private CheckBox chkCanvasRatio;
    }
}