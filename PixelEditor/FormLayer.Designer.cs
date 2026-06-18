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
            trackBar1 = new TrackBar();
            listBoxAdjustments = new ListBox();
            pictureBox1 = new PictureBox();
            btnRemoveAdjustment = new Button();
            btnAddAdjustment = new Button();
            label1 = new Label();
            cboAdjustments = new ComboBox();
            button1 = new Button();
            button2 = new Button();
            button3 = new Button();
            ((System.ComponentModel.ISupportInitialize)width).BeginInit();
            ((System.ComponentModel.ISupportInitialize)height).BeginInit();
            ((System.ComponentModel.ISupportInitialize)offsetX).BeginInit();
            ((System.ComponentModel.ISupportInitialize)offsetY).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pictureMask).BeginInit();
            groupRasterProperties.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)trackBar1).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
            SuspendLayout();
            // 
            // textBoxName
            // 
            textBoxName.Location = new Point(110, 41);
            textBoxName.Name = "textBoxName";
            textBoxName.Size = new Size(128, 23);
            textBoxName.TabIndex = 0;
            // 
            // btnOK
            // 
            btnOK.DialogResult = DialogResult.OK;
            btnOK.Location = new Point(165, 626);
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
            btnCancel.Location = new Point(246, 626);
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
            cboFillWith.Location = new Point(40, 47);
            cboFillWith.Name = "cboFillWith";
            cboFillWith.Size = new Size(128, 21);
            cboFillWith.TabIndex = 2;
            cboFillWith.SelectedIndexChanged += CboFillWith_SelectedIndexChanged;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(13, 25);
            label4.Name = "label4";
            label4.Size = new Size(53, 15);
            label4.TabIndex = 41;
            label4.Text = "Fill With:";
            // 
            // btnBackgroundColor
            // 
            btnBackgroundColor.BackColor = Color.White;
            btnBackgroundColor.Location = new Point(174, 45);
            btnBackgroundColor.Name = "btnBackgroundColor";
            btnBackgroundColor.Size = new Size(24, 24);
            btnBackgroundColor.TabIndex = 3;
            btnBackgroundColor.UseVisualStyleBackColor = false;
            btnBackgroundColor.Click += BtnBackgroundColor_Click;
            // 
            // width
            // 
            width.Location = new Point(40, 103);
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
            label6.Location = new Point(13, 85);
            label6.Name = "label6";
            label6.Size = new Size(42, 15);
            label6.TabIndex = 41;
            label6.Text = "Width:";
            // 
            // height
            // 
            height.Location = new Point(174, 103);
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
            label8.Location = new Point(13, 142);
            label8.Name = "label8";
            label8.Size = new Size(42, 15);
            label8.TabIndex = 41;
            label8.Text = "Offset:";
            // 
            // offsetX
            // 
            offsetX.Location = new Point(40, 162);
            offsetX.Maximum = new decimal(new int[] { 10000, 0, 0, 0 });
            offsetX.Minimum = new decimal(new int[] { 10000, 0, 0, int.MinValue });
            offsetX.Name = "offsetX";
            offsetX.Size = new Size(54, 23);
            offsetX.TabIndex = 9;
            // 
            // offsetY
            // 
            offsetY.Location = new Point(174, 162);
            offsetY.Maximum = new decimal(new int[] { 10000, 0, 0, 0 });
            offsetY.Minimum = new decimal(new int[] { 10000, 0, 0, int.MinValue });
            offsetY.Name = "offsetY";
            offsetY.Size = new Size(54, 23);
            offsetY.TabIndex = 11;
            // 
            // btnCenterX
            // 
            btnCenterX.Location = new Point(100, 162);
            btnCenterX.Name = "btnCenterX";
            btnCenterX.Size = new Size(50, 23);
            btnCenterX.TabIndex = 10;
            btnCenterX.Text = "Center";
            btnCenterX.UseVisualStyleBackColor = true;
            btnCenterX.Click += BtnCenterX_Click;
            // 
            // btnCenterY
            // 
            btnCenterY.Location = new Point(233, 162);
            btnCenterY.Name = "btnCenterY";
            btnCenterY.Size = new Size(50, 23);
            btnCenterY.TabIndex = 12;
            btnCenterY.Text = "Center";
            btnCenterY.UseVisualStyleBackColor = true;
            btnCenterY.Click += BtnCenterY_Click;
            // 
            // btnAutoWidth
            // 
            btnAutoWidth.Location = new Point(100, 103);
            btnAutoWidth.Name = "btnAutoWidth";
            btnAutoWidth.Size = new Size(50, 23);
            btnAutoWidth.TabIndex = 6;
            btnAutoWidth.Text = "Auto";
            btnAutoWidth.UseVisualStyleBackColor = true;
            btnAutoWidth.Click += BtnAutoWidth_Click;
            // 
            // btnAutoHeight
            // 
            btnAutoHeight.Location = new Point(234, 103);
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
            pictureMask.Size = new Size(200, 200);
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
            cboLayers.Size = new Size(159, 21);
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
            cboType.Size = new Size(128, 23);
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
            label9.Location = new Point(156, 166);
            label9.Name = "label9";
            label9.Size = new Size(12, 15);
            label9.TabIndex = 41;
            label9.Text = "x";
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.Location = new Point(156, 107);
            label7.Name = "label7";
            label7.Size = new Size(12, 15);
            label7.TabIndex = 41;
            label7.Text = "x";
            // 
            // groupRasterProperties
            // 
            groupRasterProperties.Controls.Add(trackBar1);
            groupRasterProperties.Controls.Add(listBoxAdjustments);
            groupRasterProperties.Controls.Add(cboLayers);
            groupRasterProperties.Controls.Add(pictureBox1);
            groupRasterProperties.Controls.Add(pictureMask);
            groupRasterProperties.Controls.Add(btnAutoHeight);
            groupRasterProperties.Controls.Add(label4);
            groupRasterProperties.Controls.Add(button3);
            groupRasterProperties.Controls.Add(button2);
            groupRasterProperties.Controls.Add(button1);
            groupRasterProperties.Controls.Add(btnRemoveAdjustment);
            groupRasterProperties.Controls.Add(btnAddAdjustment);
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
            groupRasterProperties.Location = new Point(12, 70);
            groupRasterProperties.Name = "groupRasterProperties";
            groupRasterProperties.Size = new Size(510, 550);
            groupRasterProperties.TabIndex = 44;
            groupRasterProperties.TabStop = false;
            // 
            // trackBar1
            // 
            trackBar1.Location = new Point(304, 293);
            trackBar1.Name = "trackBar1";
            trackBar1.Size = new Size(200, 45);
            trackBar1.TabIndex = 44;
            trackBar1.TickStyle = TickStyle.None;
            // 
            // listBoxAdjustments
            // 
            listBoxAdjustments.Font = new Font("Segoe UI", 11.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
            listBoxAdjustments.FormattingEnabled = true;
            listBoxAdjustments.Location = new Point(40, 330);
            listBoxAdjustments.Name = "listBoxAdjustments";
            listBoxAdjustments.Size = new Size(209, 164);
            listBoxAdjustments.TabIndex = 43;
            // 
            // pictureBox1
            // 
            pictureBox1.BackColor = Color.White;
            pictureBox1.BorderStyle = BorderStyle.Fixed3D;
            pictureBox1.Location = new Point(304, 345);
            pictureBox1.Name = "pictureBox1";
            pictureBox1.Size = new Size(200, 200);
            pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBox1.TabIndex = 42;
            pictureBox1.TabStop = false;
            // 
            // btnRemoveAdjustment
            // 
            btnRemoveAdjustment.Location = new Point(40, 505);
            btnRemoveAdjustment.Name = "btnRemoveAdjustment";
            btnRemoveAdjustment.Size = new Size(29, 23);
            btnRemoveAdjustment.TabIndex = 12;
            btnRemoveAdjustment.Text = "-";
            btnRemoveAdjustment.UseVisualStyleBackColor = true;
            btnRemoveAdjustment.Click += BtnCenterY_Click;
            // 
            // btnAddAdjustment
            // 
            btnAddAdjustment.Location = new Point(220, 293);
            btnAddAdjustment.Name = "btnAddAdjustment";
            btnAddAdjustment.Size = new Size(29, 23);
            btnAddAdjustment.TabIndex = 12;
            btnAddAdjustment.Text = "+";
            btnAddAdjustment.UseVisualStyleBackColor = true;
            btnAddAdjustment.Click += BtnCenterY_Click;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(13, 268);
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
            cboAdjustments.Items.AddRange(new object[] { "Brightness", "Contrast", "Exposure", "Highlights", "Shadows", "Vignette", "Saturation", "Warmth", "Tint" });
            cboAdjustments.Location = new Point(40, 293);
            cboAdjustments.Name = "cboAdjustments";
            cboAdjustments.Size = new Size(172, 21);
            cboAdjustments.TabIndex = 2;
            cboAdjustments.SelectedIndexChanged += CboFillWith_SelectedIndexChanged;
            // 
            // button1
            // 
            button1.Location = new Point(75, 505);
            button1.Name = "button1";
            button1.Size = new Size(29, 23);
            button1.TabIndex = 12;
            button1.Text = "▲";
            button1.UseVisualStyleBackColor = true;
            button1.Click += BtnCenterY_Click;
            // 
            // button2
            // 
            button2.Location = new Point(110, 505);
            button2.Name = "button2";
            button2.Size = new Size(29, 23);
            button2.TabIndex = 12;
            button2.Text = "▼";
            button2.UseVisualStyleBackColor = true;
            button2.Click += BtnCenterY_Click;
            // 
            // button3
            // 
            button3.Location = new Point(145, 505);
            button3.Name = "button3";
            button3.Size = new Size(29, 23);
            button3.TabIndex = 12;
            button3.Text = "👁";
            button3.UseVisualStyleBackColor = true;
            button3.Click += BtnCenterY_Click;
            // 
            // FormLayer
            // 
            AcceptButton = btnOK;
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = btnCancel;
            ClientSize = new Size(534, 661);
            Controls.Add(groupRasterProperties);
            Controls.Add(textBoxName);
            Controls.Add(cboType);
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
            ((System.ComponentModel.ISupportInitialize)trackBar1).EndInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
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
        private Button btnAddAdjustment;
        private ListBox listBoxAdjustments;
        private PictureBox pictureBox1;
        private Button btnRemoveAdjustment;
        private TrackBar trackBar1;
        private Button button1;
        private Button button2;
        private Button button3;
    }
}