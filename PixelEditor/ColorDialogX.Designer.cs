namespace PixelEditor
{
    partial class ColorDialogX
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ColorDialogX));
            picColorBox = new PictureBox();
            tkbHue = new TrackBar();
            btnOK = new Button();
            btnCancel = new Button();
            picMouseDownPreview = new PictureBox();
            picMouseMovePreview = new PictureBox();
            btnEyeDropper = new Button();
            picColorRange = new PictureBox();
            txtColor = new TextBox();
            ((System.ComponentModel.ISupportInitialize)picColorBox).BeginInit();
            ((System.ComponentModel.ISupportInitialize)tkbHue).BeginInit();
            ((System.ComponentModel.ISupportInitialize)picMouseDownPreview).BeginInit();
            ((System.ComponentModel.ISupportInitialize)picMouseMovePreview).BeginInit();
            ((System.ComponentModel.ISupportInitialize)picColorRange).BeginInit();
            SuspendLayout();
            // 
            // picColorBox
            // 
            picColorBox.Location = new Point(13, 59);
            picColorBox.Margin = new Padding(4, 3, 4, 3);
            picColorBox.Name = "picColorBox";
            picColorBox.Size = new Size(240, 240);
            picColorBox.TabIndex = 0;
            picColorBox.TabStop = false;
            picColorBox.Paint += PicColorBox_Paint;
            picColorBox.MouseDown += PicColorBox_MouseDown;
            picColorBox.MouseMove += PicColorBox_MouseMove;
            // 
            // tkbHue
            // 
            tkbHue.LargeChange = 30;
            tkbHue.Location = new Point(279, 51);
            tkbHue.Margin = new Padding(4, 3, 4, 3);
            tkbHue.Maximum = 360;
            tkbHue.Name = "tkbHue";
            tkbHue.Orientation = Orientation.Vertical;
            tkbHue.Size = new Size(45, 258);
            tkbHue.TabIndex = 1;
            tkbHue.TickStyle = TickStyle.None;
            tkbHue.Scroll += TkbHue_Scroll;
            // 
            // btnOK
            // 
            btnOK.Location = new Point(70, 322);
            btnOK.Margin = new Padding(4, 3, 4, 3);
            btnOK.Name = "btnOK";
            btnOK.Size = new Size(88, 27);
            btnOK.TabIndex = 2;
            btnOK.Text = "OK";
            btnOK.UseVisualStyleBackColor = true;
            btnOK.Click += BtnOK_Click;
            // 
            // btnCancel
            // 
            btnCancel.Location = new Point(165, 322);
            btnCancel.Margin = new Padding(4, 3, 4, 3);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(88, 27);
            btnCancel.TabIndex = 3;
            btnCancel.Text = "Cancel";
            btnCancel.UseVisualStyleBackColor = true;
            btnCancel.Click += BtnCancel_Click;
            // 
            // picMouseDownPreview
            // 
            picMouseDownPreview.BorderStyle = BorderStyle.FixedSingle;
            picMouseDownPreview.Location = new Point(13, 17);
            picMouseDownPreview.Margin = new Padding(4, 3, 4, 3);
            picMouseDownPreview.Name = "picMouseDownPreview";
            picMouseDownPreview.Size = new Size(28, 28);
            picMouseDownPreview.TabIndex = 4;
            picMouseDownPreview.TabStop = false;
            // 
            // picMouseMovePreview
            // 
            picMouseMovePreview.BorderStyle = BorderStyle.FixedSingle;
            picMouseMovePreview.Location = new Point(49, 17);
            picMouseMovePreview.Margin = new Padding(4, 3, 4, 3);
            picMouseMovePreview.Name = "picMouseMovePreview";
            picMouseMovePreview.Size = new Size(28, 28);
            picMouseMovePreview.TabIndex = 5;
            picMouseMovePreview.TabStop = false;
            // 
            // btnEyeDropper
            // 
            btnEyeDropper.FlatStyle = FlatStyle.Popup;
            btnEyeDropper.Image = (Image)resources.GetObject("btnEyeDropper.Image");
            btnEyeDropper.Location = new Point(261, 17);
            btnEyeDropper.Name = "btnEyeDropper";
            btnEyeDropper.Size = new Size(28, 28);
            btnEyeDropper.TabIndex = 6;
            btnEyeDropper.UseVisualStyleBackColor = true;
            btnEyeDropper.Click += BtnEyeDropper_Click;
            // 
            // picColorRange
            // 
            picColorRange.Location = new Point(261, 59);
            picColorRange.Margin = new Padding(4, 3, 4, 3);
            picColorRange.Name = "picColorRange";
            picColorRange.Size = new Size(10, 240);
            picColorRange.TabIndex = 7;
            picColorRange.TabStop = false;
            picColorRange.Paint += PicColorRange_Paint;
            picColorRange.MouseDown += PicColorRange_MouseDown;
            // 
            // txtColor
            // 
            txtColor.Font = new Font("Segoe UI", 11.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
            txtColor.Location = new Point(98, 18);
            txtColor.Name = "txtColor";
            txtColor.Size = new Size(72, 27);
            txtColor.TabIndex = 8;
            txtColor.KeyDown += TxtColor_KeyDown;
            txtColor.Leave += TxtColor_TextChanged;
            // 
            // ColorDialogX
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(334, 361);
            Controls.Add(txtColor);
            Controls.Add(picColorRange);
            Controls.Add(btnEyeDropper);
            Controls.Add(picMouseMovePreview);
            Controls.Add(picMouseDownPreview);
            Controls.Add(btnCancel);
            Controls.Add(btnOK);
            Controls.Add(tkbHue);
            Controls.Add(picColorBox);
            FormBorderStyle = FormBorderStyle.FixedToolWindow;
            Margin = new Padding(4, 3, 4, 3);
            Name = "ColorDialogX";
            Text = "Color Picker";
            ((System.ComponentModel.ISupportInitialize)picColorBox).EndInit();
            ((System.ComponentModel.ISupportInitialize)tkbHue).EndInit();
            ((System.ComponentModel.ISupportInitialize)picMouseDownPreview).EndInit();
            ((System.ComponentModel.ISupportInitialize)picMouseMovePreview).EndInit();
            ((System.ComponentModel.ISupportInitialize)picColorRange).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        private System.Windows.Forms.PictureBox picColorBox;
        private System.Windows.Forms.TrackBar tkbHue;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.PictureBox picMouseDownPreview;
        private System.Windows.Forms.PictureBox picMouseMovePreview;
        private Button btnEyeDropper;
        private PictureBox picColorRange;
        private TextBox txtColor;
    }
}