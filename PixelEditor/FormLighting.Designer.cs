namespace PixelEditor
{
    partial class FormLighting
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
            trackBar3 = new TrackBar();
            trackBar4 = new TrackBar();
            label3 = new Label();
            label4 = new Label();
            lblExposure = new Label();
            lblHighlights = new Label();
            trackBar5 = new TrackBar();
            label7 = new Label();
            lblShadows = new Label();
            trackBar6 = new TrackBar();
            label5 = new Label();
            lblVignette = new Label();
            ((System.ComponentModel.ISupportInitialize)trackBar1).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pictureSample).BeginInit();
            ((System.ComponentModel.ISupportInitialize)trackBar2).BeginInit();
            ((System.ComponentModel.ISupportInitialize)trackBar3).BeginInit();
            ((System.ComponentModel.ISupportInitialize)trackBar4).BeginInit();
            ((System.ComponentModel.ISupportInitialize)trackBar5).BeginInit();
            ((System.ComponentModel.ISupportInitialize)trackBar6).BeginInit();
            SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(20, 332);
            label1.Name = "label1";
            label1.Size = new Size(65, 15);
            label1.TabIndex = 1;
            label1.Text = "Brightness:";
            // 
            // trackBar1
            // 
            trackBar1.BackColor = SystemColors.Control;
            trackBar1.Location = new Point(91, 329);
            trackBar1.Maximum = 200;
            trackBar1.Name = "trackBar1";
            trackBar1.Size = new Size(345, 45);
            trackBar1.TabIndex = 0;
            trackBar1.TickStyle = TickStyle.None;
            trackBar1.Value = 100;
            trackBar1.Scroll += TrackBar1_Scroll;
            // 
            // btnOK
            // 
            btnOK.Location = new Point(146, 642);
            btnOK.Name = "btnOK";
            btnOK.Size = new Size(75, 23);
            btnOK.TabIndex = 1;
            btnOK.Text = "OK";
            btnOK.UseVisualStyleBackColor = true;
            btnOK.Click += BtnOK_Click;
            // 
            // btnCancel
            // 
            btnCancel.Location = new Point(227, 642);
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
            pictureSample.Location = new Point(91, 12);
            pictureSample.Name = "pictureSample";
            pictureSample.Size = new Size(300, 300);
            pictureSample.SizeMode = PictureBoxSizeMode.Zoom;
            pictureSample.TabIndex = 3;
            pictureSample.TabStop = false;
            // 
            // lblBrightness
            // 
            lblBrightness.AutoSize = true;
            lblBrightness.Location = new Point(442, 332);
            lblBrightness.Name = "lblBrightness";
            lblBrightness.Size = new Size(15, 15);
            lblBrightness.TabIndex = 1;
            lblBrightness.Text = "[]";
            // 
            // trackBar2
            // 
            trackBar2.BackColor = SystemColors.Control;
            trackBar2.Location = new Point(91, 380);
            trackBar2.Maximum = 200;
            trackBar2.Name = "trackBar2";
            trackBar2.Size = new Size(345, 45);
            trackBar2.TabIndex = 0;
            trackBar2.TickStyle = TickStyle.None;
            trackBar2.Value = 100;
            trackBar2.Scroll += TrackBar1_Scroll;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(20, 383);
            label2.Name = "label2";
            label2.Size = new Size(55, 15);
            label2.TabIndex = 1;
            label2.Text = "Contrast:";
            // 
            // lblContrast
            // 
            lblContrast.AutoSize = true;
            lblContrast.Location = new Point(442, 383);
            lblContrast.Name = "lblContrast";
            lblContrast.Size = new Size(15, 15);
            lblContrast.TabIndex = 1;
            lblContrast.Text = "[]";
            // 
            // trackBar3
            // 
            trackBar3.BackColor = SystemColors.Control;
            trackBar3.Location = new Point(91, 431);
            trackBar3.Maximum = 200;
            trackBar3.Name = "trackBar3";
            trackBar3.Size = new Size(345, 45);
            trackBar3.TabIndex = 0;
            trackBar3.TickStyle = TickStyle.None;
            trackBar3.Value = 100;
            trackBar3.Scroll += TrackBar1_Scroll;
            // 
            // trackBar4
            // 
            trackBar4.BackColor = SystemColors.Control;
            trackBar4.Location = new Point(91, 482);
            trackBar4.Maximum = 200;
            trackBar4.Name = "trackBar4";
            trackBar4.Size = new Size(345, 45);
            trackBar4.TabIndex = 0;
            trackBar4.TickStyle = TickStyle.None;
            trackBar4.Value = 100;
            trackBar4.Scroll += TrackBar1_Scroll;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(20, 434);
            label3.Name = "label3";
            label3.Size = new Size(57, 15);
            label3.TabIndex = 1;
            label3.Text = "Exposure:";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(20, 485);
            label4.Name = "label4";
            label4.Size = new Size(65, 15);
            label4.TabIndex = 1;
            label4.Text = "Highlights:";
            // 
            // lblExposure
            // 
            lblExposure.AutoSize = true;
            lblExposure.Location = new Point(442, 434);
            lblExposure.Name = "lblExposure";
            lblExposure.Size = new Size(15, 15);
            lblExposure.TabIndex = 1;
            lblExposure.Text = "[]";
            // 
            // lblHighlights
            // 
            lblHighlights.AutoSize = true;
            lblHighlights.Location = new Point(442, 485);
            lblHighlights.Name = "lblHighlights";
            lblHighlights.Size = new Size(15, 15);
            lblHighlights.TabIndex = 1;
            lblHighlights.Text = "[]";
            // 
            // trackBar5
            // 
            trackBar5.BackColor = SystemColors.Control;
            trackBar5.Location = new Point(91, 533);
            trackBar5.Maximum = 200;
            trackBar5.Name = "trackBar5";
            trackBar5.Size = new Size(345, 45);
            trackBar5.TabIndex = 0;
            trackBar5.TickStyle = TickStyle.None;
            trackBar5.Value = 100;
            trackBar5.Scroll += TrackBar1_Scroll;
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.Location = new Point(20, 536);
            label7.Name = "label7";
            label7.Size = new Size(57, 15);
            label7.TabIndex = 1;
            label7.Text = "Shadows:";
            // 
            // lblShadows
            // 
            lblShadows.AutoSize = true;
            lblShadows.Location = new Point(442, 536);
            lblShadows.Name = "lblShadows";
            lblShadows.Size = new Size(15, 15);
            lblShadows.TabIndex = 1;
            lblShadows.Text = "[]";
            // 
            // trackBar6
            // 
            trackBar6.BackColor = SystemColors.Control;
            trackBar6.Location = new Point(91, 584);
            trackBar6.Maximum = 100;
            trackBar6.Name = "trackBar6";
            trackBar6.Size = new Size(345, 45);
            trackBar6.TabIndex = 0;
            trackBar6.TickStyle = TickStyle.None;
            trackBar6.Scroll += TrackBar1_Scroll;
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new Point(20, 587);
            label5.Name = "label5";
            label5.Size = new Size(54, 15);
            label5.TabIndex = 1;
            label5.Text = "Vignette:";
            // 
            // lblVignette
            // 
            lblVignette.AutoSize = true;
            lblVignette.Location = new Point(442, 587);
            lblVignette.Name = "lblVignette";
            lblVignette.Size = new Size(15, 15);
            lblVignette.TabIndex = 1;
            lblVignette.Text = "[]";
            // 
            // FormLighting
            // 
            AcceptButton = btnOK;
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = btnCancel;
            ClientSize = new Size(484, 681);
            Controls.Add(lblVignette);
            Controls.Add(lblShadows);
            Controls.Add(lblHighlights);
            Controls.Add(lblContrast);
            Controls.Add(lblExposure);
            Controls.Add(lblBrightness);
            Controls.Add(pictureSample);
            Controls.Add(label5);
            Controls.Add(label7);
            Controls.Add(label4);
            Controls.Add(label2);
            Controls.Add(label3);
            Controls.Add(label1);
            Controls.Add(trackBar6);
            Controls.Add(trackBar5);
            Controls.Add(trackBar4);
            Controls.Add(btnCancel);
            Controls.Add(trackBar3);
            Controls.Add(trackBar2);
            Controls.Add(trackBar1);
            Controls.Add(btnOK);
            FormBorderStyle = FormBorderStyle.FixedToolWindow;
            Name = "FormLighting";
            Text = "Lighting";
            Load += FormLighting_Load;
            ((System.ComponentModel.ISupportInitialize)trackBar1).EndInit();
            ((System.ComponentModel.ISupportInitialize)pictureSample).EndInit();
            ((System.ComponentModel.ISupportInitialize)trackBar2).EndInit();
            ((System.ComponentModel.ISupportInitialize)trackBar3).EndInit();
            ((System.ComponentModel.ISupportInitialize)trackBar4).EndInit();
            ((System.ComponentModel.ISupportInitialize)trackBar5).EndInit();
            ((System.ComponentModel.ISupportInitialize)trackBar6).EndInit();
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
        private TrackBar trackBar3;
        private TrackBar trackBar4;
        private Label label3;
        private Label label4;
        private Label lblExposure;
        private Label lblHighlights;
        private TrackBar trackBar5;
        private Label label7;
        private Label lblShadows;
        private TrackBar trackBar6;
        private Label label5;
        private Label lblVignette;
    }
}