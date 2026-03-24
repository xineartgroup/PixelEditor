namespace PixelEditor
{
    partial class FormSelections
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
            trackBar1 = new TrackBar();
            label1 = new Label();
            lblAmount = new Label();
            btnOk = new Button();
            btnCancel = new Button();
            ((System.ComponentModel.ISupportInitialize)trackBar1).BeginInit();
            SuspendLayout();
            // 
            // trackBar1
            // 
            trackBar1.Location = new Point(72, 28);
            trackBar1.Maximum = 50;
            trackBar1.Name = "trackBar1";
            trackBar1.Size = new Size(229, 45);
            trackBar1.TabIndex = 0;
            trackBar1.TickStyle = TickStyle.None;
            trackBar1.Value = 1;
            trackBar1.Scroll += TrackBar1_Scroll;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(12, 28);
            label1.Name = "label1";
            label1.Size = new Size(54, 15);
            label1.TabIndex = 1;
            label1.Text = "Amount:";
            // 
            // lblAmount
            // 
            lblAmount.AutoSize = true;
            lblAmount.Location = new Point(307, 28);
            lblAmount.Name = "lblAmount";
            lblAmount.Size = new Size(15, 15);
            lblAmount.TabIndex = 1;
            lblAmount.Text = "[]";
            // 
            // btnOk
            // 
            btnOk.Location = new Point(85, 100);
            btnOk.Name = "btnOk";
            btnOk.Size = new Size(75, 23);
            btnOk.TabIndex = 2;
            btnOk.Text = "OK";
            btnOk.UseVisualStyleBackColor = true;
            btnOk.Click += BtnOk_Click;
            // 
            // btnCancel
            // 
            btnCancel.Location = new Point(166, 100);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(75, 23);
            btnCancel.TabIndex = 2;
            btnCancel.Text = "Cancel";
            btnCancel.UseVisualStyleBackColor = true;
            btnCancel.Click += BtnCancel_Click;
            // 
            // FormSelections
            // 
            AcceptButton = btnOk;
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = btnCancel;
            ClientSize = new Size(334, 141);
            Controls.Add(btnCancel);
            Controls.Add(btnOk);
            Controls.Add(lblAmount);
            Controls.Add(label1);
            Controls.Add(trackBar1);
            Name = "FormSelections";
            Text = "Grow/Shrink";
            Load += FormSelections_Load;
            ((System.ComponentModel.ISupportInitialize)trackBar1).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private TrackBar trackBar1;
        private Label label1;
        private Label lblAmount;
        private Button btnOk;
        private Button btnCancel;
    }
}