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
            label1 = new Label();
            canvasHeight = new NumericUpDown();
            canvasWidth = new NumericUpDown();
            groupBox2 = new GroupBox();
            label2 = new Label();
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
            groupBox1.Controls.Add(label1);
            groupBox1.Controls.Add(canvasHeight);
            groupBox1.Controls.Add(canvasWidth);
            groupBox1.Location = new Point(12, 12);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new Size(187, 67);
            groupBox1.TabIndex = 1;
            groupBox1.TabStop = false;
            groupBox1.Text = "Canvas Size:";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(88, 26);
            label1.Name = "label1";
            label1.Size = new Size(12, 15);
            label1.TabIndex = 1;
            label1.Text = "x";
            // 
            // canvasHeight
            // 
            canvasHeight.Location = new Point(109, 22);
            canvasHeight.Maximum = new decimal(new int[] { 4096, 0, 0, 0 });
            canvasHeight.Minimum = new decimal(new int[] { 2, 0, 0, 0 });
            canvasHeight.Name = "canvasHeight";
            canvasHeight.Size = new Size(71, 23);
            canvasHeight.TabIndex = 0;
            canvasHeight.TextAlign = HorizontalAlignment.Right;
            canvasHeight.Value = new decimal(new int[] { 2, 0, 0, 0 });
            // 
            // canvasWidth
            // 
            canvasWidth.Location = new Point(6, 22);
            canvasWidth.Maximum = new decimal(new int[] { 4096, 0, 0, 0 });
            canvasWidth.Minimum = new decimal(new int[] { 2, 0, 0, 0 });
            canvasWidth.Name = "canvasWidth";
            canvasWidth.Size = new Size(71, 23);
            canvasWidth.TabIndex = 0;
            canvasWidth.TextAlign = HorizontalAlignment.Right;
            canvasWidth.Value = new decimal(new int[] { 2, 0, 0, 0 });
            // 
            // groupBox2
            // 
            groupBox2.Controls.Add(label2);
            groupBox2.Controls.Add(layerHeight);
            groupBox2.Controls.Add(layerWidth);
            groupBox2.Location = new Point(12, 85);
            groupBox2.Name = "groupBox2";
            groupBox2.Size = new Size(187, 67);
            groupBox2.TabIndex = 1;
            groupBox2.TabStop = false;
            groupBox2.Text = "Layer Size:";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(88, 24);
            label2.Name = "label2";
            label2.Size = new Size(12, 15);
            label2.TabIndex = 1;
            label2.Text = "x";
            // 
            // layerHeight
            // 
            layerHeight.Location = new Point(109, 22);
            layerHeight.Maximum = new decimal(new int[] { 4096, 0, 0, 0 });
            layerHeight.Minimum = new decimal(new int[] { 2, 0, 0, 0 });
            layerHeight.Name = "layerHeight";
            layerHeight.Size = new Size(71, 23);
            layerHeight.TabIndex = 0;
            layerHeight.TextAlign = HorizontalAlignment.Right;
            layerHeight.Value = new decimal(new int[] { 2, 0, 0, 0 });
            // 
            // layerWidth
            // 
            layerWidth.Location = new Point(6, 22);
            layerWidth.Maximum = new decimal(new int[] { 4096, 0, 0, 0 });
            layerWidth.Minimum = new decimal(new int[] { 2, 0, 0, 0 });
            layerWidth.Name = "layerWidth";
            layerWidth.Size = new Size(71, 23);
            layerWidth.TabIndex = 0;
            layerWidth.TextAlign = HorizontalAlignment.Right;
            layerWidth.Value = new decimal(new int[] { 2, 0, 0, 0 });
            // 
            // btnOK
            // 
            btnOK.Location = new Point(73, 426);
            btnOK.Name = "btnOK";
            btnOK.Size = new Size(102, 23);
            btnOK.TabIndex = 2;
            btnOK.Text = "OK";
            btnOK.UseVisualStyleBackColor = true;
            btnOK.Click += BtnOK_Click;
            // 
            // btnCancel
            // 
            btnCancel.Location = new Point(181, 426);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(102, 23);
            btnCancel.TabIndex = 2;
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
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "FormSettings";
            Text = "Settings";
            Load += FormSettings_Load;
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
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
        private Label label1;
        private Label label2;
        private Button btnOK;
        private Button btnCancel;
    }
}