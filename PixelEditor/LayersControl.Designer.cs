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
            flowLayers.AutoSize = false;
            flowLayers.AutoScroll = true;
            flowLayers.BackColor = Color.White;
            flowLayers.BorderStyle = BorderStyle.FixedSingle;
            flowLayers.Dock = DockStyle.Fill;
            flowLayers.Location = new Point(0, 0);
            flowLayers.Name = "flowLayers";
            flowLayers.Size = new Size(185, 254);
            flowLayers.TabIndex = 0;
            flowLayers.HorizontalScroll.Enabled = false;
            flowLayers.HorizontalScroll.Visible = false;
            flowLayers.HorizontalScroll.Maximum = 0;
            flowLayers.VerticalScroll.Enabled = true;
            flowLayers.VerticalScroll.Visible = true;
            flowLayers.AutoScrollMinSize = new Size(0, 0);
            flowLayers.WrapContents = false;
            flowLayers.FlowDirection = FlowDirection.TopDown;
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
