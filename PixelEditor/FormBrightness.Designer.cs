namespace PixelEditor
{
    partial class FormBrightness
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
            label1 = new Label();
            trackBar1 = new TrackBar();
            btnOK = new Button();
            btnCancel = new Button();
            pictureSample = new PictureBox();
            lblBrightness = new Label();
            trackBar2 = new TrackBar();
            label2 = new Label();
            lblContrast = new Label();
            ((System.ComponentModel.ISupportInitialize)trackBar1).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pictureSample).BeginInit();
            ((System.ComponentModel.ISupportInitialize)trackBar2).BeginInit();
            SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(72, 221);
            label1.Name = "label1";
            label1.Size = new Size(65, 15);
            label1.TabIndex = 1;
            label1.Text = "Brightness:";
            // 
            // trackBar1
            // 
            trackBar1.BackColor = SystemColors.Control;
            trackBar1.Location = new Point(146, 218);
            trackBar1.Maximum = 100;
            trackBar1.Name = "trackBar1";
            trackBar1.Size = new Size(247, 45);
            trackBar1.TabIndex = 0;
            trackBar1.TickStyle = TickStyle.None;
            trackBar1.Value = 100;
            trackBar1.Scroll += TrackBar1_Scroll;
            // 
            // btnOK
            // 
            btnOK.Location = new Point(154, 326);
            btnOK.Name = "btnOK";
            btnOK.Size = new Size(75, 23);
            btnOK.TabIndex = 1;
            btnOK.Text = "OK";
            btnOK.UseVisualStyleBackColor = true;
            btnOK.Click += BtnOK_Click;
            // 
            // btnCancel
            // 
            btnCancel.Location = new Point(235, 326);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(75, 23);
            btnCancel.TabIndex = 1;
            btnCancel.Text = "Cancel";
            btnCancel.UseVisualStyleBackColor = true;
            btnCancel.Click += BtnCancel_Click;
            // 
            // pictureSample
            // 
            pictureSample.BackColor = Color.White;
            pictureSample.BorderStyle = BorderStyle.Fixed3D;
            pictureSample.Location = new Point(146, 12);
            pictureSample.Name = "pictureSample";
            pictureSample.Size = new Size(200, 200);
            pictureSample.SizeMode = PictureBoxSizeMode.Zoom;
            pictureSample.TabIndex = 3;
            pictureSample.TabStop = false;
            // 
            // lblBrightness
            // 
            lblBrightness.AutoSize = true;
            lblBrightness.Location = new Point(399, 221);
            lblBrightness.Name = "lblBrightness";
            lblBrightness.Size = new Size(15, 15);
            lblBrightness.TabIndex = 1;
            lblBrightness.Text = "[]";
            // 
            // trackBar2
            // 
            trackBar2.BackColor = SystemColors.Control;
            trackBar2.Location = new Point(146, 269);
            trackBar2.Maximum = 100;
            trackBar2.Name = "trackBar2";
            trackBar2.Size = new Size(247, 45);
            trackBar2.TabIndex = 0;
            trackBar2.TickStyle = TickStyle.None;
            trackBar2.Value = 100;
            trackBar2.Scroll += TrackBar1_Scroll;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(72, 272);
            label2.Name = "label2";
            label2.Size = new Size(55, 15);
            label2.TabIndex = 1;
            label2.Text = "Contrast:";
            // 
            // lblContrast
            // 
            lblContrast.AutoSize = true;
            lblContrast.Location = new Point(399, 272);
            lblContrast.Name = "lblContrast";
            lblContrast.Size = new Size(15, 15);
            lblContrast.TabIndex = 1;
            lblContrast.Text = "[]";
            // 
            // FormBrightness
            // 
            AcceptButton = btnOK;
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = btnCancel;
            ClientSize = new Size(484, 361);
            Controls.Add(lblContrast);
            Controls.Add(lblBrightness);
            Controls.Add(pictureSample);
            Controls.Add(label2);
            Controls.Add(label1);
            Controls.Add(btnCancel);
            Controls.Add(trackBar2);
            Controls.Add(trackBar1);
            Controls.Add(btnOK);
            FormBorderStyle = FormBorderStyle.FixedToolWindow;
            Name = "FormBrightness";
            Text = "Brightness and Contrast";
            Load += FormBlur_Load;
            ((System.ComponentModel.ISupportInitialize)trackBar1).EndInit();
            ((System.ComponentModel.ISupportInitialize)pictureSample).EndInit();
            ((System.ComponentModel.ISupportInitialize)trackBar2).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private Label label1;
        private TrackBar trackBar1;
        private Button btnOK;
        private Button btnCancel;
        private PictureBox pictureSample;
        private Label lblBrightness;
        private TrackBar trackBar2;
        private Label label2;
        private Label lblContrast;
    }
}