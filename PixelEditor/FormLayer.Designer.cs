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
            label3 = new Label();
            lblOpacity = new Label();
            opacity = new TrackBar();
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
            ((System.ComponentModel.ISupportInitialize)width).BeginInit();
            ((System.ComponentModel.ISupportInitialize)height).BeginInit();
            ((System.ComponentModel.ISupportInitialize)offsetX).BeginInit();
            ((System.ComponentModel.ISupportInitialize)offsetY).BeginInit();
            ((System.ComponentModel.ISupportInitialize)opacity).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pictureMask).BeginInit();
            groupRasterProperties.SuspendLayout();
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
            btnOK.Location = new Point(155, 356);
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
            btnCancel.Location = new Point(236, 356);
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
            cboFillWith.Location = new Point(66, 22);
            cboFillWith.Name = "cboFillWith";
            cboFillWith.Size = new Size(128, 21);
            cboFillWith.TabIndex = 2;
            cboFillWith.SelectedIndexChanged += CboFillWith_SelectedIndexChanged;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(7, 24);
            label4.Name = "label4";
            label4.Size = new Size(53, 15);
            label4.TabIndex = 41;
            label4.Text = "Fill With:";
            // 
            // btnBackgroundColor
            // 
            btnBackgroundColor.BackColor = Color.White;
            btnBackgroundColor.Location = new Point(200, 20);
            btnBackgroundColor.Name = "btnBackgroundColor";
            btnBackgroundColor.Size = new Size(24, 24);
            btnBackgroundColor.TabIndex = 3;
            btnBackgroundColor.UseVisualStyleBackColor = false;
            btnBackgroundColor.Click += BtnBackgroundColor_Click;
            // 
            // width
            // 
            width.Location = new Point(56, 143);
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
            label6.Location = new Point(8, 145);
            label6.Name = "label6";
            label6.Size = new Size(42, 15);
            label6.TabIndex = 41;
            label6.Text = "Width:";
            // 
            // height
            // 
            height.Location = new Point(190, 143);
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
            label8.Location = new Point(8, 197);
            label8.Name = "label8";
            label8.Size = new Size(42, 15);
            label8.TabIndex = 41;
            label8.Text = "Offset:";
            // 
            // offsetX
            // 
            offsetX.Location = new Point(56, 195);
            offsetX.Maximum = new decimal(new int[] { 10000, 0, 0, 0 });
            offsetX.Minimum = new decimal(new int[] { 10000, 0, 0, int.MinValue });
            offsetX.Name = "offsetX";
            offsetX.Size = new Size(54, 23);
            offsetX.TabIndex = 9;
            // 
            // offsetY
            // 
            offsetY.Location = new Point(190, 197);
            offsetY.Maximum = new decimal(new int[] { 10000, 0, 0, 0 });
            offsetY.Minimum = new decimal(new int[] { 10000, 0, 0, int.MinValue });
            offsetY.Name = "offsetY";
            offsetY.Size = new Size(54, 23);
            offsetY.TabIndex = 11;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(9, 82);
            label3.Name = "label3";
            label3.Size = new Size(51, 15);
            label3.TabIndex = 41;
            label3.Text = "Opacity:";
            // 
            // lblOpacity
            // 
            lblOpacity.AutoSize = true;
            lblOpacity.Location = new Point(209, 82);
            lblOpacity.Name = "lblOpacity";
            lblOpacity.Size = new Size(15, 15);
            lblOpacity.TabIndex = 41;
            lblOpacity.Text = "[]";
            // 
            // opacity
            // 
            opacity.Location = new Point(66, 82);
            opacity.Maximum = 100;
            opacity.Name = "opacity";
            opacity.Size = new Size(128, 45);
            opacity.TabIndex = 4;
            opacity.TickStyle = TickStyle.None;
            opacity.Value = 100;
            // 
            // btnCenterX
            // 
            btnCenterX.Location = new Point(116, 197);
            btnCenterX.Name = "btnCenterX";
            btnCenterX.Size = new Size(50, 23);
            btnCenterX.TabIndex = 10;
            btnCenterX.Text = "Center";
            btnCenterX.UseVisualStyleBackColor = true;
            btnCenterX.Click += BtnCenterX_Click;
            // 
            // btnCenterY
            // 
            btnCenterY.Location = new Point(249, 197);
            btnCenterY.Name = "btnCenterY";
            btnCenterY.Size = new Size(50, 23);
            btnCenterY.TabIndex = 12;
            btnCenterY.Text = "Center";
            btnCenterY.UseVisualStyleBackColor = true;
            btnCenterY.Click += BtnCenterY_Click;
            // 
            // btnAutoWidth
            // 
            btnAutoWidth.Location = new Point(116, 143);
            btnAutoWidth.Name = "btnAutoWidth";
            btnAutoWidth.Size = new Size(50, 23);
            btnAutoWidth.TabIndex = 6;
            btnAutoWidth.Text = "Auto";
            btnAutoWidth.UseVisualStyleBackColor = true;
            btnAutoWidth.Click += BtnAutoWidth_Click;
            // 
            // btnAutoHeight
            // 
            btnAutoHeight.Location = new Point(250, 143);
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
            cboLayers.Size = new Size(139, 21);
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
            label9.Location = new Point(172, 201);
            label9.Name = "label9";
            label9.Size = new Size(12, 15);
            label9.TabIndex = 41;
            label9.Text = "x";
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.Location = new Point(172, 145);
            label7.Name = "label7";
            label7.Size = new Size(12, 15);
            label7.TabIndex = 41;
            label7.Text = "x";
            // 
            // groupRasterProperties
            // 
            groupRasterProperties.Controls.Add(cboLayers);
            groupRasterProperties.Controls.Add(pictureMask);
            groupRasterProperties.Controls.Add(btnAutoHeight);
            groupRasterProperties.Controls.Add(label4);
            groupRasterProperties.Controls.Add(btnCenterY);
            groupRasterProperties.Controls.Add(btnAutoWidth);
            groupRasterProperties.Controls.Add(label3);
            groupRasterProperties.Controls.Add(btnCenterX);
            groupRasterProperties.Controls.Add(label6);
            groupRasterProperties.Controls.Add(offsetY);
            groupRasterProperties.Controls.Add(label7);
            groupRasterProperties.Controls.Add(height);
            groupRasterProperties.Controls.Add(label8);
            groupRasterProperties.Controls.Add(offsetX);
            groupRasterProperties.Controls.Add(lblOpacity);
            groupRasterProperties.Controls.Add(width);
            groupRasterProperties.Controls.Add(label9);
            groupRasterProperties.Controls.Add(btnBackgroundColor);
            groupRasterProperties.Controls.Add(label10);
            groupRasterProperties.Controls.Add(opacity);
            groupRasterProperties.Controls.Add(cboFillWith);
            groupRasterProperties.Location = new Point(12, 70);
            groupRasterProperties.Name = "groupRasterProperties";
            groupRasterProperties.Size = new Size(510, 263);
            groupRasterProperties.TabIndex = 44;
            groupRasterProperties.TabStop = false;
            // 
            // FormLayer
            // 
            AcceptButton = btnOK;
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = btnCancel;
            ClientSize = new Size(534, 391);
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
            ((System.ComponentModel.ISupportInitialize)opacity).EndInit();
            ((System.ComponentModel.ISupportInitialize)pictureMask).EndInit();
            groupRasterProperties.ResumeLayout(false);
            groupRasterProperties.PerformLayout();
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
        private Label label3;
        private Label lblOpacity;
        private TrackBar opacity;
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
    }
}