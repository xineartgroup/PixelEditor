namespace PixelEditor
{
    partial class FormCurves
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
            cboRGB = new ComboBox();
            pictureCurves = new PictureBox();
            trackBarScaleDark = new TrackBar();
            trackBarScaleLight = new TrackBar();
            btnOK = new Button();
            btnCancel = new Button();
            btnReset = new Button();
            btnDelete = new Button();
            pictureSample = new PictureBox();
            ((System.ComponentModel.ISupportInitialize)pictureCurves).BeginInit();
            ((System.ComponentModel.ISupportInitialize)trackBarScaleDark).BeginInit();
            ((System.ComponentModel.ISupportInitialize)trackBarScaleLight).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pictureSample).BeginInit();
            SuspendLayout();
            // 
            // cboRGB
            // 
            cboRGB.DropDownStyle = ComboBoxStyle.DropDownList;
            cboRGB.FormattingEnabled = true;
            cboRGB.Location = new Point(12, 318);
            cboRGB.Name = "cboRGB";
            cboRGB.Size = new Size(121, 23);
            cboRGB.TabIndex = 0;
            cboRGB.SelectedIndexChanged += CboRGB_SelectedIndexChanged;
            // 
            // pictureCurves
            // 
            pictureCurves.BackColor = Color.Gray;
            pictureCurves.BorderStyle = BorderStyle.FixedSingle;
            pictureCurves.Location = new Point(12, 347);
            pictureCurves.Name = "pictureCurves";
            pictureCurves.Size = new Size(300, 300);
            pictureCurves.TabIndex = 1;
            pictureCurves.TabStop = false;
            pictureCurves.MouseDown += PictureCurves_MouseDown;
            pictureCurves.MouseMove += PictureCurves_MouseMove;
            pictureCurves.MouseUp += PictureCurves_MouseUp;
            // 
            // trackBarScaleDark
            // 
            trackBarScaleDark.Location = new Point(12, 653);
            trackBarScaleDark.Maximum = 255;
            trackBarScaleDark.Name = "trackBarScaleDark";
            trackBarScaleDark.Size = new Size(300, 45);
            trackBarScaleDark.TabIndex = 2;
            trackBarScaleDark.TickStyle = TickStyle.None;
            trackBarScaleDark.Scroll += TrackBarScaleDark_Scroll;
            // 
            // trackBarScaleLight
            // 
            trackBarScaleLight.Location = new Point(12, 704);
            trackBarScaleLight.Maximum = 255;
            trackBarScaleLight.Name = "trackBarScaleLight";
            trackBarScaleLight.Size = new Size(300, 45);
            trackBarScaleLight.TabIndex = 2;
            trackBarScaleLight.TickStyle = TickStyle.None;
            trackBarScaleLight.Value = 255;
            trackBarScaleLight.Scroll += TrackBarScaleLight_Scroll;
            // 
            // btnOK
            // 
            btnOK.Location = new Point(70, 752);
            btnOK.Name = "btnOK";
            btnOK.Size = new Size(75, 23);
            btnOK.TabIndex = 3;
            btnOK.Text = "OK";
            btnOK.UseVisualStyleBackColor = true;
            btnOK.Click += BtnOK_Click;
            // 
            // btnCancel
            // 
            btnCancel.Location = new Point(151, 752);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(75, 23);
            btnCancel.TabIndex = 3;
            btnCancel.Text = "Cancel";
            btnCancel.UseVisualStyleBackColor = true;
            btnCancel.Click += BtnCancel_Click;
            // 
            // btnReset
            // 
            btnReset.Location = new Point(260, 317);
            btnReset.Name = "btnReset";
            btnReset.Size = new Size(23, 23);
            btnReset.TabIndex = 3;
            btnReset.UseVisualStyleBackColor = true;
            btnReset.Click += BtnReset_Click;
            // 
            // btnDelete
            // 
            btnDelete.Location = new Point(289, 317);
            btnDelete.Name = "btnDelete";
            btnDelete.Size = new Size(23, 23);
            btnDelete.TabIndex = 3;
            btnDelete.UseVisualStyleBackColor = true;
            btnDelete.Click += BtnDelete_Click;
            // 
            // pictureSample
            // 
            pictureSample.BackColor = Color.White;
            pictureSample.BorderStyle = BorderStyle.Fixed3D;
            pictureSample.Location = new Point(12, 12);
            pictureSample.Name = "pictureSample";
            pictureSample.Size = new Size(300, 300);
            pictureSample.SizeMode = PictureBoxSizeMode.Zoom;
            pictureSample.TabIndex = 1;
            pictureSample.TabStop = false;
            pictureSample.MouseDown += PictureCurves_MouseDown;
            pictureSample.MouseMove += PictureCurves_MouseMove;
            pictureSample.MouseUp += PictureCurves_MouseUp;
            // 
            // FormCurves
            // 
            AcceptButton = btnOK;
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = btnCancel;
            ClientSize = new Size(324, 793);
            Controls.Add(btnCancel);
            Controls.Add(btnDelete);
            Controls.Add(btnReset);
            Controls.Add(btnOK);
            Controls.Add(trackBarScaleLight);
            Controls.Add(trackBarScaleDark);
            Controls.Add(pictureSample);
            Controls.Add(pictureCurves);
            Controls.Add(cboRGB);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "FormCurves";
            Text = "Curves";
            Load += FormCurves_Load;
            ((System.ComponentModel.ISupportInitialize)pictureCurves).EndInit();
            ((System.ComponentModel.ISupportInitialize)trackBarScaleDark).EndInit();
            ((System.ComponentModel.ISupportInitialize)trackBarScaleLight).EndInit();
            ((System.ComponentModel.ISupportInitialize)pictureSample).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private ComboBox cboRGB;
        private PictureBox pictureCurves;
        private TrackBar trackBarScaleDark;
        private TrackBar trackBarScaleLight;
        private Button btnOK;
        private Button btnCancel;
        private Button btnReset;
        private Button btnDelete;
        private PictureBox pictureSample;
    }
}