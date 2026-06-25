namespace PixelEditor
{
    partial class FormLayer
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
            textBoxName = new TextBox();
            btnOK = new Button();
            btnCancel = new Button();
            label2 = new Label();
            cboFillWith = new ComboBox();
            label4 = new Label();
            btnBackgroundColor = new Button();
            width = new NumericUpDown();
            label6 = new Label();
            height = new NumericUpDown();
            label8 = new Label();
            offsetX = new NumericUpDown();
            offsetY = new NumericUpDown();
            btnCenterX = new Button();
            btnCenterY = new Button();
            btnAutoWidth = new Button();
            btnAutoHeight = new Button();
            pictureMask = new PictureBox();
            label10 = new Label();
            cboLayers = new ComboBox();
            cboType = new ComboBox();
            label11 = new Label();
            label9 = new Label();
            label7 = new Label();
            groupRasterProperties = new GroupBox();
            trackBarAdjustmentValue2 = new TrackBar();
            trackBarAdjustmentValue1 = new TrackBar();
            listBoxAdjustments = new ListBox();
            pictureBox2 = new PictureBox();
            btnAdjustmentVisibility = new Button();
            btnMoveAdjustmentDown = new Button();
            lblAdjustmentValue2 = new Label();
            btnMoveAdjustmentUp = new Button();
            lblAdjustmentValue1 = new Label();
            btnRemoveAdjustment = new Button();
            label1 = new Label();
            cboAdjustments = new ComboBox();
            layerImage = new PictureBox();
            ((System.ComponentModel.ISupportInitialize)width).BeginInit();
            ((System.ComponentModel.ISupportInitialize)height).BeginInit();
            ((System.ComponentModel.ISupportInitialize)offsetX).BeginInit();
            ((System.ComponentModel.ISupportInitialize)offsetY).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pictureMask).BeginInit();
            groupRasterProperties.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)trackBarAdjustmentValue2).BeginInit();
            ((System.ComponentModel.ISupportInitialize)trackBarAdjustmentValue1).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox2).BeginInit();
            ((System.ComponentModel.ISupportInitialize)layerImage).BeginInit();
            SuspendLayout();
            // 
            // textBoxName
            // 
            textBoxName.Location = new Point(110, 41);
            textBoxName.Name = "textBoxName";
            textBoxName.Size = new Size(167, 23);
            textBoxName.TabIndex = 0;
            // 
            // btnOK
            // 
            btnOK.DialogResult = DialogResult.OK;
            btnOK.Location = new Point(160, 626);
            btnOK.Name = "btnOK";
            btnOK.Size = new Size(75, 23);
            btnOK.TabIndex = 13;
            btnOK.Text = "OK";
            btnOK.UseVisualStyleBackColor = true;
            btnOK.Click += BtnOK_Click;
            // 
            // btnCancel
            // 
            btnCancel.DialogResult = DialogResult.Cancel;
            btnCancel.Location = new Point(241, 626);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(75, 23);
            btnCancel.TabIndex = 14;
            btnCancel.Text = "Cancel";
            btnCancel.UseVisualStyleBackColor = true;
            btnCancel.Click += BtnCancel_Click;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(25, 44);
            label2.Name = "label2";
            label2.Size = new Size(42, 15);
            label2.TabIndex = 41;
            label2.Text = "Name:";
            // 
            // cboFillWith
            // 
            cboFillWith.DropDownStyle = ComboBoxStyle.DropDownList;
            cboFillWith.Font = new Font("Segoe UI", 8.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
            cboFillWith.FormattingEnabled = true;
            cboFillWith.Items.AddRange(new object[] { "Transparency", "Color" });
            cboFillWith.Location = new Point(86, 20);
            cboFillWith.Name = "cboFillWith";
            cboFillWith.Size = new Size(128, 21);
            cboFillWith.TabIndex = 2;
            cboFillWith.SelectedIndexChanged += CboFillWith_SelectedIndexChanged;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(6, 22);
            label4.Name = "label4";
            label4.Size = new Size(74, 15);
            label4.TabIndex = 41;
            label4.Text = "Background:";
            // 
            // btnBackgroundColor
            // 
            btnBackgroundColor.BackColor = Color.White;
            btnBackgroundColor.Location = new Point(220, 18);
            btnBackgroundColor.Name = "btnBackgroundColor";
            btnBackgroundColor.Size = new Size(24, 24);
            btnBackgroundColor.TabIndex = 3;
            btnBackgroundColor.UseVisualStyleBackColor = false;
            btnBackgroundColor.Click += BtnBackgroundColor_Click;
            // 
            // width
            // 
            width.Location = new Point(49, 47);
            width.Maximum = new decimal(new int[] { 10000, 0, 0, 0 });
            width.Minimum = new decimal(new int[] { 10000, 0, 0, int.MinValue });
            width.Name = "width";
            width.Size = new Size(54, 23);
            width.TabIndex = 5;
            width.Value = new decimal(new int[] { 2, 0, 0, 0 });
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Location = new Point(8, 47);
            label6.Name = "label6";
            label6.Size = new Size(30, 15);
            label6.TabIndex = 41;
            label6.Text = "Size:";
            // 
            // height
            // 
            height.Location = new Point(183, 47);
            height.Maximum = new decimal(new int[] { 10000, 0, 0, 0 });
            height.Minimum = new decimal(new int[] { 10000, 0, 0, int.MinValue });
            height.Name = "height";
            height.Size = new Size(54, 23);
            height.TabIndex = 7;
            height.Value = new decimal(new int[] { 2, 0, 0, 0 });
            // 
            // label8
            // 
            label8.AutoSize = true;
            label8.Location = new Point(7, 80);
            label8.Name = "label8";
            label8.Size = new Size(42, 15);
            label8.TabIndex = 41;
            label8.Text = "Offset:";
            // 
            // offsetX
            // 
            offsetX.Location = new Point(49, 76);
            offsetX.Maximum = new decimal(new int[] { 10000, 0, 0, 0 });
            offsetX.Minimum = new decimal(new int[] { 10000, 0, 0, int.MinValue });
            offsetX.Name = "offsetX";
            offsetX.Size = new Size(54, 23);
            offsetX.TabIndex = 9;
            // 
            // offsetY
            // 
            offsetY.Location = new Point(183, 76);
            offsetY.Maximum = new decimal(new int[] { 10000, 0, 0, 0 });
            offsetY.Minimum = new decimal(new int[] { 10000, 0, 0, int.MinValue });
            offsetY.Name = "offsetY";
            offsetY.Size = new Size(54, 23);
            offsetY.TabIndex = 11;
            // 
            // btnCenterX
            // 
            btnCenterX.Location = new Point(109, 76);
            btnCenterX.Name = "btnCenterX";
            btnCenterX.Size = new Size(50, 23);
            btnCenterX.TabIndex = 10;
            btnCenterX.Text = "Center";
            btnCenterX.UseVisualStyleBackColor = true;
            btnCenterX.Click += BtnCenterX_Click;
            // 
            // btnCenterY
            // 
            btnCenterY.Location = new Point(242, 76);
            btnCenterY.Name = "btnCenterY";
            btnCenterY.Size = new Size(50, 23);
            btnCenterY.TabIndex = 12;
            btnCenterY.Text = "Center";
            btnCenterY.UseVisualStyleBackColor = true;
            btnCenterY.Click += BtnCenterY_Click;
            // 
            // btnAutoWidth
            // 
            btnAutoWidth.Location = new Point(109, 47);
            btnAutoWidth.Name = "btnAutoWidth";
            btnAutoWidth.Size = new Size(50, 23);
            btnAutoWidth.TabIndex = 6;
            btnAutoWidth.Text = "Auto";
            btnAutoWidth.UseVisualStyleBackColor = true;
            btnAutoWidth.Click += BtnAutoWidth_Click;
            // 
            // btnAutoHeight
            // 
            btnAutoHeight.Location = new Point(243, 47);
            btnAutoHeight.Name = "btnAutoHeight";
            btnAutoHeight.Size = new Size(50, 23);
            btnAutoHeight.TabIndex = 8;
            btnAutoHeight.Text = "Auto";
            btnAutoHeight.UseVisualStyleBackColor = true;
            btnAutoHeight.Click += BtnAutoHeight_Click;
            // 
            // pictureMask
            // 
            pictureMask.BackColor = Color.White;
            pictureMask.BorderStyle = BorderStyle.Fixed3D;
            pictureMask.Location = new Point(304, 47);
            pictureMask.Name = "pictureMask";
            pictureMask.Size = new Size(150, 150);
            pictureMask.SizeMode = PictureBoxSizeMode.Zoom;
            pictureMask.TabIndex = 42;
            pictureMask.TabStop = false;
            // 
            // label10
            // 
            label10.AutoSize = true;
            label10.Location = new Point(304, 21);
            label10.Name = "label10";
            label10.Size = new Size(35, 15);
            label10.TabIndex = 41;
            label10.Text = "Mask";
            // 
            // cboLayers
            // 
            cboLayers.DropDownStyle = ComboBoxStyle.DropDownList;
            cboLayers.Font = new Font("Segoe UI", 8.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
            cboLayers.FormattingEnabled = true;
            cboLayers.Location = new Point(345, 19);
            cboLayers.Name = "cboLayers";
            cboLayers.Size = new Size(109, 21);
            cboLayers.TabIndex = 2;
            cboLayers.SelectedIndexChanged += ComboBox1_SelectedIndexChanged;
            // 
            // cboType
            // 
            cboType.DropDownStyle = ComboBoxStyle.DropDownList;
            cboType.FormattingEnabled = true;
            cboType.Items.AddRange(new object[] { "Image", "Vector" });
            cboType.Location = new Point(110, 12);
            cboType.Name = "cboType";
            cboType.Size = new Size(167, 23);
            cboType.TabIndex = 43;
            cboType.SelectedIndexChanged += CboType_SelectedIndexChanged;
            // 
            // label11
            // 
            label11.AutoSize = true;
            label11.Location = new Point(25, 15);
            label11.Name = "label11";
            label11.Size = new Size(35, 15);
            label11.TabIndex = 41;
            label11.Text = "Type:";
            // 
            // label9
            // 
            label9.AutoSize = true;
            label9.Location = new Point(165, 80);
            label9.Name = "label9";
            label9.Size = new Size(12, 15);
            label9.TabIndex = 41;
            label9.Text = "x";
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.Location = new Point(165, 51);
            label7.Name = "label7";
            label7.Size = new Size(12, 15);
            label7.TabIndex = 41;
            label7.Text = "x";
            // 
            // groupRasterProperties
            // 
            groupRasterProperties.Controls.Add(trackBarAdjustmentValue2);
            groupRasterProperties.Controls.Add(trackBarAdjustmentValue1);
            groupRasterProperties.Controls.Add(listBoxAdjustments);
            groupRasterProperties.Controls.Add(cboLayers);
            groupRasterProperties.Controls.Add(pictureBox2);
            groupRasterProperties.Controls.Add(pictureMask);
            groupRasterProperties.Controls.Add(btnAutoHeight);
            groupRasterProperties.Controls.Add(label4);
            groupRasterProperties.Controls.Add(btnAdjustmentVisibility);
            groupRasterProperties.Controls.Add(btnMoveAdjustmentDown);
            groupRasterProperties.Controls.Add(lblAdjustmentValue2);
            groupRasterProperties.Controls.Add(btnMoveAdjustmentUp);
            groupRasterProperties.Controls.Add(lblAdjustmentValue1);
            groupRasterProperties.Controls.Add(btnRemoveAdjustment);
            groupRasterProperties.Controls.Add(btnCenterY);
            groupRasterProperties.Controls.Add(btnAutoWidth);
            groupRasterProperties.Controls.Add(btnCenterX);
            groupRasterProperties.Controls.Add(label6);
            groupRasterProperties.Controls.Add(offsetY);
            groupRasterProperties.Controls.Add(label7);
            groupRasterProperties.Controls.Add(height);
            groupRasterProperties.Controls.Add(label1);
            groupRasterProperties.Controls.Add(label8);
            groupRasterProperties.Controls.Add(offsetX);
            groupRasterProperties.Controls.Add(width);
            groupRasterProperties.Controls.Add(label9);
            groupRasterProperties.Controls.Add(btnBackgroundColor);
            groupRasterProperties.Controls.Add(label10);
            groupRasterProperties.Controls.Add(cboAdjustments);
            groupRasterProperties.Controls.Add(cboFillWith);
            groupRasterProperties.Location = new Point(12, 218);
            groupRasterProperties.Name = "groupRasterProperties";
            groupRasterProperties.Size = new Size(471, 393);
            groupRasterProperties.TabIndex = 44;
            groupRasterProperties.TabStop = false;
            // 
            // trackBarAdjustmentValue2
            // 
            trackBarAdjustmentValue2.Location = new Point(22, 339);
            trackBarAdjustmentValue2.Maximum = 200;
            trackBarAdjustmentValue2.Name = "trackBarAdjustmentValue2";
            trackBarAdjustmentValue2.Size = new Size(201, 45);
            trackBarAdjustmentValue2.TabIndex = 44;
            trackBarAdjustmentValue2.TickStyle = TickStyle.None;
            trackBarAdjustmentValue2.Value = 100;
            trackBarAdjustmentValue2.Scroll += TrackBarAdjustmentValue_Scroll;
            // 
            // trackBarAdjustmentValue1
            // 
            trackBarAdjustmentValue1.Location = new Point(22, 288);
            trackBarAdjustmentValue1.Maximum = 200;
            trackBarAdjustmentValue1.Name = "trackBarAdjustmentValue1";
            trackBarAdjustmentValue1.Size = new Size(201, 45);
            trackBarAdjustmentValue1.TabIndex = 44;
            trackBarAdjustmentValue1.TickStyle = TickStyle.None;
            trackBarAdjustmentValue1.Value = 100;
            trackBarAdjustmentValue1.Scroll += TrackBarAdjustmentValue_Scroll;
            // 
            // listBoxAdjustments
            // 
            listBoxAdjustments.Font = new Font("Segoe UI", 11.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
            listBoxAdjustments.FormattingEnabled = true;
            listBoxAdjustments.Location = new Point(27, 158);
            listBoxAdjustments.Name = "listBoxAdjustments";
            listBoxAdjustments.Size = new Size(128, 124);
            listBoxAdjustments.TabIndex = 43;
            listBoxAdjustments.SelectedIndexChanged += ListBoxAdjustments_SelectedIndexChanged;
            // 
            // pictureBox2
            // 
            pictureBox2.BackColor = Color.DimGray;
            pictureBox2.BorderStyle = BorderStyle.Fixed3D;
            pictureBox2.Location = new Point(304, 216);
            pictureBox2.Name = "pictureBox2";
            pictureBox2.Size = new Size(150, 150);
            pictureBox2.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBox2.TabIndex = 42;
            pictureBox2.TabStop = false;
            // 
            // btnAdjustmentVisibility
            // 
            btnAdjustmentVisibility.Location = new Point(161, 245);
            btnAdjustmentVisibility.Name = "btnAdjustmentVisibility";
            btnAdjustmentVisibility.Size = new Size(29, 23);
            btnAdjustmentVisibility.TabIndex = 12;
            btnAdjustmentVisibility.Text = "👁";
            btnAdjustmentVisibility.UseVisualStyleBackColor = true;
            btnAdjustmentVisibility.Click += BtnAdjustmentVisibility_Click;
            // 
            // btnMoveAdjustmentDown
            // 
            btnMoveAdjustmentDown.Location = new Point(161, 216);
            btnMoveAdjustmentDown.Name = "btnMoveAdjustmentDown";
            btnMoveAdjustmentDown.Size = new Size(29, 23);
            btnMoveAdjustmentDown.TabIndex = 12;
            btnMoveAdjustmentDown.Text = "▼";
            btnMoveAdjustmentDown.UseVisualStyleBackColor = true;
            btnMoveAdjustmentDown.Click += BtnMoveAdjustmentDown_Click;
            // 
            // lblAdjustmentValue2
            // 
            lblAdjustmentValue2.BackColor = Color.White;
            lblAdjustmentValue2.BorderStyle = BorderStyle.Fixed3D;
            lblAdjustmentValue2.Location = new Point(229, 339);
            lblAdjustmentValue2.Name = "lblAdjustmentValue2";
            lblAdjustmentValue2.Size = new Size(38, 20);
            lblAdjustmentValue2.TabIndex = 41;
            // 
            // btnMoveAdjustmentUp
            // 
            btnMoveAdjustmentUp.Location = new Point(161, 187);
            btnMoveAdjustmentUp.Name = "btnMoveAdjustmentUp";
            btnMoveAdjustmentUp.Size = new Size(29, 23);
            btnMoveAdjustmentUp.TabIndex = 12;
            btnMoveAdjustmentUp.Text = "▲";
            btnMoveAdjustmentUp.UseVisualStyleBackColor = true;
            btnMoveAdjustmentUp.Click += BtnMoveAdjustmentUp_Click;
            // 
            // lblAdjustmentValue1
            // 
            lblAdjustmentValue1.BackColor = Color.White;
            lblAdjustmentValue1.BorderStyle = BorderStyle.Fixed3D;
            lblAdjustmentValue1.Location = new Point(229, 288);
            lblAdjustmentValue1.Name = "lblAdjustmentValue1";
            lblAdjustmentValue1.Size = new Size(38, 20);
            lblAdjustmentValue1.TabIndex = 41;
            // 
            // btnRemoveAdjustment
            // 
            btnRemoveAdjustment.Location = new Point(161, 158);
            btnRemoveAdjustment.Name = "btnRemoveAdjustment";
            btnRemoveAdjustment.Size = new Size(29, 23);
            btnRemoveAdjustment.TabIndex = 12;
            btnRemoveAdjustment.Text = "-";
            btnRemoveAdjustment.UseVisualStyleBackColor = true;
            btnRemoveAdjustment.Click += BtnRemoveAdjustment_Click;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(6, 113);
            label1.Name = "label1";
            label1.Size = new Size(72, 15);
            label1.TabIndex = 41;
            label1.Text = "Adjustment:";
            // 
            // cboAdjustments
            // 
            cboAdjustments.DropDownStyle = ComboBoxStyle.DropDownList;
            cboAdjustments.Font = new Font("Segoe UI", 8.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
            cboAdjustments.FormattingEnabled = true;
            cboAdjustments.Items.AddRange(new object[] { "", "Brightness", "Contrast", "Exposure", "Highlights", "Shadows", "Vignette", "Saturation", "Warmth", "Tint", "Sharpness", "Blur" });
            cboAdjustments.Location = new Point(27, 131);
            cboAdjustments.Name = "cboAdjustments";
            cboAdjustments.Size = new Size(128, 21);
            cboAdjustments.TabIndex = 2;
            cboAdjustments.SelectedIndexChanged += CboAdjustments_SelectedIndexChanged;
            // 
            // layerImage
            // 
            layerImage.BackColor = Color.White;
            layerImage.BorderStyle = BorderStyle.Fixed3D;
            layerImage.Location = new Point(283, 12);
            layerImage.Name = "layerImage";
            layerImage.Size = new Size(200, 200);
            layerImage.SizeMode = PictureBoxSizeMode.Zoom;
            layerImage.TabIndex = 42;
            layerImage.TabStop = false;
            // 
            // FormLayer
            // 
            AcceptButton = btnOK;
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = btnCancel;
            ClientSize = new Size(504, 661);
            Controls.Add(groupRasterProperties);
            Controls.Add(textBoxName);
            Controls.Add(cboType);
            Controls.Add(layerImage);
            Controls.Add(label11);
            Controls.Add(btnCancel);
            Controls.Add(btnOK);
            Controls.Add(label2);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "FormLayer";
            Text = "Layer";
            Load += FormName_Load;
            ((System.ComponentModel.ISupportInitialize)width).EndInit();
            ((System.ComponentModel.ISupportInitialize)height).EndInit();
            ((System.ComponentModel.ISupportInitialize)offsetX).EndInit();
            ((System.ComponentModel.ISupportInitialize)offsetY).EndInit();
            ((System.ComponentModel.ISupportInitialize)pictureMask).EndInit();
            groupRasterProperties.ResumeLayout(false);
            groupRasterProperties.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)trackBarAdjustmentValue2).EndInit();
            ((System.ComponentModel.ISupportInitialize)trackBarAdjustmentValue1).EndInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox2).EndInit();
            ((System.ComponentModel.ISupportInitialize)layerImage).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private TextBox textBoxName;
        private Button btnOK;
        private Button btnCancel;
        private Label label2;
        private ComboBox cboFillWith;
        private Label label4;
        private Button btnBackgroundColor;
        private Label label6;
        private Label label8;
        private NumericUpDown width;
        private NumericUpDown height;
        private NumericUpDown offsetX;
        private NumericUpDown offsetY;
        private Button btnCenterX;
        private Button btnCenterY;
        private Button btnAutoWidth;
        private Button btnAutoHeight;
        private PictureBox pictureMask;
        private Label label10;
        private ComboBox cboLayers;
        private ComboBox cboType;
        private Label label11;
        private Label label9;
        private Label label7;
        private GroupBox groupRasterProperties;
        private Label label1;
        private ComboBox cboAdjustments;
        private ListBox listBoxAdjustments;
        private PictureBox layerImage;
        private Button btnRemoveAdjustment;
        private TrackBar trackBarAdjustmentValue1;
        private Button btnMoveAdjustmentUp;
        private Button btnMoveAdjustmentDown;
        private Button btnAdjustmentVisibility;
        private Label lblAdjustmentValue1;
        private PictureBox pictureBox2;
        private TrackBar trackBarAdjustmentValue2;
        private Label lblAdjustmentValue2;
    }
}