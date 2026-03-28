namespace PixelEditor
{
    public partial class FormSettings : Form
    {
        public int LayerWidth = 2;
        public int LayerHeight = 2;
        public int LayerX = 0;
        public int LayerY = 0;
        public bool ResizeImage = false;

        public FormSettings()
        {
            InitializeComponent();
        }

        private void FormSettings_Load(object sender, EventArgs e)
        {
            canvasWidth.Value = Document.Width;
            canvasHeight.Value = Document.Height;
            layerWidth.Value = LayerWidth >= layerWidth.Minimum && LayerWidth <= layerWidth.Maximum ? LayerWidth : 2;
            layerHeight.Value = LayerHeight >= layerHeight.Minimum && LayerHeight <= layerHeight.Maximum ? LayerHeight : 2;
        }

        private void BtnOK_Click(object sender, EventArgs e)
        {
            Document.Width = (int)canvasWidth.Value;
            Document.Height = (int)canvasHeight.Value;
            LayerWidth = (int)layerWidth.Value;
            LayerHeight = (int)layerHeight.Value;
            ResizeImage = chkResizeImage.Checked;
            if (chkCenterLayer.Checked)
            {
                LayerX = (Document.Width - (int)layerWidth.Value) / 2;
                LayerY = (Document.Height - (int)layerHeight.Value) / 2;
            }
            DialogResult = DialogResult.OK;
            Close();
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void BtnAutoResizeCanvas_Click(object sender, EventArgs e)
        {
            canvasWidth.Value = layerWidth.Value;
            canvasHeight.Value = layerHeight.Value;
        }

        private void BtnAutoResizeLayer_Click(object sender, EventArgs e)
        {
            layerWidth.Value = canvasWidth.Value;
            layerHeight.Value = canvasHeight.Value;
        }

        private void BtnResizeWidth_Click(object sender, EventArgs e)
        {
            int prevWidth = (int)layerWidth.Value;
            int prevHeight = (int)layerHeight.Value;
            layerWidth.Value = canvasWidth.Value;
            layerHeight.Value = canvasWidth.Value * prevHeight / prevWidth;
        }

        private void BtnResizeHeight_Click(object sender, EventArgs e)
        {
            int prevWidth = (int)layerWidth.Value;
            int prevHeight = (int)layerHeight.Value;
            layerWidth.Value = prevWidth * canvasHeight.Value / prevHeight;
            layerHeight.Value = canvasHeight.Value;
        }

        private void CanvasWidth_ValueChanged(object sender, EventArgs e)
        {
            if (chkCanvasRatio.Checked)
            {
                canvasHeight.Value = canvasWidth.Value * Document.Height / Document.Width;
            }
        }

        private void CanvasHeight_ValueChanged(object sender, EventArgs e)
        {
            if (chkCanvasRatio.Checked)
            {
                canvasWidth.Value = canvasHeight.Value * Document.Width / Document.Height;
            }
        }

        private void LayerWidth_ValueChanged(object sender, EventArgs e)
        {
            if (chkLayerRatio.Checked)
            {
                layerHeight.Value = layerWidth.Value * LayerHeight / LayerWidth;
            }
        }

        private void LayerHeight_ValueChanged(object sender, EventArgs e)
        {
            if (chkLayerRatio.Checked)
            {
                layerWidth.Value = layerHeight.Value * LayerWidth / LayerHeight;
            }
        }

        private void BtnResetCanvas_Click(object sender, EventArgs e)
        {
            canvasWidth.Value = Document.Width;
            canvasHeight.Value = Document.Height;
        }

        private void BtnResetLayer_Click(object sender, EventArgs e)
        {
            layerWidth.Value = LayerWidth >= layerWidth.Minimum && LayerWidth <= layerWidth.Maximum ? LayerWidth : 2;
            layerHeight.Value = LayerHeight >= layerHeight.Minimum && LayerHeight <= layerHeight.Maximum ? LayerHeight : 2;
        }
    }
}
