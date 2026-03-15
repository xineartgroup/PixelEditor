namespace PixelEditor
{
    partial class FormSharpness
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
            btnOK = new Button();
            btnCancel = new Button();
            pictureSample = new PictureBox();
            trackBar6 = new TrackBar();
            label5 = new Label();
            lblSharpness = new Label();
            ((System.ComponentModel.ISupportInitialize)pictureSample).BeginInit();
            ((System.ComponentModel.ISupportInitialize)trackBar6).BeginInit();
            SuspendLayout();
            // 
            // btnOK
            // 
            btnOK.Location = new Point(146, 416);
            btnOK.Name = "btnOK";
            btnOK.Size = new Size(75, 23);
            btnOK.TabIndex = 1;
            btnOK.Text = "OK";
            btnOK.UseVisualStyleBackColor = true;
            btnOK.Click += BtnOK_Click;
            // 
            // btnCancel
            // 
            btnCancel.Location = new Point(227, 416);
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
            // trackBar6
            // 
            trackBar6.BackColor = SystemColors.Control;
            trackBar6.Location = new Point(91, 342);
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
            label5.Location = new Point(20, 345);
            label5.Name = "label5";
            label5.Size = new Size(63, 15);
            label5.TabIndex = 1;
            label5.Text = "Sharpness:";
            // 
            // lblSharpness
            // 
            lblSharpness.AutoSize = true;
            lblSharpness.Location = new Point(442, 345);
            lblSharpness.Name = "lblSharpness";
            lblSharpness.Size = new Size(15, 15);
            lblSharpness.TabIndex = 1;
            lblSharpness.Text = "[]";
            // 
            // FormSharpness
            // 
            AcceptButton = btnOK;
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = btnCancel;
            ClientSize = new Size(484, 453);
            Controls.Add(lblSharpness);
            Controls.Add(pictureSample);
            Controls.Add(label5);
            Controls.Add(trackBar6);
            Controls.Add(btnCancel);
            Controls.Add(btnOK);
            FormBorderStyle = FormBorderStyle.FixedToolWindow;
            Name = "FormSharpness";
            Text = "Sharpness";
            Load += FormLighting_Load;
            ((System.ComponentModel.ISupportInitialize)pictureSample).EndInit();
            ((System.ComponentModel.ISupportInitialize)trackBar6).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private Button btnOK;
        private Button btnCancel;
        private PictureBox pictureSample;
        private TrackBar trackBar6;
        private Label label5;
        private Label lblSharpness;
    }
}