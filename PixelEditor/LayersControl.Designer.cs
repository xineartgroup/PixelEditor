namespace PixelEditor
{
    partial class LayersControl
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            flowLayers = new FlowLayoutPanel();
            SuspendLayout();
            // 
            // flowLayers
            // 
            flowLayers.AutoScroll = true;
            flowLayers.BackColor = Color.White;
            flowLayers.BorderStyle = BorderStyle.FixedSingle;
            flowLayers.Dock = DockStyle.Top;
            flowLayers.Location = new Point(0, 0);
            flowLayers.Name = "flowLayers";
            flowLayers.Size = new Size(185, 194);
            flowLayers.TabIndex = 0;
            // 
            // LayersControl
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(flowLayers);
            MinimumSize = new Size(185, 200);
            Name = "LayersControl";
            Size = new Size(185, 200);
            ResumeLayout(false);
        }

        #endregion

        private FlowLayoutPanel flowLayers;
    }
}
