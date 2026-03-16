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
            cboBlendMode = new ComboBox();
            label1 = new Label();
            label2 = new Label();
            cboFillWith = new ComboBox();
            label4 = new Label();
            btnBackgroundColor = new Button();
            label5 = new Label();
            width = new NumericUpDown();
            label6 = new Label();
            label7 = new Label();
            height = new NumericUpDown();
            label8 = new Label();
            label9 = new Label();
            offsetX = new NumericUpDown();
            offsetY = new NumericUpDown();
            label3 = new Label();
            lblOpacity = new Label();
            opacity = new TrackBar();
            btnCenterX = new Button();
            btnCenterY = new Button();
            btnAutoWidth = new Button();
            btnAutoHeight = new Button();
            ((System.ComponentModel.ISupportInitialize)width).BeginInit();
            ((System.ComponentModel.ISupportInitialize)height).BeginInit();
            ((System.ComponentModel.ISupportInitialize)offsetX).BeginInit();
            ((System.ComponentModel.ISupportInitialize)offsetY).BeginInit();
            ((System.ComponentModel.ISupportInitialize)opacity).BeginInit();
            SuspendLayout();
            // 
            // textBoxName
            // 
            textBoxName.Location = new Point(97, 12);
            textBoxName.Name = "textBoxName";
            textBoxName.Size = new Size(128, 23);
            textBoxName.TabIndex = 0;
            // 
            // btnOK
            // 
            btnOK.DialogResult = DialogResult.OK;
            btnOK.Location = new Point(66, 326);
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
            btnCancel.Location = new Point(147, 326);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(75, 23);
            btnCancel.TabIndex = 14;
            btnCancel.Text = "Cancel";
            btnCancel.UseVisualStyleBackColor = true;
            btnCancel.Click += BtnCancel_Click;
            // 
            // cboBlendMode
            // 
            cboBlendMode.DropDownStyle = ComboBoxStyle.DropDownList;
            cboBlendMode.Font = new Font("Segoe UI", 8.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
            cboBlendMode.FormattingEnabled = true;
            cboBlendMode.Location = new Point(97, 41);
            cboBlendMode.Name = "cboBlendMode";
            cboBlendMode.Size = new Size(128, 21);
            cboBlendMode.TabIndex = 1;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(12, 44);
            label1.Name = "label1";
            label1.Size = new Size(40, 15);
            label1.TabIndex = 41;
            label1.Text = "Blend:";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(12, 15);
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
            cboFillWith.Items.AddRange(new object[] { "Color", "Transparency" });
            cboFillWith.Location = new Point(97, 68);
            cboFillWith.Name = "cboFillWith";
            cboFillWith.Size = new Size(128, 21);
            cboFillWith.TabIndex = 2;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(12, 70);
            label4.Name = "label4";
            label4.Size = new Size(53, 15);
            label4.TabIndex = 41;
            label4.Text = "Fill With:";
            // 
            // btnBackgroundColor
            // 
            btnBackgroundColor.BackColor = Color.White;
            btnBackgroundColor.Location = new Point(201, 95);
            btnBackgroundColor.Name = "btnBackgroundColor";
            btnBackgroundColor.Size = new Size(24, 24);
            btnBackgroundColor.TabIndex = 3;
            btnBackgroundColor.UseVisualStyleBackColor = false;
            btnBackgroundColor.Click += BtnBackgroundColor_Click;
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new Point(10, 100);
            label5.Name = "label5";
            label5.Size = new Size(106, 15);
            label5.TabIndex = 41;
            label5.Text = "Background Color:";
            // 
            // width
            // 
            width.Location = new Point(97, 185);
            width.Maximum = new decimal(new int[] { 10000, 0, 0, 0 });
            width.Minimum = new decimal(new int[] { 10000, 0, 0, int.MinValue });
            width.Name = "width";
            width.Size = new Size(65, 23);
            width.TabIndex = 5;
            width.Value = new decimal(new int[] { 2, 0, 0, 0 });
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Location = new Point(14, 187);
            label6.Name = "label6";
            label6.Size = new Size(42, 15);
            label6.TabIndex = 41;
            label6.Text = "Width:";
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.Location = new Point(14, 218);
            label7.Name = "label7";
            label7.Size = new Size(46, 15);
            label7.TabIndex = 41;
            label7.Text = "Height:";
            // 
            // height
            // 
            height.Location = new Point(97, 216);
            height.Maximum = new decimal(new int[] { 10000, 0, 0, 0 });
            height.Minimum = new decimal(new int[] { 10000, 0, 0, int.MinValue });
            height.Name = "height";
            height.Size = new Size(65, 23);
            height.TabIndex = 7;
            height.Value = new decimal(new int[] { 2, 0, 0, 0 });
            // 
            // label8
            // 
            label8.AutoSize = true;
            label8.Location = new Point(14, 247);
            label8.Name = "label8";
            label8.Size = new Size(52, 15);
            label8.TabIndex = 41;
            label8.Text = "Offset X:";
            // 
            // label9
            // 
            label9.AutoSize = true;
            label9.Location = new Point(14, 278);
            label9.Name = "label9";
            label9.Size = new Size(52, 15);
            label9.TabIndex = 41;
            label9.Text = "Offset Y:";
            // 
            // offsetX
            // 
            offsetX.Location = new Point(97, 245);
            offsetX.Maximum = new decimal(new int[] { 10000, 0, 0, 0 });
            offsetX.Minimum = new decimal(new int[] { 10000, 0, 0, int.MinValue });
            offsetX.Name = "offsetX";
            offsetX.Size = new Size(65, 23);
            offsetX.TabIndex = 9;
            // 
            // offsetY
            // 
            offsetY.Location = new Point(97, 276);
            offsetY.Maximum = new decimal(new int[] { 10000, 0, 0, 0 });
            offsetY.Minimum = new decimal(new int[] { 10000, 0, 0, int.MinValue });
            offsetY.Name = "offsetY";
            offsetY.Size = new Size(65, 23);
            offsetY.TabIndex = 11;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(12, 134);
            label3.Name = "label3";
            label3.Size = new Size(51, 15);
            label3.TabIndex = 41;
            label3.Text = "Opacity:";
            // 
            // lblOpacity
            // 
            lblOpacity.AutoSize = true;
            lblOpacity.Location = new Point(231, 134);
            lblOpacity.Name = "lblOpacity";
            lblOpacity.Size = new Size(15, 15);
            lblOpacity.TabIndex = 41;
            lblOpacity.Text = "[]";
            // 
            // opacity
            // 
            opacity.Location = new Point(97, 134);
            opacity.Maximum = 100;
            opacity.Name = "opacity";
            opacity.Size = new Size(128, 45);
            opacity.TabIndex = 4;
            opacity.TickStyle = TickStyle.None;
            opacity.Value = 100;
            // 
            // btnCenterX
            // 
            btnCenterX.Location = new Point(171, 245);
            btnCenterX.Name = "btnCenterX";
            btnCenterX.Size = new Size(75, 23);
            btnCenterX.TabIndex = 10;
            btnCenterX.Text = "Center";
            btnCenterX.UseVisualStyleBackColor = true;
            btnCenterX.Click += BtnCenterX_Click;
            // 
            // btnCenterY
            // 
            btnCenterY.Location = new Point(171, 276);
            btnCenterY.Name = "btnCenterY";
            btnCenterY.Size = new Size(75, 23);
            btnCenterY.TabIndex = 12;
            btnCenterY.Text = "Center";
            btnCenterY.UseVisualStyleBackColor = true;
            btnCenterY.Click += BtnCenterY_Click;
            // 
            // btnAutoWidth
            // 
            btnAutoWidth.Location = new Point(171, 183);
            btnAutoWidth.Name = "btnAutoWidth";
            btnAutoWidth.Size = new Size(75, 23);
            btnAutoWidth.TabIndex = 6;
            btnAutoWidth.Text = "Auto Size";
            btnAutoWidth.UseVisualStyleBackColor = true;
            btnAutoWidth.Click += BtnAutoWidth_Click;
            // 
            // btnAutoHeight
            // 
            btnAutoHeight.Location = new Point(171, 214);
            btnAutoHeight.Name = "btnAutoHeight";
            btnAutoHeight.Size = new Size(75, 23);
            btnAutoHeight.TabIndex = 8;
            btnAutoHeight.Text = "Auto Size";
            btnAutoHeight.UseVisualStyleBackColor = true;
            btnAutoHeight.Click += BtnAutoHeight_Click;
            // 
            // FormLayer
            // 
            AcceptButton = btnOK;
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = btnCancel;
            ClientSize = new Size(284, 361);
            Controls.Add(btnAutoHeight);
            Controls.Add(btnCenterY);
            Controls.Add(btnAutoWidth);
            Controls.Add(btnCenterX);
            Controls.Add(offsetY);
            Controls.Add(height);
            Controls.Add(offsetX);
            Controls.Add(width);
            Controls.Add(btnBackgroundColor);
            Controls.Add(opacity);
            Controls.Add(cboFillWith);
            Controls.Add(cboBlendMode);
            Controls.Add(label2);
            Controls.Add(label9);
            Controls.Add(lblOpacity);
            Controls.Add(label8);
            Controls.Add(label7);
            Controls.Add(label6);
            Controls.Add(label3);
            Controls.Add(label5);
            Controls.Add(label4);
            Controls.Add(label1);
            Controls.Add(btnCancel);
            Controls.Add(btnOK);
            Controls.Add(textBoxName);
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
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private TextBox textBoxName;
        private Button btnOK;
        private Button btnCancel;
        private ComboBox cboBlendMode;
        private Label label1;
        private Label label2;
        private ComboBox cboFillWith;
        private Label label4;
        private Button btnBackgroundColor;
        private Label label5;
        private Label label6;
        private Label label7;
        private Label label8;
        private Label label9;
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
    }
}