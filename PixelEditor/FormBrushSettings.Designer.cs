namespace PixelEditor
{
    partial class FormBrushSettings
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
            groupMagicWand = new GroupBox();
            selectionThreshold = new TrackBar();
            label12 = new Label();
            label13 = new Label();
            cboMWSelectionMode = new ComboBox();
            groupMagicWand.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)selectionThreshold).BeginInit();
            SuspendLayout();
            // 
            // groupMagicWand
            // 
            groupMagicWand.Controls.Add(selectionThreshold);
            groupMagicWand.Controls.Add(label12);
            groupMagicWand.Controls.Add(label13);
            groupMagicWand.Controls.Add(cboMWSelectionMode);
            groupMagicWand.Location = new Point(77, 58);
            groupMagicWand.Name = "groupMagicWand";
            groupMagicWand.Size = new Size(230, 116);
            groupMagicWand.TabIndex = 0;
            groupMagicWand.TabStop = false;
            groupMagicWand.Text = "Magic Wand";
            // 
            // selectionThreshold
            // 
            selectionThreshold.Location = new Point(74, 65);
            selectionThreshold.Name = "selectionThreshold";
            selectionThreshold.Size = new Size(150, 45);
            selectionThreshold.TabIndex = 2;
            selectionThreshold.TickStyle = TickStyle.None;
            selectionThreshold.Scroll += SelectionThreshold_Scroll;
            // 
            // label12
            // 
            label12.AutoSize = true;
            label12.Location = new Point(6, 65);
            label12.Name = "label12";
            label12.Size = new Size(63, 15);
            label12.TabIndex = 1;
            label12.Text = "Threshold:";
            // 
            // label13
            // 
            label13.AutoSize = true;
            label13.Location = new Point(6, 25);
            label13.Name = "label13";
            label13.Size = new Size(57, 15);
            label13.TabIndex = 1;
            label13.Text = "Select By:";
            // 
            // cboMWSelectionMode
            // 
            cboMWSelectionMode.DropDownStyle = ComboBoxStyle.DropDownList;
            cboMWSelectionMode.FormattingEnabled = true;
            cboMWSelectionMode.Items.AddRange(new object[] { "All", "Red", "Green", "Blue", "Alpha" });
            cboMWSelectionMode.Location = new Point(74, 22);
            cboMWSelectionMode.Name = "cboMWSelectionMode";
            cboMWSelectionMode.Size = new Size(150, 23);
            cboMWSelectionMode.TabIndex = 0;
            // 
            // FormBrushSettings
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(groupMagicWand);
            Name = "FormBrushSettings";
            Text = "FormBrushSettings";
            groupMagicWand.ResumeLayout(false);
            groupMagicWand.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)selectionThreshold).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private GroupBox groupMagicWand;
        private TrackBar selectionThreshold;
        private Label label12;
        private Label label13;
        private ComboBox cboMWSelectionMode;
    }
}