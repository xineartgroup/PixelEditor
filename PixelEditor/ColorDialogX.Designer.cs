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
            tkbSaturation = new TrackBar();
            picSaturation = new PictureBox();
            tkbValue = new TrackBar();
            picValue = new PictureBox();
            groupBox1 = new GroupBox();
            btnAdd = new Button();
            picSave22 = new PictureBox();
            picSave21 = new PictureBox();
            picSave20 = new PictureBox();
            picSave18 = new PictureBox();
            picSave16 = new PictureBox();
            picSave14 = new PictureBox();
            picSave12 = new PictureBox();
            picSave10 = new PictureBox();
            picSave19 = new PictureBox();
            picSave17 = new PictureBox();
            picSave15 = new PictureBox();
            picSave13 = new PictureBox();
            picSave11 = new PictureBox();
            picSave08 = new PictureBox();
            picSave09 = new PictureBox();
            picSave06 = new PictureBox();
            picSave07 = new PictureBox();
            picSave04 = new PictureBox();
            picSave05 = new PictureBox();
            picSave02 = new PictureBox();
            picSave03 = new PictureBox();
            picSave01 = new PictureBox();
            ((System.ComponentModel.ISupportInitialize)picColorBox).BeginInit();
            ((System.ComponentModel.ISupportInitialize)tkbHue).BeginInit();
            ((System.ComponentModel.ISupportInitialize)picMouseDownPreview).BeginInit();
            ((System.ComponentModel.ISupportInitialize)picMouseMovePreview).BeginInit();
            ((System.ComponentModel.ISupportInitialize)picColorRange).BeginInit();
            ((System.ComponentModel.ISupportInitialize)tkbSaturation).BeginInit();
            ((System.ComponentModel.ISupportInitialize)picSaturation).BeginInit();
            ((System.ComponentModel.ISupportInitialize)tkbValue).BeginInit();
            ((System.ComponentModel.ISupportInitialize)picValue).BeginInit();
            groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)picSave22).BeginInit();
            ((System.ComponentModel.ISupportInitialize)picSave21).BeginInit();
            ((System.ComponentModel.ISupportInitialize)picSave20).BeginInit();
            ((System.ComponentModel.ISupportInitialize)picSave18).BeginInit();
            ((System.ComponentModel.ISupportInitialize)picSave16).BeginInit();
            ((System.ComponentModel.ISupportInitialize)picSave14).BeginInit();
            ((System.ComponentModel.ISupportInitialize)picSave12).BeginInit();
            ((System.ComponentModel.ISupportInitialize)picSave10).BeginInit();
            ((System.ComponentModel.ISupportInitialize)picSave19).BeginInit();
            ((System.ComponentModel.ISupportInitialize)picSave17).BeginInit();
            ((System.ComponentModel.ISupportInitialize)picSave15).BeginInit();
            ((System.ComponentModel.ISupportInitialize)picSave13).BeginInit();
            ((System.ComponentModel.ISupportInitialize)picSave11).BeginInit();
            ((System.ComponentModel.ISupportInitialize)picSave08).BeginInit();
            ((System.ComponentModel.ISupportInitialize)picSave09).BeginInit();
            ((System.ComponentModel.ISupportInitialize)picSave06).BeginInit();
            ((System.ComponentModel.ISupportInitialize)picSave07).BeginInit();
            ((System.ComponentModel.ISupportInitialize)picSave04).BeginInit();
            ((System.ComponentModel.ISupportInitialize)picSave05).BeginInit();
            ((System.ComponentModel.ISupportInitialize)picSave02).BeginInit();
            ((System.ComponentModel.ISupportInitialize)picSave03).BeginInit();
            ((System.ComponentModel.ISupportInitialize)picSave01).BeginInit();
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
            tkbHue.Location = new Point(279, 52);
            tkbHue.Margin = new Padding(4, 3, 4, 3);
            tkbHue.Maximum = 360;
            tkbHue.Name = "tkbHue";
            tkbHue.Orientation = Orientation.Vertical;
            tkbHue.Size = new Size(45, 255);
            tkbHue.TabIndex = 1;
            tkbHue.TickStyle = TickStyle.None;
            tkbHue.Scroll += TkbHue_Scroll;
            // 
            // btnOK
            // 
            btnOK.Location = new Point(149, 422);
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
            btnCancel.Location = new Point(244, 422);
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
            btnEyeDropper.Location = new Point(434, 18);
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
            // tkbSaturation
            // 
            tkbSaturation.LargeChange = 30;
            tkbSaturation.Location = new Point(349, 52);
            tkbSaturation.Margin = new Padding(4, 3, 4, 3);
            tkbSaturation.Maximum = 360;
            tkbSaturation.Name = "tkbSaturation";
            tkbSaturation.Orientation = Orientation.Vertical;
            tkbSaturation.Size = new Size(45, 255);
            tkbSaturation.TabIndex = 1;
            tkbSaturation.TickStyle = TickStyle.None;
            tkbSaturation.Scroll += TkbSaturation_Scroll;
            // 
            // picSaturation
            // 
            picSaturation.Location = new Point(331, 59);
            picSaturation.Margin = new Padding(4, 3, 4, 3);
            picSaturation.Name = "picSaturation";
            picSaturation.Size = new Size(10, 240);
            picSaturation.TabIndex = 7;
            picSaturation.TabStop = false;
            picSaturation.Paint += PicSaturation_Paint;
            picSaturation.MouseDown += PicSaturation_MouseDown;
            // 
            // tkbValue
            // 
            tkbValue.LargeChange = 30;
            tkbValue.Location = new Point(417, 52);
            tkbValue.Margin = new Padding(4, 3, 4, 3);
            tkbValue.Maximum = 360;
            tkbValue.Name = "tkbValue";
            tkbValue.Orientation = Orientation.Vertical;
            tkbValue.Size = new Size(45, 255);
            tkbValue.TabIndex = 1;
            tkbValue.TickStyle = TickStyle.None;
            tkbValue.Scroll += TkbValue_Scroll;
            // 
            // picValue
            // 
            picValue.Location = new Point(399, 59);
            picValue.Margin = new Padding(4, 3, 4, 3);
            picValue.Name = "picValue";
            picValue.Size = new Size(10, 240);
            picValue.TabIndex = 7;
            picValue.TabStop = false;
            picValue.Paint += PicValue_Paint;
            picValue.MouseDown += PicValue_MouseDown;
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(btnAdd);
            groupBox1.Controls.Add(picSave22);
            groupBox1.Controls.Add(picSave21);
            groupBox1.Controls.Add(picSave20);
            groupBox1.Controls.Add(picSave18);
            groupBox1.Controls.Add(picSave16);
            groupBox1.Controls.Add(picSave14);
            groupBox1.Controls.Add(picSave12);
            groupBox1.Controls.Add(picSave10);
            groupBox1.Controls.Add(picSave19);
            groupBox1.Controls.Add(picSave17);
            groupBox1.Controls.Add(picSave15);
            groupBox1.Controls.Add(picSave13);
            groupBox1.Controls.Add(picSave11);
            groupBox1.Controls.Add(picSave08);
            groupBox1.Controls.Add(picSave09);
            groupBox1.Controls.Add(picSave06);
            groupBox1.Controls.Add(picSave07);
            groupBox1.Controls.Add(picSave04);
            groupBox1.Controls.Add(picSave05);
            groupBox1.Controls.Add(picSave02);
            groupBox1.Controls.Add(picSave03);
            groupBox1.Controls.Add(picSave01);
            groupBox1.Location = new Point(13, 316);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new Size(449, 100);
            groupBox1.TabIndex = 9;
            groupBox1.TabStop = false;
            groupBox1.Text = "Saved Colors:";
            // 
            // btnAdd
            // 
            btnAdd.Location = new Point(414, 22);
            btnAdd.Margin = new Padding(4, 3, 4, 3);
            btnAdd.Name = "btnAdd";
            btnAdd.Size = new Size(28, 28);
            btnAdd.TabIndex = 6;
            btnAdd.Text = "+";
            btnAdd.UseVisualStyleBackColor = true;
            btnAdd.Click += BtnAdd_Click;
            // 
            // picSave22
            // 
            picSave22.BorderStyle = BorderStyle.FixedSingle;
            picSave22.Location = new Point(368, 56);
            picSave22.Margin = new Padding(4, 3, 4, 3);
            picSave22.Name = "picSave22";
            picSave22.Size = new Size(28, 28);
            picSave22.TabIndex = 5;
            picSave22.TabStop = false;
            picSave22.Click += PicSave_Click;
            // 
            // picSave21
            // 
            picSave21.BorderStyle = BorderStyle.FixedSingle;
            picSave21.Location = new Point(368, 22);
            picSave21.Margin = new Padding(4, 3, 4, 3);
            picSave21.Name = "picSave21";
            picSave21.Size = new Size(28, 28);
            picSave21.TabIndex = 5;
            picSave21.TabStop = false;
            picSave21.Click += PicSave_Click;
            // 
            // picSave20
            // 
            picSave20.BorderStyle = BorderStyle.FixedSingle;
            picSave20.Location = new Point(332, 56);
            picSave20.Margin = new Padding(4, 3, 4, 3);
            picSave20.Name = "picSave20";
            picSave20.Size = new Size(28, 28);
            picSave20.TabIndex = 5;
            picSave20.TabStop = false;
            picSave20.Click += PicSave_Click;
            // 
            // picSave18
            // 
            picSave18.BorderStyle = BorderStyle.FixedSingle;
            picSave18.Location = new Point(259, 56);
            picSave18.Margin = new Padding(4, 3, 4, 3);
            picSave18.Name = "picSave18";
            picSave18.Size = new Size(28, 28);
            picSave18.TabIndex = 5;
            picSave18.TabStop = false;
            picSave18.Click += PicSave_Click;
            // 
            // picSave16
            // 
            picSave16.BorderStyle = BorderStyle.FixedSingle;
            picSave16.Location = new Point(187, 56);
            picSave16.Margin = new Padding(4, 3, 4, 3);
            picSave16.Name = "picSave16";
            picSave16.Size = new Size(28, 28);
            picSave16.TabIndex = 5;
            picSave16.TabStop = false;
            picSave16.Click += PicSave_Click;
            // 
            // picSave14
            // 
            picSave14.BorderStyle = BorderStyle.FixedSingle;
            picSave14.Location = new Point(115, 56);
            picSave14.Margin = new Padding(4, 3, 4, 3);
            picSave14.Name = "picSave14";
            picSave14.Size = new Size(28, 28);
            picSave14.TabIndex = 5;
            picSave14.TabStop = false;
            picSave14.Click += PicSave_Click;
            // 
            // picSave12
            // 
            picSave12.BorderStyle = BorderStyle.FixedSingle;
            picSave12.Location = new Point(43, 56);
            picSave12.Margin = new Padding(4, 3, 4, 3);
            picSave12.Name = "picSave12";
            picSave12.Size = new Size(28, 28);
            picSave12.TabIndex = 5;
            picSave12.TabStop = false;
            picSave12.Click += PicSave_Click;
            // 
            // picSave10
            // 
            picSave10.BorderStyle = BorderStyle.FixedSingle;
            picSave10.Location = new Point(332, 22);
            picSave10.Margin = new Padding(4, 3, 4, 3);
            picSave10.Name = "picSave10";
            picSave10.Size = new Size(28, 28);
            picSave10.TabIndex = 5;
            picSave10.TabStop = false;
            picSave10.Click += PicSave_Click;
            // 
            // picSave19
            // 
            picSave19.BorderStyle = BorderStyle.FixedSingle;
            picSave19.Location = new Point(295, 56);
            picSave19.Margin = new Padding(4, 3, 4, 3);
            picSave19.Name = "picSave19";
            picSave19.Size = new Size(28, 28);
            picSave19.TabIndex = 5;
            picSave19.TabStop = false;
            picSave19.Click += PicSave_Click;
            // 
            // picSave17
            // 
            picSave17.BorderStyle = BorderStyle.FixedSingle;
            picSave17.Location = new Point(223, 56);
            picSave17.Margin = new Padding(4, 3, 4, 3);
            picSave17.Name = "picSave17";
            picSave17.Size = new Size(28, 28);
            picSave17.TabIndex = 5;
            picSave17.TabStop = false;
            picSave17.Click += PicSave_Click;
            // 
            // picSave15
            // 
            picSave15.BorderStyle = BorderStyle.FixedSingle;
            picSave15.Location = new Point(151, 56);
            picSave15.Margin = new Padding(4, 3, 4, 3);
            picSave15.Name = "picSave15";
            picSave15.Size = new Size(28, 28);
            picSave15.TabIndex = 5;
            picSave15.TabStop = false;
            picSave15.Click += PicSave_Click;
            // 
            // picSave13
            // 
            picSave13.BorderStyle = BorderStyle.FixedSingle;
            picSave13.Location = new Point(79, 56);
            picSave13.Margin = new Padding(4, 3, 4, 3);
            picSave13.Name = "picSave13";
            picSave13.Size = new Size(28, 28);
            picSave13.TabIndex = 5;
            picSave13.TabStop = false;
            picSave13.Click += PicSave_Click;
            // 
            // picSave11
            // 
            picSave11.BorderStyle = BorderStyle.FixedSingle;
            picSave11.Location = new Point(7, 56);
            picSave11.Margin = new Padding(4, 3, 4, 3);
            picSave11.Name = "picSave11";
            picSave11.Size = new Size(28, 28);
            picSave11.TabIndex = 5;
            picSave11.TabStop = false;
            picSave11.Click += PicSave_Click;
            // 
            // picSave08
            // 
            picSave08.BorderStyle = BorderStyle.FixedSingle;
            picSave08.Location = new Point(259, 22);
            picSave08.Margin = new Padding(4, 3, 4, 3);
            picSave08.Name = "picSave08";
            picSave08.Size = new Size(28, 28);
            picSave08.TabIndex = 5;
            picSave08.TabStop = false;
            picSave08.Click += PicSave_Click;
            // 
            // picSave09
            // 
            picSave09.BorderStyle = BorderStyle.FixedSingle;
            picSave09.Location = new Point(295, 22);
            picSave09.Margin = new Padding(4, 3, 4, 3);
            picSave09.Name = "picSave09";
            picSave09.Size = new Size(28, 28);
            picSave09.TabIndex = 5;
            picSave09.TabStop = false;
            picSave09.Click += PicSave_Click;
            // 
            // picSave06
            // 
            picSave06.BorderStyle = BorderStyle.FixedSingle;
            picSave06.Location = new Point(187, 22);
            picSave06.Margin = new Padding(4, 3, 4, 3);
            picSave06.Name = "picSave06";
            picSave06.Size = new Size(28, 28);
            picSave06.TabIndex = 5;
            picSave06.TabStop = false;
            picSave06.Click += PicSave_Click;
            // 
            // picSave07
            // 
            picSave07.BorderStyle = BorderStyle.FixedSingle;
            picSave07.Location = new Point(223, 22);
            picSave07.Margin = new Padding(4, 3, 4, 3);
            picSave07.Name = "picSave07";
            picSave07.Size = new Size(28, 28);
            picSave07.TabIndex = 5;
            picSave07.TabStop = false;
            picSave07.Click += PicSave_Click;
            // 
            // picSave04
            // 
            picSave04.BorderStyle = BorderStyle.FixedSingle;
            picSave04.Location = new Point(115, 22);
            picSave04.Margin = new Padding(4, 3, 4, 3);
            picSave04.Name = "picSave04";
            picSave04.Size = new Size(28, 28);
            picSave04.TabIndex = 5;
            picSave04.TabStop = false;
            picSave04.Click += PicSave_Click;
            // 
            // picSave05
            // 
            picSave05.BorderStyle = BorderStyle.FixedSingle;
            picSave05.Location = new Point(151, 22);
            picSave05.Margin = new Padding(4, 3, 4, 3);
            picSave05.Name = "picSave05";
            picSave05.Size = new Size(28, 28);
            picSave05.TabIndex = 5;
            picSave05.TabStop = false;
            picSave05.Click += PicSave_Click;
            // 
            // picSave02
            // 
            picSave02.BorderStyle = BorderStyle.FixedSingle;
            picSave02.Location = new Point(43, 22);
            picSave02.Margin = new Padding(4, 3, 4, 3);
            picSave02.Name = "picSave02";
            picSave02.Size = new Size(28, 28);
            picSave02.TabIndex = 5;
            picSave02.TabStop = false;
            picSave02.Click += PicSave_Click;
            // 
            // picSave03
            // 
            picSave03.BorderStyle = BorderStyle.FixedSingle;
            picSave03.Location = new Point(79, 22);
            picSave03.Margin = new Padding(4, 3, 4, 3);
            picSave03.Name = "picSave03";
            picSave03.Size = new Size(28, 28);
            picSave03.TabIndex = 5;
            picSave03.TabStop = false;
            picSave03.Click += PicSave_Click;
            // 
            // picSave01
            // 
            picSave01.BorderStyle = BorderStyle.FixedSingle;
            picSave01.Location = new Point(7, 22);
            picSave01.Margin = new Padding(4, 3, 4, 3);
            picSave01.Name = "picSave01";
            picSave01.Size = new Size(28, 28);
            picSave01.TabIndex = 5;
            picSave01.TabStop = false;
            picSave01.Click += PicSave_Click;
            // 
            // ColorDialogX
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(484, 461);
            Controls.Add(groupBox1);
            Controls.Add(txtColor);
            Controls.Add(picValue);
            Controls.Add(picSaturation);
            Controls.Add(picColorRange);
            Controls.Add(btnEyeDropper);
            Controls.Add(picMouseMovePreview);
            Controls.Add(picMouseDownPreview);
            Controls.Add(btnCancel);
            Controls.Add(btnOK);
            Controls.Add(tkbValue);
            Controls.Add(tkbSaturation);
            Controls.Add(tkbHue);
            Controls.Add(picColorBox);
            FormBorderStyle = FormBorderStyle.FixedToolWindow;
            Margin = new Padding(4, 3, 4, 3);
            Name = "ColorDialogX";
            Text = "Color Picker";
            FormClosing += ColorDialogX_FormClosing;
            Load += ColorDialogX_Load;
            VisibleChanged += ColorDialogX_VisibleChanged;
            ((System.ComponentModel.ISupportInitialize)picColorBox).EndInit();
            ((System.ComponentModel.ISupportInitialize)tkbHue).EndInit();
            ((System.ComponentModel.ISupportInitialize)picMouseDownPreview).EndInit();
            ((System.ComponentModel.ISupportInitialize)picMouseMovePreview).EndInit();
            ((System.ComponentModel.ISupportInitialize)picColorRange).EndInit();
            ((System.ComponentModel.ISupportInitialize)tkbSaturation).EndInit();
            ((System.ComponentModel.ISupportInitialize)picSaturation).EndInit();
            ((System.ComponentModel.ISupportInitialize)tkbValue).EndInit();
            ((System.ComponentModel.ISupportInitialize)picValue).EndInit();
            groupBox1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)picSave22).EndInit();
            ((System.ComponentModel.ISupportInitialize)picSave21).EndInit();
            ((System.ComponentModel.ISupportInitialize)picSave20).EndInit();
            ((System.ComponentModel.ISupportInitialize)picSave18).EndInit();
            ((System.ComponentModel.ISupportInitialize)picSave16).EndInit();
            ((System.ComponentModel.ISupportInitialize)picSave14).EndInit();
            ((System.ComponentModel.ISupportInitialize)picSave12).EndInit();
            ((System.ComponentModel.ISupportInitialize)picSave10).EndInit();
            ((System.ComponentModel.ISupportInitialize)picSave19).EndInit();
            ((System.ComponentModel.ISupportInitialize)picSave17).EndInit();
            ((System.ComponentModel.ISupportInitialize)picSave15).EndInit();
            ((System.ComponentModel.ISupportInitialize)picSave13).EndInit();
            ((System.ComponentModel.ISupportInitialize)picSave11).EndInit();
            ((System.ComponentModel.ISupportInitialize)picSave08).EndInit();
            ((System.ComponentModel.ISupportInitialize)picSave09).EndInit();
            ((System.ComponentModel.ISupportInitialize)picSave06).EndInit();
            ((System.ComponentModel.ISupportInitialize)picSave07).EndInit();
            ((System.ComponentModel.ISupportInitialize)picSave04).EndInit();
            ((System.ComponentModel.ISupportInitialize)picSave05).EndInit();
            ((System.ComponentModel.ISupportInitialize)picSave02).EndInit();
            ((System.ComponentModel.ISupportInitialize)picSave03).EndInit();
            ((System.ComponentModel.ISupportInitialize)picSave01).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        private PictureBox picColorBox;
        private TrackBar tkbHue;
        private Button btnOK;
        private Button btnCancel;
        private PictureBox picMouseDownPreview;
        private PictureBox picMouseMovePreview;
        private Button btnEyeDropper;
        private PictureBox picColorRange;
        private TextBox txtColor;
        private NumericUpDown numericS;
        private TrackBar tkbSaturation;
        private PictureBox picSaturation;
        private TrackBar tkbValue;
        private PictureBox picValue;
        private GroupBox groupBox1;
        private Button btnAdd;
        private PictureBox picSave01;
        private PictureBox picSave02;
        private PictureBox picSave03;
        private PictureBox picSave04;
        private PictureBox picSave05;
        private PictureBox picSave06;
        private PictureBox picSave07;
        private PictureBox picSave08;
        private PictureBox picSave09;
        private PictureBox picSave10;
        private PictureBox picSave11;
        private PictureBox picSave12;
        private PictureBox picSave13;
        private PictureBox picSave14;
        private PictureBox picSave15;
        private PictureBox picSave16;
        private PictureBox picSave17;
        private PictureBox picSave18;
        private PictureBox picSave19;
        private PictureBox picSave20;
        private PictureBox picSave21;
        private PictureBox picSave22;
    }
}