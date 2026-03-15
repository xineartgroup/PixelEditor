namespace PixelEditor
{
    partial class FormBlur
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
            tabControl1 = new TabControl();
            tabPage1 = new TabPage();
            label3 = new Label();
            lblRadius = new Label();
            label2 = new Label();
            label1 = new Label();
            trackBar2 = new TrackBar();
            trackBar1 = new TrackBar();
            tabPage2 = new TabPage();
            btnOK = new Button();
            btnCancel = new Button();
            pictureSample = new PictureBox();
            tabControl1.SuspendLayout();
            tabPage1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)trackBar2).BeginInit();
            ((System.ComponentModel.ISupportInitialize)trackBar1).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pictureSample).BeginInit();
            SuspendLayout();
            // 
            // tabControl1
            // 
            tabControl1.Controls.Add(tabPage1);
            tabControl1.Controls.Add(tabPage2);
            tabControl1.Location = new Point(12, 318);
            tabControl1.Name = "tabControl1";
            tabControl1.SelectedIndex = 0;
            tabControl1.Size = new Size(460, 322);
            tabControl1.TabIndex = 0;
            // 
            // tabPage1
            // 
            tabPage1.BackColor = Color.White;
            tabPage1.Controls.Add(label3);
            tabPage1.Controls.Add(lblRadius);
            tabPage1.Controls.Add(label2);
            tabPage1.Controls.Add(label1);
            tabPage1.Controls.Add(trackBar2);
            tabPage1.Controls.Add(trackBar1);
            tabPage1.Location = new Point(4, 24);
            tabPage1.Name = "tabPage1";
            tabPage1.Padding = new Padding(3);
            tabPage1.Size = new Size(452, 294);
            tabPage1.TabIndex = 0;
            tabPage1.Text = "Gaussian";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(415, 83);
            label3.Name = "label3";
            label3.Size = new Size(15, 15);
            label3.TabIndex = 1;
            label3.Text = "[]";
            // 
            // lblRadius
            // 
            lblRadius.AutoSize = true;
            lblRadius.Location = new Point(415, 32);
            lblRadius.Name = "lblRadius";
            lblRadius.Size = new Size(15, 15);
            lblRadius.TabIndex = 1;
            lblRadius.Text = "[]";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(16, 83);
            label2.Name = "label2";
            label2.Size = new Size(40, 15);
            label2.TabIndex = 1;
            label2.Text = "Size Y:";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(16, 32);
            label1.Name = "label1";
            label1.Size = new Size(40, 15);
            label1.TabIndex = 1;
            label1.Text = "Size X:";
            // 
            // trackBar2
            // 
            trackBar2.BackColor = Color.White;
            trackBar2.Location = new Point(77, 80);
            trackBar2.Maximum = 100;
            trackBar2.Minimum = 1;
            trackBar2.Name = "trackBar2";
            trackBar2.Size = new Size(332, 45);
            trackBar2.TabIndex = 0;
            trackBar2.TickStyle = TickStyle.None;
            trackBar2.Value = 1;
            trackBar2.Scroll += TrackBar1_Scroll;
            // 
            // trackBar1
            // 
            trackBar1.BackColor = Color.White;
            trackBar1.Location = new Point(77, 29);
            trackBar1.Maximum = 100;
            trackBar1.Minimum = 1;
            trackBar1.Name = "trackBar1";
            trackBar1.Size = new Size(332, 45);
            trackBar1.TabIndex = 0;
            trackBar1.TickStyle = TickStyle.None;
            trackBar1.Value = 1;
            trackBar1.Scroll += TrackBar1_Scroll;
            // 
            // tabPage2
            // 
            tabPage2.Location = new Point(4, 24);
            tabPage2.Name = "tabPage2";
            tabPage2.Padding = new Padding(3);
            tabPage2.Size = new Size(452, 159);
            tabPage2.TabIndex = 1;
            tabPage2.Text = "Other";
            tabPage2.UseVisualStyleBackColor = true;
            // 
            // btnOK
            // 
            btnOK.Location = new Point(146, 646);
            btnOK.Name = "btnOK";
            btnOK.Size = new Size(75, 23);
            btnOK.TabIndex = 1;
            btnOK.Text = "OK";
            btnOK.UseVisualStyleBackColor = true;
            btnOK.Click += BtnOK_Click;
            // 
            // btnCancel
            // 
            btnCancel.Location = new Point(227, 646);
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
            pictureSample.Location = new Point(93, 12);
            pictureSample.Name = "pictureSample";
            pictureSample.Size = new Size(300, 300);
            pictureSample.SizeMode = PictureBoxSizeMode.Zoom;
            pictureSample.TabIndex = 3;
            pictureSample.TabStop = false;
            // 
            // FormBlur
            // 
            AcceptButton = btnOK;
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = btnCancel;
            ClientSize = new Size(484, 681);
            Controls.Add(pictureSample);
            Controls.Add(btnCancel);
            Controls.Add(btnOK);
            Controls.Add(tabControl1);
            FormBorderStyle = FormBorderStyle.FixedToolWindow;
            Name = "FormBlur";
            Text = "Blur";
            Load += FormBlur_Load;
            tabControl1.ResumeLayout(false);
            tabPage1.ResumeLayout(false);
            tabPage1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)trackBar2).EndInit();
            ((System.ComponentModel.ISupportInitialize)trackBar1).EndInit();
            ((System.ComponentModel.ISupportInitialize)pictureSample).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private TabControl tabControl1;
        private TabPage tabPage1;
        private TabPage tabPage2;
        private Label lblRadius;
        private Label label1;
        private TrackBar trackBar1;
        private Button btnOK;
        private Button btnCancel;
        private PictureBox pictureSample;
        private Label label3;
        private Label label2;
        private TrackBar trackBar2;
    }
}